// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.IO;

namespace Katabasis
{
	internal class Resources
	{
		private static byte[]? _spriteEffect;
		private static byte[]? _yuvToRGBAEffect;

		public static byte[] SpriteEffect => _spriteEffect ??= GetResource("SpriteEffect");

		/* This Effect is used by the Xiph VideoPlayer. */
		public static byte[] YUVToRGBAEffect => _yuvToRGBAEffect ??= GetResource("YUVToRGBAEffect");

		private static byte[] GetResource(string name)
		{
			var stream = typeof(Resources).Assembly.GetManifestResourceStream(
				$"Katabasis.Graphics.Effect.StockEffects.FXB.{name}.fxb");

			using MemoryStream ms = new();
			stream!.CopyTo(ms);
			return ms.ToArray();
		}
	}
}
