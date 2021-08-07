// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Numerics;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.packedvector.ipackedvector.aspx
	public interface IPackedVector
	{
		void PackFromVector4(Vector4 vector);

		Vector4 ToVector4();
	}

	// PackedVector Generic interface
	// http://msdn.microsoft.com/en-us/library/bb197661.aspx
	public interface IPackedVector<TPacked> : IPackedVector
	{
		TPacked PackedValue { get; set; }
	}
}
