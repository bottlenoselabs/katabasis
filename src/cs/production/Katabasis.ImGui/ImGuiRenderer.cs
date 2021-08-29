using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using static imgui;

namespace Katabasis.ImGui
{
    public unsafe class ImGuiRenderer
    {
        private ImGuiEffect _effect;
        private readonly RasterizerState _rasterizerState;

        private byte[] _vertexData;
        private VertexBuffer _vertexBuffer;
        private int _vertexBufferSize;

        private byte[] _indexData;
        private IndexBuffer _indexBuffer;
        private int _indexBufferSize;

        // Textures
        private readonly Dictionary<IntPtr, Texture2D> _loadedTextures;

        private int _textureId;
        private IntPtr? _fontTextureId;

        // Input
        private int _scrollWheelValue;

        private readonly List<int> _keys = new();

        public ImGuiRenderer()
        {
            var context = igCreateContext(default);
            igSetCurrentContext(context);

            _loadedTextures = new Dictionary<IntPtr, Texture2D>();

            _rasterizerState = new RasterizerState
            {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            SetupInput();
        }

        /// <summary>
        ///     Creates a texture and loads the font data from ImGui.
        /// </summary>
        public virtual void RebuildFontAtlas()
        {
            // Get font texture from ImGui
            var io = igGetIO();
            ulong* pixelData;
            long width;
            long height;
            long bytesPerPixel;
            ImFontAtlas_GetTexDataAsRGBA32(io->Fonts, &pixelData, &width, &height, &bytesPerPixel);

            // Copy the data to a managed array
            var pixels = new byte[width * height * bytesPerPixel];
            Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length);

            // Create and register the texture as an XNA texture
            var tex2d = new Texture2D((int)width, (int)height, false, SurfaceFormat.Color);
            tex2d.SetData(pixels);

            // Should a texture already have been build previously, unbind it first so it can be deallocated
            if (_fontTextureId.HasValue)
            {
                UnbindTexture(_fontTextureId.Value);
            }

            // Bind the new texture to an ImGui-friendly id
            _fontTextureId = BindTexture(tex2d);

            // Let ImGui know where to find the texture
            ImFontAtlas_SetTexID(io->Fonts, new ImTextureID { Data = (void*)_fontTextureId });
            ImFontAtlas_ClearTexData(io->Fonts); // Clears CPU side texture data
        }

        /// <summary>
        ///     Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image" />. That
        ///     pointer is then used by ImGui to let us know what texture to draw
        /// </summary>
        public virtual IntPtr BindTexture(Texture2D texture)
        {
            var id = new IntPtr(_textureId++);

            _loadedTextures.Add(id, texture);

            return id;
        }

        /// <summary>
        ///     Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
        /// </summary>
        public virtual void UnbindTexture(IntPtr textureId)
        {
            _loadedTextures.Remove(textureId);
        }

        /// <summary>
        ///     Sets up ImGui for a new frame, should be called at frame start
        /// </summary>
        public virtual void BeforeLayout(GameTime gameTime)
        {
            igGetIO()->DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput();

            igNewFrame();
        }

        /// <summary>
        ///     Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is
        ///     drawn using ImGui.** calls
        /// </summary>
        public virtual void AfterLayout()
        {
            igRender();
            RenderDrawData(igGetDrawData());
        }

        /// <summary>
        ///     Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
        /// </summary>
        protected virtual void SetupInput()
        {
            var io = igGetIO();
            var keyMap = io->KeyMap;

            _keys.Add(keyMap[ImGuiKey_Tab] = (int)Keys.Tab);
            _keys.Add(keyMap[ImGuiKey_LeftArrow] = (int)Keys.Left);
            _keys.Add(keyMap[ImGuiKey_RightArrow] = (int)Keys.Right);
            _keys.Add(keyMap[ImGuiKey_UpArrow] = (int)Keys.Up);
            _keys.Add(keyMap[ImGuiKey_DownArrow] = (int)Keys.Down);
            _keys.Add(keyMap[ImGuiKey_PageUp] = (int)Keys.PageUp);
            _keys.Add(keyMap[ImGuiKey_PageDown] = (int)Keys.PageDown);
            _keys.Add(keyMap[ImGuiKey_Home] = (int)Keys.Home);
            _keys.Add(keyMap[ImGuiKey_End] = (int)Keys.End);
            _keys.Add(keyMap[ImGuiKey_Delete] = (int)Keys.Delete);
            _keys.Add(keyMap[ImGuiKey_Backspace] = (int)Keys.Back);
            _keys.Add(keyMap[ImGuiKey_Enter] = (int)Keys.Enter);
            _keys.Add(keyMap[ImGuiKey_Escape] = (int)Keys.Escape);
            _keys.Add(keyMap[ImGuiKey_Space] = (int)Keys.Space);
            _keys.Add(keyMap[ImGuiKey_A] = (int)Keys.A);
            _keys.Add(keyMap[ImGuiKey_C] = (int)Keys.C);
            _keys.Add(keyMap[ImGuiKey_V] = (int)Keys.V);
            _keys.Add(keyMap[ImGuiKey_X] = (int)Keys.X);
            _keys.Add(keyMap[ImGuiKey_Y] = (int)Keys.Y);
            _keys.Add(keyMap[ImGuiKey_Z] = (int)Keys.Z);

            TextInputEXT.TextInput += OnTextInput;

            ImFontAtlas_AddFontDefault(io->Fonts, default);
        }
        
        private static void OnTextInput(char c)
        {
            if (c == '\t') return;
            ImGuiIO_AddInputCharacter(igGetIO(), c);
        }

        /// <summary>
        ///     Updates the <see cref="Effect" /> to the current matrices and texture
        /// </summary>
        protected virtual Effect UpdateEffect(Texture2D texture)
        {
            _effect = _effect ?? new BasicEffect(_graphicsDevice);

            var io = igGetIO();

            _effect.World = Matrix.Identity;
            _effect.View = Matrix.Identity;
            _effect.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
            _effect.TextureEnabled = true;
            _effect.Texture = texture;
            _effect.VertexColorEnabled = true;

            return _effect;
        }

        /// <summary>
        ///     Sends XNA input state to ImGui
        /// </summary>
        protected virtual void UpdateInput()
        {
            var io = igGetIO();

            var mouse = Mouse.GetState();
            var keyboard = Keyboard.GetState();

            for (var i = 0; i < _keys.Count; i++)
            {
                io->KeysDown[_keys[i]] = keyboard.IsKeyDown((Keys)_keys[i]);
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

        /// <summary>
        ///     Gets the geometry as set up by ImGui and sends it to the graphics device
        /// </summary>
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

            // Setup projection
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

                vtxOffset += cmdList->VtxBuffer.Size;
                idxOffset += cmdList->IdxBuffer.Size;
            }

            // Copy the managed byte arrays to the gpu vertex- and index buffers
            _vertexBuffer.SetData(_vertexData, 0, drawData->TotalVtxCount * ImGuiVertexDeclaration.Size);
            _indexBuffer.SetData(_indexData, 0, drawData->TotalIdxCount * sizeof(ushort));
        }

        private void RenderCommandLists(ImDrawData* drawData)
        {
            var graphicsDevice = GraphicsDevice.Instance;
            
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;

            var vtxOffset = 0;
            var idxOffset = 0;

            for (var n = 0; n < drawData->CmdListsCount; n++)
            {
                var cmdList = drawData->CmdLists[n];

                for (var cmdi = 0; cmdi < cmdList->CmdBuffer.Size; cmdi++)
                {
                    var drawCmd = cmdList->CmdBuffer.Data[cmdi];

                    if (!_loadedTextures.ContainsKey((IntPtr)drawCmd.TextureId.Data))
                    {
                        throw new InvalidOperationException(
                            $"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    graphicsDevice.ScissorRectangle = new Rectangle(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    var effect = UpdateEffect(_loadedTextures[(IntPtr)drawCmd.TextureId.Data]);

                    foreach (var pass in effect.CurrentTechnique!.Passes)
                    {
                        pass.Apply();
                        graphicsDevice.DrawIndexedPrimitives(
                            primitiveType: PrimitiveType.TriangleList,
                            baseVertex: vtxOffset,
                            minVertexIndex: 0,
                            numVertices: cmdList->VtxBuffer.Size,
                            startIndex: idxOffset,
                            primitiveCount: (int)drawCmd.ElemCount / 3
                        );
                    }

                    idxOffset += (int)drawCmd.ElemCount;
                }

                vtxOffset += cmdList->VtxBuffer.Size;
            }
        }
    }
}