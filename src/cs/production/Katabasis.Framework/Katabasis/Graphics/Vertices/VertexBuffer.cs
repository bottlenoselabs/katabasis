// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class VertexBuffer : GraphicsResource
	{
		internal IntPtr _buffer;

		public VertexBuffer(
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage)
			: this(
				vertexDeclaration,
				vertexCount,
				bufferUsage,
				false)
		{
		}

		public VertexBuffer(
			Type type,
			int vertexCount,
			BufferUsage bufferUsage)
			: this(
				VertexDeclaration.FromType(type),
				vertexCount,
				bufferUsage,
				false)
		{
		}

		protected VertexBuffer(
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage,
			bool dynamic)
		{
			GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			VertexDeclaration = vertexDeclaration;
			VertexCount = vertexCount;
			BufferUsage = bufferUsage;

			// Make sure the graphics device is assigned in the vertex declaration.
			if (vertexDeclaration.GraphicsDevice != GraphicsDevice)
			{
				vertexDeclaration.GraphicsDevice = GraphicsDevice;
			}

			_buffer = FNA3D.FNA3D_GenVertexBuffer(
				GraphicsDevice.GLDevice,
				(byte)(dynamic ? 1 : 0),
				bufferUsage,
				VertexCount * VertexDeclaration.VertexStride);
		}

		public BufferUsage BufferUsage { get; }

		public int VertexCount { get; }

		public VertexDeclaration VertexDeclaration { get; }

		public void GetData<T>(T[] data)
			where T : struct =>
			GetData(
				0,
				data,
				0,
				data.Length,
				Marshal.SizeOf(typeof(T)));

		public void GetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			GetData(
				0,
				data,
				startIndex,
				elementCount,
				Marshal.SizeOf(typeof(T)));

		public void GetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride)
			where T : struct
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (data.Length < startIndex + elementCount)
			{
				throw new ArgumentOutOfRangeException(
					nameof(elementCount),
					"This parameter must be a valid index within the array.");
			}

			if (BufferUsage == BufferUsage.WriteOnly)
			{
				throw new NotSupportedException(
					"Calling GetData on a resource that was created with BufferUsage.WriteOnly is not supported.");
			}

			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			if (vertexStride == 0)
			{
				vertexStride = elementSizeInBytes;
			}
			else if (vertexStride < elementSizeInBytes)
			{
				throw new ArgumentOutOfRangeException(
					nameof(vertexStride),
					"The vertex stride is too small for the type of data requested. This is not allowed.");
			}

			if (elementCount > 1 &&
			    elementCount * vertexStride > VertexCount * VertexDeclaration.VertexStride)
			{
				throw new InvalidOperationException(
					"The array is not the correct size for the amount of data requested.");
			}

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_GetVertexBufferData(
				GraphicsDevice.GLDevice,
				_buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				elementCount,
				elementSizeInBytes,
				vertexStride);

			handle.Free();
		}

		public void SetData<T>(T[] data)
			where T : struct =>
			SetData(
				0,
				data,
				0,
				data.Length,
				Marshal.SizeOf(typeof(T)));

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			SetData(
				0,
				data,
				startIndex,
				elementCount,
				Marshal.SizeOf(typeof(T)));

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride)
			where T : struct
		{
			ErrorCheck(data, startIndex, elementCount, vertexStride);

			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetVertexBufferData(
				GraphicsDevice.GLDevice,
				_buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				elementCount,
				elementSizeInBytes,
				vertexStride,
				SetDataOptions.None);

			handle.Free();
		}

		public void SetDataPointerEXT(
			int offsetInBytes,
			IntPtr data,
			int dataLength,
			SetDataOptions options) =>
			FNA3D.FNA3D_SetVertexBufferData(
				GraphicsDevice.GLDevice,
				_buffer,
				offsetInBytes,
				data,
				dataLength,
				1,
				1,
				options);

		[Conditional("DEBUG")]
		internal void ErrorCheck<T>(
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride)
			where T : struct
		{
			if (data == null)
			{
				throw new ArgumentNullException(nameof(data));
			}

			if (startIndex + elementCount > data.Length || elementCount <= 0)
			{
				throw new InvalidOperationException(
					"The array specified in the data parameter is not the correct size for the amount of data requested.");
			}

			if (elementCount > 1 &&
			    elementCount * vertexStride > VertexCount * VertexDeclaration.VertexStride)
			{
				throw new InvalidOperationException(
					"The vertex stride is larger than the vertex buffer.");
			}

			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			if (vertexStride == 0)
			{
				vertexStride = elementSizeInBytes;
			}

			if (vertexStride < elementSizeInBytes)
			{
				throw new ArgumentOutOfRangeException(
					$"The vertex stride must be greater than or equal to the size of the specified data ({elementSizeInBytes}).");
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
				FNA3D.FNA3D_AddDisposeVertexBuffer(
					GraphicsDevice.GLDevice,
					_buffer);
			}

			base.Dispose(disposing);
		}
	}
}
