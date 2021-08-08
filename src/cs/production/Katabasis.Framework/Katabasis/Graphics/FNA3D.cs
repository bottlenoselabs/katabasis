// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using ObjCRuntime;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Native naming conventions.")]
	internal static class FNA3D
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void FNA3D_LogFunc(IntPtr msg);

		private const string _nativeLibraryName = "FNA3D";
		private static readonly FNA3D_Image_WriteFunc _writeFunc = INTERNAL_Write;
		private static int _writeGlobal;
		private static readonly Dictionary<IntPtr, Stream> _writeStreams = new();
		private static readonly FNA3D_Image_ReadFunc _readFunc = INTERNAL_Read;
		private static readonly FNA3D_Image_SkipFunc _skipFunc = INTERNAL_Skip;
		private static readonly FNA3D_Image_EOFFunc _eofFunc = INTERNAL_EOF;
		private static int _readGlobal;

		private static readonly Dictionary<IntPtr, Stream> _readStreams =
			new();

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetStringMarker(
			IntPtr device,
			[MarshalAs(UnmanagedType.LPWStr)] string text);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_HookLogFunctions(
			FNA3D_LogFunc info,
			FNA3D_LogFunc warn,
			FNA3D_LogFunc error);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern uint FNA3D_PrepareWindowAttributes();

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetDrawableSize(
			IntPtr window,
			out int w,
			out int h);

		/* IntPtr refers to an FNA3D_Device* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateDevice(
			ref FNA3D_PresentationParameters presentationParameters,
			byte debugMode);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DestroyDevice(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			ref Rectangle sourceRectangle,
			ref Rectangle destinationRectangle,
			IntPtr overrideWindowHandle);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			IntPtr sourceRectangle, /* null Rectangle */
			IntPtr destinationRectangle, /* null Rectangle */
			IntPtr overrideWindowHandle);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			ref Rectangle sourceRectangle,
			IntPtr destinationRectangle, /* null Rectangle */
			IntPtr overrideWindowHandle);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SwapBuffers(
			IntPtr device,
			IntPtr sourceRectangle, /* null Rectangle */
			ref Rectangle destinationRectangle,
			IntPtr overrideWindowHandle);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_Clear(
			IntPtr device,
			ClearOptions options,
			ref Vector4 color,
			float depth,
			int stencil);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawIndexedPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			IntPtr indices, /* FNA3D_Buffer* */
			IndexElementSize indexElementSize);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawInstancedPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int baseVertex,
			int minVertexIndex,
			int numVertices,
			int startIndex,
			int primitiveCount,
			int instanceCount,
			IntPtr indices, /* FNA3D_Buffer* */
			IndexElementSize indexElementSize);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_DrawPrimitives(
			IntPtr device,
			PrimitiveType primitiveType,
			int vertexStart,
			int primitiveCount);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetViewport(
			IntPtr device,
			ref FNA3D_Viewport viewport);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetScissorRect(
			IntPtr device,
			ref Rectangle scissor);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetBlendFactor(
			IntPtr device,
			out Color blendFactor);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetBlendFactor(
			IntPtr device,
			ref Color blendFactor);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMultiSampleMask(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetMultiSampleMask(
			IntPtr device,
			int mask);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetReferenceStencil(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetReferenceStencil(
			IntPtr device,
			int reference);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetBlendState(
			IntPtr device,
			ref FNA3D_BlendState blendState);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetDepthStencilState(
			IntPtr device,
			ref FNA3D_DepthStencilState depthStencilState);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyRasterizerState(
			IntPtr device,
			ref FNA3D_RasterizerState rasterizerState);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_VerifySampler(
			IntPtr device,
			int index,
			IntPtr texture, /* FNA3D_Texture* */
			ref FNA3D_SamplerState sampler);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_VerifyVertexSampler(
			IntPtr device,
			int index,
			IntPtr texture, /* FNA3D_Texture* */
			ref FNA3D_SamplerState sampler);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_ApplyVertexBufferBindings(
			IntPtr device,
			FNA3D_VertexBufferBinding* bindings,
			int numBindings,
			byte bindingsUpdated,
			int baseVertex);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetRenderTargets(
			IntPtr device,
			IntPtr renderTargets, /* FNA3D_RenderTargetBinding* */
			int numRenderTargets,
			IntPtr depthStencilBuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat,
			byte preserveDepthStencilContents);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern unsafe void FNA3D_SetRenderTargets(
			IntPtr device,
			FNA3D_RenderTargetBinding* renderTargets,
			int numRenderTargets,
			IntPtr depthStencilBuffer, /* FNA3D_Renderbuffer */
			DepthFormat depthFormat,
			byte preserveDepthStencilContents);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResolveTarget(
			IntPtr device,
			ref FNA3D_RenderTargetBinding target);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ResetBackbuffer(
			IntPtr device,
			ref FNA3D_PresentationParameters presentationParameters);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ReadBackbuffer(
			IntPtr device,
			int x,
			int y,
			int w,
			int h,
			IntPtr data,
			int dataLen);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetBackbufferSize(
			IntPtr device,
			out int w,
			out int h);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern SurfaceFormat FNA3D_GetBackbufferSurfaceFormat(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern DepthFormat FNA3D_GetBackbufferDepthFormat(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetBackbufferMultiSampleCount(IntPtr device);

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTexture2D(
			IntPtr device,
			SurfaceFormat format,
			int width,
			int height,
			int levelCount,
			byte isRenderTarget);

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTexture3D(
			IntPtr device,
			SurfaceFormat format,
			int width,
			int height,
			int depth,
			int levelCount);

		/* IntPtr refers to an FNA3D_Texture* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateTextureCube(
			IntPtr device,
			SurfaceFormat format,
			int size,
			int levelCount,
			byte isRenderTarget);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeTexture(
			IntPtr device,
			/* FNA3D_Texture* */
			IntPtr texture);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureData2D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureData3D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int z,
			int w,
			int h,
			int d,
			int level,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureDataCube(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			CubeMapFace cubeMapFace,
			int level,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetTextureDataYUV(
			IntPtr device,
			IntPtr y,
			IntPtr u,
			IntPtr v,
			int yWidth,
			int yHeight,
			int uvWidth,
			int uvHeight,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData2D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			int level,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureData3D(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int z,
			int w,
			int h,
			int d,
			int level,
			IntPtr data,
			int dataLength);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetTextureDataCube(
			IntPtr device,
			IntPtr texture,
			int x,
			int y,
			int w,
			int h,
			CubeMapFace cubeMapFace,
			int level,
			IntPtr data,
			int dataLength);

		/* IntPtr refers to an FNA3D_Renderbuffer* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenColorRenderbuffer(
			IntPtr device,
			int width,
			int height,
			SurfaceFormat format,
			int multiSampleCount,
			/* FNA3D_Texture* */
			IntPtr texture);

		/* IntPtr refers to an FNA3D_Renderbuffer* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenDepthStencilRenderbuffer(
			IntPtr device,
			int width,
			int height,
			DepthFormat format,
			int multiSampleCount);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeRenderbuffer(
			IntPtr device,
			IntPtr renderbuffer);

		/* IntPtr refers to an FNA3D_Buffer* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenVertexBuffer(
			IntPtr device,
			byte dynamic,
			BufferUsage usage,
			int sizeInBytes);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeVertexBuffer(
			IntPtr device,
			IntPtr buffer);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetVertexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride,
			SetDataOptions options);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetVertexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int elementCount,
			int elementSizeInBytes,
			int vertexStride);

		/* IntPtr refers to an FNA3D_Buffer* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_GenIndexBuffer(
			IntPtr device,
			byte dynamic,
			BufferUsage usage,
			int sizeInBytes);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeIndexBuffer(
			IntPtr device,
			IntPtr buffer);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetIndexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetIndexBufferData(
			IntPtr device,
			IntPtr buffer,
			int offsetInBytes,
			IntPtr data,
			int dataLength);

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_CreateEffect(
			IntPtr device,
			byte[] effectCode,
			int length,
			out IntPtr effect,
			out IntPtr effectData);

		/* IntPtr refers to an FNA3D_Effect* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_CloneEffect(
			IntPtr device,
			IntPtr cloneSource,
			out IntPtr effect,
			out IntPtr effectData);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeEffect(
			IntPtr device,
			IntPtr effect);

		/* effect refers to a MOJOSHADER_effect*, technique to a MOJOSHADER_effectTechnique* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_SetEffectTechnique(
			IntPtr device,
			IntPtr effect,
			IntPtr technique);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_ApplyEffect(
			IntPtr device,
			IntPtr effect,
			uint pass,
			/* MOJOSHADER_effectStateChanges* */
			IntPtr stateChanges);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_BeginPassRestore(
			IntPtr device,
			IntPtr effect,
			/* MOJOSHADER_effectStateChanges* */
			IntPtr stateChanges);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_EndPassRestore(
			IntPtr device,
			IntPtr effect);

		/* IntPtr refers to an FNA3D_Query* */
		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern IntPtr FNA3D_CreateQuery(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_AddDisposeQuery(
			IntPtr device,
			IntPtr query);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_QueryBegin(
			IntPtr device,
			IntPtr query);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_QueryEnd(
			IntPtr device,
			IntPtr query);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_QueryComplete(
			IntPtr device,
			IntPtr query);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_QueryPixelCount(IntPtr device, IntPtr query);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsDXT1(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsS3TC(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsHardwareInstancing(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern byte FNA3D_SupportsNoOverwrite(IntPtr device);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_GetMaxTextureSlots(
			IntPtr device,
			out int textures,
			out int vertexTextures);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern int FNA3D_GetMaxMultiSampleCount(
			IntPtr device,
			SurfaceFormat format,
			int preferredMultiSampleCount);

		public static void WritePNGStream(
			Stream stream,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data)
		{
			IntPtr context;
			lock (_writeStreams)
			{
				context = (IntPtr)_writeGlobal++;
				_writeStreams.Add(context, stream);
			}

			FNA3D_Image_SavePNG(
				_writeFunc,
				context,
				srcW,
				srcH,
				dstW,
				dstH,
				data);

			lock (_writeStreams)
			{
				_writeStreams.Remove(context);
			}
		}

		public static void WriteJPGStream(
			Stream stream,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data,
			int quality)
		{
			IntPtr context;
			lock (_writeStreams)
			{
				context = (IntPtr)_writeGlobal++;
				_writeStreams.Add(context, stream);
			}

			FNA3D_Image_SaveJPG(
				_writeFunc,
				context,
				srcW,
				srcH,
				dstW,
				dstH,
				data,
				quality);

			lock (_writeStreams)
			{
				_writeStreams.Remove(context);
			}
		}

		public static IntPtr ReadImageStream(
			Stream stream,
			out int width,
			out int height,
			out int len,
			int forceW = -1,
			int forceH = -1,
			bool zoom = false)
		{
			IntPtr context;
			lock (_readStreams)
			{
				context = (IntPtr)_readGlobal++;
				_readStreams.Add(context, stream);
			}

			var pixels = FNA3D_Image_Load(
				_readFunc,
				_skipFunc,
				_eofFunc,
				context,
				out width,
				out height,
				out len,
				forceW,
				forceH,
				(byte)(zoom ? 1 : 0));

			lock (_readStreams)
			{
				_readStreams.Remove(context);
			}

			return pixels;
		}

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		public static extern void FNA3D_Image_Free(IntPtr mem);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr FNA3D_Image_Load(
			FNA3D_Image_ReadFunc readFunc,
			FNA3D_Image_SkipFunc skipFunc,
			FNA3D_Image_EOFFunc eofFunc,
			IntPtr context,
			out int width,
			out int height,
			out int len,
			int forceW,
			int forceH,
			byte zoom);

		[MonoPInvokeCallback(typeof(FNA3D_Image_ReadFunc))]
		private static int INTERNAL_Read(
			IntPtr context,
			IntPtr data,
			int size)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[context];
			}

			byte[] buf = new byte[size]; // FIXME: Preallocate!
			var result = stream.Read(buf, 0, size);
			Marshal.Copy(buf, 0, data, result);
			return result;
		}

		[MonoPInvokeCallback(typeof(FNA3D_Image_SkipFunc))]
		private static void INTERNAL_Skip(IntPtr context, int n)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[context];
			}

			stream.Seek(n, SeekOrigin.Current);
		}

		[MonoPInvokeCallback(typeof(FNA3D_Image_EOFFunc))]
		private static int INTERNAL_EOF(IntPtr context)
		{
			Stream stream;
			lock (_readStreams)
			{
				stream = _readStreams[context];
			}

			return stream.Position == stream.Length ? 1 : 0;
		}

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FNA3D_Image_SavePNG(
			FNA3D_Image_WriteFunc writeFunc,
			IntPtr context,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data);

		[DllImport(_nativeLibraryName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void FNA3D_Image_SaveJPG(
			FNA3D_Image_WriteFunc writeFunc,
			IntPtr context,
			int srcW,
			int srcH,
			int dstW,
			int dstH,
			IntPtr data,
			int quality);

		[MonoPInvokeCallback(typeof(FNA3D_Image_WriteFunc))]
		private static void INTERNAL_Write(
			IntPtr context,
			IntPtr data,
			int size)
		{
			Stream stream;
			lock (_writeStreams)
			{
				stream = _writeStreams[context];
			}

			byte[] buf = new byte[size]; // FIXME: Preallocate!
			Marshal.Copy(data, buf, 0, size);
			stream.Write(buf, 0, size);
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_Viewport
		{
			public int X;
			public int Y;
			public int W;
			public int H;
			public float MinDepth;
			public float MaxDepth;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_BlendState
		{
			public Blend ColorSourceBlend;
			public Blend ColorDestinationBlend;
			public BlendFunction ColorBlendFunction;
			public Blend AlphaSourceBlend;
			public Blend AlphaDestinationBlend;
			public BlendFunction AlphaBlendFunction;
			public ColorWriteChannels ColorWriteEnable;
			public ColorWriteChannels ColorWriteEnable1;
			public ColorWriteChannels ColorWriteEnable2;
			public ColorWriteChannels ColorWriteEnable3;
			public Color BlendFactor;
			public int MultiSampleMask;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_DepthStencilState
		{
			public byte DepthBufferEnable;
			public byte DepthBufferWriteEnable;
			public CompareFunction DepthBufferFunction;
			public byte StencilEnable;
			public int StencilMask;
			public int StencilWriteMask;
			public byte TwoSidedStencilMode;
			public StencilOperation StencilFail;
			public StencilOperation StencilDepthBufferFail;
			public StencilOperation StencilPass;
			public CompareFunction StencilFunction;
			public StencilOperation CounterClockwiseStencilFail;
			public StencilOperation CounterClockwiseStencilDepthBufferFail;
			public StencilOperation CounterClockwiseStencilPass;
			public CompareFunction CounterClockwiseStencilFunction;
			public int ReferenceStencil;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_RasterizerState
		{
			public FillMode FillMode;
			public CullMode CullMode;
			public float DepthBias;
			public float SlopeScaleDepthBias;
			public byte ScissorTestEnable;
			public byte MultiSampleAntiAlias;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_SamplerState
		{
			public TextureFilter Filter;
			public TextureAddressMode AddressU;
			public TextureAddressMode AddressV;
			public TextureAddressMode AddressW;
			public float MipMapLevelOfDetailBias;
			public int MaxAnisotropy;
			public int MaxMipLevel;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_VertexDeclaration
		{
			public int VertexStride;
			public int ElementCount;
			public IntPtr Elements; /* FNA3D_VertexElement* */
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_VertexBufferBinding
		{
			public IntPtr VertexBuffer; /* FNA3D_Buffer* */
			public FNA3D_VertexDeclaration VertexDeclaration;
			public int VertexOffset;
			public int InstanceFrequency;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_RenderTargetBinding
		{
			public byte Type;
			public int Data1; /* width for 2D, size for Cube */
			public int Data2; /* height for 2D, face for Cube */
			public int LevelCount;
			public int MultiSampleCount;
			public IntPtr Texture;
			public IntPtr ColorBuffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct FNA3D_PresentationParameters
		{
			public int BackBufferWidth;
			public int BackBufferHeight;
			public SurfaceFormat BackBufferFormat;
			public int MultiSampleCount;
			public IntPtr DeviceWindowHandle;
			public byte IsFullScreen;
			public DepthFormat DepthStencilFormat;
			public PresentInterval PresentationInterval;
			public DisplayOrientation DisplayOrientation;
			public RenderTargetUsage RenderTargetUsage;
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FNA3D_Image_ReadFunc(
			IntPtr context,
			IntPtr data,
			int size);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void FNA3D_Image_SkipFunc(
			IntPtr context,
			int n);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FNA3D_Image_EOFFunc(IntPtr context);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void FNA3D_Image_WriteFunc(
			IntPtr context,
			IntPtr data,
			int size);
	}
}
