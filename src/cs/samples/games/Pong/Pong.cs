using System;
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples;

public static class Pong
{
	// AUTOPONG originally by MrGrak https://github.com/MrGrak MiT License 2022

	public static Viewport Viewport = new(0, 0, 1280, 720);

	public const int PointsPerGame = 4;

	public static GameState PreviousGameState;
	public static GameState CurrentGameState;

	public struct GameState
	{
		public PlayerState Player1;
		public PlayerState Player2;
		public BallState Ball;
	}

	public struct PlayerState
	{
		public byte Index;
		public int Points;
		public PaddleState Paddle;

		public readonly bool IsPlayer1 => Index == 0;
	}

	public struct PaddleState
	{
		public Vector2 Position; // center of the paddle
		public int Width;
		public int Height;
		public Rectangle HitBox;

		public readonly float HalfWidth => Width * 0.5f;
		public readonly float HalfHeight => Height * 0.5f;
	}

	public struct BallState
	{
		public Vector2 Velocity;
		public Vector2 Position; // center of the ball
		public int Width;
		public int Height;
		public Rectangle HitBox;

		public readonly float HalfWidth => Width * 0.5f;
		public readonly float HalfHeight => Width * 0.5f;
		public readonly float ScreenPositionX => Position.X - HalfWidth;
		public readonly float ScreenPositionY => Position.Y - HalfHeight;
	}

	private static readonly Random Rand = new();

	public static void LoadContent()
	{
		Sounds.Load();
		Graphics.Load();
		Reset();
	}

	public static void ViewportChanged()
	{
		ref var player1Paddle = ref CurrentGameState.Player1.Paddle;
		player1Paddle.Position.X = Viewport.X + player1Paddle.HalfWidth;

		ref var player2Paddle = ref CurrentGameState.Player2.Paddle;
		player2Paddle.Position.X = Viewport.Width - player2Paddle.HalfWidth;

		ref var ball = ref CurrentGameState.Ball;
		ball.Position = new Vector2(Viewport.Width * 0.5f, Viewport.Height * 0.5f);
	}

	public static void Reset()
	{
		Sounds.Reset();
		Graphics.Reset();

		ResetBall(ref CurrentGameState.Ball);
		ResetPlayer(ref CurrentGameState.Player1);
		ResetPlayer(ref CurrentGameState.Player2);

		// make sure player indexes are correct; used for logic
		CurrentGameState.Player1.Index = 0;
		CurrentGameState.Player2.Index = 1;

		// when we reset the game make sure that there is no previous state; otherwise rendering logic would be incorrect
		PreviousGameState = CurrentGameState;
	}

	private static void ResetPlayerPaddles(ref PlayerState state)
	{
		ref var paddleState = ref state.Paddle;
		paddleState.Width = 20;
		paddleState.Height = 100;

		if (state.IsPlayer1)
		{
			paddleState.Position = new Vector2(Viewport.X + paddleState.HalfWidth, Viewport.Height * 0.5f);
		}
		else
		{
			paddleState.Position = new Vector2(Viewport.Width - paddleState.HalfWidth, Viewport.Height * 0.5f);
		}

		paddleState.HitBox = HitBox(state.Paddle.Position, state.Paddle.Width, state.Paddle.Height);
	}

	private static void ResetBall(ref BallState state)
	{
		var startingSpeedPixelsPerSecond = 250;
		state.Velocity = new Vector2(1, 0.1f) * startingSpeedPixelsPerSecond;
		state.Position = new Vector2(Viewport.Width * 0.5f, Viewport.Height * 0.5f);
		state.Width = 10;
		state.Height = 10;
		state.HitBox = HitBox(state.Position, state.Width, state.Height);
	}

	private static void ResetPlayer(ref PlayerState state)
	{
		state.Points = 0;
		ResetPlayerPaddles(ref state);
	}

	public static void UpdateGame(GameTime gameTime)
	{
		ref var state = ref CurrentGameState;

		UpdateBall(gameTime, ref state.Ball);
		UpdatePlayer1(ref state.Player1);
		UpdatePlayerCheater(ref state.Player2, ref state.Ball);
		CheckForPlayerWinsGame(ref state);

		ref var previousState = ref PreviousGameState;
		previousState = state;

		Sounds.TryPlayResetJingle();
	}

	private static void UpdateBall(GameTime gameTime, ref BallState state)
	{
		LimitBallVelocity(ref state);
		ApplyBallPhysics(gameTime, ref state);
		CheckForBallCollisionWithPlayerPaddles(ref state);
		CheckForBallHitsPlayerSide(ref state);
		CheckForBallOutOfBounds(ref state);
	}

	private static void LimitBallVelocity(ref BallState state)
	{
		// limit how fast the ball can move each frame
		var maxVelocity = new Vector2(1000f, 1000f);
		var minVelocity = new Vector2(-1000f, -1000f);
		state.Velocity = Vector2.Clamp(state.Velocity, minVelocity, maxVelocity);
	}

	private static void ApplyBallPhysics(GameTime gameTime, ref BallState state)
	{
		// apply velocity to position
		state.Position += state.Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
	}

	private static void CheckForBallCollisionWithPlayerPaddles(ref BallState state)
	{
		// check if the ball collides with either of the player's paddles

		var player1Paddle = CurrentGameState.Player1.Paddle;
		if (state.HitBox.Intersects(player1Paddle.HitBox))
		{
			// make the ball move in the other direction; increases by a percentage so the game ends quickly
			state.Velocity.X *= -1.25f;

			// increase the Y velocity by a percentage
			state.Velocity.Y *= 1.15f;

			// reset the position of the ball to be outside of the paddle
			state.Position.X = player1Paddle.Position.X + player1Paddle.HalfWidth + state.HalfWidth;

			Sounds.PlayBallHitsPaddle();
		}

		var player2Paddle = CurrentGameState.Player2.Paddle;
		if (state.HitBox.Intersects(player2Paddle.HitBox))
		{
			// ake the ball move in the other direction; increases by a percentage so the game ends quickly
			state.Velocity.X *= -1.25f;

			// increase the Y velocity by a percentage
			state.Velocity.Y *= 1.15f;

			// reset the position of the ball to be outside of the paddle
			state.Position.X = player2Paddle.Position.X - player2Paddle.HalfWidth - state.HalfWidth;

			Sounds.PlayBallHitsPaddle();
		}
	}

	private static void CheckForBallHitsPlayerSide(ref BallState state)
	{
		if (state.ScreenPositionX < Viewport.X)
		{
			// point for right: player 2
			state.Position.X = Viewport.X + state.HalfWidth;
			state.Velocity.X *= -1;
			CurrentGameState.Player2.Points++;
			Sounds.PlayPlayerGainsPoint();
		}
		else if (state.Position.X > Viewport.Width)
		{
			// point for left: player 1
			state.Position.X = Viewport.Width - state.HalfWidth;
			state.Velocity.X *= -1;
			CurrentGameState.Player1.Points++;
			Sounds.PlayPlayerGainsPoint();
		}
	}

	private static void CheckForBallOutOfBounds(ref BallState state)
	{
		if (state.ScreenPositionY < Viewport.Y) // limit to minimum Y pos
		{
			state.Position.Y = Viewport.Y + state.HalfHeight;
			state.Velocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
		}
		else if (state.ScreenPositionY + state.Height > Viewport.Height) // limit to maximum Y pos
		{
			state.Position.Y = Viewport.Height - state.HalfHeight;
			state.Velocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
		}
	}

	private static void UpdatePlayer1(ref PlayerState state)
	{
		var mouseState = Mouse.GetState();
		state.Paddle.Position.Y = mouseState.Y;

		CheckPaddleOutOfBounds(ref state.Paddle);
	}

	private static void UpdatePlayerCheater(ref PlayerState state, ref BallState ball)
	{
		// cheater ai, always has the center of the paddle towards the ball
		state.Paddle.Position.Y = ball.Position.Y;

		CheckPaddleOutOfBounds(ref state.Paddle);
	}

	public static void CheckPaddleOutOfBounds(ref PaddleState paddle)
	{
		// limit how far paddles can travel on Y axis so they dont exceed top or bottom
		if (paddle.Position.Y - paddle.HalfHeight < Viewport.Y)
		{
			paddle.Position.Y = Viewport.Y + paddle.HalfHeight;
		}
		else if (paddle.Position.Y + paddle.HalfHeight > Viewport.Height)
		{
			paddle.Position.Y = Viewport.Height - paddle.HalfHeight;
		}
	}

	private static void CheckForPlayerWinsGame(ref GameState state)
	{
		var player1 = state.Player1;
		var player2 = state.Player2;

		if (player1.Points >= PointsPerGame)
		{
			Reset();
		}
		else if (player2.Points >= PointsPerGame)
		{
			Reset();
		}
	}

	public static void Draw()
	{
		Graphics.Begin();

		// draw dots down center
		var dotsCount = Viewport.Height / 20;
		for (var i = 0; i < dotsCount; i++)
		{
			const int dotSize = 8;
			Graphics.DrawRectangle(
				new Rectangle((int)(Viewport.Width * 0.5f - dotSize * 0.5f), 5 + i * 20, dotSize, dotSize),
				Color.White * 0.2f);
		}

		ref var currentState = ref CurrentGameState;
		ref var previousState = ref PreviousGameState;

		// draw player1 paddle and update hit box
		ref var player1 = ref currentState.Player1;
		var player1PaddlePosition = Vector2.Lerp(
			currentState.Player1.Paddle.Position,
			previousState.Player1.Paddle.Position, 0.5f);
		var player1PaddleHitBox = HitBox(player1PaddlePosition, player1.Paddle.Width, player1.Paddle.Height);
		player1.Paddle.HitBox = player1PaddleHitBox;
		Graphics.DrawRectangle(player1PaddleHitBox, Color.White);

		// draw player1 paddle and update hit box
		ref var player2 = ref currentState.Player2;
		var player2PaddlePosition = Vector2.Lerp(
			currentState.Player2.Paddle.Position,
			previousState.Player2.Paddle.Position, 0.5f);
		var player2PaddleHitBox = HitBox(player2PaddlePosition, player2.Paddle.Width, player2.Paddle.Height);
		player2.Paddle.HitBox = player2PaddleHitBox;
		Graphics.DrawRectangle(player2PaddleHitBox, Color.White);

		// draw ball and update hit box
		ref var ball = ref currentState.Ball;
		var ballPosition = Vector2.Lerp(
			currentState.Ball.Position,
			previousState.Ball.Position, 0.5f);
		var ballHitBox = HitBox(ballPosition, ball.Width, ball.Height);
		ball.HitBox = ballHitBox;
		Graphics.DrawRectangle(ballHitBox, Color.White);

		// draw current game points
		for (var i = 0; i < player1.Points; i++)
		{
			Graphics.DrawRectangle(new Rectangle((int)(Viewport.Width * 0.5f - 25 - i * 12), 10, 10, 10), Color.White * 1.0f);
		}

		for (var i = 0; i < player2.Points; i++)
		{
			Graphics.DrawRectangle(new Rectangle((int)(Viewport.Width * 0.5f + 15 + i * 12), 10, 10, 10), Color.White * 1.0f);
		}

		// // draw the position vector of player 1 for debugging purposes
		// Graphics.DrawRectangle(new Rectangle((int)CurrentGameState.Player1.Paddle.Position.X - 2, (int)CurrentGameState.Player1.Paddle.Position.Y - 2, 4, 4), Color.Red);
		// // draw the position vector of player 2 for debugging purposes
		// Graphics.DrawRectangle(new Rectangle((int)CurrentGameState.Player2.Paddle.Position.X - 2, (int)CurrentGameState.Player2.Paddle.Position.Y - 2, 4, 4), Color.Red);

		Graphics.End();
	}

	private static Rectangle HitBox(Vector2 position, int width, int height)
	{
		return new Rectangle(
			(int)(position.X - width * 0.5f),
			(int)(position.Y - height * 0.5f),
			width,
			height);
	}
}
