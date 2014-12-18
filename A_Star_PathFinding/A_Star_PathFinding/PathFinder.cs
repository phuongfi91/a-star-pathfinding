using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace A_Star_PathFinding
{
	public enum SelectedTool { Start, End, D5, D10, D20, D40, Wall, Erase }
	public enum SelectedAlgorithm { AStar, Dijkstra }
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class PathFinder : Microsoft.Xna.Framework.Game
	{
		SpriteBatch spriteBatch;
		static public GraphicsDeviceManager Graphics;
		static public Texture2D TileSheet;
		static public SpriteFont Font;
		static public bool IsRunning;
		static public bool IsFinished;
		static public SelectedTool SelectedTool;
		static public SelectedAlgorithm SelectedAlgorithm;
		static public Point Initiation;
		static public Point Destination;
		static public Camera Camera;
		static public Map Map;

		// A Star Helpers
		static public List<Point> closedSet;
		static public List<Point> openedSet;
		static public int[] prev;
		static public int[] g;
		static public int[] h;
		static public int[] f;

		public PathFinder()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			this.IsMouseVisible = true;
			this.Window.Title = "A Star Path Finding - By Phuong D. Nguyen";
			Graphics.IsFullScreen = false;
			Graphics.PreferredBackBufferWidth = 800;
			Graphics.PreferredBackBufferHeight = 512;
			Graphics.ApplyChanges();
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			Font = Content.Load<SpriteFont>(@"Fonts\Font");
			TileSheet = Content.Load<Texture2D>(@"Textures\Square");
			Map = new Map(32, 32);
			Camera = new Camera(this); Camera.Speed = 10;
			Camera.WholeWorldRectangle = new Rectangle(0, 0,
						Map.MapWidth * Map.TileWidth,
						Map.MapHeight * Map.TileHeight);
			Initiation = new Point(0, 0);
			Destination = new Point(31, 31);

			base.Initialize();
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
		}

		/// <summary>
		/// UnloadContent will be called once per game and is the place to unload
		/// all content.
		/// </summary>
		protected override void UnloadContent()
		{
			// TODO: Unload any non ContentManager content here
		}

		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input, and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			// TODO: Add your update logic here
			HandleInput();
			Camera.Update(gameTime);

			// Shortest Path Algorithm
			if (!IsRunning && Keyboard.GetState().IsKeyDown(Keys.Enter))
			{
				IsRunning = true;
				IsFinished = false;
				prev = new int[Map.MapHeight * Map.MapWidth];
				g = new int[Map.MapWidth * Map.MapHeight];
				h = new int[Map.MapWidth * Map.MapHeight];
				f = new int[Map.MapWidth * Map.MapHeight];
				closedSet = new List<Point>();
				openedSet = new List<Point>();
				openedSet.Add(Initiation);
				g[Node(Initiation)] = 0;
				h[Node(Initiation)] = ManhattanDistance(Initiation);
				f[Node(Initiation)] = g[Node(Initiation)] + h[Node(Initiation)];
			}
			else if (!IsRunning)
			{
				IsFinished = true;
			}

			if (IsRunning && openedSet.Count != 0)
			{
				Point x = openedSet[0];
				for (int i = 1; i < openedSet.Count; ++i)
				{
					if (f[Node(openedSet[i])] < f[Node(x)])
					{
						x = openedSet[i];
					}
				}
				if (x == Destination)
				{
					IsRunning = false;
					return;
				}
				openedSet.Remove(x);
				closedSet.Add(x);
				Point y = Point.Zero;
				for (int i = -1; i < 2; ++i)
					for (int j = -1; j < 2; ++j)
					{
						if (x.X + i < 0
							||
							x.Y + j < 0
							||
							x.X + i >= Map.MapWidth
							||
							x.Y + j >= Map.MapHeight)
						{
							continue;
						}

						y = new Point(x.X + i, x.Y + j);
						if (closedSet.Contains(y)
							||
							Map.IsWallTile(y))
						{
							continue;
						}
						int tentative_g = g[Node(x)] + Map.MapSquares[y.X, y.Y];
						bool tentativeIsBetter;

						if (!openedSet.Contains(y))
						{
							openedSet.Add(y);
							tentativeIsBetter = true;
						}
						else if (tentative_g < g[Node(y)])
						{
							tentativeIsBetter = true;
						}
						else tentativeIsBetter = false;

						if (tentativeIsBetter)
						{
							prev[Node(y)] = Node(x);
							g[Node(y)] = tentative_g;
							h[Node(y)] = ManhattanDistance(y);
							f[Node(y)] = g[Node(y)] + h[Node(y)];
						}
					}
			}

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			// TODO: Add your drawing code here
			spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

			Map.Draw(gameTime, spriteBatch);
			Color[] colors1 = new Color[8];
			for (int i = 0; i < colors1.Length; ++i)
			{
				colors1[i] = Color.White;
			}
			colors1[(int)SelectedTool] = Color.Yellow;

			Color[] colors2 = new Color[2];
			for (int i = 0; i < colors2.Length; ++i)
			{
				colors2[i] = Color.White;
			}
			colors2[(int)SelectedAlgorithm] = Color.DeepSkyBlue;

			spriteBatch.DrawString(Font, "Tools", new Vector2(520, 20), Color.White);
			spriteBatch.DrawString(Font, "1. Start", new Vector2(520, 40), colors1[0]);
			spriteBatch.DrawString(Font, "2. End", new Vector2(520, 60), colors1[1]);
			spriteBatch.DrawString(Font, "3. D5", new Vector2(520, 80), colors1[2]);
			spriteBatch.DrawString(Font, "4. D10", new Vector2(520, 100), colors1[3]);
			spriteBatch.DrawString(Font, "5. D20", new Vector2(520, 120), colors1[4]);
			spriteBatch.DrawString(Font, "6. D40", new Vector2(520, 140), colors1[5]);
			spriteBatch.DrawString(Font, "7. Wall", new Vector2(520, 160), colors1[6]);
			spriteBatch.DrawString(Font, "8. Erase", new Vector2(520, 180), colors1[7]);

			spriteBatch.DrawString(Font, "Press Enter to Run", new Vector2(530, 220), Color.White);
			spriteBatch.DrawString(Font, "Press Esc to Clear", new Vector2(530, 240), Color.White);
			spriteBatch.DrawString(Font, "Press Ctrl-S to Save", new Vector2(530, 260), Color.White);
			spriteBatch.DrawString(Font, "Press Ctrl-L to Load", new Vector2(530, 280), Color.White);

			spriteBatch.DrawString(Font, "9. A* Algorithm", new Vector2(520, 320), colors2[0]);
			spriteBatch.DrawString(Font, "0. Dijkstra Algorithm", new Vector2(520, 340), colors2[1]);

			if (g != null)
			{
				spriteBatch.DrawString(Font, "Shortest Path = " + g[Node(Destination)], new Vector2(520, 380), Color.White);
			}

			spriteBatch.End();

			base.Draw(gameTime);
		}

		static public int Node(Point point)
		{
			return point.X + point.Y * Map.MapWidth;
		}

		private int ManhattanDistance(Point point)
		{
			switch (SelectedAlgorithm)
			{
				case SelectedAlgorithm.AStar:
					return Math.Abs(point.X - Destination.X) + Math.Abs(point.Y - Destination.Y);
				default:
					return 0;
			}
		}

		// Xử lý trường hợp người chơi nhấn Esc
		private void HandleInput()
		{
			KeyboardState keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.Up))
			{
				Camera.Move(new Vector2(0, -Camera.Speed));
			}

			if (keyboardState.IsKeyDown(Keys.Down))
			{
				Camera.Move(new Vector2(0, Camera.Speed));
			}

			if (keyboardState.IsKeyDown(Keys.Left))
			{
				Camera.Move(new Vector2(-Camera.Speed, 0));
			}

			if (keyboardState.IsKeyDown(Keys.Right))
			{
				Camera.Move(new Vector2(Camera.Speed, 0));
			}

			if (keyboardState.IsKeyDown(Keys.D1))
			{
				SelectedTool = SelectedTool.Start;
			}

			if (keyboardState.IsKeyDown(Keys.D2))
			{
				SelectedTool = SelectedTool.End;
			}

			if (keyboardState.IsKeyDown(Keys.D3))
			{
				SelectedTool = SelectedTool.D5;
			}

			if (keyboardState.IsKeyDown(Keys.D4))
			{
				SelectedTool = SelectedTool.D10;
			}

			if (keyboardState.IsKeyDown(Keys.D5))
			{
				SelectedTool = SelectedTool.D20;
			}

			if (keyboardState.IsKeyDown(Keys.D6))
			{
				SelectedTool = SelectedTool.D40;
			}

			if (keyboardState.IsKeyDown(Keys.D7))
			{
				SelectedTool = SelectedTool.Wall;
			}

			if (keyboardState.IsKeyDown(Keys.D8))
			{
				SelectedTool = SelectedTool.Erase;
			}

			if (keyboardState.IsKeyDown(Keys.D9))
			{
				SelectedAlgorithm = SelectedAlgorithm.AStar;
			}

			if (keyboardState.IsKeyDown(Keys.D0))
			{
				SelectedAlgorithm = SelectedAlgorithm.Dijkstra;
			}

			if (keyboardState.IsKeyDown(Keys.Escape))
			{
				Reset();
			}

			if ((keyboardState.IsKeyDown(Keys.LeftControl)
				||
				keyboardState.IsKeyDown(Keys.RightControl))
				&&
				keyboardState.IsKeyDown(Keys.S))
			{
				System.Windows.Forms.SaveFileDialog sfd = 
					new System.Windows.Forms.SaveFileDialog();
				sfd.DefaultExt = ".map";
				if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK
					&&
					sfd.FileName != "")
				{
					Map.Save(sfd.FileName);
				}
			}

			if ((keyboardState.IsKeyDown(Keys.LeftControl)
				||
				keyboardState.IsKeyDown(Keys.RightControl))
				&&
				keyboardState.IsKeyDown(Keys.L))
			{
				System.Windows.Forms.OpenFileDialog lfd = 
					new System.Windows.Forms.OpenFileDialog();
				lfd.DefaultExt = ".map";
				if (lfd.ShowDialog() == System.Windows.Forms.DialogResult.OK
					&&
					lfd.FileName != "")
				{
					Map.Load(lfd.FileName);
				}
			}
		}

		static public void Reset()
		{
			IsRunning = false;
			openedSet.Clear();
			closedSet.Clear();
			prev = null;
			g = null;
			h = null;
			f = null;
		}
	}
}
