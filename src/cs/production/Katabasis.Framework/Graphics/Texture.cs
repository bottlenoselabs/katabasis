// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Native naming conventions.")]
	public abstract unsafe class Texture : GraphicsResource
	{
		internal IntPtr _texture;

		public SurfaceFormat Format { get; protected set; }

		public int LevelCount { get; protected set; }

		internal static int CalculateMipLevels(
			int width,
			int height = 0,
			int depth = 0)
		{
			var levels = 1;
			for (var size = Math.Max(Math.Max(width, height), depth);
				size > 1;
				levels += 1)
			{
				size /= 2;
			}

			return levels;
		}

		internal static int CalculateDDSLevelSize(int width, int height, SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.ColorBgraEXT:
					return ((width * 32) + 7) / 8 * height;
				case SurfaceFormat.HalfVector4:
					return ((width * 64) + 7) / 8 * height;
				case SurfaceFormat.Vector4:
					return ((width * 128) + 7) / 8 * height;
				default:
				{
					var blockSize = 16;
					if (format == SurfaceFormat.Dxt1)
					{
						blockSize = 8;
					}

					width = Math.Max(width, 1);
					height = Math.Max(height, 1);
					return (width + 3) / 4 *
					       ((height + 3) / 4) *
					       blockSize;
				}
			}
		}

		// DDS loading extension, based on MojoDDS
		internal static void ParseDDS(
			BinaryReader reader,
			out SurfaceFormat format,
			out int width,
			out int height,
			out int levels,
			out bool isCube)
		{
			// A whole bunch of magic numbers, yay DDS!
			const uint DDS_MAGIC = 0x20534444;
			// ReSharper disable IdentifierTypo
			const uint DDS_HEADERSIZE = 124;
			const uint DDS_PIXFMTSIZE = 32;
			const uint DDSD_CAPS = 0x1;
			const uint DDSD_HEIGHT = 0x2;
			const uint DDSD_WIDTH = 0x4;
			const uint DDSD_PITCH = 0x8;
			const uint DDSD_FMT = 0x1000;
			const uint DDSD_LINEARSIZE = 0x80000;
			const uint DDSD_REQ = DDSD_CAPS | DDSD_HEIGHT | DDSD_WIDTH | DDSD_FMT;
			const uint DDSCAPS_MIPMAP = 0x400000;
			const uint DDSCAPS_TEXTURE = 0x1000;
			const uint DDSCAPS2_CUBEMAP = 0x200;
			const uint DDPF_FOURCC = 0x4;
			const uint DDPF_RGB = 0x40;
			// ReSharper restore IdentifierTypo
			const uint FOURCC_DXT1 = 0x31545844;
			const uint FOURCC_DXT3 = 0x33545844;
			const uint FOURCC_DXT5 = 0x35545844;
			const uint FOURCC_DX10 = 0x30315844;
			const uint pitchAndLinear = DDSD_PITCH | DDSD_LINEARSIZE;

			// File should start with 'DDS '
			if (reader.ReadUInt32() != DDS_MAGIC)
			{
				throw new NotSupportedException("Not a DDS!");
			}

			// Texture info
			var size = reader.ReadUInt32();
			if (size != DDS_HEADERSIZE)
			{
				throw new NotSupportedException("Invalid DDS header!");
			}

			var flags = reader.ReadUInt32();
			if ((flags & DDSD_REQ) != DDSD_REQ)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}

			if ((flags & pitchAndLinear) == pitchAndLinear)
			{
				throw new NotSupportedException("Invalid DDS flags!");
			}

			height = reader.ReadInt32();
			width = reader.ReadInt32();
			reader.ReadUInt32(); // dwPitchOrLinearSize, unused
			reader.ReadUInt32(); // dwDepth, unused
			levels = reader.ReadInt32();

			// "Reserved"
			reader.ReadBytes(4 * 11);

			// Format info
			var formatSize = reader.ReadUInt32();
			if (formatSize != DDS_PIXFMTSIZE)
			{
				// ReSharper disable once StringLiteralTypo
				throw new NotSupportedException("Bogus PIXFMTSIZE!");
			}

			var formatFlags = reader.ReadUInt32();
			var formatFourCC = reader.ReadUInt32();
			var formatRGBBitCount = reader.ReadUInt32();
			var formatRBitMask = reader.ReadUInt32();
			var formatGBitMask = reader.ReadUInt32();
			var formatBBitMask = reader.ReadUInt32();
			var formatABitMask = reader.ReadUInt32();

			// dwCaps "stuff"
			var caps = reader.ReadUInt32();
			if ((caps & DDSCAPS_TEXTURE) == 0)
			{
				throw new NotSupportedException("Not a texture!");
			}

			isCube = false;

			var caps2 = reader.ReadUInt32();
			if (caps2 != 0)
			{
				if ((caps2 & DDSCAPS2_CUBEMAP) == DDSCAPS2_CUBEMAP)
				{
					isCube = true;
				}
				else
				{
					throw new NotSupportedException("Invalid caps2!");
				}
			}

			reader.ReadUInt32(); // dwCaps3, unused
			reader.ReadUInt32(); // dwCaps4, unused

			// "Reserved"
			reader.ReadUInt32();

			// Mipmap sanity check
			if ((caps & DDSCAPS_MIPMAP) != DDSCAPS_MIPMAP)
			{
				levels = 1;
			}

			// Determine texture format
			if ((formatFlags & DDPF_FOURCC) == DDPF_FOURCC)
			{
				format = formatFourCC switch
				{
					// D3DFMT_A16B16G16R16F
					0x71 => SurfaceFormat.HalfVector4,
					// D3DFMT_A32B32G32R32F
					0x74 => SurfaceFormat.Vector4,
					FOURCC_DXT1 => SurfaceFormat.Dxt1,
					FOURCC_DXT3 => SurfaceFormat.Dxt3,
					FOURCC_DXT5 => SurfaceFormat.Dxt5,
					DDPF_FOURCC => SurfaceFormat.Bc7EXT,
					FOURCC_DX10 => SurfaceFormatDX10(reader),
					_ => throw new NotSupportedException("Unsupported DDS texture format")
				};
			}
			else if ((formatFlags & DDPF_RGB) == DDPF_RGB)
			{
				if (formatRGBBitCount != 32 ||
				    formatRBitMask != 0x00FF0000 ||
				    formatGBitMask != 0x0000FF00 ||
				    formatBBitMask != 0x000000FF ||
				    formatABitMask != 0xFF000000)
				{
					throw new NotSupportedException("Unsupported DDS texture format");
				}

				format = SurfaceFormat.ColorBgraEXT;
			}
			else
			{
				throw new NotSupportedException("Unsupported DDS texture format");
			}
		}

		private static SurfaceFormat SurfaceFormatDX10(BinaryReader reader)
		{
			// If the fourCC is DX10, there is an extra header with additional format information.
			var dxgiFormat = reader.ReadUInt32();

			// These values are taken from the DXGI_FORMAT enum.
			var result = dxgiFormat switch
			{
				2 => SurfaceFormat.Vector4,
				10 => SurfaceFormat.HalfVector4,
				71 => SurfaceFormat.Dxt1,
				74 => SurfaceFormat.Dxt3,
				77 => SurfaceFormat.Dxt5,
				98 => SurfaceFormat.Bc7EXT,
				99 => SurfaceFormat.Bc7SrgbEXT,
				_ => throw new NotSupportedException("Unsupported DDS texture format")
			};

			var resourceDimension = reader.ReadUInt32();
			// These values are taken from the D3D10_RESOURCE_DIMENSION enum.
			switch (resourceDimension)
			{
				case 0: // Unknown
				case 1: // Buffer
					throw new NotSupportedException("Unsupported DDS texture format");
			}

			/*
			  * This flag seemingly only indicates if the texture is a cube map.
			  * This is already determined above. Cool!
			  */
			var miscFlag = reader.ReadUInt32();

			/*
			  * Indicates the number of elements in the texture array.
			  * We don't support texture arrays so just throw if it's greater than 1.
			  */
			var arraySize = reader.ReadUInt32();
			if (arraySize > 1)
			{
				throw new NotSupportedException("Unsupported DDS texture format");
			}

			reader.ReadUInt32(); // reserved

			return result;
		}

		protected static int GetBlockSizeSquared(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.Dxt5SrgbEXT:
				case SurfaceFormat.Bc7EXT:
				case SurfaceFormat.Bc7SrgbEXT:
					return 16;
				case SurfaceFormat.Alpha8:
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
				case SurfaceFormat.ColorSrgbEXT:
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.HdrBlendable:
				case SurfaceFormat.Vector4:
					return 1;
				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", "Format");
			}
		}

		internal static int GetFormatSize(SurfaceFormat format)
		{
			switch (format)
			{
				case SurfaceFormat.Dxt1:
					return 8;
				case SurfaceFormat.Dxt3:
				case SurfaceFormat.Dxt5:
				case SurfaceFormat.Dxt5SrgbEXT:
				case SurfaceFormat.Bc7EXT:
				case SurfaceFormat.Bc7SrgbEXT:
					return 16;
				case SurfaceFormat.Alpha8:
					return 1;
				case SurfaceFormat.Bgr565:
				case SurfaceFormat.Bgra4444:
				case SurfaceFormat.Bgra5551:
				case SurfaceFormat.HalfSingle:
				case SurfaceFormat.NormalizedByte2:
					return 2;
				case SurfaceFormat.Color:
				case SurfaceFormat.Single:
				case SurfaceFormat.Rg32:
				case SurfaceFormat.HalfVector2:
				case SurfaceFormat.NormalizedByte4:
				case SurfaceFormat.Rgba1010102:
				case SurfaceFormat.ColorBgraEXT:
				case SurfaceFormat.ColorSrgbEXT:
					return 4;
				case SurfaceFormat.HalfVector4:
				case SurfaceFormat.Rgba64:
				case SurfaceFormat.Vector2:
				case SurfaceFormat.HdrBlendable:
					return 8;
				case SurfaceFormat.Vector4:
					return 16;
				default:
					throw new ArgumentException("Should be a value defined in SurfaceFormat", nameof(format));
			}
		}

		internal static int GetPixelStoreAlignment(SurfaceFormat format) =>
			// ReSharper disable once CommentTypo
			/*
	         * https://github.com/FNA-XNA/FNA/pull/238
	         * https://www.khronos.org/registry/OpenGL/specs/gl/glspec21.pdf
	         * OpenGL 2.1 Specification, section 3.6.1, table 3.1 specifies that the "pixelstorei" alignment cannot exceed 8
	         */
			Math.Min(8, GetFormatSize(format));

		internal static void ValidateGetDataFormat(SurfaceFormat format, int elementSizeInBytes)
		{
			if (GetFormatSize(format) % elementSizeInBytes != 0)
			{
				throw new ArgumentException("The type you are using for T in this method is an invalid size for this resource.");
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
				GraphicsDevice.Textures.RemoveDisposedTexture(this);
				GraphicsDevice.VertexTextures.RemoveDisposedTexture(this);
				FNA3D.FNA3D_AddDisposeTexture(GraphicsDevice.Device, (FNA3D.FNA3D_Texture*)_texture);
			}

			base.Dispose(disposing);
		}
	}
}
