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
                var up = new Exception("Unexpected size for ImGui vertex");
                throw up;
            }

            Declaration = new VertexDeclaration(
                Size,

                // Position
                new VertexElement(
                    0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

                // UV
                new VertexElement(
                    8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                // Color
                new VertexElement(
                    16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        }
    }
}