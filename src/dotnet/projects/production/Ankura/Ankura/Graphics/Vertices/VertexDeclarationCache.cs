// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Ankura
{
    internal static class VertexDeclarationCache<T>
        where T : struct, IVertexType
    {
        private static VertexDeclaration? _cached;

        public static VertexDeclaration? VertexDeclaration => _cached ??= VertexDeclaration.FromType(typeof(T));
    }
}
