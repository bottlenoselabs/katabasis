// Copyright (c) Bottlenose Labs Inc. (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples
{
	public static class AutoPong
	{
		// AUTOPONG by MrGrak MiT License 2022

		public struct PlayerState
		{
			public int Points;
		}

		public static SpriteBatch SpriteBatch = null!;

		public static int GameBoundsX = 1280;
		public static int GameBoundsY = 720;

		public static Rectangle PaddleLeft;
		public static Rectangle PaddleRight;

		public static Rectangle Ball;
		public static Vector2 BallVelocity;
		public static Vector2 BallPosition;
		public static float BallSpeed = 15.0f;

		public static Texture2D Texture = null!;

		private static readonly Random Rand = new();
		public static byte HitCounter;

		public static PlayerState Player1State;
		public static PlayerState Player2State;
		public static int PointsPerGame = 4;

		public static AudioSource Sound = null!;
		public static int JingleCounter;

		public static void LoadContent()
		{
			Texture = new Texture2D(1, 1);
			Texture.SetData(new[] { Color.White });
			Sound = new AudioSource();
		}

		public static void Reset()
		{
			const int PADDLE_HEIGHT = 100;
			PaddleLeft = new Rectangle(0 + 10, 150, 20, PADDLE_HEIGHT);
			PaddleRight = new Rectangle(GameBoundsX - 30, 150, 20, PADDLE_HEIGHT);

			BallPosition = new Vector2(GameBoundsX / 3.0f, 200);
			Ball = new Rectangle((int)BallPosition.X, (int)BallPosition.Y, 10, 10);
			BallVelocity = new Vector2(1, 0.1f);

			Player1State.Points = 0;
			Player2State.Points = 0;
			JingleCounter = 0;
		}

		public static void Update()
		{
			UpdateBall();
			SimulateLeftPaddleInput();
			SimulateRightPaddleInput();
			CheckForPlayersState();
			PlayResetJingle();
		}

		private static void UpdateBall()
		{
			//limit how fast ball can move each frame
			float maxVelocity = 1.5f;
			if (BallVelocity.X > maxVelocity)
			{
				BallVelocity.X = maxVelocity;
			}
			else if (BallVelocity.X < -maxVelocity)
			{
				BallVelocity.X = -maxVelocity;
			}

			if (BallVelocity.Y > maxVelocity)
			{
				BallVelocity.Y = maxVelocity;
			}
			else if (BallVelocity.Y < -maxVelocity)
			{
				BallVelocity.Y = -maxVelocity;
			}

			//apply velocity to position
			BallPosition.X += BallVelocity.X * BallSpeed;
			BallPosition.Y += BallVelocity.Y * BallSpeed;

			//check for collision with paddles
			HitCounter++;
			if (HitCounter > 10)
			{
				if (PaddleLeft.Intersects(Ball))
				{
					BallVelocity.X *= -1;
					BallVelocity.Y *= 1.1f;
					HitCounter = 0;
					BallPosition.X = PaddleLeft.X + PaddleLeft.Width + 10;
					PlayWave(220.0f, 50, WaveType.Sin, Sound, 0.3f);
				}

				if (PaddleRight.Intersects(Ball))
				{
					BallVelocity.X *= -1;
					BallVelocity.Y *= 1.1f;
					HitCounter = 0;
					BallPosition.X = PaddleRight.X - 10;
					PlayWave(220.0f, 50, WaveType.Sin, Sound, 0.3f);
				}
			}

			//bounce on screen
			if (BallPosition.X < 0) //point for right
			{
				BallPosition.X = 1;
				BallVelocity.X *= -1;
				Player2State.Points++;
				PlayWave(440.0f, 50, WaveType.Square, Sound, 0.3f);
			}
			else if (BallPosition.X > GameBoundsX) //point for left
			{
				BallPosition.X = GameBoundsX - 1;
				BallVelocity.X *= -1;
				Player1State.Points++;
				PlayWave(440.0f, 50, WaveType.Square, Sound, 0.3f);
			}

			if (BallPosition.Y < 0 + 10) //limit to minimum Y pos
			{
				BallPosition.Y = 10 + 1;
				BallVelocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
			}
			else if (BallPosition.Y > GameBoundsY - 10) //limit to maximum Y pos
			{
				BallPosition.Y = GameBoundsY - 11;
				BallVelocity.Y *= -(1 + Rand.Next(-100, 101) * 0.005f);
			}
		}

		private static void SimulateLeftPaddleInput()
		{
			//simple ai, not very good, moves random amount each frame
			var amount = Rand.Next(0, 6);
			var paddleCenter = PaddleLeft.Y + PaddleLeft.Height / 2;
			if (paddleCenter < BallPosition.Y - 20)
			{
				PaddleLeft.Y += amount;
			}
			else if (paddleCenter > BallPosition.Y + 20)
			{
				PaddleLeft.Y -= amount;
			}

			LimitPaddle(ref PaddleLeft);
		}

		private static void SimulateRightPaddleInput()
		{
			//simple ai, better than left, moves % each frame
			var paddleCenter = PaddleRight.Y + PaddleRight.Height / 2;
			if (paddleCenter < BallPosition.Y - 20)
			{
				PaddleRight.Y -= (int)((paddleCenter - BallPosition.Y) * 0.08f);
			}
			else if (paddleCenter > BallPosition.Y + 20)
			{
				PaddleRight.Y += (int)((BallPosition.Y - paddleCenter) * 0.08f);
			}

			LimitPaddle(ref PaddleRight);
		}

		private static void CheckForPlayersState()
		{
			if (Player1State.Points >= PointsPerGame)
			{
				Reset();
			}
			else if (Player2State.Points >= PointsPerGame)
			{
				Reset();
			}
		}

		private static void PlayResetJingle()
		{
			//use jingle counter as a timeline to play notes
			JingleCounter++;

			int speed = 7; //play a short A minor chord
			if (JingleCounter == speed * 1)
			{
				PlayWave(440.0f, 100, WaveType.Sin, Sound, 0.2f);
			}
			else if (JingleCounter == speed * 2)
			{
				PlayWave(523.25f, 100, WaveType.Sin, Sound, 0.2f);
			}
			else if (JingleCounter == speed * 3)
			{
				PlayWave(659.25f, 100, WaveType.Sin, Sound, 0.2f);
			}
			else if (JingleCounter == speed * 4)
			{
				PlayWave(783.99f, 100, WaveType.Sin, Sound, 0.2f);
			}
			//only play this jingle once
			else if (JingleCounter > speed * 4)
			{
				JingleCounter = int.MaxValue - 1;
			}
		}

		public static void Draw()
		{
			SpriteBatch.Begin(SpriteSortMode.Deferred,
				BlendState.AlphaBlend, SamplerState.
				PointClamp, DepthStencilState.None,
				RasterizerState.CullNone);

			//draw dots down center
			var total = GameBoundsY / 20;
			for (var i = 0; i < total; i++)
			{
				DrawRectangle(new Rectangle(GameBoundsX / 2 - 4, 5 + (i * 20), 8, 8), Color.White * 0.2f);
			}

			//draw paddles
			DrawRectangle(PaddleLeft, Color.White);
			DrawRectangle(PaddleRight, Color.White);

			//draw ball
			Ball.X = (int)BallPosition.X;
			Ball.Y = (int)BallPosition.Y;
			DrawRectangle(Ball, Color.White);

			//draw current game points
			for (var i = 0; i < Player1State.Points; i++)
			{
				DrawRectangle(new Rectangle((GameBoundsX / 2 - 25) - i * 12, 10, 10, 10), Color.White * 1.0f);
			}

			for (var i = 0; i < Player2State.Points; i++)
			{
				DrawRectangle(new Rectangle((GameBoundsX / 2 + 15) + i * 12, 10, 10, 10), Color.White * 1.0f);
			}

			SpriteBatch.End();
		}

		public static void DrawRectangle(Rectangle rec, Color color)
		{
			Vector2 pos = new Vector2(rec.X, rec.Y);
			SpriteBatch.Draw(Texture, pos, rec, color * 1.0f,
				0, Vector2.Zero, 1.0f,
				SpriteEffects.None, 0.00001f);
		}

		public static void LimitPaddle(ref Rectangle paddle)
		{
			//limit how far paddles can travel on Y axis so they dont exceed top or bottom
			if (paddle.Y < 10) { paddle.Y = 10; }
			else if (paddle.Y + paddle.Height > GameBoundsY - 10)
			{ paddle.Y = GameBoundsY - 10 - paddle.Height; }
		}

		public static void PlayWave(double freq, short durMs, WaveType wt, AudioSource src, float volume)
		{
			src.Instance.Stop();

			src.BufferSize = src.Instance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(durMs));
			src.Buffer = new byte[src.BufferSize];

			var size = src.BufferSize - 1;
			for (var i = 0; i < size; i += 2)
			{
				var time = (double)src.TotalTime / src.SampleRate;

				short currentSample = wt switch
				{
					WaveType.Sin => (short)(Math.Sin(2 * Math.PI * freq * time) * short.MaxValue * volume),
					WaveType.Tan => (short)(Math.Tan(2 * Math.PI * freq * time) * short.MaxValue * volume),
					WaveType.Square => (short)(Math.Sign(Math.Sin(2 * Math.PI * freq * time)) * (double)short.MaxValue * volume),
					WaveType.Noise => (short)(Rand.Next(-short.MaxValue, short.MaxValue) * volume),
					_ => 0
				};

				src.Buffer[i] = (byte)(currentSample & 0xFF);
				src.Buffer[i + 1] = (byte)(currentSample >> 8);
				src.TotalTime += 2;
			}

			src.Instance.SubmitBuffer(src.Buffer);
			src.Instance.Play();
		}
	}

	public enum WaveType { Sin, Tan, Square, Noise }

	public class AudioSource
	{
		public int SampleRate = 48000;
		public DynamicSoundEffectInstance Instance;
		public byte[] Buffer;
		public int BufferSize;
		public int TotalTime;

		public AudioSource()
		{
			Instance = new DynamicSoundEffectInstance(SampleRate, AudioChannels.Mono);
			BufferSize = Instance.GetSampleSizeInBytes(TimeSpan.FromMilliseconds(500));
			Buffer = new byte[BufferSize];
			Instance.Volume = 0.4f;
			Instance.IsLooped = false;
		}
	}

	public class App : Game
	{
		public App()
		{
			Window.Title = "Katabasis Samples; AutoPong";
			GraphicsDeviceManager.Instance.PreferredBackBufferWidth = AutoPong.GameBoundsX;
			GraphicsDeviceManager.Instance.PreferredBackBufferHeight = AutoPong.GameBoundsY;
		}

		protected override void LoadContent()
		{
			IsMouseVisible = true;
			AutoPong.SpriteBatch = new SpriteBatch();
			AutoPong.LoadContent();
			AutoPong.Reset();
		}

		protected override void Update(GameTime gameTime)
		{
			AutoPong.Update();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			AutoPong.Draw();
		}

	}
}
