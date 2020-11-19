#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System.Numerics;

namespace Microsoft.Xna.Framework.Content
{
	class MatrixReader : ContentTypeReader<Matrix4x4>
	{
		#region Protected Read Method

		protected internal override Matrix4x4 Read(
			ContentReader input,
			Matrix4x4 existingInstance
		) {
			// 4x4 matrix
			return new Matrix4x4(
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle(),
				input.ReadSingle()
			);
		}

		#endregion
	}
}
