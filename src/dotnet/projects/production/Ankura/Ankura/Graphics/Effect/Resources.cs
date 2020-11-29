// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.IO;

namespace Ankura
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
                $"Microsoft.Xna.Framework.Graphics.Effect.StockEffects.FXB.{name}.fxb");
            using MemoryStream ms = new MemoryStream();
            stream!.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
