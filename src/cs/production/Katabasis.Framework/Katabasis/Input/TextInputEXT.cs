// Copyright (c) Craftworkgames (https://github.com/craftworkgames). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory (https://github.com/craftworkgames/Katabasis) for full license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace Katabasis
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Needs tests.")]
	public static class TextInputEXT
	{
		public static event Action<char>? TextInput;

		public static event Action<string, int, int>? TextEditing;

		public static void StartTextInput() => FNAPlatform.StartTextInput();

		public static void StopTextInput() => FNAPlatform.StopTextInput();

		public static void SetInputRectangle(Rectangle rectangle) => FNAPlatform.SetTextInputRectangle(rectangle);

		internal static void OnTextInput(char c) => TextInput?.Invoke(c);

		internal static void OnTextEditing(string text, int start, int length) => TextEditing?.Invoke(text, start, length);
	}
}
