// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

namespace Katabasis
{
    public class DepthStencilState : GraphicsResource
    {
        public static readonly DepthStencilState Default = new DepthStencilState(
            "DepthStencilState.Default",
            true,
            true);

        public static readonly DepthStencilState DepthRead = new DepthStencilState(
            "DepthStencilState.DepthRead",
            true,
            false);

        public static readonly DepthStencilState None = new DepthStencilState(
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
            get => _state.DepthBufferEnable == 1;
            set => _state.DepthBufferEnable = (byte)(value ? 1 : 0);
        }

        public bool DepthBufferWriteEnable
        {
            get => _state.DepthBufferWriteEnable == 1;
            set => _state.DepthBufferWriteEnable = (byte)(value ? 1 : 0);
        }

        public StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get => _state.CounterClockwiseStencilDepthBufferFail;
            set => _state.CounterClockwiseStencilDepthBufferFail = value;
        }

        public StencilOperation CounterClockwiseStencilFail
        {
            get => _state.CounterClockwiseStencilFail;
            set => _state.CounterClockwiseStencilFail = value;
        }

        public CompareFunction CounterClockwiseStencilFunction
        {
            get => _state.CounterClockwiseStencilFunction;
            set => _state.CounterClockwiseStencilFunction = value;
        }

        public StencilOperation CounterClockwiseStencilPass
        {
            get => _state.CounterClockwiseStencilPass;
            set => _state.CounterClockwiseStencilPass = value;
        }

        public CompareFunction DepthBufferFunction
        {
            get => _state.DepthBufferFunction;
            set => _state.DepthBufferFunction = value;
        }

        public int ReferenceStencil
        {
            get => _state.ReferenceStencil;
            set => _state.ReferenceStencil = value;
        }

        public StencilOperation StencilDepthBufferFail
        {
            get => _state.StencilDepthBufferFail;
            set => _state.StencilDepthBufferFail = value;
        }

        public bool StencilEnable
        {
            get => _state.StencilEnable == 1;
            set => _state.StencilEnable = (byte)(value ? 1 : 0);
        }

        public StencilOperation StencilFail
        {
            get => _state.StencilFail;
            set => _state.StencilFail = value;
        }

        public CompareFunction StencilFunction
        {
            get => _state.StencilFunction;
            set => _state.StencilFunction = value;
        }

        public int StencilMask
        {
            get => _state.StencilMask;
            set => _state.StencilMask = value;
        }

        public StencilOperation StencilPass
        {
            get => _state.StencilPass;
            set => _state.StencilPass = value;
        }

        public int StencilWriteMask
        {
            get => _state.StencilWriteMask;
            set => _state.StencilWriteMask = value;
        }

        public bool TwoSidedStencilMode
        {
            get => _state.TwoSidedStencilMode == 1;
            set => _state.TwoSidedStencilMode = (byte)(value ? 1 : 0);
        }
    }
}
