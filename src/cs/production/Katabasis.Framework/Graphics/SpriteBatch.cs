// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace bottlenoselabs.Katabasis
{
	/* MSDN Docs:
	 * http://msdn.microsoft.com/en-us/library/microsoft.xna.framework.graphics.spritebatch.aspx
	 * Other References:
	 * http://directxtk.codeplex.com/SourceControl/changeset/view/17079#Src/SpriteBatch.cpp
	 * http://gamedev.stackexchange.com/questions/21220/how-exactly-does-xnas-spritebatch-work
	 */
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "TODO: Need tests.")]
	public unsafe class SpriteBatch : GraphicsResource
	{
		// As defined by the HiDef profile spec
		private const int MaxSprites = 2048;
		private const int MaxVertices = MaxSprites * 4;
		private const int MaxIndices = MaxSprites * 6;

		/* If you use this file to make your own SpriteBatch, take the
		 * shader source and binary and load it as a file. Find it in
		 * src/Graphics/Effect/StockEffects/, the HLSL and FXB folders!
		 * -flibit
		*/
		private static readonly byte[] _spriteEffectCode = Resources.SpriteEffect;
		private static readonly short[] _indexData = GenerateIndexArray();
		private static readonly TextureComparer TextureCompare = new();
		private static readonly BackToFrontComparer BackToFrontCompare = new();
		private static readonly FrontToBackComparer FrontToBackCompare = new();

		// Used to quickly flip text for DrawString
		private static readonly float[] AxisDirectionX = {-1.0f, 1.0f, -1.0f, 1.0f};

		private static readonly float[] AxisDirectionY = {-1.0f, -1.0f, 1.0f, 1.0f};

		private static readonly float[] AxisIsMirroredX = {0.0f, 1.0f, 0.0f, 1.0f};

		private static readonly float[] AxisIsMirroredY = {0.0f, 0.0f, 1.0f, 1.0f};

		// Used to calculate texture coordinates
		private static readonly float[] CornerOffsetX = {0.0f, 1.0f, 0.0f, 1.0f};

		private static readonly float[] CornerOffsetY = {0.0f, 0.0f, 1.0f, 1.0f};

		private readonly IndexBuffer _indexBuffer;

		// Default SpriteBatch Effect
		private readonly Effect _spriteEffect;
		private readonly EffectPass _spriteEffectPass;
		private readonly IntPtr _spriteMatrixTransform;
		private readonly bool _supportsNoOverwrite;

		// Buffer objects used for actual drawing
		private readonly DynamicVertexBuffer _vertexBuffer;

		// Tracks Begin/End calls
		private bool _beginCalled;

		// Keep render state for non-Immediate modes.
		private BlendState? _blendState;

		// Where are we in the vertex buffer ring?
		private int _bufferOffset;

		// User-provided Effect, if applicable
		private Effect? _customEffect;
		private DepthStencilState? _depthStencilState;

		// How many sprites are in the current batch?
		private int _numSprites;
		private RasterizerState? _rasterizerState;
		private SamplerState? _samplerState;
		private IntPtr[] _sortedSpriteInfos; // SpriteInfo*[]

		// Current sort mode
		private SpriteSortMode _sortMode;

		// Local data stored before buffering to GPU
		private SpriteInfo[] _spriteInfos;
		private Texture2D[] _textureInfo;

		// Matrix to be used when creating the projection matrix
		private Matrix4x4 _transformMatrix;
		private VertexPositionColorTexture4[] _vertexInfo;

		public SpriteBatch()
		{
			GraphicsDevice = GraphicsDeviceManager.Instance.GraphicsDevice;

			_vertexInfo = new VertexPositionColorTexture4[MaxSprites];
			_textureInfo = new Texture2D[MaxSprites];
			_spriteInfos = new SpriteInfo[MaxSprites];
			_sortedSpriteInfos = new IntPtr[MaxSprites];
			_vertexBuffer = new DynamicVertexBuffer(
				typeof(VertexPositionColorTexture),
				MaxVertices,
				BufferUsage.WriteOnly);

			_indexBuffer = new IndexBuffer(
				IndexElementSize.SixteenBits,
				MaxIndices,
				BufferUsage.WriteOnly);

			_indexBuffer.SetData(_indexData);

			_spriteEffect = new Effect(_spriteEffectCode);
			_spriteMatrixTransform = _spriteEffect.Parameters!["MatrixTransform"]!._values;
			_spriteEffectPass = _spriteEffect.CurrentTechnique!.Passes[0]!;

			_beginCalled = false;
			_numSprites = 0;
			_supportsNoOverwrite = FNA3D.FNA3D_SupportsNoOverwrite(GraphicsDevice.Device) == 1;
		}

		public void End()
		{
			if (!_beginCalled)
			{
				throw new InvalidOperationException(
					"End was called, but Begin has not yet been called. You must call Begin  successfully before you can call End.");
			}

			_beginCalled = false;

			if (_sortMode != SpriteSortMode.Immediate)
			{
				FlushBatch();
			}

			_customEffect = null;
		}

		protected override void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				_spriteEffect.Dispose();
				_indexBuffer.Dispose();
				_vertexBuffer.Dispose();
			}

			base.Dispose(disposing);
		}

		private static short[] GenerateIndexArray()
		{
			short[] result = new short[MaxIndices];
			for (int i = 0, j = 0; i < MaxIndices; i += 6, j += 4)
			{
				result[i] = (short)j;
				result[i + 1] = (short)(j + 1);
				result[i + 2] = (short)(j + 2);
				result[i + 3] = (short)(j + 3);
				result[i + 4] = (short)(j + 2);
				result[i + 5] = (short)(j + 1);
			}

			return result;
		}

		public void Begin() =>
			Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				SamplerState.LinearClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix4x4.Identity);

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState) =>
			Begin(
				sortMode,
				blendState,
				SamplerState.LinearClamp,
				DepthStencilState.None,
				RasterizerState.CullCounterClockwise,
				null,
				Matrix4x4.Identity);

		public void Begin(
			SpriteSortMode sortMode,
			BlendState blendState,
			SamplerState samplerState,
			DepthStencilState depthStencilState,
			RasterizerState rasterizerState) =>
			Begin(
				sortMode,
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				null,
				Matrix4x4.Identity);

		public void Begin(
			SpriteSortMode sortMode,
			BlendState? blendState,
			SamplerState? samplerState,
			DepthStencilState? depthStencilState,
			RasterizerState? rasterizerState,
			Effect? effect) =>
			Begin(
				sortMode,
				blendState,
				samplerState,
				depthStencilState,
				rasterizerState,
				effect,
				Matrix4x4.Identity);

		public void Begin(
			SpriteSortMode sortMode,
			BlendState? blendState,
			SamplerState? samplerState,
			DepthStencilState? depthStencilState,
			RasterizerState? rasterizerState,
			Effect? effect,
			Matrix4x4 transformationMatrix)
		{
			if (_beginCalled)
			{
				throw new InvalidOperationException(
					"Begin has been called before calling End after the last call to Begin. Begin cannot be called again until End has been successfully called.");
			}

			_beginCalled = true;
			_sortMode = sortMode;
			_blendState = blendState ?? BlendState.AlphaBlend;
			_samplerState = samplerState ?? SamplerState.LinearClamp;
			_depthStencilState = depthStencilState ?? DepthStencilState.None;
			_rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
			_customEffect = effect;
			_transformMatrix = transformationMatrix;

			if (sortMode == SpriteSortMode.Immediate)
			{
				PrepRenderState();
			}
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Color color)
		{
			CheckBegin("Draw");
			PushSprite(
				texture,
				0.0f,
				0.0f,
				1.0f,
				1.0f,
				position.X,
				position.Y,
				texture.Width,
				texture.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0);
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color)
		{
			float sourceX, sourceY, sourceW, sourceH;
			float destW, destH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float)texture.Width;
				sourceY = sourceRectangle.Value.Y / (float)texture.Height;
				sourceW = sourceRectangle.Value.Width / (float)texture.Width;
				sourceH = sourceRectangle.Value.Height / (float)texture.Height;
				destW = sourceRectangle.Value.Width;
				destH = sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				destW = texture.Width;
				destH = texture.Height;
			}

			CheckBegin("Draw");
			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				destW,
				destH,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0);
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth)
		{
			CheckBegin("Draw");
			float sourceX, sourceY, sourceW, sourceH;
			var destW = scale;
			var destH = scale;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float)texture.Width;
				sourceY = sourceRectangle.Value.Y / (float)texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat) / texture.Width;

				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat) / texture.Height;

				destW *= sourceRectangle.Value.Width;
				destH *= sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				destW *= texture.Width;
				destH *= texture.Height;
			}

			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				destW,
				destH,
				color,
				origin.X / sourceW / texture.Width,
				origin.Y / sourceH / texture.Height,
				(float)Math.Sin(rotation),
				(float)Math.Cos(rotation),
				layerDepth,
				(byte)(effects & (SpriteEffects)0x03));
		}

		public void Draw(
			Texture2D texture,
			Vector2 position,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth)
		{
			CheckBegin("Draw");
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float)texture.Width;
				sourceY = sourceRectangle.Value.Y / (float)texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat) / texture.Width;

				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat) / texture.Height;

				scale.X *= sourceRectangle.Value.Width;
				scale.Y *= sourceRectangle.Value.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
				scale.X *= texture.Width;
				scale.Y *= texture.Height;
			}

			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				position.X,
				position.Y,
				scale.X,
				scale.Y,
				color,
				origin.X / sourceW / texture.Width,
				origin.Y / sourceH / texture.Height,
				(float)Math.Sin(rotation),
				(float)Math.Cos(rotation),
				layerDepth,
				(byte)(effects & (SpriteEffects)0x03));
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Color color)
		{
			CheckBegin("Draw");
			PushSprite(
				texture,
				0.0f,
				0.0f,
				1.0f,
				1.0f,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color)
		{
			CheckBegin("Draw");
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float)texture.Width;
				sourceY = sourceRectangle.Value.Y / (float)texture.Height;
				sourceW = sourceRectangle.Value.Width / (float)texture.Width;
				sourceH = sourceRectangle.Value.Height / (float)texture.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
			}

			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				0.0f,
				0.0f,
				0.0f,
				1.0f,
				0.0f,
				0);
		}

		public void Draw(
			Texture2D texture,
			Rectangle destinationRectangle,
			Rectangle? sourceRectangle,
			Color color,
			float rotation,
			Vector2 origin,
			SpriteEffects effects,
			float layerDepth)
		{
			CheckBegin("Draw");
			float sourceX, sourceY, sourceW, sourceH;
			if (sourceRectangle.HasValue)
			{
				sourceX = sourceRectangle.Value.X / (float)texture.Width;
				sourceY = sourceRectangle.Value.Y / (float)texture.Height;
				sourceW = Math.Sign(sourceRectangle.Value.Width) * Math.Max(
					Math.Abs(sourceRectangle.Value.Width),
					MathHelper.MachineEpsilonFloat) / texture.Width;

				sourceH = Math.Sign(sourceRectangle.Value.Height) * Math.Max(
					Math.Abs(sourceRectangle.Value.Height),
					MathHelper.MachineEpsilonFloat) / texture.Height;
			}
			else
			{
				sourceX = 0.0f;
				sourceY = 0.0f;
				sourceW = 1.0f;
				sourceH = 1.0f;
			}

			PushSprite(
				texture,
				sourceX,
				sourceY,
				sourceW,
				sourceH,
				destinationRectangle.X,
				destinationRectangle.Y,
				destinationRectangle.Width,
				destinationRectangle.Height,
				color,
				origin.X / sourceW / texture.Width,
				origin.Y / sourceH / texture.Height,
				(float)Math.Sin(rotation),
				(float)Math.Cos(rotation),
				layerDepth,
				(byte)(effects & (SpriteEffects)0x03));
		}

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			DrawString(
				spriteFont,
				text,
				position,
				color,
				0.0f,
				Vector2.Zero,
				Vector2.One,
				SpriteEffects.None,
				0.0f);
		}

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth)
		{
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			DrawString(
				spriteFont,
				text,
				position,
				color,
				rotation,
				origin,
				new Vector2(scale),
				effects,
				layerDepth);
		}

		public void DrawString(
			SpriteFont spriteFont,
			StringBuilder text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth)
		{
			/* FIXME: This method is a duplicate of DrawString(string)!
			 * The only difference is how we iterate through the StringBuilder.
			 * We don't use ToString() since it generates garbage.
			 * -flibit
			 */
			CheckBegin("DrawString");
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			if (text.Length == 0)
			{
				return;
			}

			effects &= (SpriteEffects)0x03;

			/* We pull all these internal variables in at once so
			 * anyone who wants to use this file to make their own
			 * SpriteBatch can easily replace these with reflection.
			 * -flibit
			 */
			Texture2D textureValue = spriteFont._textureValue;
			List<Rectangle> glyphData = spriteFont._glyphData;
			List<Rectangle> croppingData = spriteFont._croppingData;
			List<Vector3> kerning = spriteFont._kerning;
			Dictionary<char, int> characterIndexMap = spriteFont._characterIndexMap;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offsets/axes, using the string size for flipped text
			var baseOffset = origin;
			var axisDirX = AxisDirectionX[(int)effects];
			var axisDirY = AxisDirectionY[(int)effects];
			var axisDirMirrorX = 0.0f;
			var axisDirMirrorY = 0.0f;
			if (effects != SpriteEffects.None)
			{
				var size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * AxisIsMirroredX[(int)effects];
				baseOffset.Y -= size.Y * AxisIsMirroredY[(int)effects];
				axisDirMirrorX = AxisIsMirroredX[(int)effects];
				axisDirMirrorY = AxisIsMirroredY[(int)effects];
			}

			var curOffset = Vector2.Zero;
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
						curOffset.X = 0.0f;
						curOffset.Y += spriteFont.LineSpacing;
						firstInLine = true;
						continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				if (!characterIndexMap.TryGetValue(c, out var index))
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException("Text contains characters that cannot be resolved by this SpriteFont.", nameof(text));
					}

					index = characterIndexMap[spriteFont.DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				var cKern = kerning[index];
				if (firstInLine)
				{
					curOffset.X += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + cKern.X;
				}

				// Calculate the character origin
				var cCrop = croppingData[index];
				var cGlyph = glyphData[index];
				var offsetX = baseOffset.X + ((curOffset.X + cCrop.X) * axisDirX);
				var offsetY = baseOffset.Y + ((curOffset.Y + cCrop.Y) * axisDirY);
				if (effects != SpriteEffects.None)
				{
					offsetX += cGlyph.Width * axisDirMirrorX;
					offsetY += cGlyph.Height * axisDirMirrorY;
				}

				// Draw!
				var sourceW = Math.Sign(cGlyph.Width) * Math.Max(
					Math.Abs(cGlyph.Width),
					MathHelper.MachineEpsilonFloat) / textureValue.Width;

				var sourceH = Math.Sign(cGlyph.Height) * Math.Max(
					Math.Abs(cGlyph.Height),
					MathHelper.MachineEpsilonFloat) / textureValue.Height;

				PushSprite(
					textureValue,
					cGlyph.X / (float)textureValue.Width,
					cGlyph.Y / (float)textureValue.Height,
					sourceW,
					sourceH,
					position.X,
					position.Y,
					cGlyph.Width * scale.X,
					cGlyph.Height * scale.Y,
					color,
					offsetX / sourceW / textureValue.Width,
					offsetY / sourceH / textureValue.Height,
					(float)Math.Sin(rotation),
					(float)Math.Cos(rotation),
					layerDepth,
					(byte)effects);

				/* Add the character width and right-side
				 * bearing to the line width.
				 */
				curOffset.X += cKern.Y + cKern.Z;
			}
		}

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color) =>
			DrawString(
				spriteFont,
				text,
				position,
				color,
				0.0f,
				Vector2.Zero,
				Vector2.One,
				SpriteEffects.None,
				0.0f);

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			float scale,
			SpriteEffects effects,
			float layerDepth) =>
			DrawString(
				spriteFont,
				text,
				position,
				color,
				rotation,
				origin,
				new Vector2(scale),
				effects,
				layerDepth);

		public void DrawString(
			SpriteFont spriteFont,
			string text,
			Vector2 position,
			Color color,
			float rotation,
			Vector2 origin,
			Vector2 scale,
			SpriteEffects effects,
			float layerDepth)
		{
			/* FIXME: This method is a duplicate of DrawString(StringBuilder)!
			 * The only difference is how we iterate through the string.
			 * -flibit
			 */
			CheckBegin("DrawString");
			if (text == null)
			{
				throw new ArgumentNullException(nameof(text));
			}

			if (text.Length == 0)
			{
				return;
			}

			effects &= (SpriteEffects)0x03;

			/* We pull all these internal variables in at once so
			 * anyone who wants to use this file to make their own
			 * SpriteBatch can easily replace these with reflection.
			 * -flibit
			 */
			Texture2D textureValue = spriteFont._textureValue;
			List<Rectangle> glyphData = spriteFont._glyphData;
			List<Rectangle> croppingData = spriteFont._croppingData;
			List<Vector3> kerning = spriteFont._kerning;
			Dictionary<char, int> characterIndexMap = spriteFont._characterIndexMap;

			// FIXME: This needs an accuracy check! -flibit

			// Calculate offsets/axes, using the string size for flipped text
			var baseOffset = origin;
			var axisDirX = AxisDirectionX[(int)effects];
			var axisDirY = AxisDirectionY[(int)effects];
			var axisDirMirrorX = 0.0f;
			var axisDirMirrorY = 0.0f;
			if (effects != SpriteEffects.None)
			{
				var size = spriteFont.MeasureString(text);
				baseOffset.X -= size.X * AxisIsMirroredX[(int)effects];
				baseOffset.Y -= size.Y * AxisIsMirroredY[(int)effects];
				axisDirMirrorX = AxisIsMirroredX[(int)effects];
				axisDirMirrorY = AxisIsMirroredY[(int)effects];
			}

			var curOffset = Vector2.Zero;
			var firstInLine = true;
			foreach (var c in text)
			{
				switch (c)
				{
					// Special characters
					case '\r':
						continue;
					case '\n':
						curOffset.X = 0.0f;
						curOffset.Y += spriteFont.LineSpacing;
						firstInLine = true;
						continue;
				}

				/* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
				if (!characterIndexMap.TryGetValue(c, out var index))
				{
					if (!spriteFont.DefaultCharacter.HasValue)
					{
						throw new ArgumentException("Text contains characters that cannot be resolved by this SpriteFont.", nameof(text));
					}

					index = characterIndexMap[spriteFont.DefaultCharacter.Value];
				}

				/* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
				var cKern = kerning[index];
				if (firstInLine)
				{
					curOffset.X += Math.Abs(cKern.X);
					firstInLine = false;
				}
				else
				{
					curOffset.X += spriteFont.Spacing + cKern.X;
				}

				// Calculate the character origin
				var cCrop = croppingData[index];
				var cGlyph = glyphData[index];
				var offsetX = baseOffset.X + ((curOffset.X + cCrop.X) * axisDirX);
				var offsetY = baseOffset.Y + ((curOffset.Y + cCrop.Y) * axisDirY);
				if (effects != SpriteEffects.None)
				{
					offsetX += cGlyph.Width * axisDirMirrorX;
					offsetY += cGlyph.Height * axisDirMirrorY;
				}

				// Draw!
				var sourceW = Math.Sign(cGlyph.Width) * Math.Max(
					Math.Abs(cGlyph.Width),
					MathHelper.MachineEpsilonFloat) / textureValue.Width;

				var sourceH = Math.Sign(cGlyph.Height) * Math.Max(
					Math.Abs(cGlyph.Height),
					MathHelper.MachineEpsilonFloat) / textureValue.Height;

				PushSprite(
					textureValue,
					cGlyph.X / (float)textureValue.Width,
					cGlyph.Y / (float)textureValue.Height,
					sourceW,
					sourceH,
					position.X,
					position.Y,
					cGlyph.Width * scale.X,
					cGlyph.Height * scale.Y,
					color,
					offsetX / sourceW / textureValue.Width,
					offsetY / sourceH / textureValue.Height,
					(float)Math.Sin(rotation),
					(float)Math.Cos(rotation),
					layerDepth,
					(byte)effects);

				curOffset.X += cKern.Y + cKern.Z;
			}
		}

		private unsafe void PushSprite(
			Texture2D texture,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects)
		{
			if (_numSprites >= _vertexInfo.Length)
			{
				/* We're out of room, add another batch max
				 * to the total array size. This is required for
				 * sprite sorting accuracy; note that we do NOT
				 * increase the graphics buffer sizes!
				 * -flibit
				 */
				var newMax = _vertexInfo.Length + MaxSprites;
				Array.Resize(ref _vertexInfo, newMax);
				Array.Resize(ref _textureInfo, newMax);
				Array.Resize(ref _spriteInfos, newMax);
				Array.Resize(ref _sortedSpriteInfos, newMax);
			}

			// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
			switch (_sortMode)
			{
				case SpriteSortMode.Immediate:
				{
					int offset;
					fixed (VertexPositionColorTexture4* sprite = &_vertexInfo[0])
					{
						GenerateVertexInfo(
							sprite,
							sourceX,
							sourceY,
							sourceW,
							sourceH,
							destinationX,
							destinationY,
							destinationW,
							destinationH,
							color,
							originX,
							originY,
							rotationSin,
							rotationCos,
							depth,
							effects);

						if (_supportsNoOverwrite)
						{
							offset = UpdateVertexBuffer(0, 1);
						}
						else
						{
							/* We do NOT use Discard here because
						 * it would be stupid to reallocate the
						 * whole buffer just for one sprite.
						 *
						 * Unless you're using this to blit a
						 * target, stop using Immediate ya donut
						 * -flibit
						 */
							offset = 0;
							_vertexBuffer.SetDataPointerEXT(
								0,
								(IntPtr)sprite,
								VertexPositionColorTexture4.RealStride,
								SetDataOptions.None);
						}
					}

					DrawPrimitives(texture, offset, 1);
					break;
				}

				case SpriteSortMode.Deferred:
				{
					fixed (VertexPositionColorTexture4* sprite = &_vertexInfo[_numSprites])
					{
						GenerateVertexInfo(
							sprite,
							sourceX,
							sourceY,
							sourceW,
							sourceH,
							destinationX,
							destinationY,
							destinationW,
							destinationH,
							color,
							originX,
							originY,
							rotationSin,
							rotationCos,
							depth,
							effects);
					}

					_textureInfo[_numSprites] = texture;
					_numSprites += 1;
					break;
				}

				default:
				{
					fixed (SpriteInfo* spriteInfo = &_spriteInfos[_numSprites])
					{
						spriteInfo->TextureHash = texture.GetHashCode();
						spriteInfo->SourceX = sourceX;
						spriteInfo->SourceY = sourceY;
						spriteInfo->SourceW = sourceW;
						spriteInfo->SourceH = sourceH;
						spriteInfo->DestinationX = destinationX;
						spriteInfo->DestinationY = destinationY;
						spriteInfo->DestinationW = destinationW;
						spriteInfo->DestinationH = destinationH;
						spriteInfo->Color = color;
						spriteInfo->OriginX = originX;
						spriteInfo->OriginY = originY;
						spriteInfo->RotationSin = rotationSin;
						spriteInfo->RotationCos = rotationCos;
						spriteInfo->Depth = depth;
						spriteInfo->Effects = effects;
					}

					_textureInfo[_numSprites] = texture;
					_numSprites += 1;
					break;
				}
			}
		}

		private unsafe void FlushBatch()
		{
			PrepRenderState();

			if (_numSprites == 0)
			{
				// Nothing to do.
				return;
			}

			if (_sortMode != SpriteSortMode.Deferred)
			{
				IComparer<IntPtr> comparer = _sortMode switch
				{
					SpriteSortMode.Texture => TextureCompare,
					SpriteSortMode.BackToFront => BackToFrontCompare,
					_ => FrontToBackCompare
				};

				fixed (SpriteInfo* spriteInfo = &_spriteInfos[0])
				{
					fixed (IntPtr* sortedSpriteInfo = &_sortedSpriteInfos[0])
					{
						fixed (VertexPositionColorTexture4* sprites = &_vertexInfo[0])
						{
							for (var i = 0; i < _numSprites; i += 1)
							{
								sortedSpriteInfo[i] = (IntPtr)(&spriteInfo[i]);
							}

							Array.Sort(
								_sortedSpriteInfos,
								_textureInfo,
								0,
								_numSprites,
								comparer);

							for (var i = 0; i < _numSprites; i += 1)
							{
								var info = (SpriteInfo*)sortedSpriteInfo[i];
								GenerateVertexInfo(
									&sprites[i],
									info->SourceX,
									info->SourceY,
									info->SourceW,
									info->SourceH,
									info->DestinationX,
									info->DestinationY,
									info->DestinationW,
									info->DestinationH,
									info->Color,
									info->OriginX,
									info->OriginY,
									info->RotationSin,
									info->RotationCos,
									info->Depth,
									info->Effects);
							}
						}
					}
				}
			}

			var arrayOffset = 0;
			nextBatch:
			var batchSize = Math.Min(_numSprites, MaxSprites);
			var baseOff = UpdateVertexBuffer(arrayOffset, batchSize);
			var offset = 0;

			Texture2D curTexture = _textureInfo[arrayOffset];
			for (var i = 1; i < batchSize; i += 1)
			{
				Texture2D tex = _textureInfo[arrayOffset + i];
				if (tex != curTexture)
				{
					DrawPrimitives(curTexture, baseOff + offset, i - offset);
					curTexture = tex;
					offset = i;
				}
			}

			DrawPrimitives(curTexture, baseOff + offset, batchSize - offset);

			if (_numSprites > MaxSprites)
			{
				_numSprites -= MaxSprites;
				arrayOffset += MaxSprites;
				goto nextBatch;
			}

			_numSprites = 0;
		}

		private unsafe int UpdateVertexBuffer(int start, int count)
		{
			int offset;
			SetDataOptions options;
			if (_bufferOffset + count > MaxSprites ||
			    !_supportsNoOverwrite)
			{
				offset = 0;
				options = SetDataOptions.Discard;
			}
			else
			{
				offset = _bufferOffset;
				options = SetDataOptions.NoOverwrite;
			}

			fixed (VertexPositionColorTexture4* p = &_vertexInfo[start])
			{
				/* We use Discard here because the last batch
				 * may still be executing, and we can't always
				 * trust the driver to use a staging buffer for
				 * buffer uploads that overlap between commands.
				 *
				 * If you aren't using the whole vertex buffer,
				 * that's your own fault. Use the whole buffer!
				 * -flibit
				 */
				_vertexBuffer.SetDataPointerEXT(
					offset * VertexPositionColorTexture4.RealStride,
					(IntPtr)p,
					count * VertexPositionColorTexture4.RealStride,
					options);
			}

			_bufferOffset = offset + count;
			return offset;
		}

		private static unsafe void GenerateVertexInfo(
			VertexPositionColorTexture4* sprite,
			float sourceX,
			float sourceY,
			float sourceW,
			float sourceH,
			float destinationX,
			float destinationY,
			float destinationW,
			float destinationH,
			Color color,
			float originX,
			float originY,
			float rotationSin,
			float rotationCos,
			float depth,
			byte effects)
		{
			var cornerX = -originX * destinationW;
			var cornerY = -originY * destinationH;
			sprite->Position0.X = (-rotationSin * cornerY) +
			                      (rotationCos * cornerX) +
			                      destinationX;

			sprite->Position0.Y = (rotationCos * cornerY) +
			                      (rotationSin * cornerX) +
			                      destinationY;

			cornerX = (1.0f - originX) * destinationW;
			cornerY = -originY * destinationH;
			sprite->Position1.X = (-rotationSin * cornerY) +
			                      (rotationCos * cornerX) +
			                      destinationX;

			sprite->Position1.Y = (rotationCos * cornerY) +
			                      (rotationSin * cornerX) +
			                      destinationY;

			cornerX = -originX * destinationW;
			cornerY = (1.0f - originY) * destinationH;
			sprite->Position2.X = (-rotationSin * cornerY) +
			                      (rotationCos * cornerX) +
			                      destinationX;

			sprite->Position2.Y = (rotationCos * cornerY) +
			                      (rotationSin * cornerX) +
			                      destinationY;

			cornerX = (1.0f - originX) * destinationW;
			cornerY = (1.0f - originY) * destinationH;
			sprite->Position3.X = (-rotationSin * cornerY) +
			                      (rotationCos * cornerX) +
			                      destinationX;

			sprite->Position3.Y = (rotationCos * cornerY) +
			                      (rotationSin * cornerX) +
			                      destinationY;

			fixed (float* flipX = &CornerOffsetX[0])
			{
				fixed (float* flipY = &CornerOffsetY[0])
				{
					sprite->TextureCoordinate0.X = (flipX[0 ^ effects] * sourceW) + sourceX;
					sprite->TextureCoordinate0.Y = (flipY[0 ^ effects] * sourceH) + sourceY;
					sprite->TextureCoordinate1.X = (flipX[1 ^ effects] * sourceW) + sourceX;
					sprite->TextureCoordinate1.Y = (flipY[1 ^ effects] * sourceH) + sourceY;
					sprite->TextureCoordinate2.X = (flipX[2 ^ effects] * sourceW) + sourceX;
					sprite->TextureCoordinate2.Y = (flipY[2 ^ effects] * sourceH) + sourceY;
					sprite->TextureCoordinate3.X = (flipX[3 ^ effects] * sourceW) + sourceX;
					sprite->TextureCoordinate3.Y = (flipY[3 ^ effects] * sourceH) + sourceY;
				}
			}

			sprite->Position0.Z = depth;
			sprite->Position1.Z = depth;
			sprite->Position2.Z = depth;
			sprite->Position3.Z = depth;
			sprite->Color0 = color;
			sprite->Color1 = color;
			sprite->Color2 = color;
			sprite->Color3 = color;
		}

		private void PrepRenderState()
		{
			GraphicsDevice.BlendState = _blendState!;
			GraphicsDevice.SamplerStates[0] = _samplerState;
			GraphicsDevice.DepthStencilState = _depthStencilState!;
			GraphicsDevice.RasterizerState = _rasterizerState!;

			GraphicsDevice.SetVertexBuffer(_vertexBuffer);
			GraphicsDevice.Indices = _indexBuffer;

			var viewport = GraphicsDevice.Viewport;

			// Inlined CreateOrthographicOffCenter * transformMatrix
			var tfWidth = (float)(2.0 / viewport.Width);
			var tfHeight = (float)(-2.0 / viewport.Height);
			var dstPtr = (float*)_spriteMatrixTransform;
			dstPtr[0] = (tfWidth * _transformMatrix.M11) - _transformMatrix.M14;
			dstPtr[1] = (tfWidth * _transformMatrix.M21) - _transformMatrix.M24;
			dstPtr[2] = (tfWidth * _transformMatrix.M31) - _transformMatrix.M34;
			dstPtr[3] = (tfWidth * _transformMatrix.M41) - _transformMatrix.M44;
			dstPtr[4] = (tfHeight * _transformMatrix.M12) + _transformMatrix.M14;
			dstPtr[5] = (tfHeight * _transformMatrix.M22) + _transformMatrix.M24;
			dstPtr[6] = (tfHeight * _transformMatrix.M32) + _transformMatrix.M34;
			dstPtr[7] = (tfHeight * _transformMatrix.M42) + _transformMatrix.M44;
			dstPtr[8] = _transformMatrix.M13;
			dstPtr[9] = _transformMatrix.M23;
			dstPtr[10] = _transformMatrix.M33;
			dstPtr[11] = _transformMatrix.M43;
			dstPtr[12] = _transformMatrix.M14;
			dstPtr[13] = _transformMatrix.M24;
			dstPtr[14] = _transformMatrix.M34;
			dstPtr[15] = _transformMatrix.M44;

			// FIXME: When is this actually applied? -flibit
			_spriteEffectPass.Apply();
		}

		private void DrawPrimitives(Texture texture, int baseSprite, int batchSize)
		{
			if (_customEffect != null)
			{
				foreach (var pass in _customEffect.CurrentTechnique!.Passes)
				{
					pass.Apply();
					// Set this _after_ Apply, otherwise EffectParameters override it!
					GraphicsDevice.Textures[0] = texture;
					GraphicsDevice.DrawIndexedPrimitives(
						PrimitiveType.TriangleList,
						baseSprite * 4,
						0,
						batchSize * 4,
						0,
						batchSize * 2);
				}
			}
			else
			{
				GraphicsDevice.Textures[0] = texture;
				GraphicsDevice.DrawIndexedPrimitives(
					PrimitiveType.TriangleList,
					baseSprite * 4,
					0,
					batchSize * 4,
					0,
					batchSize * 2);
			}
		}

		private void CheckBegin(string method)
		{
			if (!_beginCalled)
			{
				throw new InvalidOperationException(
					$"{method} was called, but Begin has not yet been called. Begin must be called successfully before you can call {method}.");
			}
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct VertexPositionColorTexture4 : IVertexType
		{
			public const int RealStride = 96;

			VertexDeclaration IVertexType.VertexDeclaration => throw new NotImplementedException();

			public Vector3 Position0;
			public Color Color0;
			public Vector2 TextureCoordinate0;
			public Vector3 Position1;
			public Color Color1;
			public Vector2 TextureCoordinate1;
			public Vector3 Position2;
			public Color Color2;
			public Vector2 TextureCoordinate2;
			public Vector3 Position3;
			public Color Color3;
			public Vector2 TextureCoordinate3;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct SpriteInfo
		{
			/* We store the hash instead of the Texture2D because
			 * it allows this to stay an unmanaged type and prevents
			 * us from constantly calling GetHashCode during sorts.
			 */
			public int TextureHash;
			public float SourceX;
			public float SourceY;
			public float SourceW;
			public float SourceH;
			public float DestinationX;
			public float DestinationY;
			public float DestinationW;
			public float DestinationH;
			public Color Color;
			public float OriginX;
			public float OriginY;
			public float RotationSin;
			public float RotationCos;
			public float Depth;
			public byte Effects;
		}

		private class TextureComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				var p1 = (SpriteInfo*)i1;
				var p2 = (SpriteInfo*)i2;
				return p1->TextureHash.CompareTo(p2->TextureHash);
			}
		}

		private class BackToFrontComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				var p1 = (SpriteInfo*)i1;
				var p2 = (SpriteInfo*)i2;
				return p2->Depth.CompareTo(p1->Depth);
			}
		}

		private class FrontToBackComparer : IComparer<IntPtr>
		{
			public unsafe int Compare(IntPtr i1, IntPtr i2)
			{
				var p1 = (SpriteInfo*)i1;
				var p2 = (SpriteInfo*)i2;
				return p1->Depth.CompareTo(p2->Depth);
			}
		}
	}
}
