// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
    public class RenderTarget2D : Texture2D, IRenderTarget
    {
        private readonly IntPtr _glDepthStencilBuffer;
        private readonly IntPtr _glColorBuffer;

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

        public DepthFormat DepthStencilFormat { get; }

        public int MultiSampleCount { get; }

        public RenderTargetUsage RenderTargetUsage { get; }

        public bool IsContentLost => false;

        /// <inheritdoc />
        IntPtr IRenderTarget.DepthStencilBuffer => _glDepthStencilBuffer;

        /// <inheritdoc />
        IntPtr IRenderTarget.ColorBuffer => _glColorBuffer;

        public RenderTarget2D(int width, int height)
            : this(
                width,
                height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None,
                0,
                RenderTargetUsage.DiscardContents)
        {
        }

        public RenderTarget2D(
            int width,
            int height,
            bool mipMap,
            SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat)
            : this(
                width,
                height,
                mipMap,
                preferredFormat,
                preferredDepthFormat,
                0,
                RenderTargetUsage.DiscardContents)
        {
        }

        public RenderTarget2D(
            int width,
            int height,
            bool mipMap,
            SurfaceFormat preferredFormat,
            DepthFormat preferredDepthFormat,
            int preferredMultiSampleCount,
            RenderTargetUsage usage)
            : base(
                width,
                height,
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
                    Width,
                    Height,
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
                Width,
                Height,
                DepthStencilFormat,
                MultiSampleCount);
        }
    }
}
