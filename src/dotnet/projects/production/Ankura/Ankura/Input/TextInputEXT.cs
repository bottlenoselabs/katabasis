// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ankura
{
    [SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
    public static class TextInputEXT
    {
        public static event Action<char>? TextInput;

        public static event Action<string, int, int>? TextEditing;

        public static void StartTextInput()
        {
            FNAPlatform.StartTextInput();
        }

        public static void StopTextInput()
        {
            FNAPlatform.StopTextInput();
        }

        public static void SetInputRectangle(Rectangle rectangle)
        {
            FNAPlatform.SetTextInputRectangle(rectangle);
        }

        internal static void OnTextInput(char c)
        {
            TextInput?.Invoke(c);
        }

        internal static void OnTextEditing(string text, int start, int length)
        {
            TextEditing?.Invoke(text, start, length);
        }
    }
}
