// Copyright (c) Craftwork Games. All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;

namespace Katabasis
{
	// http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.spritefont.aspx
	public sealed class SpriteFont
	{
		/* This is not a part of the spec as far as we know, but we
		 * added this because it's WAY faster than going to characterMap
		 * and calling IndexOf on each character.
		 */
		internal Dictionary<char, int> _characterIndexMap;
		internal List<char> _characterMap;
		internal List<Rectangle> _croppingData;
		internal List<Rectangle> _glyphData;
		internal List<Vector3> _kerning;

		/* If, by chance, you're seeing this and thinking about using
		 * reflection to access the fields:
		 * Don't.
		 * To date, one (1) game is using the fields directly,
		 * even though the properties are publicly accessible.
		 * Not even FNA uses the fields directly.
		 * -ade
		 */
		internal int _lineSpacing;

		internal float _spacing;

		/* I've had a bunch of games use reflection on SpriteFont to get
		 * this data. Keep these names as they are for XNA4 accuracy!
		 * -flibit
		 */
		internal Texture2D _textureValue;

		internal SpriteFont(
			Texture2D texture,
			List<Rectangle> glyphBounds,
			List<Rectangle> cropping,
			List<char> characters,
			int lineSpacing,
			float spacing,
			List<Vector3> kerningData,
			char? defaultCharacter)
		{
			Characters = new ReadOnlyCollection<char>(characters.ToArray());
			DefaultCharacter = defaultCharacter;
			LineSpacing = lineSpacing;
			Spacing = spacing;

			_textureValue = texture;
			_glyphData = glyphBounds;
			_croppingData = cropping;
			_kerning = kerningData;
			_characterMap = characters;

			_characterIndexMap = new Dictionary<char, int>(characters.Count);
			for (var i = 0; i < characters.Count; i += 1)
			{
				_characterIndexMap[characters[i]] = i;
			}
		}

		public ReadOnlyCollection<char> Characters { get; }

		public char? DefaultCharacter { get; set; }

		public int LineSpacing
		{
			get => _lineSpacing;
			set => _lineSpacing = value;
		}

		public float Spacing
		{
			get => _spacing;
			set => _spacing = value;
		}

		public Vector2 MeasureString(string text)
		{
			/* FIXME: This method is a duplicate of MeasureString(StringBuilder)!
			 * The only difference is how we iterate through the string.
			 * -flibit
			 */
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			// FIXME: This needs an accuracy check! -flibit

			var result = Vector2.Zero;
			var curLineWidth = 0.0f;
			float finalLineHeight = LineSpacing;
			var firstInLine = true;

			foreach (var c in text)
			{
				switch (c)
				{
					// Special characters
					case '\r':
						continue;
					case '\n':
						result.X = Math.Max(result.X, curLineWidth);
						result.Y += LineSpacing;
						curLineWidth = 0.0f;
						finalLineHeight = LineSpacing;
						firstInLine = true;
						continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				if (!_characterIndexMap.TryGetValue(c, out var index))
				{
					if (!DefaultCharacter.HasValue)
					{
						throw new ArgumentException("Text contains characters that cannot be resolved by this SpriteFont.", nameof(text));
					}

					index = _characterIndexMap[DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				var cKern = _kerning[index];
				if (firstInLine)
				{
					curLineWidth += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += Spacing + cKern.X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += cKern.Y + cKern.Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				var cCropHeight = _croppingData[index].Height;
				if (cCropHeight > finalLineHeight)
				{
					finalLineHeight = cCropHeight;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}

		public Vector2 MeasureString(StringBuilder text)
		{
			/* FIXME: This method is a duplicate of MeasureString(string)!
			 * The only difference is how we iterate through the StringBuilder.
			 * We don't use ToString() since it generates garbage.
			 * -flibit
			 */
			if (text.Length == 0)
			{
				return Vector2.Zero;
			}

			// FIXME: This needs an accuracy check! -flibit

			var result = Vector2.Zero;
			var curLineWidth = 0.0f;
			float finalLineHeight = LineSpacing;
			var firstInLine = true;

			for (var i = 0; i < text.Length; i += 1)
			{
				var c = text[i];

				switch (c)
				{
					// Special characters
					case '\r':
						continue;
					case '\n':
						result.X = Math.Max(result.X, curLineWidth);
						result.Y += LineSpacing;
						curLineWidth = 0.0f;
						finalLineHeight = LineSpacing;
						firstInLine = true;
						continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				if (!_characterIndexMap.TryGetValue(c, out var index))
				{
					if (!DefaultCharacter.HasValue)
					{
						throw new ArgumentException("Text contains characters that cannot be resolved by this SpriteFont.", nameof(text));
					}

					index = _characterIndexMap[DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				var cKern = _kerning[index];
				if (firstInLine)
				{
					curLineWidth += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curLineWidth += Spacing + cKern.X;
				}

				/* Add the character width and right-side bearing to the line
				 * width.
				 */
				curLineWidth += cKern.Y + cKern.Z;

				/* If a character is taller than the default line height,
				 * increase the height to that of the line's tallest character.
				 */
				var cCropHeight = _croppingData[index].Height;
				if (cCropHeight > finalLineHeight)
				{
					finalLineHeight = cCropHeight;
				}
			}

			// Calculate the final width/height of the text box
			result.X = Math.Max(result.X, curLineWidth);
			result.Y += finalLineHeight;

			return result;
		}
	}
}
