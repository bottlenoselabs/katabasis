// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Need tests.")]
    public class TextureCube : Texture
    {
        public int Size { get; }

        public TextureCube(int size, bool mipMap, SurfaceFormat format)
        {
            GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
            Size = size;
            LevelCount = mipMap ? CalculateMipLevels(size) : 1;

            // TODO: Use QueryRenderTargetFormat!
            if (this is IRenderTarget &&
                format != SurfaceFormat.Color &&
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
                Format = SurfaceFormat.Color;
            }
            else
            {
                Format = format;
            }

            _texture = FNA3D.FNA3D_CreateTextureCube(
                GraphicsDevice.GLDevice,
                Format,
                Size,
                LevelCount,
                (byte)(this is IRenderTarget ? 1 : 0));
        }

        public static TextureCube DDSFromStreamEXT(Stream stream)
        {
            // Begin BinaryReader, ignoring a tab!
            using BinaryReader reader = new BinaryReader(stream);

            ParseDDS(
                reader,
                out var format,
                out var width,
                out _,
                out var levels,
                out var levelSize,
                out var blockSize);

            // Allocate/Load texture
            var result = new TextureCube(width, levels > 1, format);

            if (stream is MemoryStream memoryStream &&
                memoryStream.TryGetBuffer(out byte[] tex))
            {
                for (var face = 0; face < 6; face += 1)
                {
                    var mipLevelSize = levelSize;
                    for (var i = 0; i < levels; i += 1)
                    {
                        result.SetData(
                            (CubeMapFace)face,
                            i,
                            null,
                            tex,
                            (int)memoryStream.Seek(0, SeekOrigin.Current),
                            mipLevelSize);
                        memoryStream.Seek(
                            mipLevelSize,
                            SeekOrigin.Current);
                        mipLevelSize = Math.Max(
                            mipLevelSize >> 2,
                            blockSize);
                    }
                }
            }
            else
            {
                for (var face = 0; face < 6; face += 1)
                {
                    var mipLevelSize = levelSize;
                    for (var i = 0; i < levels; i += 1)
                    {
                        tex = reader.ReadBytes(mipLevelSize);
                        result.SetData(
                            (CubeMapFace)face,
                            i,
                            null,
                            tex,
                            0,
                            tex.Length);
                        mipLevelSize = Math.Max(
                            mipLevelSize >> 2,
                            blockSize);
                    }
                }
            }

            // End BinaryReader

            // Finally.
            return result;
        }

        public void SetData<T>(CubeMapFace cubeMapFace, T[] data)
            where T : struct
        {
            SetData(
                cubeMapFace,
                0,
                null,
                data,
                0,
                data.Length);
        }

        public void SetData<T>(
            CubeMapFace cubeMapFace,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            SetData(
                cubeMapFace,
                0,
                null,
                data,
                startIndex,
                elementCount);
        }

        public void SetData<T>(
            CubeMapFace cubeMapFace,
            int level,
            Rectangle? rect,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int xOffset, yOffset, width, height;
            if (rect.HasValue)
            {
                xOffset = rect.Value.X;
                yOffset = rect.Value.Y;
                width = rect.Value.Width;
                height = rect.Value.Height;
            }
            else
            {
                xOffset = 0;
                yOffset = 0;
                width = Math.Max(1, Size >> level);
                height = Math.Max(1, Size >> level);
            }

            var elementSizeInBytes = Marshal.SizeOf(typeof(T));
            var handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            FNA3D.FNA3D_SetTextureDataCube(
                GraphicsDevice.GLDevice,
                _texture,
                xOffset,
                yOffset,
                width,
                height,
                cubeMapFace,
                level,
                handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
                elementCount * elementSizeInBytes);
            handle.Free();
        }

        public void SetDataPointerEXT(
            CubeMapFace cubeMapFace,
            int level,
            Rectangle? rect,
            IntPtr data,
            int dataLength)
        {
            if (data == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(data));
            }

            int xOffset, yOffset, width, height;
            if (rect.HasValue)
            {
                xOffset = rect.Value.X;
                yOffset = rect.Value.Y;
                width = rect.Value.Width;
                height = rect.Value.Height;
            }
            else
            {
                xOffset = 0;
                yOffset = 0;
                width = Math.Max(1, Size >> level);
                height = Math.Max(1, Size >> level);
            }

            FNA3D.FNA3D_SetTextureDataCube(
                GraphicsDevice.GLDevice,
                _texture,
                xOffset,
                yOffset,
                width,
                height,
                cubeMapFace,
                level,
                data,
                dataLength);
        }

        public void GetData<T>(
            CubeMapFace cubeMapFace,
            T[] data)
            where T : struct
        {
            GetData(
                cubeMapFace,
                0,
                null,
                data,
                0,
                data.Length);
        }

        public void GetData<T>(
            CubeMapFace cubeMapFace,
            T[] data,
            int startIndex,
            int elementCount)
            where T : struct
        {
            GetData(
                cubeMapFace,
                0,
                null,
                data,
                startIndex,
                elementCount);
        }

        public void GetData<T>(
            CubeMapFace cubeMapFace,
            int level,
            Rectangle? rect,
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
                throw new ArgumentException(
                    "The data passed has a length of " + data.Length +
                    " but " + elementCount + " pixels have been requested.");
            }

            int subX, subY, subW, subH;
            if (rect == null)
            {
                subX = 0;
                subY = 0;
                subW = Size >> level;
                subH = Size >> level;
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
            FNA3D.FNA3D_GetTextureDataCube(
                GraphicsDevice.GLDevice,
                _texture,
                subX,
                subY,
                subW,
                subH,
                cubeMapFace,
                level,
                handle.AddrOfPinnedObject() + (startIndex * elementSizeInBytes),
                elementCount * elementSizeInBytes);
            handle.Free();
        }
    }
}
