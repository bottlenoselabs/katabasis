// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Not used?")]
	public sealed class Song : IEquatable<Song>, IDisposable
	{
		internal string _handle;

		internal Song(string fileName, string? name = null)
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(fileName);
			}

			_handle = fileName;
			Name = name ?? string.Empty;
			IsDisposed = false;
		}

		internal Song(string fileName, int durationMS)
			: this(fileName) =>
			Duration = TimeSpan.FromMilliseconds(durationMS);

		public bool IsDisposed { get; private set; }

		public string Name { get; }

		public TimeSpan Duration { get; internal set; }

		public bool IsProtected => false;

		public bool IsRated => false;

		public int PlayCount { get; internal set; }

		public int Rating => 0;

		public int TrackNumber => 0;

		public void Dispose()
		{
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}

		public bool Equals(Song? other) => !ReferenceEquals(other, null) && _handle == other._handle;

		public static Song FromUri(string name, Uri uri)
		{
			string path;
			if (uri.IsAbsoluteUri)
			{
				// If it's absolute, be sure we can actually get it...
				if (!uri.IsFile)
				{
					throw new InvalidOperationException("Only local file URIs are supported for now");
				}

				path = uri.LocalPath;
			}
			else
			{
				path = Path.Combine(
					TitleLocation.Path,
					uri.ToString());
			}

			return new Song(path, name);
		}

		~Song() => Dispose();

		public override bool Equals(object? obj) => obj != null && Equals(obj as Song);

		public static bool operator ==(Song? song1, Song? song2) => song1?.Equals(song2) ?? ReferenceEquals(song2, null);

		public static bool operator !=(Song? song1, Song? song2) => !(song1 == song2);

		public override int GetHashCode() => RuntimeHelpers.GetHashCode(this);
	}
}
