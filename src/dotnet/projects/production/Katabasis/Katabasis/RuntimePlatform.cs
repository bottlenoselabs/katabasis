// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
    /// <summary>
    ///     Defines the runtime platforms.
    /// </summary>
    [Flags]
    [SuppressMessage("ReSharper", "MemberCanBeInternal", Justification = "Public API.")]
    public enum RuntimePlatform
    {
        /// <summary>
        ///     Unknown target platform.
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Desktop versions of Windows on 32-bit or 64-bit computing architecture. Includes Windows 7, Windows 8.1,
        ///     Windows 10, and up.
        /// </summary>
        Windows = 1 << 0,

        /// <summary>
        ///     Desktop versions of macOS on 64-bit computing architecture. Includes macOS 10.9 and up.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Product name.")]
        [SuppressMessage("ReSharper", "SA1300", Justification = "Product name.")]
        macOS = 1 << 1,

        /// <summary>
        ///     Desktop distributions of Linux on 64-bit computing architecture. Includes, but is not limited to,
        ///     CentOS, Debian, Fedora, and Ubuntu.
        /// </summary>
        Linux = 1 << 2,

        /// <summary>
        ///     Mobile versions of Android on 64-bit computing architecture. Includes Android 5.x and up.
        /// </summary>
        Android = 1 << 3,

        /// <summary>
        ///     Mobile versions of iOS on 64-bit computing architecture. Includes iOS 11.x and up.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Product name.")]
        [SuppressMessage("ReSharper", "SA1300", Justification = "Product name.")]
        iOS = 1 << 4,

        // TODO: tvOS, RaspberryPi, WebAssembly, PlayStation4, PlayStationVita, Switch etc
    }
}
