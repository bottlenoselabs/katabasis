// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public unsafe class DynamicIndexBuffer : IndexBuffer
	{
		public DynamicIndexBuffer(
			IndexElementSize indexElementSize,
			int indexCount,
			BufferUsage usage)
			: base(
				indexElementSize,
				indexCount,
				usage,
				true)
		{
		}

		public DynamicIndexBuffer(
			Type indexType,
			int indexCount,
			BufferUsage usage)
			: base(
				indexType,
				indexCount,
				usage,
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
			SetDataOptions options)
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
			ErrorCheck(data, startIndex, elementCount);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Buffer*)Buffer,
				0,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T)))),
				elementCount * Marshal.SizeOf(typeof(T)),
				(FNA3D.FNA3D_SetDataOptions)options);

			handle.Free();
		}
	}
}
