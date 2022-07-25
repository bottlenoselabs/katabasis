// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace bottlenoselabs.Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	public sealed class StorageDevice
	{
		private static readonly string StorageRoot = FNAPlatform.GetStorageRoot();
		private static readonly DriveInfo? Drive = FNAPlatform.GetDriveInfo(StorageRoot);

		private readonly PlayerIndex? _devicePlayer;

		internal StorageDevice(PlayerIndex? player) => _devicePlayer = player;

		public long FreeSpace
		{
			get
			{
				try
				{
					// null drive means the OS denied info, so we get to guess!
					return Drive?.AvailableFreeSpace ?? long.MaxValue;
				}
				catch (Exception e)
				{
					// Storage root was invalid or unavailable.
					throw new StorageDeviceNotConnectedException(
						"The storage device bound to the container is not connected.",
						e);
				}
			}
		}

		/// <summary>
		///     Returns true if this StorageDevice path is accessible, false otherwise.
		/// </summary>
		public bool IsConnected
		{
			get
			{
				try
				{
					// null drive means the OS denied info, so we get to guess!
					if (Drive == null)
					{
						return true;
					}

					return Drive.IsReady;
				}
				catch
				{
					// The storageRoot path is invalid / has been removed.
					return false;
				}
			}
		}

		/// <summary>
		///     Returns the total size of device.
		/// </summary>
		public long TotalSpace
		{
			get
			{
				try
				{
					// null drive means the OS denied info, so we get to guess!
					if (Drive == null)
					{
						return long.MaxValue;
					}

					return Drive.TotalSize;
				}
				catch (Exception e)
				{
					// Storage root was invalid or unavailable.
					throw new StorageDeviceNotConnectedException(
						"The storage device bound to the container is not connected.",
						e);
				}
			}
		}

		public void DeleteContainer(string titleName) => throw new NotImplementedException();

		/// <summary>
		///     Fired when a device is removed or inserted.
		/// </summary>
		public static event EventHandler<EventArgs>? DeviceChanged;

		[SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "TODO: Remove")]
		private void OnDeviceChanged() => DeviceChanged?.Invoke(this, EventArgs.Empty);

		public IAsyncResult BeginOpenContainer(
			string displayName,
			AsyncCallback? callback,
			object? state)
		{
			IAsyncResult result = new OpenContainerLie(state, displayName);
			callback?.Invoke(result);

			return result;
		}

		public StorageContainer EndOpenContainer(IAsyncResult result)
		{
			var openContainerLie = result as OpenContainerLie;
			return new StorageContainer(
				this,
				openContainerLie!.DisplayName,
				StorageRoot,
				_devicePlayer);
		}

		public static IAsyncResult BeginShowSelector(
			AsyncCallback callback,
			object state) =>
			BeginShowSelector(
				0,
				0,
				callback,
				state);

		public static IAsyncResult BeginShowSelector(
			PlayerIndex player,
			AsyncCallback? callback,
			object? state) =>
			BeginShowSelector(
				player,
				0,
				0,
				callback,
				state);

		public static IAsyncResult BeginShowSelector(
			int sizeInBytes,
			int directoryCount,
			AsyncCallback? callback,
			object? state)
		{
			IAsyncResult result = new ShowSelectorLie(state, null);
			callback?.Invoke(result);

			return result;
		}

		public static IAsyncResult BeginShowSelector(
			PlayerIndex player,
			int sizeInBytes,
			int directoryCount,
			AsyncCallback? callback,
			object? state)
		{
			IAsyncResult result = new ShowSelectorLie(state, player);
			callback?.Invoke(result);

			return result;
		}

		public static StorageDevice EndShowSelector(IAsyncResult result)
		{
			var showSelectorLie = result as ShowSelectorLie;
			return new StorageDevice(showSelectorLie!.PlayerIndex);
		}

		private class NotAsyncLie : IAsyncResult
		{
			protected NotAsyncLie(object? state)
			{
				AsyncState = state;
				AsyncWaitHandle = new ManualResetEvent(true);
			}

			public object? AsyncState { get; }

			public bool CompletedSynchronously => true;

			public bool IsCompleted => true;

			public WaitHandle AsyncWaitHandle { get; }
		}

		private class ShowSelectorLie : NotAsyncLie
		{
			public readonly PlayerIndex? PlayerIndex;

			public ShowSelectorLie(object? state, PlayerIndex? playerIndex)
				: base(state) =>
				PlayerIndex = playerIndex;
		}

		private class OpenContainerLie : NotAsyncLie
		{
			public readonly string DisplayName;

			public OpenContainerLie(object? state, string displayName)
				: base(state) =>
				DisplayName = displayName;
		}
	}
}
