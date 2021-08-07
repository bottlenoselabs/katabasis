// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class DynamicIndexBuffer : IndexBuffer
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
				GraphicsDevice.GLDevice,
				_buffer,
				offsetInBytes,
				handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T))),
				elementCount * Marshal.SizeOf(typeof(T)),
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
			ErrorCheck(data, startIndex, elementCount);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetIndexBufferData(
				GraphicsDevice.GLDevice,
				_buffer,
				0,
				handle.AddrOfPinnedObject() + (startIndex * Marshal.SizeOf(typeof(T))),
				elementCount * Marshal.SizeOf(typeof(T)),
				options);

			handle.Free();
		}
	}
}
