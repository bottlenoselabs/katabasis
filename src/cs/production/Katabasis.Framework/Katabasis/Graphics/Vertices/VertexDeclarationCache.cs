// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	internal static class VertexDeclarationCache<T>
		where T : struct, IVertexType
	{
		[SuppressMessage("ReSharper", "StaticMemberInGenericType", Justification = "Cache for the generic type.")]
		private static VertexDeclaration? _cached;

		public static VertexDeclaration VertexDeclaration => _cached ??= VertexDeclaration.FromType(typeof(T));
	}
}
