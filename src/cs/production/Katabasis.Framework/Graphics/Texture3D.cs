// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class Texture3D : Texture
	{
		public Texture3D(
			int width,
			int height,
			int depth,
			bool mipMap,
			SurfaceFormat format)
		{
			GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
			Width = width;
			Height = height;
			Depth = depth;
			LevelCount = mipMap ? CalculateMipLevels(width, height) : 1;
			Format = format;

			_texture = (IntPtr)FNA3D.FNA3D_CreateTexture3D(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_SurfaceFormat)Format,
				Width,
				Height,
				Depth,
				LevelCount);
		}

		public int Width { get; }

		public int Height { get; }

		public int Depth { get; }

		public void SetData<T>(T[] data)
			where T : struct =>
			SetData(
				data,
				0,
				data.Length);

		public void SetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			SetData(
				0,
				0,
				0,
				Width,
				Height,
				0,
				Depth,
				data,
				startIndex,
				elementCount);

		public void SetData<T>(
			int level,
			int left,
			int top,
			int right,
			int bottom,
			int front,
			int back,
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_SetTextureData3D(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Texture*)_texture,
				left,
				top,
				front,
				right - left,
				bottom - top,
				back - front,
				level,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
				elementCount * elementSizeInBytes);

			handle.Free();
		}

		public void SetDataPointerEXT(
			int level,
			int left,
			int top,
			int right,
			int bottom,
			int front,
			int back,
			IntPtr data,
			int dataLength)
		{
			if (data == IntPtr.Zero)
			{
				throw new ArgumentNullException(nameof(data));
			}

			FNA3D.FNA3D_SetTextureData3D(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Texture*)_texture,
				left,
				top,
				front,
				right - left,
				bottom - top,
				back - front,
				level,
				(void*)data,
				dataLength);
		}

		public void GetData<T>(T[] data)
			where T : struct =>
			GetData(data, 0, data.Length);

		public void GetData<T>(
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct =>
			GetData(
				0,
				0,
				0,
				Width,
				Height,
				0,
				Depth,
				data,
				startIndex,
				elementCount);

		public void GetData<T>(
			int level,
			int left,
			int top,
			int right,
			int bottom,
			int front,
			int back,
			T[] data,
			int startIndex,
			int elementCount)
			where T : struct
		{
			if (data == null || data.Length == 0)
			{
				throw new ArgumentException("data cannot be null");
			}

			if (data.Length < startIndex + elementCount)
			{
				throw new ArgumentException("The data passed has a length of " + data.Length + " but " + elementCount + " pixels have been requested.");
			}

			if (left < 0 || left >= right || top < 0 || top >= bottom || front < 0 || front >= back)
			{
				throw new ArgumentException("Neither box size nor box position can be negative");
			}

			var elementSizeInBytes = Marshal.SizeOf(typeof(T));
			ValidateGetDataFormat(Format, elementSizeInBytes);

			var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
			FNA3D.FNA3D_GetTextureData3D(
				GraphicsDevice.Device,
				(FNA3D.FNA3D_Texture*)_texture,
				left,
				top,
				front,
				right - left,
				bottom - top,
				back - front,
				level,
				(void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
				elementCount * elementSizeInBytes);

			handle.Free();
		}
	}
}
