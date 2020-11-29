// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public class GraphicsDevice : IDisposable
    {
        // Per XNA4 General Spec
        internal const int MaxTextureSamples = 16;

        // Per XNA4 HiDef Spec
        internal const int MaxVertexAttributes = 16;
        internal const int MaxRenderTargetBindings = 4;
        internal const int MaxVertexTextureSamplers = 4;

        // Some of these are internal for validation purposes
        internal readonly RenderTargetBinding[] _renderTargetBindings = new RenderTargetBinding[MaxRenderTargetBindings];
        internal int _renderTargetCount;

        internal readonly IntPtr GLDevice;
        internal readonly PipelineCache PipelineCache;

        /* We have to store this internally because we flip the Viewport for
        * when we aren't rendering to a target. I'd love to remove this.
        * -flibit
        */
        private Viewport _viewport;

        /* We have to store this internally because we flip the Rectangle for
         * when we aren't rendering to a target. I'd love to remove this.
         * -flibit
         */
        private Rectangle _scissorRectangle;

        private readonly FNA3D.FNA3D_RenderTargetBinding[] _nativeTargetBindings =
            new FNA3D.FNA3D_RenderTargetBinding[MaxRenderTargetBindings];

        private readonly FNA3D.FNA3D_RenderTargetBinding[] _nativeTargetBindingsNext =
            new FNA3D.FNA3D_RenderTargetBinding[MaxRenderTargetBindings];

        // Used to prevent alloc on SetRenderTarget()
        private readonly RenderTargetBinding[] _singleTargetCache = new RenderTargetBinding[1];

        private readonly VertexBufferBinding[] _vertexBufferBindings =
            new VertexBufferBinding[MaxVertexAttributes];

        private readonly FNA3D.FNA3D_VertexBufferBinding[] _nativeBufferBindings =
            new FNA3D.FNA3D_VertexBufferBinding[MaxVertexAttributes];

        private int _vertexBufferCount;
        private bool _vertexBuffersUpdated;

        // Used for client arrays
        private IntPtr _userVertexBuffer;
        private IntPtr _userIndexBuffer;
        private int _userVertexBufferSize;
        private int _userIndexBufferSize;
        private BlendState? _currentBlend;
        private DepthStencilState? _currentDepthStencil;

        private readonly bool[] _modifiedSamplers = new bool[MaxTextureSamples];
        private readonly bool[] _modifiedVertexSamplers = new bool[MaxVertexTextureSamplers];

        /* Use WeakReference for the global resources list as we do not
         * know when a resource may be disposed and collected. We do not
         * want to prevent a resource from being collected by holding a
         * strong reference to it in this list.
         */
        private readonly List<WeakReference> _resources = new List<WeakReference>();
        private readonly object _resourcesLock = new object();

        /* On Intel Integrated graphics, there is a fast hw unit for doing
         * clears to colors where all components are either 0 or 255.
         * Despite XNA4 using Purple here, we use black (in Release) to avoid
         * performance warnings on Intel/Mesa.
         */
#if DEBUG
        private static readonly Vector4 DiscardColor = new Color(68, 34, 136, 255).ToVector4();
#else
		private static readonly Vector4 DiscardColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
#endif

        public bool IsDisposed { get; private set; }

        public GraphicsDeviceStatus GraphicsDeviceStatus => GraphicsDeviceStatus.Normal;

        public GraphicsAdapter Adapter { get; private set; }

        public GraphicsProfile GraphicsProfile { get; }

        public PresentationParameters PresentationParameters { get; private set; }

        public DisplayMode DisplayMode
        {
            get
            {
                if (PresentationParameters.IsFullScreen)
                {
                    FNA3D.FNA3D_GetBackbufferSize(GLDevice, out var w, out var h);
                    return new DisplayMode(
                        w,
                        h,
                        FNA3D.FNA3D_GetBackbufferSurfaceFormat(GLDevice));
                }

                return Adapter.CurrentDisplayMode;
            }
        }

        public TextureCollection Textures { get; }

        public SamplerStateCollection SamplerStates { get; }

        public TextureCollection VertexTextures { get; }

        public SamplerStateCollection VertexSamplerStates { get; }

        public BlendState BlendState { get; set; }

        public DepthStencilState DepthStencilState { get; set; }

        public RasterizerState RasterizerState { get; set; }

        public Rectangle ScissorRectangle
        {
            get => _scissorRectangle;
            set
            {
                _scissorRectangle = value;
                FNA3D.FNA3D_SetScissorRect(
                    GLDevice,
                    ref value);
            }
        }

        public Viewport Viewport
        {
            get => _viewport;
            set
            {
                _viewport = value;
                FNA3D.FNA3D_SetViewport(GLDevice, ref value._viewport);
            }
        }

        public Color BlendFactor
        {
            get
            {
                FNA3D.FNA3D_GetBlendFactor(GLDevice, out var result);
                return result;
            }
            set =>
                /* FIXME: Does this affect the value found in
                 * BlendState?
                 * -flibit
                 */
                FNA3D.FNA3D_SetBlendFactor(GLDevice, ref value);
        }

        public int MultiSampleMask
        {
            get => FNA3D.FNA3D_GetMultiSampleMask(GLDevice);
            set =>
                /* FIXME: Does this affect the value found in
                 * BlendState?
                 * -flibit
                 */
                FNA3D.FNA3D_SetMultiSampleMask(GLDevice, value);
        }

        public int ReferenceStencil
        {
            get => FNA3D.FNA3D_GetReferenceStencil(GLDevice);
            set =>
                /* FIXME: Does this affect the value found in
                 * DepthStencilState?
                 * -flibit
                 */
                FNA3D.FNA3D_SetReferenceStencil(GLDevice, value);
        }

        public IndexBuffer? Indices { get; set; }

        // We never lose devices, but lol XNA4 compliance -flibit
#pragma warning disable 67
        public event EventHandler<EventArgs>? DeviceLost;
#pragma warning restore 67

        public event EventHandler<EventArgs>? DeviceReset;

        public event EventHandler<EventArgs>? DeviceResetting;

        public event EventHandler<ResourceCreatedEventArgs>? ResourceCreated;

        public event EventHandler<ResourceDestroyedEventArgs>? ResourceDestroyed;

        public event EventHandler<EventArgs>? Disposing;

        // TODO: Hook this up to GraphicsResource
        internal void OnResourceCreated(object resource)
        {
            ResourceCreated?.Invoke(this, new ResourceCreatedEventArgs(resource));
        }

        // TODO: Hook this up to GraphicsResource
        internal void OnResourceDestroyed(string name, object tag)
        {
            ResourceDestroyed?.Invoke(this, new ResourceDestroyedEventArgs(name, tag));
        }

        public GraphicsDevice(
            GraphicsAdapter adapter,
            GraphicsProfile graphicsProfile,
            PresentationParameters presentationParameters)
        {
            // Set the properties from the constructor parameters.
            Adapter = adapter;
            PresentationParameters =
                presentationParameters ?? throw new ArgumentNullException(nameof(presentationParameters));
            GraphicsProfile = graphicsProfile;
            PresentationParameters.MultiSampleCount =
                MathHelper.ClosestMSAAPower(PresentationParameters.MultiSampleCount);

            // Set up the FNA3D Device
            try
            {
                // ReSharper disable once JoinDeclarationAndInitializer
                byte debugMode;
#if DEBUG
                debugMode = 1;
#endif
                GLDevice = FNA3D.FNA3D_CreateDevice(ref PresentationParameters._parameters, debugMode);
            }
            catch (Exception e)
            {
                throw new NoSuitableGraphicsDeviceException(e.Message);
            }

            // The mouse needs to know this for faux-backbuffer mouse scaling.
            Mouse._backBufferWidth = PresentationParameters.BackBufferWidth;
            Mouse._backBufferHeight = PresentationParameters.BackBufferHeight;

            // The Touch Panel needs this too, for the same reason.
            TouchPanel.DisplayWidth = PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = PresentationParameters.BackBufferHeight;

            // Force set the default render states.
            BlendState = BlendState.Opaque;
            DepthStencilState = DepthStencilState.Default;
            RasterizerState = RasterizerState.CullCounterClockwise;

            // Initialize the Texture/Sampler state containers
            FNA3D.FNA3D_GetMaxTextureSlots(
                GLDevice,
                out var maxTextures,
                out var maxVertexTextures);
            Textures = new TextureCollection(
                maxTextures,
                _modifiedSamplers);
            SamplerStates = new SamplerStateCollection(
                maxTextures,
                _modifiedSamplers);
            VertexTextures = new TextureCollection(
                maxVertexTextures,
                _modifiedVertexSamplers);
            VertexSamplerStates = new SamplerStateCollection(
                maxVertexTextures,
                _modifiedVertexSamplers);

            // Set the default viewport and scissor rect.
            Viewport = new Viewport(PresentationParameters.Bounds);
            ScissorRectangle = Viewport.Bounds;

            // Allocate the pipeline cache to be used by Effects
            PipelineCache = new PipelineCache();
        }

        ~GraphicsDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Present()
        {
            FNA3D.FNA3D_SwapBuffers(
                GLDevice,
                IntPtr.Zero,
                IntPtr.Zero,
                PresentationParameters.DeviceWindowHandle);
        }

        public void Present(
            Rectangle? sourceRectangle,
            Rectangle? destinationRectangle,
            IntPtr overrideWindowHandle)
        {
            if (overrideWindowHandle == IntPtr.Zero)
            {
                overrideWindowHandle = PresentationParameters.DeviceWindowHandle;
            }

            if (sourceRectangle.HasValue && destinationRectangle.HasValue)
            {
                var src = sourceRectangle.Value;
                var dst = destinationRectangle.Value;
                FNA3D.FNA3D_SwapBuffers(
                    GLDevice,
                    ref src,
                    ref dst,
                    overrideWindowHandle);
            }
            else if (sourceRectangle.HasValue)
            {
                var src = sourceRectangle.Value;
                FNA3D.FNA3D_SwapBuffers(
                    GLDevice,
                    ref src,
                    IntPtr.Zero,
                    overrideWindowHandle);
            }
            else if (destinationRectangle.HasValue)
            {
                var dst = destinationRectangle.Value;
                FNA3D.FNA3D_SwapBuffers(
                    GLDevice,
                    IntPtr.Zero,
                    ref dst,
                    overrideWindowHandle);
            }
            else
            {
                FNA3D.FNA3D_SwapBuffers(
                    GLDevice,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    overrideWindowHandle);
            }
        }

        public void Reset()
        {
            Reset(PresentationParameters, Adapter);
        }

        public void Reset(PresentationParameters presentationParameters)
        {
            Reset(presentationParameters, Adapter);
        }

        public void Reset(
            PresentationParameters presentationParameters,
            GraphicsAdapter graphicsAdapter)
        {
            PresentationParameters = presentationParameters;
            Adapter = graphicsAdapter;

            // Verify MSAA before we really start...
            PresentationParameters.MultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
                GLDevice,
                PresentationParameters.BackBufferFormat,
                MathHelper.ClosestMSAAPower(PresentationParameters.MultiSampleCount));

            // We're about to reset, let the application know.
            DeviceResetting?.Invoke(this, EventArgs.Empty);

            /* FIXME: Why are we not doing this...? -flibit
            lock (resourcesLock)
            {
                foreach (WeakReference resource in resources)
                {
                    object target = resource.Target;
                    if (target != null)
                    {
                        (target as GraphicsResource).GraphicsDeviceResetting();
                    }
                }

                // Remove references to resources that have been garbage collected.
                resources.RemoveAll(wr => !wr.IsAlive);
            }
            */

            /* Reset the backbuffer first, before doing anything else.
             * The GLDevice needs to know what we're up to right away.
             * -flibit
             */
            FNA3D.FNA3D_ResetBackbuffer(
                GLDevice,
                ref PresentationParameters._parameters);

            // The mouse needs to know this for faux-backbuffer mouse scaling.
            Mouse._backBufferWidth = PresentationParameters.BackBufferWidth;
            Mouse._backBufferHeight = PresentationParameters.BackBufferHeight;

            // The Touch Panel needs this too, for the same reason.
            TouchPanel.DisplayWidth = PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = PresentationParameters.BackBufferHeight;

            // Now, update the viewport
            Viewport = new Viewport(
                0,
                0,
                PresentationParameters.BackBufferWidth,
                PresentationParameters.BackBufferHeight);

            // Update the scissor rectangle to our new default target size
            ScissorRectangle = new Rectangle(
                0,
                0,
                PresentationParameters.BackBufferWidth,
                PresentationParameters.BackBufferHeight);

            // We just reset, let the application know.
            DeviceReset?.Invoke(this, EventArgs.Empty);
        }

        public void Clear(Color color)
        {
            Clear(
                ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                color.ToVector4(),
                Viewport.MaxDepth,
                0);
        }

        public void Clear(ClearOptions options, Color color, float depth, int stencil)
        {
            Clear(
                options,
                color.ToVector4(),
                depth,
                stencil);
        }

        public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            /* FIXME: PresentationParameters.DepthStencilFormat is probably
            * a more accurate value here, but the Backbuffer may disagree.
            * -flibit
            */
            var renderTarget = _renderTargetBindings[0].RenderTarget as IRenderTarget;
            var dsFormat = _renderTargetCount == 0 ? FNA3D.FNA3D_GetBackbufferDepthFormat(GLDevice) : renderTarget!.DepthStencilFormat;

            if (dsFormat == DepthFormat.None)
            {
                options &= ClearOptions.Target;
            }
            else if (dsFormat != DepthFormat.Depth24Stencil8)
            {
                options &= ~ClearOptions.Stencil;
            }

            FNA3D.FNA3D_Clear(
                GLDevice,
                options,
                ref color,
                depth,
                stencil);
        }

        public void GetBackBufferData<T>(T[] data)
            where T : struct
        {
            GetBackBufferData(null, data, 0, data.Length);
        }

        public void GetBackBufferData<T>(
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            GetBackBufferData(null, data, startIndex, elementCount);
        }

        public void GetBackBufferData<T>(
            Rectangle? rect,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            int x, y, w, h;
            if (rect == null)
            {
                x = 0;
                y = 0;
                FNA3D.FNA3D_GetBackbufferSize(
                    GLDevice,
                    out w,
                    out h);
            }
            else
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                w = rect.Value.Width;
                h = rect.Value.Height;
            }

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            Texture.ValidateGetDataFormat(
                FNA3D.FNA3D_GetBackbufferSurfaceFormat(GLDevice),
                elementSizeInBytes);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            FNA3D.FNA3D_ReadBackbuffer(
                GLDevice,
                x,
                y,
                w,
                h,
                handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
                data.Length * elementSizeInBytes);
            handle.Free();
        }

        public void SetRenderTarget(RenderTarget2D? renderTarget)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _singleTargetCache[0] = new RenderTargetBinding(renderTarget);
                SetRenderTargets(_singleTargetCache);
            }
        }

        public void SetRenderTarget(RenderTargetCube? renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
            {
                SetRenderTargets(null);
            }
            else
            {
                _singleTargetCache[0] = new RenderTargetBinding(renderTarget, cubeMapFace);
                SetRenderTargets(_singleTargetCache);
            }
        }

        public void SetRenderTargets(params RenderTargetBinding[]? renderTargets)
        {
            // Checking for redundant SetRenderTargets...
            if (renderTargets == null && _renderTargetCount == 0)
            {
                return;
            }

            if (renderTargets != null && renderTargets.Length == _renderTargetCount)
            {
                var isRedundant = true;
                for (var i = 0; i < renderTargets.Length; i += 1)
                {
                    if (renderTargets[i].RenderTarget != _renderTargetBindings[i].RenderTarget ||
                        renderTargets[i].CubeMapFace != _renderTargetBindings[i].CubeMapFace)
                    {
                        isRedundant = false;
                        break;
                    }
                }

                if (isRedundant)
                {
                    return;
                }
            }

            int newWidth;
            int newHeight;
            RenderTargetUsage clearTarget;
            if (renderTargets == null || renderTargets.Length == 0)
            {
                FNA3D.FNA3D_SetRenderTargets(
                    GLDevice,
                    IntPtr.Zero,
                    0,
                    IntPtr.Zero,
                    DepthFormat.None,
                    (byte)(PresentationParameters.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0)); /* lol c# */

                // Set the viewport/scissor to the size of the backbuffer.
                newWidth = PresentationParameters.BackBufferWidth;
                newHeight = PresentationParameters.BackBufferHeight;
                clearTarget = PresentationParameters.RenderTargetUsage;

                // Resolve previous targets, if needed
                for (var i = 0; i < _renderTargetCount; i += 1)
                {
                    FNA3D.FNA3D_ResolveTarget(GLDevice, ref _nativeTargetBindings[i]);
                }

                Array.Clear(_renderTargetBindings, 0, _renderTargetBindings.Length);
                Array.Clear(_nativeTargetBindings, 0, _nativeTargetBindings.Length);
                _renderTargetCount = 0;
            }
            else
            {
                var target = renderTargets[0].RenderTarget as IRenderTarget;
                unsafe
                {
                    fixed (FNA3D.FNA3D_RenderTargetBinding* rt = &_nativeTargetBindingsNext[0])
                    {
                        PrepareRenderTargetBindings(rt, renderTargets);
                        FNA3D.FNA3D_SetRenderTargets(
                            GLDevice,
                            rt,
                            renderTargets.Length,
                            target!.DepthStencilBuffer,
                            target.DepthStencilFormat,
                            (byte)(target.RenderTargetUsage != RenderTargetUsage.DiscardContents ? 1 : 0)); /* lol c# */
                    }
                }

                // Set the viewport/scissor to the size of the first render target.
                newWidth = target.Width;
                newHeight = target.Height;
                clearTarget = target.RenderTargetUsage;

                // Resolve previous targets, if needed
                for (var i = 0; i < _renderTargetCount; i += 1)
                {
                    // We only need to resolve if the target is no longer bound.
                    var stillBound = false;
                    for (var j = 0; j < renderTargets.Length; j += 1)
                    {
                        if (_renderTargetBindings[i].RenderTarget == renderTargets[j].RenderTarget)
                        {
                            stillBound = true;
                            break;
                        }
                    }

                    if (stillBound)
                    {
                        continue;
                    }

                    FNA3D.FNA3D_ResolveTarget(GLDevice, ref _nativeTargetBindings[i]);
                }

                Array.Clear(_renderTargetBindings, 0, _renderTargetBindings.Length);
                Array.Copy(renderTargets, _renderTargetBindings, renderTargets.Length);
                Array.Clear(_nativeTargetBindings, 0, _nativeTargetBindings.Length);
                Array.Copy(_nativeTargetBindingsNext, _nativeTargetBindings, renderTargets.Length);
                _renderTargetCount = renderTargets.Length;
            }

            // Apply new GL state, clear target if requested
            Viewport = new Viewport(0, 0, newWidth, newHeight);
            ScissorRectangle = new Rectangle(0, 0, newWidth, newHeight);
            if (clearTarget == RenderTargetUsage.DiscardContents)
            {
                Clear(
                    ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
                    DiscardColor,
                    Viewport.MaxDepth,
                    0);
            }
        }

        public RenderTargetBinding[] GetRenderTargets()
        {
            // Return a correctly sized copy our internal array.
            RenderTargetBinding[] bindings = new RenderTargetBinding[_renderTargetCount];
            Array.Copy(_renderTargetBindings, bindings, _renderTargetCount);
            return bindings;
        }

        public void SetVertexBuffer(VertexBuffer vertexBuffer)
        {
            SetVertexBuffer(vertexBuffer, 0);
        }

        public void SetVertexBuffer(VertexBuffer? vertexBuffer, int vertexOffset)
        {
            if (vertexBuffer == null)
            {
                if (_vertexBufferCount == 0)
                {
                    return;
                }

                for (var i = 0; i < _vertexBufferCount; i += 1)
                {
                    _vertexBufferBindings[i] = VertexBufferBinding.None;
                }

                _vertexBufferCount = 0;
                _vertexBuffersUpdated = true;
                return;
            }

            if (!ReferenceEquals(_vertexBufferBindings[0].VertexBuffer, vertexBuffer) ||
                _vertexBufferBindings[0].VertexOffset != vertexOffset)
            {
                _vertexBufferBindings[0] = new VertexBufferBinding(
                    vertexBuffer,
                    vertexOffset);
                _vertexBuffersUpdated = true;
            }

            if (_vertexBufferCount > 1)
            {
                for (var i = 1; i < _vertexBufferCount; i += 1)
                {
                    _vertexBufferBindings[i] = VertexBufferBinding.None;
                }

                _vertexBuffersUpdated = true;
            }

            _vertexBufferCount = 1;
        }

        public void SetVertexBuffers(params VertexBufferBinding[]? vertexBuffers)
        {
            if (vertexBuffers == null)
            {
                if (_vertexBufferCount == 0)
                {
                    return;
                }

                for (var j = 0; j < _vertexBufferCount; j += 1)
                {
                    _vertexBufferBindings[j] = VertexBufferBinding.None;
                }

                _vertexBufferCount = 0;
                _vertexBuffersUpdated = true;
                return;
            }

            if (vertexBuffers.Length > _vertexBufferBindings.Length)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(vertexBuffers),
                    $"Max Vertex Buffers supported is {_vertexBufferBindings.Length}");
            }

            var i = 0;
            while (i < vertexBuffers.Length)
            {
                if (!ReferenceEquals(_vertexBufferBindings[i].VertexBuffer, vertexBuffers[i].VertexBuffer) ||
                    _vertexBufferBindings[i].VertexOffset != vertexBuffers[i].VertexOffset ||
                    _vertexBufferBindings[i].InstanceFrequency != vertexBuffers[i].InstanceFrequency)
                {
                    _vertexBufferBindings[i] = vertexBuffers[i];
                    _vertexBuffersUpdated = true;
                }

                i += 1;
            }

            if (vertexBuffers.Length < _vertexBufferCount)
            {
                while (i < _vertexBufferCount)
                {
                    _vertexBufferBindings[i] = VertexBufferBinding.None;
                    i += 1;
                }

                _vertexBuffersUpdated = true;
            }

            _vertexBufferCount = vertexBuffers.Length;
        }

        public VertexBufferBinding[] GetVertexBuffers()
        {
            VertexBufferBinding[] result = new VertexBufferBinding[_vertexBufferCount];
            Array.Copy(
                _vertexBufferBindings,
                result,
                _vertexBufferCount);
            return result;
        }

        public void DrawIndexedPrimitives(
            PrimitiveType primitiveType,
            int baseVertex,
            int minVertexIndex,
            int numVertices,
            int startIndex,
            int primitiveCount)
        {
            ApplyState();

            PrepareVertexBindingArray(baseVertex);

            FNA3D.FNA3D_DrawIndexedPrimitives(
                GLDevice,
                primitiveType,
                baseVertex,
                minVertexIndex,
                numVertices,
                startIndex,
                primitiveCount,
                Indices!._buffer,
                Indices.IndexElementSize);
        }

        public void DrawInstancedPrimitives(
            PrimitiveType primitiveType,
            int baseVertex,
            int minVertexIndex,
            int numVertices,
            int startIndex,
            int primitiveCount,
            int instanceCount)
        {
            // If this device doesn't have the support, just explode now before it's too late.
            if (FNA3D.FNA3D_SupportsHardwareInstancing(GLDevice) == 0)
            {
                throw new NoSuitableGraphicsDeviceException("Your hardware does not support hardware instancing!");
            }

            ApplyState();

            PrepareVertexBindingArray(baseVertex);

            FNA3D.FNA3D_DrawInstancedPrimitives(
                GLDevice,
                primitiveType,
                baseVertex,
                minVertexIndex,
                numVertices,
                startIndex,
                primitiveCount,
                instanceCount,
                Indices!._buffer,
                Indices!.IndexElementSize);
        }

        public void DrawPrimitives(
            PrimitiveType primitiveType,
            int vertexStart,
            int primitiveCount)
        {
            ApplyState();

            PrepareVertexBindingArray(0);

            FNA3D.FNA3D_DrawPrimitives(
                GLDevice,
                primitiveType,
                vertexStart,
                primitiveCount);
        }

        public void DrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int numVertices,
            short[] indexData,
            int indexOffset,
            int primitiveCount)
            where T : struct, IVertexType
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                numVertices,
                vertexOffset,
                VertexDeclarationCache<T>.VertexDeclaration);
            PrepareUserIndexBuffer(
                ibHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                indexOffset,
                2);

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();

            FNA3D.FNA3D_DrawIndexedPrimitives(
                GLDevice,
                primitiveType,
                0,
                0,
                numVertices,
                0,
                primitiveCount,
                _userIndexBuffer,
                IndexElementSize.SixteenBits);
        }

        public void DrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int numVertices,
            short[] indexData,
            int indexOffset,
            int primitiveCount,
            VertexDeclaration vertexDeclaration)
            where T : struct
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                numVertices,
                vertexOffset,
                vertexDeclaration);
            PrepareUserIndexBuffer(
                ibHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                indexOffset,
                2);

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();

            FNA3D.FNA3D_DrawIndexedPrimitives(
                GLDevice,
                primitiveType,
                0,
                0,
                numVertices,
                0,
                primitiveCount,
                _userIndexBuffer,
                IndexElementSize.SixteenBits);
        }

        public void DrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int numVertices,
            int[] indexData,
            int indexOffset,
            int primitiveCount)
            where T : struct, IVertexType
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                numVertices,
                vertexOffset,
                VertexDeclarationCache<T>.VertexDeclaration);
            PrepareUserIndexBuffer(
                ibHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                indexOffset,
                4);

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();

            FNA3D.FNA3D_DrawIndexedPrimitives(
                GLDevice,
                primitiveType,
                0,
                0,
                numVertices,
                0,
                primitiveCount,
                _userIndexBuffer,
                IndexElementSize.ThirtyTwoBits);
        }

        public void DrawUserIndexedPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int numVertices,
            int[] indexData,
            int indexOffset,
            int primitiveCount,
            VertexDeclaration vertexDeclaration)
            where T : struct
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            var ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                numVertices,
                vertexOffset,
                vertexDeclaration);
            PrepareUserIndexBuffer(
                ibHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                indexOffset,
                4);

            // Release the handles.
            ibHandle.Free();
            vbHandle.Free();

            FNA3D.FNA3D_DrawIndexedPrimitives(
                GLDevice,
                primitiveType,
                0,
                0,
                numVertices,
                0,
                primitiveCount,
                _userIndexBuffer,
                IndexElementSize.ThirtyTwoBits);
        }

        public void DrawUserPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int primitiveCount)
            where T : struct, IVertexType
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                vertexOffset,
                VertexDeclarationCache<T>.VertexDeclaration);

            // Release the handles.
            vbHandle.Free();

            FNA3D.FNA3D_DrawPrimitives(
                GLDevice,
                primitiveType,
                0,
                primitiveCount);
        }

        public void DrawUserPrimitives<T>(
            PrimitiveType primitiveType,
            T[] vertexData,
            int vertexOffset,
            int primitiveCount,
            VertexDeclaration vertexDeclaration)
            where T : struct
        {
            ApplyState();

            // Pin the buffers.
            var vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);

            PrepareUserVertexBuffer(
                vbHandle.AddrOfPinnedObject(),
                PrimitiveVertices(primitiveType, primitiveCount),
                vertexOffset,
                vertexDeclaration);

            // Release the handles.
            vbHandle.Free();

            FNA3D.FNA3D_DrawPrimitives(
                GLDevice,
                primitiveType,
                0,
                primitiveCount);
        }

        public void SetStringMarkerEXT(string text)
        {
            FNA3D.FNA3D_SetStringMarker(GLDevice, text);
        }

        /* Needed by VideoPlayer */
        internal static unsafe void PrepareRenderTargetBindings(
            FNA3D.FNA3D_RenderTargetBinding* b,
            RenderTargetBinding[] bindings)
        {
            for (var i = 0; i < bindings.Length; i += 1, b += 1)
            {
                var texture = bindings[i].RenderTarget;
                var rt = texture as IRenderTarget;
                if (texture is RenderTargetCube)
                {
                    b->Type = 1;
                    b->Data1 = rt!.Width;
                    b->Data2 = (int)bindings[i].CubeMapFace;
                }
                else
                {
                    b->Type = 0;
                    b->Data1 = rt!.Width;
                    b->Data2 = rt.Height;
                }

                b->LevelCount = rt.LevelCount;
                b->MultiSampleCount = rt.MultiSampleCount;
                b->Texture = texture!._texture;
                b->ColorBuffer = rt.ColorBuffer;
            }
        }

        internal void AddResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Add(resourceReference);
            }
        }

        internal void RemoveResourceReference(WeakReference resourceReference)
        {
            lock (_resourcesLock)
            {
                _resources.Remove(resourceReference);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // We're about to dispose, notify the application.
                    Disposing?.Invoke(this, EventArgs.Empty);

                    /* Dispose of all remaining graphics resources before
                     * disposing of the GraphicsDevice.
                     */
                    lock (_resourcesLock)
                    {
                        foreach (WeakReference resource in _resources.ToArray())
                        {
                            var target = resource.Target;
                            (target as IDisposable)?.Dispose();
                        }

                        _resources.Clear();
                    }

                    if (_userVertexBuffer != IntPtr.Zero)
                    {
                        FNA3D.FNA3D_AddDisposeVertexBuffer(
                            GLDevice,
                            _userVertexBuffer);
                    }

                    if (_userIndexBuffer != IntPtr.Zero)
                    {
                        FNA3D.FNA3D_AddDisposeIndexBuffer(
                            GLDevice,
                            _userIndexBuffer);
                    }

                    // Dispose of the GL Device/Context
                    FNA3D.FNA3D_DestroyDevice(GLDevice);
                }

                IsDisposed = true;
            }
        }

        private void ApplyState()
        {
            // Update Blend/DepthStencil, if applicable
            if (_currentBlend != BlendState)
            {
                FNA3D.FNA3D_SetBlendState(
                    GLDevice,
                    ref BlendState._state);
                _currentBlend = BlendState;
            }

            if (_currentDepthStencil != DepthStencilState)
            {
                FNA3D.FNA3D_SetDepthStencilState(
                    GLDevice,
                    ref DepthStencilState._state);
                _currentDepthStencil = DepthStencilState;
            }

            // Always update RasterizerState, as it depends on other device states
            FNA3D.FNA3D_ApplyRasterizerState(
                GLDevice,
                ref RasterizerState._state);

            for (var sampler = 0; sampler < _modifiedSamplers.Length; sampler += 1)
            {
                if (!_modifiedSamplers[sampler])
                {
                    continue;
                }

                _modifiedSamplers[sampler] = false;

                FNA3D.FNA3D_VerifySampler(
                    GLDevice,
                    sampler,
                    Textures[sampler] != null ? Textures[sampler]!._texture : IntPtr.Zero,
                    ref SamplerStates[sampler]!._state);
            }

            for (var sampler = 0; sampler < _modifiedVertexSamplers.Length; sampler += 1)
            {
                if (!_modifiedVertexSamplers[sampler])
                {
                    continue;
                }

                _modifiedVertexSamplers[sampler] = false;

                /* Believe it or not, this is actually how VertexTextures are
                 * stored in XNA4! Their D3D9 renderer just uses the last 4
                 * slots available in the device's sampler array. So that's what
                 * we get to do.
                 * -flibit
                 */
                FNA3D.FNA3D_VerifyVertexSampler(
                    GLDevice,
                    sampler,
                    VertexTextures[sampler] != null ? VertexTextures[sampler]!._texture : IntPtr.Zero,
                    ref VertexSamplerStates[sampler]!._state);
            }
        }

        private unsafe void PrepareVertexBindingArray(int baseVertex)
        {
            fixed (FNA3D.FNA3D_VertexBufferBinding* b = &_nativeBufferBindings[0])
            {
                for (var i = 0; i < _vertexBufferCount; i += 1)
                {
                    VertexBuffer buffer = _vertexBufferBindings[i].VertexBuffer;
                    b[i].VertexBuffer = buffer._buffer;
                    b[i].VertexDeclaration.VertexStride = buffer.VertexDeclaration.VertexStride;
                    b[i].VertexDeclaration.ElementCount = buffer.VertexDeclaration._elements.Length;
                    b[i].VertexDeclaration.Elements = buffer.VertexDeclaration._elementsPin;
                    b[i].VertexOffset = _vertexBufferBindings[i].VertexOffset;
                    b[i].InstanceFrequency = _vertexBufferBindings[i].InstanceFrequency;
                }

                FNA3D.FNA3D_ApplyVertexBufferBindings(
                    GLDevice,
                    b,
                    _vertexBufferCount,
                    (byte)(_vertexBuffersUpdated ? 1 : 0),
                    baseVertex);
            }

            _vertexBuffersUpdated = false;
        }

        private unsafe void PrepareUserVertexBuffer(
            IntPtr vertexData,
            int numVertices,
            int vertexOffset,
            VertexDeclaration vertexDeclaration)
        {
            var len = numVertices * vertexDeclaration.VertexStride;
            var offset = vertexOffset * vertexDeclaration.VertexStride;
            vertexDeclaration.GraphicsDevice = this;

            if (len > _userVertexBufferSize)
            {
                if (_userVertexBuffer != IntPtr.Zero)
                {
                    FNA3D.FNA3D_AddDisposeVertexBuffer(
                        GLDevice,
                        _userVertexBuffer);
                }

                _userVertexBuffer = FNA3D.FNA3D_GenVertexBuffer(
                    GLDevice,
                    1,
                    BufferUsage.WriteOnly,
                    len);
                _userVertexBufferSize = len;
            }

            FNA3D.FNA3D_SetVertexBufferData(
                GLDevice,
                _userVertexBuffer,
                0,
                vertexData + offset,
                len,
                1,
                1,
                SetDataOptions.Discard);

            fixed (FNA3D.FNA3D_VertexBufferBinding* b = &_nativeBufferBindings[0])
            {
                b->VertexBuffer = _userVertexBuffer;
                b->VertexDeclaration.VertexStride = vertexDeclaration.VertexStride;
                b->VertexDeclaration.ElementCount = vertexDeclaration._elements.Length;
                b->VertexDeclaration.Elements = vertexDeclaration._elementsPin;
                b->VertexOffset = 0;
                b->InstanceFrequency = 0;
                FNA3D.FNA3D_ApplyVertexBufferBindings(GLDevice, b, 1, 1, 0);
            }

            _vertexBuffersUpdated = true;
        }

        private void PrepareUserIndexBuffer(
            IntPtr indexData,
            int numIndices,
            int indexOffset,
            int indexElementSizeInBytes)
        {
            var len = numIndices * indexElementSizeInBytes;
            if (len > _userIndexBufferSize)
            {
                if (_userIndexBuffer != IntPtr.Zero)
                {
                    FNA3D.FNA3D_AddDisposeIndexBuffer(
                        GLDevice,
                        _userIndexBuffer);
                }

                _userIndexBuffer = FNA3D.FNA3D_GenIndexBuffer(
                    GLDevice,
                    1,
                    BufferUsage.WriteOnly,
                    len);
                _userIndexBufferSize = len;
            }

            FNA3D.FNA3D_SetIndexBufferData(
                GLDevice,
                _userIndexBuffer,
                0,
                indexData + (indexOffset * indexElementSizeInBytes),
                len,
                SetDataOptions.Discard);
        }

        private static int PrimitiveVertices(PrimitiveType primitiveType, int primitiveCount)
        {
            return primitiveType switch
            {
                PrimitiveType.TriangleList => primitiveCount * 3,
                PrimitiveType.TriangleStrip => primitiveCount + 2,
                PrimitiveType.LineList => primitiveCount * 2,
                PrimitiveType.LineStrip => primitiveCount + 1,
                PrimitiveType.PointListEXT => primitiveCount,
                _ => throw new InvalidOperationException("Unrecognized primitive type!")
            };
        }
    }
}
