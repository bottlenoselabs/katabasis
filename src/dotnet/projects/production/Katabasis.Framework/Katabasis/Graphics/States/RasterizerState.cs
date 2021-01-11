// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public class RasterizerState : GraphicsResource
	{
		public static readonly RasterizerState CullClockwise = new(
			"RasterizerState.CullClockwise",
			CullMode.CullClockwiseFace);

		public static readonly RasterizerState CullCounterClockwise = new(
			"RasterizerState.CullCounterClockwise",
			CullMode.CullCounterClockwiseFace);

		public static readonly RasterizerState CullNone = new(
			"RasterizerState.CullNone",
			CullMode.None);

		internal FNA3D.FNA3D_RasterizerState _state;

		public RasterizerState()
		{
			CullMode = CullMode.CullCounterClockwiseFace;
			FillMode = FillMode.Solid;
			DepthBias = 0;
			MultiSampleAntiAlias = true;
			ScissorTestEnable = false;
			SlopeScaleDepthBias = 0;
		}

		private RasterizerState(
			string name,
			CullMode cullMode)
			: this()
		{
			Name = name;
			CullMode = cullMode;
		}

		public CullMode CullMode
		{
			get => _state.CullMode;
			set => _state.CullMode = value;
		}

		public float DepthBias
		{
			get => _state.DepthBias;
			set => _state.DepthBias = value;
		}

		public FillMode FillMode
		{
			get => _state.FillMode;
			set => _state.FillMode = value;
		}

		public bool MultiSampleAntiAlias
		{
			get => _state.MultiSampleAntiAlias == 1;
			set => _state.MultiSampleAntiAlias = value ? 1 : 0;
		}

		public bool ScissorTestEnable
		{
			get => _state.ScissorTestEnable == 1;
			set => _state.ScissorTestEnable = value ? 1 : 0;
		}

		public float SlopeScaleDepthBias
		{
			get => _state.SlopeScaleDepthBias;
			set => _state.SlopeScaleDepthBias = value;
		}
	}
}
