// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public class RenderTargetCube : TextureCube, IRenderTarget
    {
        private readonly IntPtr _glDepthStencilBuffer;
        private readonly IntPtr _glColorBuffer;

        public DepthFormat DepthStencilFormat { get; }

        public int MultiSampleCount { get; }

        public RenderTargetUsage RenderTargetUsage { get; }

        int IRenderTarget.Width => Size;

        int IRenderTarget.Height => Size;

        IntPtr IRenderTarget.DepthStencilBuffer => _glDepthStencilBuffer;

        IntPtr IRenderTarget.ColorBuffer => _glColorBuffer;

        public bool IsContentLost => false;

#pragma warning disable 0067
        // We never lose data, but lol XNA4 compliance -flibit
        public event EventHandler<EventArgs>? ContentLost;
#pragma warning restore 0067

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (_glColorBuffer != IntPtr.Zero)
                {
                    FNA3D.FNA3D_AddDisposeRenderbuffer(GraphicsDevice.GLDevice, _glColorBuffer);
                }

                if (_glDepthStencilBuffer != IntPtr.Zero)
                {
                    FNA3D.FNA3D_AddDisposeRenderbuffer(GraphicsDevice.GLDevice, _glDepthStencilBuffer);
                }
            }

            base.Dispose(disposing);
        }

        public RenderTargetCube(
            int size,
            bool mipMap,
            SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat)
            : this(
                size,
                mipMap,
                preferredFormat,
                preferredDepthFormat,
                0,
                RenderTargetUsage.DiscardContents)
        {
        }

        public RenderTargetCube(
            int size,
            bool mipMap,
            SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount,
            RenderTargetUsage usage)
            : base(
                size,
                mipMap,
                preferredFormat)
        {
            var graphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;
            DepthStencilFormat = preferredDepthFormat;
            MultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
                graphicsDevice.GLDevice,
                Format,
                MathHelper.ClosestMSAAPower(preferredMultiSampleCount));
            RenderTargetUsage = usage;

            if (MultiSampleCount > 0)
            {
                _glColorBuffer = FNA3D.FNA3D_GenColorRenderbuffer(
                    graphicsDevice.GLDevice,
                    Size,
                    Size,
                    Format,
                    MultiSampleCount,
                    _texture);
            }

            // If we don't need a depth buffer then we're done.
            if (DepthStencilFormat == DepthFormat.None)
            {
                return;
            }

            _glDepthStencilBuffer = FNA3D.FNA3D_GenDepthStencilRenderbuffer(
                graphicsDevice.GLDevice,
                Size,
                Size,
                DepthStencilFormat,
                MultiSampleCount);
        }
    }
}
