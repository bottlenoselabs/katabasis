// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
	[SuppressMessage("ReSharper", "UnusedMemberInSuper.Global", Justification = "TODO: Not used?")]
	public abstract class GameWindow
	{
		private string _title = string.Empty;

		[DefaultValue(false)]
		public abstract bool AllowUserResizing { get; set; }

		public abstract Rectangle ClientBounds { get; }

		public abstract DisplayOrientation CurrentOrientation { get; internal set; }

		public abstract IntPtr Handle { get; }

		public abstract string ScreenDeviceName { get; }

		public string Title
		{
			get => _title;
			set
			{
				if (_title != value)
				{
					SetTitle(value);
					_title = value;
				}
			}
		}

		public virtual bool IsBorderlessEXT
		{
			get => false;
			set => throw new NotImplementedException();
		}

		public event EventHandler<EventArgs>? ClientSizeChanged;

		public event EventHandler<EventArgs>? OrientationChanged;

		public event EventHandler<EventArgs>? ScreenDeviceNameChanged;

		public abstract void BeginScreenDeviceChange(bool willBeFullScreen);

		public abstract void EndScreenDeviceChange(
			string screenDeviceName,
			int clientWidth,
			int clientHeight);

		public void EndScreenDeviceChange(string screenDeviceName) =>
			EndScreenDeviceChange(
				screenDeviceName,
				ClientBounds.Width,
				ClientBounds.Height);

		protected internal abstract void SetSupportedOrientations(DisplayOrientation orientations);

		protected void OnActivated()
		{
		}

		protected void OnClientSizeChanged() => ClientSizeChanged?.Invoke(this, EventArgs.Empty);

		protected void OnDeactivated()
		{
		}

		protected void OnOrientationChanged() => OrientationChanged?.Invoke(this, EventArgs.Empty);

		protected void OnPaint()
		{
		}

		protected void OnScreenDeviceNameChanged() => ScreenDeviceNameChanged?.Invoke(this, EventArgs.Empty);

		protected abstract void SetTitle(string title);
	}
}
