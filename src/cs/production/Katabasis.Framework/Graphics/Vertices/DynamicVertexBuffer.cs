// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public unsafe class DynamicVertexBuffer : VertexBuffer
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
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				offsetInBytes,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
				elementCount,
				elementSizeInBytes,
				vertexStride,
				(FNA3D.FNA3D_SetDataOptions)options);

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
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				0,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
				elementCount,
				elementSizeInBytes,
				elementSizeInBytes,
				(FNA3D.FNA3D_SetDataOptions)options);

			handle.Free();
		}
	}
}
