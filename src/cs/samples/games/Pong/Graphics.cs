using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples;

public static class Graphics
{
	public static SpriteBatch SpriteBatch = null!;
	public static Texture2D PixelTexture = null!;

	public static void Load()
	{
		SpriteBatch = new SpriteBatch();
		PixelTexture = new Texture2D(1, 1);
		PixelTexture.SetData(new[] { Color.White });
	}

	public static void Begin()
	{
		SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
	}

	public static void DrawRectangle(Rectangle rectangle, Color color)
	{
		var pos = new Vector2(rectangle.X, rectangle.Y);
		SpriteBatch.Draw(Graphics.PixelTexture, pos, rectangle, color * 1.0f, 0, Vector2.Zero, 1.0f, SpriteEffects.None, 0.00001f);
	}

	public static void End()
	{
		SpriteBatch.End();
	}

	public static void Reset()
	{
	}
}
