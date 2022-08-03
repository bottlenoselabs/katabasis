// Copyright (c) BottlenoseLabs (https://github.com/bottlenoselabs). All rights reserved.
// Licensed under the MS-PL license. See LICENSE file in the Git repository root directory for full license information.
using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Diagnostics;

namespace bottlenoselabs.Katabasis.Samples
{
	public static class AutoTetris
	{
		//AUTO TETRIS by MrGrak MiT License 2022

		//TODO:
		//figure out how to remove rows of blocks that align
		//this can be done based on Y position comparison,
		//but it's not elegant/clean

		//if there is block in far left position, place hitbox
		//across that row. check for 10 collisions. if there
		//were 10 collisions, then remove hit blocks. 
		//move blocks above hitbox down 1 tile
		//repeat for all blocks in left position, starting at top
		//this row completion needs to happen on a timer, it
		//should not happen all at once

		//extend example with all tetris shapes
		//invent new tetris shapes too, always 4 blocks

		//add soundfx for blocks moving down
		//add soundfx for shape rotating
		//add soundfx for completing a row
		//add soundfx for losing game/reset

		//play musical jingle that is tetris theme

		//add different colored levels, shown at bottom of window
		//change colors per level, increase speed of gravity too

		//write ai that will play tetris on it's own if player doesn't
		//supply inputs after a timer has expired (auto plays)



		public static SpriteBatch SB;
		public static Game GAME;

		public static int TileSize = 16;
		public static int GameTileWidth = 14;
		public static int GameTileHeight = 24;
		public static int GameBoundsX = TileSize * GameTileWidth;
		public static int GameBoundsY = TileSize * GameTileHeight;

		public static Texture2D Texture;
		public static Rectangle DrawRec = new Rectangle(0, 0, 3, 3);
		static Random Rand = new();

		public enum ShapeType { None, T, L, Z, Square, Line }
		public static ShapeType Stype = ShapeType.None;
		public static int ShapeRotation = 0;
		public static int CurrentShapeX = 0;
		public static int CurrentShapeY = 0;

		public static List<Rectangle> CurrentShape;
		public static List<Rectangle> PlacedShapes;

		public static int GravityTimer = 0;
		public static int GravityTargetFrames = 30;
		
		public static KeyboardState CurrentKeyboardState;
		public static KeyboardState PreviousKeyboardState;
		public static int InputLimiter = 0;


		public static void Reset()
		{
			if (Texture == null)
			{   //create texture to draw with if it doesn't exist
				Texture = new Texture2D(1, 1);
				Texture.SetData<Color>(new Color[] { Color.White });
			}

			if (CurrentShape == null)
			{   //shapes always have 4 recs
				CurrentShape = new List<Rectangle>();
				CurrentShape.Add(new Rectangle(-100, -100, TileSize, TileSize));
				CurrentShape.Add(new Rectangle(-100, -100, TileSize, TileSize));
				CurrentShape.Add(new Rectangle(-100, -100, TileSize, TileSize));
				CurrentShape.Add(new Rectangle(-100, -100, TileSize, TileSize));
			}

			PlacedShapes = new List<Rectangle>();

			CurrentShapeX = TileSize * 4;
			CurrentShapeY = TileSize;
			Stype = ShapeType.None;
			GravityTimer = 0;
		}

		public static void Update()
		{
			bool CheckCollisions = false;


			#region Handle Input

			//update keyboard input
			PreviousKeyboardState = CurrentKeyboardState;
			CurrentKeyboardState = Keyboard.GetState();

			//movement deltas
			int MoveX = 0; int MoveY = 0;

			//limit how often specific input is applied, for game feel
			InputLimiter++;
			if(InputLimiter >= 7)
			{
				InputLimiter = 0;

				//move current shape right
				if (CurrentKeyboardState.IsKeyDown(Keys.Right))
				{
					MoveX++; //we want to move right
					CheckCollisions = true;
				}
				//move current shape left
				else if (CurrentKeyboardState.IsKeyDown(Keys.Left))
				{
					MoveX--; //we want to move left
					CheckCollisions = true;
				}
				//move current shape down
				else if (CurrentKeyboardState.IsKeyDown(Keys.Down))
				{
					MoveY++; //we want to move down
					CheckCollisions = true;
					InputLimiter = 4; //speed up this input a bit
				}
			}

			//rotate shape clockwise
			if (CurrentKeyboardState.IsKeyDown(Keys.Up) &&
				PreviousKeyboardState.IsKeyUp(Keys.Up))
			{
				ShapeRotation++; //limit to 4 rotations (0-3)
				if (ShapeRotation > 3) { ShapeRotation = 0; }
				CheckCollisions = true;
			}

			#endregion


			#region Apply Gravity, spawn new current shape

			GravityTimer++;
			if (GravityTimer >= GravityTargetFrames)
			{
				GravityTimer = 0;
				CheckCollisions = true;

				if (Stype == ShapeType.None)
				{   //spawn a new current shape
					SpawnNewCurrentShape();
				}
				else
				{   //periodically apply gravity
					if (MoveY == 0) { MoveY++; }
				}
			}

			#endregion


			#region Check Collisions

			if (CheckCollisions)
			{
				//apply movement deltas multipled by tilesize
				CurrentShapeX += TileSize * MoveX;
				CurrentShapeY += TileSize * MoveY;

				//move current shape recs into new position
				UpdateCurrentShape();

				//check for collision with ground and placed blocks
				bool Collision = false;

				//check for collision with 'sides' of play area
				if (CheckVerticalWalls())
				{   //undo movement delta on X axis, allow sliding
					CurrentShapeX -= TileSize * MoveX;
					MoveX = 0; //clear movement delta
					UpdateCurrentShape();
				}

				//check for collision with placed blocks + ground
				for (int i = 0; i < CurrentShape.Count; i++)
				{
					//ground check
					if (CurrentShape[i].Y >= TileSize * (GameTileHeight - 4))
					{ Collision = true; goto CollisionResolution; }
					//placed shapes check
					for (int j = 0; j < PlacedShapes.Count; j++)
					{   //note early exit from loops via goto
						if (CurrentShape[i].Intersects(PlacedShapes[j]))
						{ Collision = true; goto CollisionResolution; }
					}
				}

				CollisionResolution:
				if (Collision)
				{
					//undo any movement deltas
					CurrentShapeX -= TileSize * MoveX;
					CurrentShapeY -= TileSize * MoveY;

					//move current shape recs into new position
					UpdateCurrentShape();

					//copy current shape to place shapes list
					for (int i = 0; i < CurrentShape.Count; i++)
					{ PlacedShapes.Add(CurrentShape[i]); }

					//clear current shape
					Stype = ShapeType.None;
				}
			}

			#endregion


			#region Check For GameOver / Reset State

			for (int j = 0; j < PlacedShapes.Count; j++)
			{
				//check for exceeding top of play area
				if (PlacedShapes[j].Y < TileSize * 1)
				{ Reset(); return; }
			}

			#endregion


		}

		public static void Draw()
		{
			SB.Begin(SpriteSortMode.Deferred,
				BlendState.AlphaBlend, SamplerState.
				PointClamp, DepthStencilState.None,
				RasterizerState.CullNone);

			//draw center playing area white outline
			DrawRectangle(new Rectangle(TileSize, TileSize, 
				TileSize * 12, TileSize * 20), Color.White * 0.1f);

			//draw center playing area black interior
			DrawRectangle(new Rectangle(TileSize * 2, TileSize * 1, 
				TileSize * 10, TileSize * 19), Color.Black * 1.0f);

			//draw current shape
			if(Stype != ShapeType.None)
			{
				for (int i = 0; i < CurrentShape.Count; i++)
				{ DrawRectangle(CurrentShape[i], Color.Red); }
			}

			//draw placed shapes
			for (int i = 0; i < PlacedShapes.Count; i++)
			{ DrawRectangle(PlacedShapes[i], Color.Blue); }

			SB.End();
		}

		public static void DrawRectangle(Rectangle Rec, Color color)
		{
			Vector2 pos = new Vector2(Rec.X, Rec.Y);
			SB.Draw(Texture, pos, Rec, color * 1.0f,
				0, Vector2.Zero, 1.0f,
				SpriteEffects.None, 0.00001f);
		}

		public static void SpawnNewCurrentShape()
		{
			//place new shape at top of play area
			CurrentShapeX = TileSize * ((GameTileWidth - 1) / 2);
			CurrentShapeY = TileSize;
			//choose a type of shape
			Stype = ShapeType.T;
		}

		public static void UpdateCurrentShape()
		{
			//update position of all current shape recs
			Rectangle Rec0 = CurrentShape[0];
			Rectangle Rec1 = CurrentShape[1];
			Rectangle Rec2 = CurrentShape[2];
			Rectangle Rec3 = CurrentShape[3];


			#region T Shape

			if (Stype == ShapeType.T)
			{
				if (ShapeRotation == 0)
				{
					//left
					Rec0.X = CurrentShapeX - TileSize;
					Rec0.Y = CurrentShapeY;
					//center
					Rec1.X = CurrentShapeX;
					Rec1.Y = CurrentShapeY;
					//right
					Rec2.X = CurrentShapeX + TileSize;
					Rec2.Y = CurrentShapeY;
					//below
					Rec3.X = CurrentShapeX;
					Rec3.Y = CurrentShapeY + TileSize;
				}
				else if (ShapeRotation == 1)
				{
					//left
					Rec0.X = CurrentShapeX - TileSize;
					Rec0.Y = CurrentShapeY;
					//center
					Rec1.X = CurrentShapeX;
					Rec1.Y = CurrentShapeY;
					//above
					Rec2.X = CurrentShapeX;
					Rec2.Y = CurrentShapeY - TileSize;
					//below
					Rec3.X = CurrentShapeX;
					Rec3.Y = CurrentShapeY + TileSize;
				}
				else if (ShapeRotation == 2)
				{
					//left
					Rec0.X = CurrentShapeX - TileSize;
					Rec0.Y = CurrentShapeY;
					//center
					Rec1.X = CurrentShapeX;
					Rec1.Y = CurrentShapeY;
					//above
					Rec2.X = CurrentShapeX;
					Rec2.Y = CurrentShapeY - TileSize;
					//right
					Rec3.X = CurrentShapeX + TileSize;
					Rec3.Y = CurrentShapeY;
				}
				else if (ShapeRotation == 3)
				{
					//below
					Rec0.X = CurrentShapeX;
					Rec0.Y = CurrentShapeY + TileSize;
					//center
					Rec1.X = CurrentShapeX;
					Rec1.Y = CurrentShapeY;
					//above
					Rec2.X = CurrentShapeX;
					Rec2.Y = CurrentShapeY - TileSize;
					//right
					Rec3.X = CurrentShapeX + TileSize;
					Rec3.Y = CurrentShapeY;
				}
			}

			#endregion


			//write updated pos back to current shape
			CurrentShape[0] = Rec0;
			CurrentShape[1] = Rec1;
			CurrentShape[2] = Rec2;
			CurrentShape[3] = Rec3;
		}

		public static bool CheckVerticalWalls()
		{
			for (int i = 0; i < CurrentShape.Count; i++)
			{
				int Xpos = CurrentShape[i].X;

				if (Xpos < TileSize * 2) { return true; }
				else if (Xpos + CurrentShape[i].Width > TileSize * (GameTileWidth - 2))
				{ return true; }
			}

			return false;
		}

	}

	public class App : Game
	{
		public App()
		{
			Window.Title = "Katabasis Samples; AutoTetris";
			GraphicsDeviceManager.Instance.PreferredBackBufferWidth = AutoTetris.GameBoundsX;
			GraphicsDeviceManager.Instance.PreferredBackBufferHeight = AutoTetris.GameBoundsY;
		}

		protected override void LoadContent()
		{
			AutoTetris.GAME = this;
			IsMouseVisible = true;
			AutoTetris.SB = new SpriteBatch();
			AutoTetris.Reset();
		}

		protected override void Update(GameTime gameTime)
		{
			AutoTetris.Update();
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);
			AutoTetris.Draw();
		}

	}
}
