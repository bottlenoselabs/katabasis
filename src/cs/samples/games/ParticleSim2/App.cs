// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.IO;
using System.Numerics;

namespace bottlenoselabs.Katabasis.Samples
{
	// PARTICLE SIM 2 by MrGrak MiT License 2022
	// particles follow around mouse cursor, but are also pushed away when too close
	// left click hold to run sim, right click to reset sim

	public enum ParticleID : byte { Inactive, Orange, Teal }

	public struct Particle
	{   //7 * 4 = 28 bytes, aligned on 4 bytes
		public ParticleID Id; //byte
							  //padding of 3 bytes
		public float X, Y; //current position
		public float accX, accY; //accelerations
		public float preX, preY; //last position
	}

	public static class ParticleSystem
	{
		public static GraphicsDeviceManager GDM;
		public static SpriteBatch SB;

		public const int size = 50000;
		public static Particle[] data = new Particle[size];

		static Random Rand = new Random();
		public static Texture2D texture;
		public static Rectangle drawRec = new Rectangle(0, 0, 3, 3);
		public static int lastActive = 0;
		public static MouseState MS;

		public static void Reset()
		{
			//acts as a constructor as well
			if (texture == null)
			{   //set up a general texture we can draw dots with, if required
				texture = new Texture2D(1, 1);
				texture.SetData<Color>(new Color[] { Color.White });
			}

			//reset particles to inactive state
			for (int i = 0; i < size; i++)
			{ data[i].Id = ParticleID.Inactive; }
			lastActive = 0; //reset index too

			//spawn all particles in random locations on screen
			for (int i = 0; i < size; i++)
			{
				if (i % 2 == 0)
				{
					Spawn(ParticleID.Orange,
						Rand.Next(0, 1280 / 2),
						Rand.Next(0, 720), 0, 0);
				}
				else
				{
					Spawn(ParticleID.Teal,
						Rand.Next(1280 / 2, 1280),
						Rand.Next(0, 720), 0, 0);
				}
			}
		}

		public static void Spawn(
			ParticleID ID,          //type of particle to spawn
			float X, float Y,       //spawn x, y position 
			float accX, float accY  //initial x, y acceleration
			)
		{
			Particle P = new Particle();
			P.X = X; P.preX = X;
			P.Y = Y; P.preY = Y;
			P.accX = accX;
			P.accY = accY;
			P.Id = ID;
			//save P to heap
			data[lastActive] = P;
			lastActive++;
			//bound dead index to array size
			if (lastActive >= size)
			{ lastActive = size - 1; }
		}

		public static void Update()
		{
			//get mouse state
			MS = Mouse.GetState();

			//reset with r click
			if (MS.RightButton == ButtonState.Pressed) { Reset(); }

			//player must hold left to sim
			if (MS.LeftButton != ButtonState.Pressed) { return; }

			//shorten these for later use
			int width = 1280;
			int height = 720;

			#region Step 1 - update particle system

			for (int i = lastActive - 1; i >= 0; i--)
			{
				Particle P = data[i];

				//bound to screen
				if (P.X < 0) { P.X = 0; }
				else if (P.X > width) { P.X = width; P.accX = 0; }

				if (P.Y < 0) { P.Y = 0; }
				else if (P.Y > height) { P.Y = height; P.accY = 0; }

				#region Pull Towards Mouse (or center)

				float distX = P.X - MS.X;
				float distY = P.Y - MS.Y;
				float distance = (float)Math.Sqrt((distX * distX) + (distY * distY));

				//very cheap gravity
				P.accX -= distX / distance * 0.03f;
				P.accY -= distY / distance * 0.03f;

				if (distance <= 100) //radius of mouse influence
				{
					//push away from center
					if (MS.X < P.X)
					{ P.accX += 1.1f; }
					else { P.accX -= 1.1f; }

					if (MS.Y < P.Y)
					{ P.accY += 1.1f; }
					else { P.accY -= 1.1f; }
				}

				#endregion

				//calculate velocity using current and previous pos
				float velocityX = P.X - P.preX;
				float velocityY = P.Y - P.preY;

				//store previous positions (the current positions)
				P.preX = P.X;
				P.preY = P.Y;

				//set next positions using current + velocity + acceleration
				P.X = P.X + velocityX + P.accX;
				P.Y = P.Y + velocityY + P.accY;

				//clear accelerations
				P.accX = 0; P.accY = 0;

				//write local to heap
				data[i] = P;
			}

			#endregion

		}

		public static void Draw()
		{   //open spritebatch and draw active particles
			SB.Begin(SpriteSortMode.Deferred, BlendState.Additive,
				SamplerState.PointClamp, DepthStencilState.None,
				RasterizerState.CullNone);

			int s = lastActive; //size
			for (int i = 0; i < s; i++)
			{   //if particle is active, draw it
				if (data[i].Id > 0)
				{
					Color ColorToDraw = Color.Orange;
					if (data[i].Id == ParticleID.Teal)
					{ ColorToDraw = Color.Teal; }

					Vector2 pos = new Vector2(data[i].X, data[i].Y);
					SB.Draw(texture,
						pos,
						drawRec,
						ColorToDraw,
						0,
						Vector2.Zero,
						1.0f, //scale
						SpriteEffects.None,
						i * 0.00001f);
				}   //layer particles based on index
			}

			SB.End();
		}

	}

	public class App : Game
	{
		public App()
		{
			Window.Title = "Katabasis Samples; ParticleSim";
			GraphicsDeviceManager.Instance.PreferredBackBufferWidth = 1280;
			GraphicsDeviceManager.Instance.PreferredBackBufferHeight = 720;
		}

		protected override void LoadContent()
		{
			IsMouseVisible = true;
			ParticleSystem.SB = new SpriteBatch();
			ParticleSystem.Reset();
		}

		protected override void Update(GameTime gameTime)
		{
			ParticleSystem.Update();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			ParticleSystem.Draw();
		}

	}
}
