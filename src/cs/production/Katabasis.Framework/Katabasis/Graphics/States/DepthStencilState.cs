// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
namespace Katabasis
{
	public class DepthStencilState : GraphicsResource
	{
		public static readonly DepthStencilState Default = new(
			"DepthStencilState.Default",
			true,
			true);

		public static readonly DepthStencilState DepthRead = new(
			"DepthStencilState.DepthRead",
			true,
			false);

		public static readonly DepthStencilState None = new(
			"DepthStencilState.None",
			false,
			false);

		internal FNA3D.FNA3D_DepthStencilState _state;

		public DepthStencilState()
		{
			DepthBufferEnable = true;
			DepthBufferWriteEnable = true;
			DepthBufferFunction = CompareFunction.LessEqual;
			StencilEnable = false;
			StencilFunction = CompareFunction.Always;
			StencilPass = StencilOperation.Keep;
			StencilFail = StencilOperation.Keep;
			StencilDepthBufferFail = StencilOperation.Keep;
			TwoSidedStencilMode = false;
			CounterClockwiseStencilFunction = CompareFunction.Always;
			CounterClockwiseStencilFail = StencilOperation.Keep;
			CounterClockwiseStencilPass = StencilOperation.Keep;
			CounterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
			StencilMask = int.MaxValue;
			StencilWriteMask = int.MaxValue;
			ReferenceStencil = 0;
		}

		private DepthStencilState(
			string name,
			bool depthBufferEnable,
			bool depthBufferWriteEnable)
			: this()
		{
			Name = name;
			DepthBufferEnable = depthBufferEnable;
			DepthBufferWriteEnable = depthBufferWriteEnable;
		}

		public bool DepthBufferEnable
		{
			get => _state.depthBufferEnable == 1;
			set => _state.depthBufferEnable = (byte)(value ? 1 : 0);
		}

		public bool DepthBufferWriteEnable
		{
			get => _state.depthBufferWriteEnable == 1;
			set => _state.depthBufferWriteEnable = (byte)(value ? 1 : 0);
		}

		public StencilOperation CounterClockwiseStencilDepthBufferFail
		{
			get => (StencilOperation)_state.ccwStencilDepthBufferFail;
			set => _state.ccwStencilDepthBufferFail = (FNA3D.FNA3D_StencilOperation)value;
		}

		public StencilOperation CounterClockwiseStencilFail
		{
			get => (StencilOperation)_state.ccwStencilFail;
			set => _state.ccwStencilFail = (FNA3D.FNA3D_StencilOperation)value;
		}

		public CompareFunction CounterClockwiseStencilFunction
		{
			get => (CompareFunction)_state.ccwStencilFunction;
			set => _state.ccwStencilFunction = (FNA3D.FNA3D_CompareFunction)value;
		}

		public StencilOperation CounterClockwiseStencilPass
		{
			get => (StencilOperation)_state.ccwStencilPass;
			set => _state.ccwStencilPass = (FNA3D.FNA3D_StencilOperation)value;
		}

		public CompareFunction DepthBufferFunction
		{
			get => (CompareFunction)_state.depthBufferFunction;
			set => _state.depthBufferFunction = (FNA3D.FNA3D_CompareFunction)value;
		}

		public int ReferenceStencil
		{
			get => _state.referenceStencil;
			set => _state.referenceStencil = value;
		}

		public StencilOperation StencilDepthBufferFail
		{
			get => (StencilOperation)_state.stencilDepthBufferFail;
			set => _state.stencilDepthBufferFail = (FNA3D.FNA3D_StencilOperation)value;
		}

		public bool StencilEnable
		{
			get => _state.stencilEnable == 1;
			set => _state.stencilEnable = (byte)(value ? 1 : 0);
		}

		public StencilOperation StencilFail
		{
			get => (StencilOperation)_state.stencilFail;
			set => _state.stencilFail = (FNA3D.FNA3D_StencilOperation)value;
		}

		public CompareFunction StencilFunction
		{
			get => (CompareFunction)_state.stencilFunction;
			set => _state.stencilFunction = (FNA3D.FNA3D_CompareFunction)value;
		}

		public int StencilMask
		{
			get => _state.stencilMask;
			set => _state.stencilMask = value;
		}

		public StencilOperation StencilPass
		{
			get => (StencilOperation)_state.stencilPass;
			set => _state.stencilPass = (FNA3D.FNA3D_StencilOperation)value;
		}

		public int StencilWriteMask
		{
			get => _state.stencilWriteMask;
			set => _state.stencilWriteMask = value;
		}

		public bool TwoSidedStencilMode
		{
			get => _state.twoSidedStencilMode == 1;
			set => _state.twoSidedStencilMode = (byte)(value ? 1 : 0);
		}
	}
}
