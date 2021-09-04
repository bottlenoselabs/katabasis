// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
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
			get => (CullMode)_state.cullMode;
			set => _state.cullMode = (FNA3D.FNA3D_CullMode)value;
		}

		public float DepthBias
		{
			get => _state.depthBias;
			set => _state.depthBias = value;
		}

		public FillMode FillMode
		{
			get => (FillMode)_state.fillMode;
			set => _state.fillMode = (FNA3D.FNA3D_FillMode)value;
		}

		public bool MultiSampleAntiAlias
		{
			get => _state.multiSampleAntiAlias == 1;
			set => _state.multiSampleAntiAlias = (byte)(value ? 1 : 0);
		}

		public bool ScissorTestEnable
		{
			get => _state.scissorTestEnable == 1;
			set => _state.scissorTestEnable = (byte)(value ? 1 : 0);
		}

		public float SlopeScaleDepthBias
		{
			get => _state.slopeScaleDepthBias;
			set => _state.slopeScaleDepthBias = value;
		}
	}
}
