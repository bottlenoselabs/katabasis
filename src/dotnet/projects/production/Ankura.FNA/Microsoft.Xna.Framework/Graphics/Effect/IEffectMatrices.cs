#region License
/* FNA - XNA4 Reimplementation for Desktop Platforms
 * Copyright 2009-2020 Ethan Lee and the MonoGame Team
 *
 * Released under the Microsoft Public License.
 * See LICENSE for details.
 */
#endregion

using System.Numerics;

namespace Microsoft.Xna.Framework.Graphics
{
	public interface IEffectMatrices
	{
		Matrix4x4 Projection
		{
			get;
			set;
		}

		Matrix4x4 View
		{
			get;
			set;
		}

		Matrix4x4 World
		{
			get;
			set;
		}
	}
}
