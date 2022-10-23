// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System.IO;
using System.Resources;

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
			var assembly = typeof(Resources).Assembly;
			var resourceName = $"bottlenoselabs.Katabasis.Graphics.Effect.{name}.fxb";
			var stream = assembly.GetManifestResourceStream(resourceName);

			if (stream == null)
			{
				var resources = assembly.GetManifestResourceNames();
				var exceptionMessage =
					$"Could not find the resource '{resourceName}'. The following resources do exist: '{string.Join("','", resources)}'";
				throw new MissingManifestResourceException(exceptionMessage);
			}

			using MemoryStream ms = new();
			stream!.CopyTo(ms);
			return ms.ToArray();
		}
	}
}
