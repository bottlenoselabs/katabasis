// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace bottlenoselabs.Katabasis
{
	[PublicAPI]
	public static class TextInputEXT
	{
		public static event Action<char>? TextInput;

		public static event Action<string, int, int>? TextEditing;

		public static bool IsTextInputActive()
		{
			return FNAPlatform.IsTextInputActive();
		}

		public static void StartTextInput() => FNAPlatform.StartTextInput();

		public static void StopTextInput() => FNAPlatform.StopTextInput();

		public static void SetInputRectangle(Rectangle rectangle) => FNAPlatform.SetTextInputRectangle(rectangle);

		internal static void OnTextInput(char c) => TextInput?.Invoke(c);

		internal static void OnTextEditing(string text, int start, int length) => TextEditing?.Invoke(text, start, length);
	}
}
