// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	public unsafe class GraphicsDeviceManager : IDisposable
	{
		public static readonly int DefaultBackBufferWidth = 800;
		public static readonly int DefaultBackBufferHeight = 480;
		private readonly Game _game;
		private readonly bool _supportsOrientations;
		private bool _changed;
		private bool _disposed;
		private bool _drawBegun;

		private bool _isFullScreen;
		private bool _preferMultiSampling;
		private SurfaceFormat _preferredBackBufferFormat;
		private int _preferredBackBufferHeight;
		private int _preferredBackBufferWidth;
		private DepthFormat _preferredDepthStencilFormat;
		private int _resizedBackBufferHeight;
		private int _resizedBackBufferWidth;
		private DisplayOrientation _supportedOrientations;
		private bool _synchronizeWithVerticalRetrace;
		private bool _useResizedBackBuffer;

		internal GraphicsDeviceManager(Game game)
		{
			if (Instance != null)
			{
				throw new InvalidOperationException("GraphicsDeviceManager already created!");
			}

			_game = game;
			GraphicsDevice = null!;

			_supportedOrientations = DisplayOrientation.Default;

			_preferredBackBufferHeight = DefaultBackBufferHeight;
			_preferredBackBufferWidth = DefaultBackBufferWidth;

			_preferredBackBufferFormat = SurfaceFormat.Color;
			_preferredDepthStencilFormat = DepthFormat.Depth24;

			_synchronizeWithVerticalRetrace = true;

			_preferMultiSampling = false;

			_changed = true;
			_useResizedBackBuffer = false;
			_supportsOrientations = FNAPlatform.SupportsOrientationChanges();
			game.Window.ClientSizeChanged += OnClientSizeChanged;
		}

		public static GraphicsDeviceManager Instance { get; internal set; } = null!;

		public GraphicsProfile GraphicsProfile { get; set; }

		public GraphicsDevice GraphicsDevice { get; private set; }

		public bool IsFullScreen
		{
			get => _isFullScreen;
			set
			{
				_isFullScreen = value;
				_changed = true;
			}
		}

		public bool PreferMultiSampling
		{
			get => _preferMultiSampling;
			set
			{
				_preferMultiSampling = value;
				_changed = true;
			}
		}

		public SurfaceFormat PreferredBackBufferFormat
		{
			get => _preferredBackBufferFormat;
			set
			{
				_preferredBackBufferFormat = value;
				_changed = true;
			}
		}

		public int PreferredBackBufferHeight
		{
			get => _preferredBackBufferHeight;
			set
			{
				_preferredBackBufferHeight = value;
				_changed = true;
			}
		}

		public int PreferredBackBufferWidth
		{
			get => _preferredBackBufferWidth;
			set
			{
				_preferredBackBufferWidth = value;
				_changed = true;
			}
		}

		public DepthFormat PreferredDepthStencilFormat
		{
			get => _preferredDepthStencilFormat;
			set
			{
				_preferredDepthStencilFormat = value;
				_changed = true;
			}
		}

		public bool SynchronizeWithVerticalRetrace
		{
			get => _synchronizeWithVerticalRetrace;
			set
			{
				_synchronizeWithVerticalRetrace = value;
				_changed = true;
			}
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
		public DisplayOrientation SupportedOrientations
		{
			get => _supportedOrientations;
			set
			{
				_supportedOrientations = value;
				_changed = true;
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposed;

		public event EventHandler<EventArgs>? DeviceCreated;

		public event EventHandler<EventArgs>? DeviceDisposing;

		public event EventHandler<EventArgs>? DeviceReset;

		public event EventHandler<EventArgs>? DeviceResetting;

		public event EventHandler<PreparingDeviceSettingsEventArgs>? PreparingDeviceSettings;

		~GraphicsDeviceManager() => Dispose(false);

		public void ApplyChanges()
		{
			/* Calling ApplyChanges() before CreateDevice() forces CreateDevice.
			 * We can then return early since CreateDevice basically does all of
			 * this work for us anyway.
			 *
			 * Note that if you hit this block, it's probably because you called
			 * ApplyChanges in the constructor. The device created here gets
			 * destroyed and recreated by the game, so maybe don't do that!
			 *
			 * -flibit
			 */
			if (GraphicsDevice == null)
			{
				CreateDevice();
				return;
			}

			// ApplyChanges() calls with no actual changes should be ignored.
			if (!_changed && !_useResizedBackBuffer)
			{
				return;
			}

			// Recreate device information before resetting
			GraphicsDeviceInformation gdi = new();
			gdi.Adapter = GraphicsDevice.Adapter;
			gdi.PresentationParameters = GraphicsDevice.PresentationParameters.Clone();
			CreateGraphicsDeviceInformation(gdi);

			// Prepare the window...
			if (_supportsOrientations)
			{
				_game.Window.SetSupportedOrientations(
					_supportedOrientations);
			}

			_game.Window.BeginScreenDeviceChange(gdi.PresentationParameters.IsFullScreen);
			_game.Window.EndScreenDeviceChange(
				gdi.Adapter.DeviceName,
				gdi.PresentationParameters.BackBufferWidth,
				gdi.PresentationParameters.BackBufferHeight);

			// FIXME: Everything below should be before EndScreenDeviceChange! -flibit

			// Reset!
			GraphicsDevice.Reset(
				gdi.PresentationParameters,
				gdi.Adapter);

			_changed = false;
		}

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
		public void ToggleFullScreen()
		{
			IsFullScreen = !IsFullScreen;
			ApplyChanges();
		}

		internal void CreateDevice()
		{
			// This function can recreate the device from scratch!
			GraphicsDevice?.Dispose();

			// Set the default device information
			GraphicsDeviceInformation gdi = new();
			gdi.Adapter = GraphicsAdapter.DefaultAdapter;
			gdi.PresentationParameters = new PresentationParameters();
			gdi.PresentationParameters.DeviceWindowHandle = _game.Window.Handle;
			CreateGraphicsDeviceInformation(gdi);

			// Prepare the window...
			if (_supportsOrientations)
			{
				_game.Window.SetSupportedOrientations(_supportedOrientations);
			}

			_game.Window.BeginScreenDeviceChange(gdi.PresentationParameters.IsFullScreen);
			_game.Window.EndScreenDeviceChange(
				gdi.Adapter.DeviceName,
				gdi.PresentationParameters.BackBufferWidth,
				gdi.PresentationParameters.BackBufferHeight);

			// FIXME: Everything below should be before EndScreenDeviceChange! -flibit

			// Create the GraphicsDevice, hook the callbacks
			GraphicsDevice = new GraphicsDevice(
				gdi.Adapter,
				gdi.GraphicsProfile,
				gdi.PresentationParameters);

			GraphicsDevice.Disposing += OnDeviceDisposing;
			GraphicsDevice.DeviceResetting += OnDeviceResetting;
			GraphicsDevice.DeviceReset += OnDeviceReset;

			// Call the DeviceCreated Event
			OnDeviceCreated(this, EventArgs.Empty);
		}

		internal bool BeginDraw()
		{
			_drawBegun = true;
			return true;
		}

		internal void EndDraw()
		{
			if (_drawBegun)
			{
				_drawBegun = false;
				GraphicsDevice.Present();
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					GraphicsDevice.Dispose();
				}

				Disposed?.Invoke(this, EventArgs.Empty);

				_disposed = true;
			}
		}

		protected virtual void OnDeviceCreated(object? sender, EventArgs args) => DeviceCreated?.Invoke(sender, args);

		protected virtual void OnDeviceDisposing(object? sender, EventArgs args) => DeviceDisposing?.Invoke(this, args);

		protected virtual void OnDeviceReset(object? sender, EventArgs args) => DeviceReset?.Invoke(this, args);

		protected virtual void OnDeviceResetting(object? sender, EventArgs args) => DeviceResetting?.Invoke(this, args);

		protected virtual void OnPreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs args) =>
			PreparingDeviceSettings?.Invoke(sender, args);

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Remove.")]
		protected virtual bool CanResetDevice(GraphicsDeviceInformation newDeviceInfo) => throw new NotImplementedException();

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Remove.")]
		protected virtual GraphicsDeviceInformation FindBestDevice(bool anySuitableDevice) => throw new NotImplementedException();

		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Remove.")]
		protected virtual void RankDevices(List<GraphicsDeviceInformation> foundDevices) => throw new NotImplementedException();

		private void OnClientSizeChanged(object? sender, EventArgs e)
		{
			var window = sender as GameWindow;
			var size = window!.ClientBounds;
			_resizedBackBufferWidth = size.Width;
			_resizedBackBufferHeight = size.Height;

			FNAPlatform.ScaleForWindow(window.Handle, true, ref _resizedBackBufferWidth, ref _resizedBackBufferHeight);

			_useResizedBackBuffer = true;
			ApplyChanges();
		}

		private void CreateGraphicsDeviceInformation(GraphicsDeviceInformation gdi)
		{
			/* Apply the GraphicsDevice changes to the new Parameters.
			 * Note that PreparingDeviceSettings can override any of these!
			 * -flibit
			 */
			if (_useResizedBackBuffer)
			{
				gdi.PresentationParameters!.BackBufferWidth =
					_resizedBackBufferWidth;

				gdi.PresentationParameters.BackBufferHeight =
					_resizedBackBufferHeight;

				_useResizedBackBuffer = false;
			}
			else
			{
				if (!_supportsOrientations)
				{
					gdi.PresentationParameters!.BackBufferWidth =
						PreferredBackBufferWidth;

					gdi.PresentationParameters.BackBufferHeight =
						PreferredBackBufferHeight;
				}
				else
				{
					/* Flip the backbuffer dimensions to scale
					 * appropriately to the current orientation.
					 */
					var min = Math.Min(PreferredBackBufferWidth, PreferredBackBufferHeight);
					var max = Math.Max(PreferredBackBufferWidth, PreferredBackBufferHeight);

					if (gdi.PresentationParameters!.DisplayOrientation == DisplayOrientation.Portrait)
					{
						gdi.PresentationParameters.BackBufferWidth = min;
						gdi.PresentationParameters.BackBufferHeight = max;
					}
					else
					{
						gdi.PresentationParameters.BackBufferWidth = max;
						gdi.PresentationParameters.BackBufferHeight = min;
					}
				}
			}

			gdi.PresentationParameters.BackBufferFormat =
				PreferredBackBufferFormat;

			gdi.PresentationParameters.DepthStencilFormat =
				PreferredDepthStencilFormat;

			gdi.PresentationParameters.IsFullScreen =
				IsFullScreen;

			gdi.PresentationParameters.PresentationInterval =
				SynchronizeWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;

			if (!PreferMultiSampling)
			{
				gdi.PresentationParameters.MultiSampleCount = 0;
			}
			else if (gdi.PresentationParameters.MultiSampleCount == 0)
			{
				/* XNA4 seems to have an upper limit of 8, but I'm willing to
				 * limit this only in GraphicsDeviceManager's default setting.
				 * If you want even higher values, Reset() with a custom value.
				 * -flibit
				 */
				var maxMultiSampleCount = FNA3D.FNA3D_GetMaxMultiSampleCount(
					GraphicsDevice.Device,
					(FNA3D.FNA3D_SurfaceFormat)gdi.PresentationParameters.BackBufferFormat,
					8);

				gdi.PresentationParameters.MultiSampleCount = Math.Min(
					maxMultiSampleCount,
					8);
			}

			gdi.GraphicsProfile = GraphicsProfile;

			// Give the user a chance to override the above settings.
			OnPreparingDeviceSettings(
				this,
				new PreparingDeviceSettingsEventArgs(gdi));
		}
	}
}
