// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
    public sealed class VideoPlayer : IDisposable
    {
        private DynamicSoundEffectInstance? _audioStream;
        private Effect? _shaderProgram;
        private IntPtr _stateChangesPtr;
        private readonly Texture2D?[] _yuvTextures = new Texture2D[3];
        private Viewport _viewport;
        private bool _isMuted;
        private IntPtr _yuvData;
        private int _yuvDataLen;
        private int _currentFrame;
        private float _volume;
        private const int AudioBufferSize = 4096 * 2;
        private static readonly float[] _audioData = new float[AudioBufferSize];
        private static GCHandle _audioHandle = GCHandle.Alloc(_audioData, GCHandleType.Pinned);
        private readonly IntPtr _audioDataPtr = _audioHandle.AddrOfPinnedObject();
        private VertexBufferBinding _vertexBuffer;
        private readonly FNA3D.FNA3D_RenderTargetBinding[] _nativeVideoTexture = new FNA3D.FNA3D_RenderTargetBinding[3];
        private readonly FNA3D.FNA3D_RenderTargetBinding[] _nativeOldTargets = new FNA3D.FNA3D_RenderTargetBinding[GraphicsDevice.MaxRenderTargetBindings];
        // Used to restore our previous GL state.
        private readonly Texture?[] _oldTextures = new Texture[3];
        private readonly SamplerState?[] _oldSamplers = new SamplerState[3];
        private RenderTargetBinding[]? _oldTargets;
        private VertexBufferBinding[]? _oldBuffers;
        private BlendState? _prevBlend;
        private DepthStencilState? _prevDepthStencil;
        private RasterizerState? _prevRasterizer;
        private Viewport _prevViewport;

        // We use this to update our PlayPosition.
        private readonly Stopwatch _timer;

        // Store this to optimize things on our end.
        private readonly RenderTargetBinding[] _videoTexture;

        // We need to access the GraphicsDevice frequently.
        private GraphicsDevice? _currentDevice;

        private static readonly VertexPositionTexture[] Vertices =
        {
            new VertexPositionTexture(
                new Vector3(-1.0f, -1.0f, 0.0f),
                new Vector2(0.0f, 1.0f)),
            new VertexPositionTexture(
                new Vector3(1.0f, -1.0f, 0.0f),
                new Vector2(1.0f, 1.0f)),
            new VertexPositionTexture(
                new Vector3(-1.0f, 1.0f, 0.0f),
                new Vector2(0.0f, 0.0f)),
            new VertexPositionTexture(
                new Vector3(1.0f, 1.0f, 0.0f),
                new Vector2(1.0f, 0.0f))
        };

        public bool IsDisposed { get; private set; }

        public bool IsLooped { get; set; }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                _isMuted = value;
                UpdateVolume();
            }
        }

        public TimeSpan PlayPosition => _timer.Elapsed;

        public MediaState State { get; private set; }

        public Video? Video { get; private set; }

        public float Volume
        {
            get => _volume;
            set
            {
                if (value > 1.0f)
                {
                    _volume = 1.0f;
                }
                else if (value < 0.0f)
                {
                    _volume = 0.0f;
                }
                else
                {
                    _volume = value;
                }

                UpdateVolume();
            }
        }

        public VideoPlayer()
        {
            // Initialize public members.
            IsDisposed = false;
            IsLooped = false;
            IsMuted = false;
            State = MediaState.Stopped;
            Volume = 1.0f;

            // Initialize private members.
            _timer = new Stopwatch();
            _videoTexture = new RenderTargetBinding[1];
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            // Stop the VideoPlayer. This gets almost everything...
            Stop();

            // Destroy the other GL bits.
            GL_dispose();

            // Dispose the DynamicSoundEffectInstance
            if (_audioStream != null)
            {
                _audioStream.Dispose();
                _audioStream = null;
            }

            // Dispose the Texture.
            var texture = _videoTexture[0].RenderTarget;
            texture?.Dispose();

            // Free the YUV buffer
            if (_yuvData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_yuvData);
                _yuvData = IntPtr.Zero;
            }

            // Okay, we out.
            IsDisposed = true;
        }

        public Texture2D GetTexture()
        {
            CheckDisposed();

            if (Video == null)
            {
                throw new InvalidOperationException();
            }

            // Be sure we can even get something from theorafile...
            if (State == MediaState.Stopped ||
                Video._theora == IntPtr.Zero ||
                Theorafile.tf_hasvideo(Video._theora) == 0)
            {
                // Screw it, give them the old one.
                var texture = _videoTexture[0].RenderTarget as Texture2D;
                return texture!;
            }

            var thisFrame = (int)(_timer.Elapsed.TotalMilliseconds / (1000.0 / Video._fps));
            if (thisFrame > _currentFrame)
            {
                // Only update the textures if we need to!
                if (Theorafile.tf_readvideo(Video._theora, _yuvData, thisFrame - _currentFrame) ==
                    1 || _currentFrame == -1)
                {
                    UpdateTexture();
                }

                _currentFrame = thisFrame;
            }

            // Check for the end...
            var ended = Theorafile.tf_eos(Video._theora) == 1;
            if (_audioStream != null)
            {
                ended &= _audioStream.PendingBufferCount == 0;
            }

            if (ended)
            {
                // FIXME: This is part of the Duration hack!
                if (Video._needsDurationHack)
                {
                    Video.Duration = _timer.Elapsed; // FIXME: Frames * FPS? -flibit
                }

                // Stop and reset the timer. If we're looping, the loop will start it again.
                _timer.Stop();
                _timer.Reset();

                // Kill whatever audio/video we've got
                if (_audioStream != null)
                {
                    _audioStream.Stop();
                    _audioStream.Dispose();
                    _audioStream = null;
                }

                // Reset the stream no matter what happens next
                Theorafile.tf_reset(Video._theora);

                // If looping, go back to the start. Otherwise, we'll be exiting.
                if (IsLooped)
                {
                    // Starting over!
                    InitializeTheoraStream();

                    // Start! Again!
                    _timer.Start();
                    _audioStream?.Play();
                }
                else
                {
                    // We out
                    State = MediaState.Stopped;
                }
            }

            // Finally.
            var result = _videoTexture[0].RenderTarget as Texture2D;
            return result!;
        }

        public void Play(Video video)
        {
            CheckDisposed();

            // We need to assign this regardless of what happens next.
            Video = video;

            // FIXME: This is a part of the Duration hack!
            if (Video._needsDurationHack)
            {
                Video.Duration = TimeSpan.MaxValue;
            }

            // Check the player state before attempting anything.
            if (State != MediaState.Stopped)
            {
                return;
            }

            // Update the player state now, before initializing
            State = MediaState.Playing;

            // Carve out YUV buffer before doing any decoder work
            if (_yuvData != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_yuvData);
            }

            _yuvDataLen = (Video._yWidth * Video._yHeight) +
                          (Video._uvWidth * Video._uvHeight * 2);
            _yuvData = Marshal.AllocHGlobal(_yuvDataLen);

            // Hook up the decoder to this player
            InitializeTheoraStream();

            // Set up the texture data
            if (Theorafile.tf_hasvideo(Video._theora) == 1)
            {
                // The VideoPlayer will use the GraphicsDevice that is set now.
                if (_currentDevice == null)
                {
                    GL_dispose();
                    _currentDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
                    GL_initialize();
                }

                var overlap = _videoTexture[0];
                _videoTexture[0] = new RenderTargetBinding(
                    new RenderTarget2D(
                        Video._yWidth,
                        Video._yHeight,
                        false,
                        SurfaceFormat.Color,
                        DepthFormat.None,
                        0,
                        RenderTargetUsage.PreserveContents));
                overlap.RenderTarget?.Dispose();

                GL_setupTextures(Video._yWidth, Video._yHeight, Video._uvWidth, Video._uvHeight);
            }

            // The player can finally start now!
            _timer.Start();
            _audioStream?.Play();
        }

        public void Stop()
        {
            CheckDisposed();

            // Check the player state before attempting anything.
            if (State == MediaState.Stopped)
            {
                return;
            }

            // Update the player state.
            State = MediaState.Stopped;

            // Wait for the player to end if it's still going.
            _timer.Stop();
            _timer.Reset();
            if (_audioStream != null)
            {
                _audioStream.Stop();
                _audioStream.Dispose();
                _audioStream = null;
            }

            Theorafile.tf_reset(Video!._theora);
        }

        public void Pause()
        {
            CheckDisposed();

            // Check the player state before attempting anything.
            if (State != MediaState.Playing)
            {
                return;
            }

            // Update the player state.
            State = MediaState.Paused;

            // Pause timer, audio.
            _timer.Stop();
            _audioStream?.Pause();
        }

        public void Resume()
        {
            CheckDisposed();

            // Check the player state before attempting anything.
            if (State != MediaState.Paused)
            {
                return;
            }

            // Update the player state.
            State = MediaState.Playing;

            // Unpause timer, audio.
            _timer.Start();
            _audioStream?.Resume();
        }

        private void CheckDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("VideoPlayer");
            }
        }

        private void UpdateVolume()
        {
            if (_audioStream == null)
            {
                return;
            }

            if (IsMuted)
            {
                _audioStream.Volume = 0.0f;
            }
            else
            {
                /* FIXME: Works around MasterVolume only temporarily!
                 * We need to detach this source from the AL listener properties.
                 * -flibit
                 */
                _audioStream.Volume = Volume * (1.0f / SoundEffect.MasterVolume);
            }
        }

        private void OnBufferRequest(object sender, EventArgs args)
        {
            var samples = Theorafile.tf_readaudio(
                Video!._theora,
                _audioDataPtr,
                AudioBufferSize);
            if (samples > 0)
            {
                _audioStream!.SubmitFloatBufferEXT(
                    _audioData,
                    0,
                    samples);
            }
            else if (Theorafile.tf_eos(Video._theora) == 1)
            {
                // Okay, we ran out. No need for this!
                _audioStream!.BufferNeeded -= OnBufferRequest;
            }
        }

        private void UpdateTexture()
        {
            // Prepare YUV GL textures with our current frame data
            FNA3D.FNA3D_SetTextureDataYUV(
                _currentDevice!.GLDevice,
                _yuvTextures[0]!._texture,
                _yuvTextures[1]!._texture,
                _yuvTextures[2]!._texture,
                Video!._yWidth,
                Video._yHeight,
                Video._uvWidth,
                Video._uvHeight,
                _yuvData,
                _yuvDataLen);

            // Draw the YUV textures to the framebuffer with our shader.
            GL_pushState();
            _currentDevice.DrawPrimitives(
                PrimitiveType.TriangleStrip,
                0,
                2);
            GL_popState();
        }

        private void InitializeTheoraStream()
        {
            // Grab the first video frame ASAP.
            while (Theorafile.tf_readvideo(Video!._theora, _yuvData, 1) == 0)
            {
            }

            // Grab the first bit of audio. We're trying to start the decoding ASAP.
            if (Theorafile.tf_hasaudio(Video._theora) == 1)
            {
                Theorafile.tf_audioinfo(Video._theora, out var channels, out var sampleRate);
                _audioStream = new DynamicSoundEffectInstance(
                    sampleRate,
                    (AudioChannels)channels);
                _audioStream.BufferNeeded += OnBufferRequest;
                UpdateVolume();

                // Fill and queue the buffers.
                for (var i = 0; i < 4; i += 1)
                {
                    OnBufferRequest(_audioStream, EventArgs.Empty);
                    if (_audioStream.PendingBufferCount == i)
                    {
                        break;
                    }
                }
            }

            _currentFrame = -1;
        }

        private void GL_initialize()
        {
            // Load the YUV->RGBA Effect
            _shaderProgram = new Effect(Resources.YUVToRGBAEffect);
            unsafe
            {
                _stateChangesPtr = Marshal.AllocHGlobal(sizeof(Effect.MOJOSHADER_effectStateChanges));
            }

            // Allocate the vertex buffer
            _vertexBuffer = new VertexBufferBinding(
                new VertexBuffer(
                    VertexPositionTexture.VertexDeclaration,
                    4,
                    BufferUsage.WriteOnly));
            _vertexBuffer.VertexBuffer!.SetData(Vertices);
        }

        private void GL_dispose()
        {
            if (_currentDevice == null)
            {
                // We never initialized to begin with...
                return;
            }

            _currentDevice = null;

            // Delete the Effect
            _shaderProgram?.Dispose();

            if (_stateChangesPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_stateChangesPtr);
            }

            // Delete the vertex buffer
            _vertexBuffer.VertexBuffer?.Dispose();

            // Delete the textures if they exist
            for (var i = 0; i < 3; i += 1)
            {
                var yuvTexture = _yuvTextures[i];
                yuvTexture?.Dispose();
            }
        }

        private void GL_setupTextures(int yWidth, int yHeight, int uvWidth, int uvHeight)
        {
            // Allocate YUV GL textures
            for (var i = 0; i < 3; i += 1)
            {
                var yuvTexture = _yuvTextures[i];
                yuvTexture?.Dispose();
            }

            _yuvTextures[0] = new Texture2D(
                yWidth,
                yHeight,
                false,
                SurfaceFormat.Alpha8);
            _yuvTextures[1] = new Texture2D(
                uvWidth,
                uvHeight,
                false,
                SurfaceFormat.Alpha8);
            _yuvTextures[2] = new Texture2D(
                uvWidth,
                uvHeight,
                false,
                SurfaceFormat.Alpha8);

            // Precalculate the viewport
            _viewport = new Viewport(0, 0, yWidth, yHeight);
        }

        private void GL_pushState()
        {
            // Begin the effect, flagging to restore previous state on end
            FNA3D.FNA3D_BeginPassRestore(
                _currentDevice!.GLDevice,
                _shaderProgram!._glEffect,
                _stateChangesPtr);

            // Prep our samplers
            for (var i = 0; i < 3; i += 1)
            {
                _oldTextures[i] = _currentDevice.Textures[i];
                _oldSamplers[i] = _currentDevice.SamplerStates[i];
                _currentDevice.Textures[i] = _yuvTextures[i];
                _currentDevice.SamplerStates[i] = SamplerState.LinearClamp;
            }

            // Prep buffers
            _oldBuffers = _currentDevice.GetVertexBuffers();
            _currentDevice.SetVertexBuffers(_vertexBuffer);

            // Prep target bindings
            _oldTargets = _currentDevice.GetRenderTargets();

            unsafe
            {
                fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &_nativeVideoTexture[0])
                {
                    GraphicsDevice.PrepareRenderTargetBindings(rt, _videoTexture);
                    FNA3D.FNA3D_SetRenderTargets(
                        _currentDevice.GLDevice,
                        rt,
                        _videoTexture.Length,
                        IntPtr.Zero,
                        DepthFormat.None,
                        0);
                }
            }

            // Prep render state
            _prevBlend = _currentDevice.BlendState;
            _prevDepthStencil = _currentDevice.DepthStencilState;
            _prevRasterizer = _currentDevice.RasterizerState;
            _currentDevice.BlendState = BlendState.Opaque;
            _currentDevice.DepthStencilState = DepthStencilState.None;
            _currentDevice.RasterizerState = RasterizerState.CullNone;

            // Prep viewport
            _prevViewport = _currentDevice.Viewport;
            FNA3D.FNA3D_SetViewport(_currentDevice.GLDevice, ref _viewport._viewport);
        }

        private void GL_popState()
        {
            // End the effect, restoring the previous shader state
            FNA3D.FNA3D_EndPassRestore(_currentDevice!.GLDevice, _shaderProgram!._glEffect);

            // Restore GL state
            _currentDevice.BlendState = _prevBlend;
            _currentDevice.DepthStencilState = _prevDepthStencil;
            _currentDevice.RasterizerState = _prevRasterizer;
            _prevBlend = null;
            _prevDepthStencil = null;
            _prevRasterizer = null;

            /* Restore targets using GLDevice directly.
             * This prevents accidental clearing of previously bound targets.
             */
            if (_oldTargets == null || _oldTargets.Length == 0)
            {
                FNA3D.FNA3D_SetRenderTargets(
                    _currentDevice.GLDevice,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    DepthFormat.None,
                    0);
            }
            else
            {
                var oldTarget = _oldTargets[0].RenderTarget as IRenderTarget;

                unsafe
                {
                    fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &_nativeOldTargets[0])
                    {
                        GraphicsDevice.PrepareRenderTargetBindings(rt, _oldTargets);
                        FNA3D.FNA3D_SetRenderTargets(
                            _currentDevice.GLDevice,
                            rt,
                            _oldTargets.Length,
                            oldTarget!.DepthStencilBuffer,
                            oldTarget.DepthStencilFormat,
                            (byte)(oldTarget.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0));
                    }
                }
            }

            _oldTargets = null;

            // Set viewport AFTER setting targets!
            FNA3D.FNA3D_SetViewport(_currentDevice.GLDevice, ref _prevViewport._viewport);

            // Restore buffers
            _currentDevice.SetVertexBuffers(_oldBuffers);
            _oldBuffers = null;

            // Restore samplers
            for (var i = 0; i < 3; i += 1)
            {
                /* The application may have set a texture ages
                 * ago, only to not unset after disposing. We
                 * have to avoid an ObjectDisposedException!
                 */
                var oldTexture = _oldTextures[i];
                if (oldTexture == null || !oldTexture.IsDisposed)
                {
                    _currentDevice.Textures[i] = _oldTextures[i];
                }

                _currentDevice.SamplerStates[i] = _oldSamplers[i];
                _oldTextures[i] = null;
                _oldSamplers[i] = null;
            }
        }
    }
}
