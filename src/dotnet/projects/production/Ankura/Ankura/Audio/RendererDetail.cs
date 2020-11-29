// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [Serializable]
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

        public override bool Equals(object? obj)
        {
            return obj is RendererDetail detail && RendererId.Equals(detail.RendererId);
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode", Justification = "Mutable value type.")]
        public override int GetHashCode()
        {
            return RendererId.GetHashCode();
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public static bool operator ==(RendererDetail left, RendererDetail right)
        {
            return left.RendererId.Equals(right.RendererId);
        }

        public static bool operator !=(RendererDetail left, RendererDetail right)
        {
            return !left.RendererId.Equals(right.RendererId);
        }
    }
}
