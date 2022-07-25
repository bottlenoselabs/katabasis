// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using static bottlenoselabs.imgui;

namespace bottlenoselabs.Katabasis.ImGui
{
    [PublicAPI]
    public unsafe class ImGuiRenderer
    {
        private ImGuiEffect? _effect;
        private readonly RasterizerState _rasterizerState;

        private byte[] _vertexData = null!;
        private VertexBuffer? _vertexBuffer;
        private int _vertexBufferSize;

        private byte[] _indexData = null!;
        private IndexBuffer? _indexBuffer;
        private int _indexBufferSize;

        private readonly Dictionary<IntPtr, Texture2D> _textures;

        private int _textureId;
        private IntPtr? _fontTextureId;
        private int _scrollWheelValue;

        private readonly List<int> _keys = new();

        public ImGuiRenderer()
        {
            var context = igCreateContext(default);
            igSetCurrentContext(context);

            _textures = new Dictionary<IntPtr, Texture2D>();

            _rasterizerState = new RasterizerState
            {
                CullMode = CullMode.CullCounterClockwiseFace,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            SetupInput();
            ImFontAtlas_AddFontDefault(igGetIO()->Fonts, default);
        }

        /// <summary>
        ///     Creates a texture and loads the font data from ImGui.
        /// </summary>
        public void BuildFontAtlas()
        {
            var io = igGetIO();
            ulong* pixelData;
            int width;
            int height;
            int bytesPerPixel;
            ImFontAtlas_GetTexDataAsRGBA32(io->Fonts, &pixelData, &width, &height, &bytesPerPixel);

            var pixels = new byte[width * height * bytesPerPixel];
            Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);
            var texture = new Texture2D((int)width, (int)height, false, SurfaceFormat.Color);
            texture.SetData(pixels);

            // Should a texture already have been build previously, unbind it first so it can be deallocated
            if (_fontTextureId != null)
            {
                UnbindTexture(_fontTextureId.Value);
            }

            // Bind the new texture to an ImGui-friendly id
            _fontTextureId = BindTexture(texture);

            // Let ImGui know where to find the texture
            ImFontAtlas_SetTexID(io->Fonts, new ImTextureID { Data = (void*)_fontTextureId });
            ImFontAtlas_ClearTexData(io->Fonts); // Clears CPU side texture data
        }

        // TODO: Add XML docs.
        public IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(_textureId++);

            _textures.Add(id, texture);

            return id;
        }

        // TODO: Add XML docs.
        public void UnbindTexture(IntPtr textureId)
        {
            _textures.Remove(textureId);
        }

        /// <summary>
        ///     Sets up ImGui for the current frame.
        /// </summary>
        /// <param name="gameTime">The elapsed time to pass ImGui.</param>
        public void Begin(GameTime gameTime)
        {
            var deltaSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (deltaSeconds > 0)
            {
                igGetIO()->DeltaTime = deltaSeconds;
            }

            UpdateInput();
            igNewFrame();
        }

        /// <summary>
        ///     Flushes the ImGui function calls and sends the geometry data the graphics pipeline for rendering.
        /// </summary>
        public void End()
        {
            _effect ??= new ImGuiEffect();
            igRender();
            RenderDrawData(igGetDrawData());
        }

        private void SetupInput()
        {
            var io = igGetIO();
            var keyMap = io->KeyMap;

            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Tab] = (int)Keys.Tab);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_LeftArrow] = (int)Keys.Left);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_RightArrow] = (int)Keys.Right);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_UpArrow] = (int)Keys.Up);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_DownArrow] = (int)Keys.Down);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_PageUp] = (int)Keys.PageUp);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_PageDown] = (int)Keys.PageDown);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Home] = (int)Keys.Home);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_End] = (int)Keys.End);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Delete] = (int)Keys.Delete);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Backspace] = (int)Keys.Back);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Enter] = (int)Keys.Enter);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Escape] = (int)Keys.Escape);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Space] = (int)Keys.Space);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_A] = (int)Keys.A);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_C] = (int)Keys.C);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_V] = (int)Keys.V);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_X] = (int)Keys.X);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Y] = (int)Keys.Y);
            _keys.Add(keyMap[(int)ImGuiKey_.ImGuiKey_Z] = (int)Keys.Z);

            TextInputEXT.TextInput += OnTextInput;
        }

        private static void OnTextInput(char c)
        {
            if (c == '\t')
			{
				return;
			}

            ImGuiIO_AddInputCharacter(igGetIO(), c);
        }

        private void BindShaderParams()
        {
            var io = igGetIO();

            var displayWidth = io->DisplaySize.X;
            var displayHeight = io->DisplaySize.Y;
            var worldViewProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0f, displayWidth, displayHeight, 0f, -1f, 1f);
            _effect!.UpdateWorldViewProjectionMatrix(ref worldViewProjectionMatrix);
        }

        private void UpdateInput()
        {
            var io = igGetIO();

            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            foreach (var key in _keys)
            {
                io->KeysDown[key] = keyboard.IsKeyDown((Keys)key);
            }

            io->KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            io->KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            io->KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            io->KeySuper = keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows);

            var graphicsDevice = GraphicsDevice.Instance;
            io->DisplaySize = new Vector2(graphicsDevice.PresentationParameters.BackBufferWidth, graphicsDevice.PresentationParameters.BackBufferHeight);
            io->DisplayFramebufferScale = new Vector2(1f, 1f);

            io->MousePos = new Vector2(mouse.X, mouse.Y);

            io->MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
            io->MouseDown[1] = mouse.RightButton == ButtonState.Pressed;
            io->MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed;

            var scrollDelta = mouse.ScrollWheelValue - _scrollWheelValue;
            io->MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            _scrollWheelValue = mouse.ScrollWheelValue;
        }

        private void RenderDrawData(ImDrawData* drawData)
        {
            var graphicsDevice = GraphicsDevice.Instance;
            // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
            var lastViewport = graphicsDevice.Viewport;
            var lastScissorBox = graphicsDevice.ScissorRectangle;

            graphicsDevice.BlendFactor = Color.White;
            graphicsDevice.BlendState = BlendState.NonPremultiplied;
            graphicsDevice.RasterizerState = _rasterizerState;
            graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            // Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
            ImDrawData_ScaleClipRects(drawData, igGetIO()->DisplayFramebufferScale);

            graphicsDevice.Viewport = new Viewport(
                0,
                0,
                graphicsDevice.PresentationParameters.BackBufferWidth,
                graphicsDevice.PresentationParameters.BackBufferHeight);

            UpdateBuffers(drawData);
            RenderCommandLists(drawData);

            // Restore modified state
            graphicsDevice.Viewport = lastViewport;
            graphicsDevice.ScissorRectangle = lastScissorBox;
        }

        private void UpdateBuffers(ImDrawData* drawData)
        {
            if (drawData->TotalVtxCount == 0)
            {
                return;
            }

            // Expand buffers if we need more room
            if (drawData->TotalVtxCount > _vertexBufferSize)
            {
                _vertexBuffer?.Dispose();
                _vertexBufferSize = (int)(drawData->TotalVtxCount * 1.5f);
                _vertexBuffer = new VertexBuffer(ImGuiVertexDeclaration.Declaration, _vertexBufferSize, BufferUsage.None);
                _vertexData = new byte[_vertexBufferSize * ImGuiVertexDeclaration.Size];
            }

            if (drawData->TotalIdxCount > _indexBufferSize)
            {
                _indexBuffer?.Dispose();
                _indexBufferSize = (int)(drawData->TotalIdxCount * 1.5f);
                _indexBuffer = new IndexBuffer(IndexElementSize.SixteenBits, _indexBufferSize, BufferUsage.None);
                _indexData = new byte[_indexBufferSize * sizeof(ushort)];
            }

            // Copy ImGui's vertices and indices to a set of managed byte arrays
            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData->CmdListsCount; n++)
            {
                var cmdList = drawData->CmdLists[n];

                fixed (void* vtxDstPtr = &_vertexData[vtxOffset * ImGuiVertexDeclaration.Size])
				{
					fixed (void* idxDstPtr = &_indexData[idxOffset * sizeof(ushort)])
                    {
                        Buffer.MemoryCopy(
                            cmdList->VtxBuffer.Data,
                            vtxDstPtr,
                            _vertexData.Length,
                            cmdList->VtxBuffer.Size * ImGuiVertexDeclaration.Size);
                        Buffer.MemoryCopy(
                            cmdList->IdxBuffer.Data,
                            idxDstPtr,
                            _indexData.Length,
                            cmdList->IdxBuffer.Size * sizeof(ushort));
                    }
				}

                vtxOffset += cmdList->VtxBuffer.Size;
                idxOffset += cmdList->IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            _vertexBuffer!.SetData(_vertexData, 0, drawData->TotalVtxCount * ImGuiVertexDeclaration.Size);
            _indexBuffer!.SetData(_indexData, 0, drawData->TotalIdxCount * sizeof(ushort));
        }

        private void RenderCommandLists(ImDrawData* drawData)
        {
            var graphicsDevice = GraphicsDevice.Instance;

            graphicsDevice.SetVertexBuffer(_vertexBuffer!);
            graphicsDevice.Indices = _indexBuffer;
            var lastTexture = graphicsDevice.Textures[0];

            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData->CmdListsCount; n++)
            {
                var cmdList = drawData->CmdLists[n];

                for (var cmdi = 0; cmdi < cmdList->CmdBuffer.Size; cmdi++)
                {
                    var drawCmd = cmdList->CmdBuffer.Data[cmdi];

                    if (!_textures.ContainsKey((IntPtr)drawCmd.TextureId.Data))
                    {
                        throw new InvalidOperationException(
                            $"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    var texture = _textures[(IntPtr)drawCmd.TextureId.Data];
                    GraphicsDevice.Instance.Textures[0] = texture;
                    graphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y));

                    BindShaderParams();

                    foreach (var pass in _effect!.CurrentTechnique!.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawIndexedPrimitives(
                            primitiveType: PrimitiveType.TriangleList,
                            baseVertex: vtxOffset,
                            minVertexIndex: 0,
                            numVertices: cmdList->VtxBuffer.Size,
                            startIndex: idxOffset,
                            primitiveCount: (int)drawCmd.ElemCount / 3);
                    }

                    idxOffset += (int)drawCmd.ElemCount;
                }

                vtxOffset += cmdList->VtxBuffer.Size;
            }

            graphicsDevice.Textures[0] = lastTexture;
        }
    }
}
