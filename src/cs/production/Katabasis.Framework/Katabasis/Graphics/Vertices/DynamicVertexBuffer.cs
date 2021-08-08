// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public class DynamicVertexBuffer : VertexBuffer
	{
		public DynamicVertexBuffer(
			VertexDeclaration vertexDeclaration,
			int vertexCount,
			BufferUsage bufferUsage)
			: base(
				vertexDeclaration,
				vertexCount,
				bufferUsage,
				true)
		{
		}

		public DynamicVertexBuffer(
			Type type,
			int vertexCount,
			BufferUsage bufferUsage)
			: base(
				VertexDeclaration.FromType(type),
				vertexCount,
				bufferUsage,
				true)
		{
		}

		public bool IsContentLost => false;

#pragma warning disable 0067
		// We never lose data, but lol XNA4 compliance -flibit
		public event EventHandler<EventArgs>? ContentLost;
#pragma warning restore 0067

		public void SetData<T>(
			int offsetInBytes,
			T[] data,
			int startIndex,
			int elementCount,
			int vertexStride,
			SetDataOptions options)
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
				options);

			handle.Free();
		}

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount,
			SetDataOptions options)
			where T : struct
		{
			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			ErrorCheck(data, startIndex, elementCount, elementSizeInBytes);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetVertexBufferData(
				GraphicsDevice.GLDevice,
				_buffer,
				0,
				handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
				elementCount,
				elementSizeInBytes,
				elementSizeInBytes,
				options);

			handle.Free();
		}
	}
}
