// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.IO;

namespace bottlenoselabs.Katabasis
{
	internal class Resources
	{
		private static byte[]? _spriteEffect;
		private static byte[]? _yuvToRGBAEffect;

		public static byte[] SpriteEffect => _spriteEffect ??= GetResource("Sprite.SpriteEffect");

		/* This Effect is used by the Xiph VideoPlayer. */
		public static byte[] YUVToRGBAEffect => _yuvToRGBAEffect ??= GetResource("YUVToRGBA.YUVToRGBAEffect");

		private static byte[] GetResource(string name)
		{
			var stream = typeof(Resources).Assembly.GetManifestResourceStream(
				$"bottlenoselabs.Katabasis.Graphics.Effect.{name}.fxb");

			using MemoryStream ms = new();
			stream!.CopyTo(ms);
			return ms.ToArray();
		}
	}
}
