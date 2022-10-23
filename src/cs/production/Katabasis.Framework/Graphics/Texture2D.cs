// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using static bottlenoselabs.FNA3D;

namespace bottlenoselabs.Katabasis
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
    public unsafe class Texture2D : Texture
    {
        public Texture2D(
            int width,
            int height,
            bool mipMap = false,
            SurfaceFormat format = SurfaceFormat.Color)
        {
            GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
            Width = width;
            Height = height;
            LevelCount = mipMap ? CalculateMipLevels(width, height) : 1;

            // TODO: Use QueryRenderTargetFormat!
            if (this is IRenderTarget)
            {
                if (format == SurfaceFormat.ColorSrgbEXT)
                {
                    if (FNA3D_SupportsSRGBRenderTargets(GraphicsDevice.Device) == 0)
                    {
                        // Renderable but not on this device
                        Format = SurfaceFormat.Color;
                    }
                    else
                    {
                        Format = format;
                    }
                }
                else if (format != SurfaceFormat.Color &&
                         format != SurfaceFormat.Rgba1010102 &&
                         format != SurfaceFormat.Rg32 &&
                         format != SurfaceFormat.Rgba64 &&
                         format != SurfaceFormat.Single &&
                         format != SurfaceFormat.Vector2 &&
                         format != SurfaceFormat.Vector4 &&
                         format != SurfaceFormat.HalfSingle &&
                         format != SurfaceFormat.HalfVector2 &&
                         format != SurfaceFormat.HalfVector4 &&
                         format != SurfaceFormat.HdrBlendable)
                {
                    // Not a renderable format period
                    Format = SurfaceFormat.Color;
                }
                else
                {
                    Format = format;
                }
            }
            else
            {
                Format = format;
            }

            _texture = (IntPtr)FNA3D_CreateTexture2D(
                GraphicsDevice.Device,
                (FNA3D_SurfaceFormat)Format,
                Width,
                Height,
                LevelCount,
                (byte)(this is IRenderTarget ? 1 : 0));
        }

        public int Width { get; }

        public int Height { get; }

        public Rectangle Bounds => new(0, 0, Width, Height);

        public void SetData<T>(T[] data)
            where T : struct =>
            SetData(0, null, data, 0, data.Length);

        public void SetData<T>(
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct =>
            SetData(0, null, data, startIndex, elementCount);

        public void SetData<T>(
            int level,
            Rectangle? rect,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            if (startIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            if (data.Length < (elementCount + startIndex))
            {
                throw new ArgumentOutOfRangeException(nameof(elementCount));
            }

            int x, y, w, h;
            if (rect.HasValue)
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                w = rect.Value.Width;
                h = rect.Value.Height;
            }
            else
            {
                x = 0;
                y = 0;
                w = Math.Max(Width >> level, 1);
                h = Math.Max(Height >> level, 1);
            }

            var elementSize = Marshal.SizeOf(typeof(T));
            var requiredBytes = w * h * GetFormatSize(Format) / GetBlockSizeSquared(Format);
            var availableBytes = elementCount * elementSize;
            if (requiredBytes > availableBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(rect), "The region you are trying to upload is larger than the amount of data you provided.");
            }

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            FNA3D_SetTextureData2D(
                GraphicsDevice.Device,
                (FNA3D_Texture*)_texture,
                x,
                y,
                w,
                h,
                level,
                (void*)(handle.AddrOfPinnedObject() + (startIndex * elementSize)),
                elementCount * elementSize);

            handle.Free();
        }

        public void SetDataPointerEXT(
            int level,
            Rectangle? rect,
            IntPtr data,
            int dataLength)
        {
            if (data == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int x, y, w, h;
            if (rect.HasValue)
            {
                x = rect.Value.X;
                y = rect.Value.Y;
                w = rect.Value.Width;
                h = rect.Value.Height;
            }
            else
            {
                x = 0;
                y = 0;
                w = Math.Max(Width >> level, 1);
                h = Math.Max(Height >> level, 1);
            }

            FNA3D_SetTextureData2D(
                GraphicsDevice.Device,
                (FNA3D_Texture*)_texture,
                x,
                y,
                w,
                h,
                level,
                (void*)data,
                dataLength);
        }

        public void GetData<T>(T[] data)
            where T : struct =>
            GetData(0, null, data, 0, data.Length);

        public void GetData<T>(
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct =>
            GetData(0, null, data, startIndex, elementCount);

        public void GetData<T>(
            int level,
            Rectangle? rect,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            if (data.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            if (data.Length < startIndex + elementCount)
            {
                throw new ArgumentException(
                    $"The data passed has a length of {data.Length} but {elementCount} pixels have been requested.");
            }

            int subX, subY, subW, subH;
            if (rect == null)
            {
                subX = 0;
                subY = 0;
                subW = Width >> level;
                subH = Height >> level;
            }
            else
            {
                subX = rect.Value.X;
                subY = rect.Value.Y;
                subW = rect.Value.Width;
                subH = rect.Value.Height;
            }

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            ValidateGetDataFormat(Format, elementSizeInBytes);

            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            FNA3D_GetTextureData2D(
                GraphicsDevice.Device,
                (FNA3D_Texture*)_texture,
                subX,
                subY,
                subW,
                subH,
                level,
                (void*)(handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes)),
                elementCount * elementSizeInBytes);

            handle.Free();
        }

        public void SaveAsJpeg(Stream stream, int width, int height)
        {
	        var qualityString = Environment.GetEnvironmentVariable("FNA_GRAPHICS_JPEG_SAVE_QUALITY");
	        int quality;
	        if (qualityString == null || !int.TryParse(qualityString, out quality))
	        {
		        quality = 100; // FIXME: What does XNA pick for quality? -flibit
	        }

	        var len = Width * Height * GetFormatSize(Format);
	        var data = Marshal.AllocHGlobal(len);
	        FNA3D_GetTextureData2D(
                GraphicsDevice.Device,
                (FNA3D.FNA3D_Texture*)_texture,
                0,
                0,
                Width,
                height,
                0,
                (void*)data,
                len);

	        FNA.WriteJPGStream(
                stream,
                Width,
                Height,
                width,
                height,
                data,
                quality);

	        Marshal.FreeHGlobal(data);
        }

        public void SaveAsPng(Stream stream, int width, int height)
        {
            var len = Width * Height * GetFormatSize(Format);
            var data = Marshal.AllocHGlobal(len);
            FNA3D_GetTextureData2D(
                GraphicsDevice.Device,
                (FNA3D_Texture*)_texture,
                0,
                0,
                Width,
                height,
                0,
                (void*)data,
                len);

            FNA.WritePNGStream(
                stream,
                Width,
                Height,
                width,
                height,
                data);

            Marshal.FreeHGlobal(data);
        }

        public static Texture2D FromFile(string filePath)
        {
            var stream = TitleContainer.OpenStream(filePath);
            return FromStream(stream);
        }

        public static Texture2D FromStream(Stream stream)
        {
            if (stream.CanSeek && stream.Position == stream.Length)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var pixels = FNA.ReadImageStream(
                stream,
                out var width,
                out var height,
                out var len);

            var result = new Texture2D(width, height);
            result.SetDataPointerEXT(
                0,
                null,
                pixels,
                len);

            FNA3D_Image_Free((byte*)pixels);
            return result;
        }

        public static Texture2D FromStream(
            GraphicsDevice graphicsDevice,
            Stream stream,
            int width,
            int height,
            bool zoom)
        {
            if (stream.CanSeek && stream.Position == stream.Length)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var pixels = FNA.ReadImageStream(
                stream,
                out var realWidth,
                out var realHeight,
                out var len,
                width,
                height,
                zoom);

            Texture2D result = new(realWidth, realHeight);
            result.SetDataPointerEXT(
                0,
                null,
                pixels,
                len);

            FNA3D_Image_Free((byte*)pixels);
            return result;
        }

        // ReSharper disable once InconsistentNaming
        public static void TextureDataFromStreamEXT(
            Stream stream,
            out int width,
            out int height,
            out byte[] pixels,
            int requestedWidth = -1,
            int requestedHeight = -1,
            bool zoom = false)
        {
            if (stream.CanSeek && stream.Position == stream.Length)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            var pixPtr = FNA.ReadImageStream(
                stream,
                out width,
                out height,
                out var len,
                requestedWidth,
                requestedHeight,
                zoom);

            pixels = new byte[len];
            Marshal.Copy(pixPtr, pixels, 0, len);

            FNA3D_Image_Free((byte*)pixPtr);
        }

        public static Texture2D DDSFromStreamEXT(Stream stream)
        {
            // Begin BinaryReader, ignoring a tab!
            using BinaryReader reader = new(stream);

            ParseDDS(reader, out var format, out var width, out var height, out var levels, out var isCube);

            if (isCube)
            {
                throw new FormatException("This file contains cube map data!");
            }

            // Allocate/Load texture
            var result = new Texture2D(
                width,
                height,
                levels > 1,
                format);

            if (stream is MemoryStream memoryStream &&
                memoryStream.TryGetBuffer(out byte[] tex))
            {
                for (var i = 0; i < levels; i += 1)
                {
                    var levelSize = CalculateDDSLevelSize(width >> i, height >> i, format);

                    result.SetData(
                        i,
                        null,
                        tex,
                        (int)memoryStream.Seek(0, SeekOrigin.Current),
                        levelSize);

                    memoryStream.Seek(
                        levelSize,
                        SeekOrigin.Current);
                }
            }
            else
            {
                for (var i = 0; i < levels; i += 1)
                {
                    tex = reader.ReadBytes(CalculateDDSLevelSize(width >> i, height >> i, format));
                    result.SetData(i, null, tex, 0, tex.Length);
                }
            }

            // End BinaryReader

            // Finally.
            return result;
        }
    }
}
