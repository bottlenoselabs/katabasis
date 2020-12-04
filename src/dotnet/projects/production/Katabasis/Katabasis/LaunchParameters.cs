// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;

namespace Katabasis
{
    public class LaunchParameters : Dictionary<string, string>
    {
        /* FIXME: This whole parser is one big assumption!
         *
         * I basically started with what MS programs usually accept as
         * arguments, then threw a bunch of values at XNA to see what it
         * accepted and what it didn't.
         *
         * Aside from what you see below, all I could rule out was that
         * it doesn't let you do two args as one param, and '=' is not a
         * valid value separator either. As an example, "-r:FNA.dll"
         * will work, "-r FNA.dll" and "-r=FNA.dll" will not.
         *
         * The part that bothers me the most, however, is the flag
         * indicator. It seems to let anything through as long as : is
         * there, but it trims some special chars, and does so pretty
         * broadly. You can do '-', "--", "---", etc! Lastly, in
         * addition to the chars below, I also tried '+', which didn't
         * work. I have no idea if there are any other chars to check.
         *
         * If anybody has an official standard, I'd like to see it!
         * -flibit
         */

        private static readonly char[] Flags =
        {
            '/', '-'
        };

        public LaunchParameters()
        {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string a in args)
            {
                string arg = a.TrimStart(Flags);

                /* 1 for ':', 1 for key, 1 for value */
                if (arg.Length < 3)
                {
                    continue;
                }

                /* You can have multiple :, only the first matters */
                var valueOffset = arg.IndexOf(":", 1, arg.Length - 2, StringComparison.Ordinal);
                if (valueOffset >= 0)
                {
                    /* All instances after the first are ignored */
                    string key = arg.Substring(0, valueOffset);
                    if (!ContainsKey(key))
                    {
                        Add(
                            key,
                            arg.Substring(valueOffset + 1));
                    }
                }
            }
        }
    }
}
