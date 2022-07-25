// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static bottlenoselabs.FNA3D;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API.")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
	public sealed unsafe class GraphicsDevice : IDisposable
	{
		/// <summary>
		/// 	Gets the current <see cref="GraphicsDevice" />.
		/// </summary>
		public static GraphicsDevice Instance => GraphicsDeviceManager.Instance.GraphicsDevice;

		// Per XNA4 General Spec
		internal const int MaxTextureSamples = 16;

		// Per XNA4 HiDef Spec
		internal const int MaxVertexAttributes = 16;
		internal const int MaxRenderTargetBindings = 4;
		internal const int MaxVertexTextureSamplers = 4;

		// Some of these are internal for validation purposes
		internal readonly RenderTargetBinding[] RenderTargetBindings = new RenderTargetBinding[MaxRenderTargetBindings];
		internal int RenderTargetCount;

		internal readonly FNA3D_Device* Device;
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

		private readonly FNA3D_RenderTargetBinding[] _nativeTargetBindings =
			new FNA3D_RenderTargetBinding[MaxRenderTargetBindings];

		private readonly FNA3D_RenderTargetBinding[] _nativeTargetBindingsNext =
			new FNA3D_RenderTargetBinding[MaxRenderTargetBindings];

		// Used to prevent alloc on SetRenderTarget()
		private readonly RenderTargetBinding[] _singleTargetCache = new RenderTargetBinding[1];

		private readonly VertexBufferBinding[] _vertexBufferBindings =
			new VertexBufferBinding[MaxVertexAttributes];

		private readonly FNA3D_VertexBufferBinding[] _nativeBufferBindings =
			new FNA3D_VertexBufferBinding[MaxVertexAttributes];

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
		private readonly List<WeakReference> _resources = new();
		private readonly object _resourcesLock = new();

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
					int w, h;
					FNA3D_GetBackbufferSize(Device, &w, &h);
					return new DisplayMode(
						w,
						h,
						(SurfaceFormat)FNA3D_GetBackbufferSurfaceFormat(Device));
				}

				return Adapter.CurrentDisplayMode;
			}
		}

		public TextureCollection Textures { get; }

		public SamplerStateCollection SamplerStates { get; }

		public TextureCollection VertexTextures { get; }

		public SamplerStateCollection VertexSamplerStates { get; }

		public BlendState? BlendState { get; set; }

		public DepthStencilState? DepthStencilState { get; set; }

		public RasterizerState? RasterizerState { get; set; }

		public Rectangle ScissorRectangle
		{
			get => _scissorRectangle;
			set
			{
				_scissorRectangle = value;
				FNA3D_SetScissorRect(
					Device,
					(FNA3D_Rect*)Unsafe.AsPointer(ref value));
			}
		}

		public Viewport Viewport
		{
			get => _viewport;
			set
			{
				_viewport = value;
				FNA3D_SetViewport(Device, (FNA3D_Viewport*)Unsafe.AsPointer(ref value._viewport));
			}
		}

		public Color BlendFactor
		{
			get
			{
				FNA3D_Color result;
				FNA3D_GetBlendFactor(Device, &result);
				return Unsafe.ReadUnaligned<Color>(&result);
			}

			set =>
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				FNA3D_SetBlendFactor(Device, (FNA3D_Color*)Unsafe.AsPointer(ref value));
		}

		public int MultiSampleMask
		{
			get => FNA3D_GetMultiSampleMask(Device);
			set =>
				/* FIXME: Does this affect the value found in
				 * BlendState?
				 * -flibit
				 */
				FNA3D_SetMultiSampleMask(Device, value);
		}

		public int ReferenceStencil
		{
			get => FNA3D_GetReferenceStencil(Device);
			set =>
				/* FIXME: Does this affect the value found in
				 * DepthStencilState?
				 * -flibit
				 */
				FNA3D_SetReferenceStencil(Device, value);
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
		internal void OnResourceCreated(object resource) => ResourceCreated?.Invoke(this, new ResourceCreatedEventArgs(resource));

		// TODO: Hook this up to GraphicsResource
		internal void OnResourceDestroyed(string name, object tag) => ResourceDestroyed?.Invoke(this, new ResourceDestroyedEventArgs(name, tag));

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
				// ReSharper disable once ConvertToConstant.Local
				byte debugMode;
#if DEBUG
				debugMode = 1;
#else
				debugMode = 0;
#endif
				Device = FNA3D_CreateDevice((FNA3D_PresentationParameters*)Unsafe.AsPointer(ref PresentationParameters.Parameters), debugMode);
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
			int maxTextures;
			int maxVertexTextures;
			FNA3D_GetMaxTextureSlots(
				Device,
				&maxTextures,
				&maxVertexTextures);

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

		~GraphicsDevice() => Dispose(false);

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Present() =>
			FNA3D_SwapBuffers(
				Device,
				(FNA3D_Rect*)IntPtr.Zero,
				(FNA3D_Rect*)IntPtr.Zero,
				(void*)PresentationParameters.DeviceWindowHandle);

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
				FNA3D_SwapBuffers(
					Device,
					(FNA3D_Rect*)Unsafe.AsPointer(ref src),
					(FNA3D_Rect*)Unsafe.AsPointer(ref dst),
					(void*)overrideWindowHandle);
			}
			else if (sourceRectangle.HasValue)
			{
				var src = sourceRectangle.Value;
				FNA3D_SwapBuffers(
					Device,
					(FNA3D_Rect*)Unsafe.AsPointer(ref src),
					(FNA3D_Rect*)IntPtr.Zero,
					(void*)overrideWindowHandle);
			}
			else if (destinationRectangle.HasValue)
			{
				var dst = destinationRectangle.Value;
				FNA3D_SwapBuffers(
					Device,
					(FNA3D_Rect*)IntPtr.Zero,
					(FNA3D_Rect*)Unsafe.AsPointer(ref dst),
					(void*)overrideWindowHandle);
			}
			else
			{
				FNA3D_SwapBuffers(
					Device,
					(FNA3D_Rect*)IntPtr.Zero,
					(FNA3D_Rect*)IntPtr.Zero,
					(void*)overrideWindowHandle);
			}
		}

		public void Reset() => Reset(PresentationParameters, Adapter);

		public void Reset(PresentationParameters presentationParameters) => Reset(presentationParameters, Adapter);

		public void Reset(
			PresentationParameters presentationParameters,
			GraphicsAdapter graphicsAdapter)
		{
			PresentationParameters = presentationParameters;
			Adapter = graphicsAdapter;

			// Verify MSAA before we really start...
			PresentationParameters.MultiSampleCount = FNA3D_GetMaxMultiSampleCount(
				Device,
				(FNA3D_SurfaceFormat)PresentationParameters.BackBufferFormat,
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
			FNA3D_ResetBackbuffer(
				Device,
				(FNA3D_PresentationParameters*)Unsafe.AsPointer(ref PresentationParameters.Parameters));

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

		public void Clear(Color color) =>
			Clear(
				ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil,
				color.ToVector4(),
				Viewport.MaxDepth,
				0);

		public void Clear(ClearOptions options, Color color, float depth, int stencil) =>
			Clear(
				options,
				color.ToVector4(),
				depth,
				stencil);

		public void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
		{
			/* FIXME: PresentationParameters.DepthStencilFormat is probably
			* a more accurate value here, but the Backbuffer may disagree.
			* -flibit
			*/
			var renderTarget = RenderTargetBindings[0].RenderTarget as IRenderTarget;
			var dsFormat = RenderTargetCount == 0 ? (DepthFormat)FNA3D_GetBackbufferDepthFormat(Device) : renderTarget!.DepthStencilFormat;

			if (dsFormat == DepthFormat.None)
			{
				options &= ClearOptions.Target;
			}
			else if (dsFormat != DepthFormat.Depth24Stencil8)
			{
				options &= ~ClearOptions.Stencil;
			}

			FNA3D_Clear(
				Device,
				(FNA3D_ClearOptions)options,
				(FNA3D_Vec4*)Unsafe.AsPointer(ref color),
				depth,
				stencil);
		}

		public void GetBackBufferData<T>(T[] data)
			where T : struct =>
			GetBackBufferData(null, data, 0, data.Length);

		public void GetBackBufferData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			GetBackBufferData(null, data, startIndex, elementCount);

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
				FNA3D_GetBackbufferSize(
					Device,
					&w,
					&h);
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
				(SurfaceFormat)FNA3D_GetBackbufferSurfaceFormat(Device),
				elementSizeInBytes);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D_ReadBackbuffer(
				Device,
				x,
				y,
				w,
				h,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
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
			// D3D11 requires our sampler state to be valid (i.e. not point to any of our new RTs)
			//  before we call SetRenderTargets. At this point FNA3D does not have a current copy
			//  of the managed sampler state, so we need to apply our current state now instead of
			//  before our next Clear or Draw operation.
			ApplySamplers();

			// Checking for redundant SetRenderTargets...
			if (renderTargets == null && RenderTargetCount == 0)
			{
				return;
			}

			if (renderTargets != null && renderTargets.Length == RenderTargetCount)
			{
				var isRedundant = true;
				for (var i = 0; i < renderTargets.Length; i += 1)
				{
					if (renderTargets[i].RenderTarget != RenderTargetBindings[i].RenderTarget ||
					    renderTargets[i].CubeMapFace != RenderTargetBindings[i].CubeMapFace)
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
				FNA3D_SetRenderTargets(
					Device,
					(FNA3D_RenderTargetBinding*)IntPtr.Zero,
					0,
					(FNA3D_Renderbuffer*)IntPtr.Zero,
					(FNA3D_DepthFormat)DepthFormat.None,
					PresentationParameters.RenderTargetUsage != RenderTargetUsage.DiscardContents ? (byte)1 : (byte)0); /* lol c# */

				// Set the viewport/scissor to the size of the backbuffer.
				newWidth = PresentationParameters.BackBufferWidth;
				newHeight = PresentationParameters.BackBufferHeight;
				clearTarget = PresentationParameters.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (var i = 0; i < RenderTargetCount; i += 1)
				{
					FNA3D_ResolveTarget(Device, (FNA3D_RenderTargetBinding*)Unsafe.AsPointer(ref _nativeTargetBindings[i]));
				}

				Array.Clear(RenderTargetBindings, 0, RenderTargetBindings.Length);
				Array.Clear(_nativeTargetBindings, 0, _nativeTargetBindings.Length);
				RenderTargetCount = 0;
			}
			else
			{
				var target = renderTargets[0].RenderTarget as IRenderTarget;
				fixed (FNA3D_RenderTargetBinding* rt = &_nativeTargetBindingsNext[0])
				{
					PrepareRenderTargetBindings(rt, renderTargets);
					FNA3D_SetRenderTargets(
						Device,
						rt,
						renderTargets.Length,
						(FNA3D_Renderbuffer*)target!.DepthStencilBuffer,
						(FNA3D_DepthFormat)target.DepthStencilFormat,
						target.RenderTargetUsage != RenderTargetUsage.DiscardContents ? (byte)1 : (byte)0); /* lol c# */
				}

				// Set the viewport/scissor to the size of the first render target.
				newWidth = target.Width;
				newHeight = target.Height;
				clearTarget = target.RenderTargetUsage;

				// Resolve previous targets, if needed
				for (var i = 0; i < RenderTargetCount; i += 1)
				{
					// We only need to resolve if the target is no longer bound.
					var stillBound = false;
					for (var j = 0; j < renderTargets.Length; j += 1)
					{
						if (RenderTargetBindings[i].RenderTarget == renderTargets[j].RenderTarget)
						{
							stillBound = true;
							break;
						}
					}

					if (stillBound)
					{
						continue;
					}

					FNA3D_ResolveTarget(Device, (FNA3D_RenderTargetBinding*)Unsafe.AsPointer(ref _nativeTargetBindings[i]));
				}

				Array.Clear(RenderTargetBindings, 0, RenderTargetBindings.Length);
				Array.Copy(renderTargets, RenderTargetBindings, renderTargets.Length);
				Array.Clear(_nativeTargetBindings, 0, _nativeTargetBindings.Length);
				Array.Copy(_nativeTargetBindingsNext, _nativeTargetBindings, renderTargets.Length);
				RenderTargetCount = renderTargets.Length;
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
			RenderTargetBinding[] bindings = new RenderTargetBinding[RenderTargetCount];
			Array.Copy(RenderTargetBindings, bindings, RenderTargetCount);
			return bindings;
		}

		public void SetVertexBuffer(VertexBuffer vertexBuffer) => SetVertexBuffer(vertexBuffer, 0);

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

			FNA3D_DrawIndexedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				(FNA3D_Buffer*)Indices!.Buffer,
				(FNA3D_IndexElementSize)Indices.IndexElementSize);
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
			if (FNA3D_SupportsHardwareInstancing(Device) == 0)
			{
				throw new NoSuitableGraphicsDeviceException("Your hardware does not support hardware instancing!");
			}

			ApplyState();

			PrepareVertexBindingArray(baseVertex);

			FNA3D_DrawInstancedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				baseVertex,
				minVertexIndex,
				numVertices,
				startIndex,
				primitiveCount,
				instanceCount,
				(FNA3D_Buffer*)Indices!.Buffer,
				(FNA3D_IndexElementSize)Indices!.IndexElementSize);
		}

		public void DrawPrimitives(
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount)
		{
			ApplyState();

			PrepareVertexBindingArray(0);

			FNA3D_DrawPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
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

			FNA3D_DrawIndexedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				(FNA3D_Buffer*)_userIndexBuffer,
				(FNA3D_IndexElementSize)IndexElementSize.SixteenBits);
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

			FNA3D_DrawIndexedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				(FNA3D_Buffer*)_userIndexBuffer,
				(FNA3D_IndexElementSize)IndexElementSize.SixteenBits);
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

			FNA3D_DrawIndexedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				(FNA3D_Buffer*)_userIndexBuffer,
				(FNA3D_IndexElementSize)IndexElementSize.ThirtyTwoBits);
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

			FNA3D_DrawIndexedPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				0,
				0,
				numVertices,
				0,
				primitiveCount,
				(FNA3D_Buffer*)_userIndexBuffer,
				(FNA3D_IndexElementSize)IndexElementSize.ThirtyTwoBits);
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

			FNA3D_DrawPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
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

			FNA3D_DrawPrimitives(
				Device,
				(FNA3D_PrimitiveType)primitiveType,
				0,
				primitiveCount);
		}

		// ReSharper disable once InconsistentNaming
		public void SetStringMarkerEXT(string text) => FNA3D_SetStringMarker(Device, text);

		/* Needed by VideoPlayer */
		internal static void PrepareRenderTargetBindings(
			FNA3D_RenderTargetBinding* b,
			RenderTargetBinding[] bindings)
		{
			for (var i = 0; i < bindings.Length; i += 1, b += 1)
			{
				var texture = bindings[i].RenderTarget;
				var rt = texture as IRenderTarget;
				if (texture is RenderTargetCube)
				{
					b->type = 1;
					// JANK: FNA3D doesn't have proper C API interface exposed for the struct
					Unsafe.WriteUnaligned((byte*)b + 4, rt!.Width);
					Unsafe.WriteUnaligned((byte*)b + 8, (int)bindings[i].CubeMapFace);
				}
				else
				{
					b->type = 0;
					// JANK: FNA3D doesn't have proper C API interface exposed for the struct
					Unsafe.WriteUnaligned((byte*)b + 4, rt!.Width);
					Unsafe.WriteUnaligned((byte*)b + 8, rt.Height);
				}

				b->levelCount = rt.LevelCount;
				b->multiSampleCount = rt.MultiSampleCount;
				b->texture = (FNA3D_Texture*)texture!._texture;
				b->colorBuffer = (FNA3D_Renderbuffer*)rt.ColorBuffer;
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

		private void Dispose(bool disposing)
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
						FNA3D_AddDisposeVertexBuffer(
							Device,
							(FNA3D_Buffer*)_userVertexBuffer);
					}

					if (_userIndexBuffer != IntPtr.Zero)
					{
						FNA3D_AddDisposeIndexBuffer(
							Device,
							(FNA3D_Buffer*)_userIndexBuffer);
					}

					// Dispose of the GL Device/Context
					FNA3D_DestroyDevice(Device);
				}

				IsDisposed = true;
			}
		}

		private void ApplyState()
		{
			// Update Blend/DepthStencil, if applicable
			if (_currentBlend != BlendState)
			{
				FNA3D_SetBlendState(
					Device,
					(FNA3D_BlendState*)Unsafe.AsPointer(ref BlendState!._state));

				_currentBlend = BlendState;
			}

			if (_currentDepthStencil != DepthStencilState)
			{
				FNA3D_SetDepthStencilState(
					Device,
					(FNA3D_DepthStencilState*)Unsafe.AsPointer(ref DepthStencilState!._state));

				_currentDepthStencil = DepthStencilState;
			}

			// Always update RasterizerState, as it depends on other device states
			FNA3D_ApplyRasterizerState(
				Device,
				(FNA3D_RasterizerState*)Unsafe.AsPointer(ref RasterizerState!._state));

			ApplySamplers();
		}

		private void ApplySamplers()
		{
			for (var sampler = 0; sampler < _modifiedSamplers.Length; sampler += 1)
			{
				if (!_modifiedSamplers[sampler])
				{
					continue;
				}

				_modifiedSamplers[sampler] = false;

				FNA3D_VerifySampler(
					Device,
					sampler,
					(FNA3D_Texture*)(Textures[sampler] != null ? Textures[sampler]!._texture : IntPtr.Zero),
					(FNA3D_SamplerState*)Unsafe.AsPointer(ref SamplerStates[sampler]!._state));
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
				FNA3D_VerifyVertexSampler(
					Device,
					sampler,
					(FNA3D_Texture*)(VertexTextures[sampler] != null ? VertexTextures[sampler]!._texture : IntPtr.Zero),
					(FNA3D_SamplerState*)Unsafe.AsPointer(ref VertexSamplerStates[sampler]!._state));
			}
		}

		private void PrepareVertexBindingArray(int baseVertex)
		{
			fixed (FNA3D_VertexBufferBinding* b = &_nativeBufferBindings[0])
			{
				for (var i = 0; i < _vertexBufferCount; i += 1)
				{
					var buffer = _vertexBufferBindings[i].VertexBuffer;
					b[i].vertexBuffer = (FNA3D_Buffer*)buffer!.Buffer;
					b[i].vertexDeclaration.vertexStride = buffer.VertexDeclaration.VertexStride;
					b[i].vertexDeclaration.elementCount = buffer.VertexDeclaration._elements.Length;
					b[i].vertexDeclaration.elements = (FNA3D_VertexElement*)buffer.VertexDeclaration._elementsPin;
					b[i].vertexOffset = _vertexBufferBindings[i].VertexOffset;
					b[i].instanceFrequency = _vertexBufferBindings[i].InstanceFrequency;
				}

				FNA3D_ApplyVertexBufferBindings(
					Device,
					b,
					_vertexBufferCount,
					(byte)(_vertexBuffersUpdated ? 1 : 0),
					baseVertex);
			}

			_vertexBuffersUpdated = false;
		}

		private void PrepareUserVertexBuffer(
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
					FNA3D_AddDisposeVertexBuffer(
						Device,
						(FNA3D_Buffer*)_userVertexBuffer);
				}

				_userVertexBuffer = (IntPtr)FNA3D_GenVertexBuffer(
					Device,
					1,
					(FNA3D_BufferUsage)BufferUsage.WriteOnly,
					len);

				_userVertexBufferSize = len;
			}

			FNA3D_SetVertexBufferData(
				Device,
				(FNA3D_Buffer*)_userVertexBuffer,
				0,
				(void*)(vertexData + offset),
				len,
				1,
				1,
				(FNA3D_SetDataOptions)SetDataOptions.Discard);

			fixed (FNA3D_VertexBufferBinding* b = &_nativeBufferBindings[0])
			{
				b->vertexBuffer = (FNA3D_Buffer*)_userVertexBuffer;
				b->vertexDeclaration.vertexStride = vertexDeclaration.VertexStride;
				b->vertexDeclaration.elementCount = vertexDeclaration._elements.Length;
				b->vertexDeclaration.elements = (FNA3D_VertexElement*)vertexDeclaration._elementsPin;
				b->vertexOffset = 0;
				b->instanceFrequency = 0;
				FNA3D_ApplyVertexBufferBindings(Device, b, 1, 1, 0);
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
					FNA3D_AddDisposeIndexBuffer(
						Device,
						(FNA3D_Buffer*)_userIndexBuffer);
				}

				_userIndexBuffer = (IntPtr)FNA3D_GenIndexBuffer(
					Device,
					1,
					(FNA3D_BufferUsage)BufferUsage.WriteOnly,
					len);

				_userIndexBufferSize = len;
			}

			FNA3D_SetIndexBufferData(
				Device,
				(FNA3D_Buffer*)_userIndexBuffer,
				0,
				(void*)(indexData + (indexOffset * indexElementSizeInBytes)),
				len,
				(FNA3D_SetDataOptions)SetDataOptions.Discard);
		}

		private static int PrimitiveVertices(PrimitiveType primitiveType, int primitiveCount) =>
			primitiveType switch
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
