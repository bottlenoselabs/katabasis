// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;

namespace Katabasis.ImGui
{
    public static class ImGuiVertexDeclaration
    {
        public static readonly VertexDeclaration Declaration;

        public static readonly int Size;

        static ImGuiVertexDeclaration()
        {
            unsafe
            {
                Size = sizeof(imgui.ImDrawVert);
            }

            if (Size != 20)
            {
                var up = new Exception("Unexpected size for ImGui vertex.");
                throw up;
            }

            var position = new VertexElement(offset: 0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0);
            var textureCoordinates = new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0);
            var color = new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0);

            Declaration = new VertexDeclaration(
                Size,
                position,
                textureCoordinates,
                color);
        }
    }
}
