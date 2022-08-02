// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public abstract class GraphicsResource : IDisposable
	{
		private GraphicsDevice _graphicsDevice = null!;
		private WeakReference? _selfReference;

		internal GraphicsResource()
		{
		}

		public bool IsDisposed { get; private set; }

		public string Name { get; set; } = string.Empty;

		public object? Tag { get; set; }

		public GraphicsDevice GraphicsDevice
		{
			get => _graphicsDevice;
			internal set
			{
				if (_graphicsDevice == value)
				{
					return;
				}

				/* VertexDeclaration objects can be bound to
				 * multiple GraphicsDevice objects during their
				 * lifetime. But only one GraphicsDevice should
				 * retain ownership.
				 */
				if (_selfReference != null)
				{
					_graphicsDevice.RemoveResourceReference(_selfReference);
					_selfReference = null;
				}

				_graphicsDevice = value;

				_selfReference = new WeakReference(this);
				_graphicsDevice.AddResourceReference(_selfReference);
			}
		}

		public void Dispose()
		{
			// Dispose of unmanaged objects as well
			Dispose(true);

			// Since we have been manually disposed, do not call the finalizer on this object
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

		public override string? ToString() => string.IsNullOrEmpty(Name) ? base.ToString() : Name;

		/// <summary>
		///     Called before the device is reset. Allows graphics resources to
		///     invalidate their state so they can be recreated after the device reset.
		///     Warning: This may be called after a call to Dispose() up until
		///     the resource is garbage collected.
		/// </summary>
		protected internal virtual void GraphicsDeviceResetting()
		{
		}

		/// <summary>
		///     The method that derived classes should override to implement disposing of
		///     managed and native resources.
		/// </summary>
		/// <param name="disposing">True if managed objects should be disposed.</param>
		/// <remarks>
		///     Native resources should always be released regardless of the value of the
		///     disposing parameter.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				// Do not trigger the event if called from the finalizer
				if (disposing)
				{
					Disposing?.Invoke(this, EventArgs.Empty);
				}

				// Remove from the list of graphics resources
				if (_selfReference != null)
				{
					_graphicsDevice.RemoveResourceReference(_selfReference);
					_selfReference = null;
				}

				IsDisposed = true;
			}
		}
	}
}
