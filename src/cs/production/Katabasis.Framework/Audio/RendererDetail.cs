// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[Serializable]
	[PublicAPI]
	public struct RendererDetail
	{
		public string FriendlyName { get; private set; }

		public string RendererId { get; private set; }

		internal RendererDetail(string name, string id)
			: this()
		{
			FriendlyName = name;
			RendererId = id;
		}

		public override bool Equals(object? obj) => obj is RendererDetail detail && RendererId.Equals(detail.RendererId, StringComparison.Ordinal);

		[SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
		public override int GetHashCode() => RendererId.GetHashCode();

		public override string ToString() => FriendlyName;

		public static bool operator ==(RendererDetail left, RendererDetail right) => left.RendererId.Equals(right.RendererId, StringComparison.Ordinal);

		public static bool operator !=(RendererDetail left, RendererDetail right) => !left.RendererId.Equals(right.RendererId, StringComparison.Ordinal);
	}
}
