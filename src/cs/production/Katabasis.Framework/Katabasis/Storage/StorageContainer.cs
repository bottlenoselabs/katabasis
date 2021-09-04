// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	public class StorageContainer : IDisposable
	{
		private readonly string _storagePath;

		internal StorageContainer(
			StorageDevice device,
			string name,
			string rootPath,
			PlayerIndex? playerIndex)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(nameof(name));
			}

			StorageDevice = device;
			DisplayName = name;

			/* There are two types of subfolders within a StorageContainer.
			 * The first is a PlayerX folder, X being a specified PlayerIndex.
			 * The second is AllPlayers, when PlayerIndex is NOT specified.
			 * Basically, you should NEVER expect to have ANY file in the root
			 * game save folder.
			 * -flibit
			 */
			_storagePath = Path.Combine(
				rootPath, // Title folder (EXE name)...
				name, // Container folder...
				playerIndex.HasValue ? "Player" + ((int)playerIndex.Value + 1) : "AllPlayers");

			// Create the folders, if needed.
			if (!Directory.Exists(_storagePath))
			{
				Directory.CreateDirectory(_storagePath);
			}
		}

		public string DisplayName { get; }

		public bool IsDisposed { get; private set; }

		public StorageDevice StorageDevice { get; }

		public void Dispose()
		{
			Disposing?.Invoke(this, EventArgs.Empty);
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public event EventHandler<EventArgs>? Disposing;

		public void CreateDirectory(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException(nameof(directory));
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(_storagePath, directory);

			// Now let's try to create it.
			if (!Directory.Exists(dirPath))
			{
				Directory.CreateDirectory(dirPath);
			}
		}

		public Stream CreateFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			// File name is relative, so combine with our path.
			string filePath = Path.Combine(_storagePath, file);

			// Return a new file with read/write access.
			return File.Create(filePath);
		}

		public void DeleteDirectory(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException(nameof(directory));
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(_storagePath, directory);

			// Now let's try to delete it.
			Directory.Delete(dirPath);
		}

		public void DeleteFile(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			// Relative, so combine with our path.
			string filePath = Path.Combine(_storagePath, file);

			// Now let's try to delete it.
			File.Delete(filePath);
		}

		public bool DirectoryExists(string directory)
		{
			if (string.IsNullOrEmpty(directory))
			{
				throw new ArgumentNullException(nameof(directory));
			}

			// Directory name is relative, so combine with our path.
			string dirPath = Path.Combine(_storagePath, directory);

			return Directory.Exists(dirPath);
		}

		public bool FileExists(string file)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			// File name is relative, so combine with our path.
			string filePath = Path.Combine(_storagePath, file);

			// Return a new file with read/write access.
			return File.Exists(filePath);
		}

		public string[] GetDirectoryNames()
		{
			string[] names = Directory.GetDirectories(_storagePath);
			for (var i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(_storagePath.Length + 1);
			}

			return names;
		}

		public string[] GetDirectoryNames(string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
			{
				throw new ArgumentNullException(nameof(searchPattern));
			}

			string[] names = Directory.GetDirectories(_storagePath, searchPattern);
			for (var i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(_storagePath.Length + 1);
			}

			return names;
		}

		public string[] GetFileNames()
		{
			string[] names = Directory.GetFiles(_storagePath);
			for (var i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(_storagePath.Length + 1);
			}

			return names;
		}

		public string[] GetFileNames(string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern))
			{
				throw new ArgumentNullException(nameof(searchPattern));
			}

			string[] names = Directory.GetFiles(_storagePath, searchPattern);
			for (var i = 0; i < names.Length; i += 1)
			{
				names[i] = names[i].Substring(_storagePath.Length + 1);
			}

			return names;
		}

		public Stream OpenFile(string file, FileMode fileMode) =>
			OpenFile(
				file,
				fileMode,
				FileAccess.ReadWrite,
				FileShare.ReadWrite);

		public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess) => OpenFile(file, fileMode, fileAccess, FileShare.ReadWrite);

		public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
		{
			if (string.IsNullOrEmpty(file))
			{
				throw new ArgumentNullException(nameof(file));
			}

			// Filename is relative, so combine with our path.
			string filePath = Path.Combine(_storagePath, file);

			return File.Open(filePath, fileMode, fileAccess, fileShare);
		}
	}
}
