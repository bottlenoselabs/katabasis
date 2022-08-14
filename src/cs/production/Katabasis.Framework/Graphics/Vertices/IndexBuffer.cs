// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class IndexBuffer : GraphicsResource
	{
		internal IntPtr Buffer;

		public IndexBuffer(
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage bufferUsage)
			: this(
				indexElementSize,
				indexCount,
				bufferUsage,
				false)
		{
		}

		public IndexBuffer(
			Type indexType,
			int indexCount,
			BufferUsage usage)
			: this(
				SizeForType(indexType),
				indexCount,
				usage,
				false)
		{
		}

		public IndexBuffer(
			Type indexType,
			int indexCount,
			BufferUsage usage,
			bool dynamic)
			: this(
				SizeForType(indexType),
				indexCount,
				usage,
				dynamic)
		{
		}

		public IndexBuffer(
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage usage,
			bool dynamic)
		{
			GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			IndexElementSize = indexElementSize;
			IndexCount = indexCount;
			BufferUsage = usage;

			var stride = indexElementSize == IndexElementSize.ThirtyTwoBits ? 4 : 2;

			Buffer = (IntPtr)FNA3D.FNA3D_GenIndexBuffer(
				GraphicsDevice.Device,
				(byte)(dynamic ? 1 : 0),
				(FNA3D.FNA3D_BufferUsage)usage,
				IndexCount * stride);
		}

		public BufferUsage BufferUsage { get; }

		public int IndexCount { get; }

		public IndexElementSize IndexElementSize { get; }

		public void SetDataPointerEXT(
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options) =>
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				offsetInBytes,
				(void*)data,
				dataLength,
				(FNA3D.FNA3D_SetDataOptions)options);

		[Conditional("DEBUG")]
		internal static void ErrorCheck<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length < startIndex + elementCount)
			{
				throw new InvalidOperationException(
					"The array specified in the data parameter is not the correct size for the amount of data requested.");
			}
		}

		protected internal override void GraphicsDeviceResetting()
		{
			// FIXME: Do we even want to bother with DeviceResetting for GL? -flibit
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				FNA3D.FNA3D_AddDisposeIndexBuffer(
					GraphicsDevice.Device,
					(FNA3D.FNA3D_Buffer*)Buffer);
			}

			base.Dispose(disposing);
		}

		private static IndexElementSize SizeForType(Type type)
		{
			var sizeInBytes = Marshal.SizeOf(type);

			if (sizeInBytes == 2)
			{
				return IndexElementSize.SixteenBits;
			}

			if (sizeInBytes == 4)
			{
				return IndexElementSize.ThirtyTwoBits;
			}

			throw new ArgumentOutOfRangeException(
				nameof(type), "Index buffers can only be created for types that are sixteen or thirty two bits in length");
		}

		public void GetData<T>(T[] data)
			where T : struct =>
			GetData(
				0,
				data,
				0,
				data.Length);

		public void GetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			GetData(
				0,
				data,
				startIndex,
				elementCount);

		public void GetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length < startIndex + elementCount)
			{
				throw new InvalidOperationException(
					"The array specified in the data parameter is not the correct size for the amount of data requested.");
			}

			if (BufferUsage == BufferUsage.WriteOnly)
			{
				throw new NotSupportedException(
					"This IndexBuffer was created with a usage type of BufferUsage.WriteOnly. Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			}

			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_GetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				offsetInBytes,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
				elementCount * elementSizeInBytes);

			handle.Free();
		}

		public void SetData<T>(T[] data)
			where T : struct
		{
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				0,
				(void*)handle.AddrOfPinnedObject(),
				data.Length * Marshal.SizeOf(typeof(T)),
				(FNA3D.FNA3D_SetDataOptions)SetDataOptions.None);

			handle.Free();
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			ErrorCheck(data, startIndex, elementCount);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				0,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T)))),
				elementCount * Marshal.SizeOf(typeof(T)),
				(FNA3D.FNA3D_SetDataOptions)SetDataOptions.None);

			handle.Free();
		}

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			ErrorCheck(data, startIndex, elementCount);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				offsetInBytes,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T)))),
				elementCount * Marshal.SizeOf(typeof(T)),
				(FNA3D.FNA3D_SetDataOptions)SetDataOptions.None);

			handle.Free();
		}
	}
}
