// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System.Collections;
using System.Collections.Generic;

namespace Ankura
{
    public class DisplayModeCollection : IEnumerable<DisplayMode>, IEnumerable
    {
        private readonly List<DisplayMode> _modes;

        internal DisplayModeCollection(List<DisplayMode> setModes)
        {
            _modes = setModes;
        }

        public IEnumerable<DisplayMode> this[SurfaceFormat format]
        {
            get
            {
                var list = new List<DisplayMode>();
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (DisplayMode mode in _modes)
                {
                    if (mode.Format == format)
                    {
                        list.Add(mode);
                    }
                }

                return list;
            }
        }

        public IEnumerator<DisplayMode> GetEnumerator()
        {
            return _modes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _modes.GetEnumerator();
        }
    }
}
