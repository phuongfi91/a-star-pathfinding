using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace A_Star_PathFinding
{
	/// <summary>
	/// Class TileMap giữ vai trò khởi tạo Map trong màn chơi. Đồng thời 
	/// cung cấp thông tin của Map cho lập trình viên.
	/// </summary>
	public class Map
	{
		#region Declarations
		// Ma trận Map. Đây là một mảng 2 chiều, với mỗi ô chứa một ma trận
		// con, ma trận con này sẽ chứa thông tin địa hình và các đối tượng
		// đang chiếm giữ không gian của ô.
		public int[,] MapSquares;

		// Chiều dài và chiều rộng của Map. Tuy nhiên map không nhất thiết
		// có hình chữ nhật nằm ngang, nó có thể là hình chữ nhật dọc hoặc
		// hình vuông, do đó Width và Height ở đây chỉ mang tính chất tham
		// khảo. Hoặc có thể hiểu với một nghĩa khác: Width tượng trưng cho
		// trục hoành, Height tượng trưng cho trục tung.
		public int MapWidth;
		public int MapHeight;

		// Kích thước ngang dọc của một Tile trong TileSheet.
		public int TileWidth = 16;
		public int TileHeight = 16;

		// Biến random phục vụ việc Random Map và Môi trường.
		private Random random = new Random();

		// Kích thước của TileSheet. (bao nhiêu cột, bao nhiêu dòng)
		private Point sheetSize = new Point(1, 1);

		// Texture TileSheet.
		private Texture2D tileSheet;
		#endregion

		#region Constructor
		public Map(int mapWidth, int mapHeight)
		{
			tileSheet = PathFinder.TileSheet;
			MapWidth = mapWidth;
			MapHeight = mapHeight;
			MapSquares = new int[MapWidth, MapHeight];
			for (int y = 0; y < MapHeight; y++)
			{
				for (int x = 0; x < MapWidth; x++)
				{
					MapSquares[x, y] = 1;
				}
			}
		}
		#endregion

		#region Methods
		public void Load(String mapName)
		{
			// Đọc dữ liệu map từ File map được truyền vào.
			string[] tempString = new string[2];

			// Đọc kích thước map từ file.
			StreamReader streamReader = new StreamReader(mapName);
			tempString = streamReader.ReadLine().Split(' ');
			MapHeight = Int32.Parse(tempString[0]);
			MapWidth = Int32.Parse(tempString[1]);

			// Khởi tạo map với kích thước trên.
			MapSquares = new int[MapWidth, MapHeight];

			// Đọc dữ liệu địa hình từ file vào map.
			string currentLine;
			for (int y = 0; y < MapHeight; y++)
			{
				currentLine = streamReader.ReadLine();
				tempString = currentLine.Split(' ');

				for (int x = 0; x < MapWidth; x++)
				{
					MapSquares[x, y] = Int32.Parse(tempString[x]);
				}
			}
			streamReader.Close();
		}

		public void Save(String mapName)
		{
			TextWriter textWriter = new StreamWriter(mapName);
			textWriter.WriteLine(MapHeight + " " + MapWidth);
			for (int i = 0; i < MapWidth; ++i)
			{
				textWriter.Write(MapSquares[0,i]);
				for (int j = 1; j < MapHeight; ++j)
					textWriter.Write(" " + MapSquares[j,i]);
				textWriter.WriteLine();
			}
			textWriter.Close();
		}

		// Lấy tọa độ ô trên map theo tọa độ Pixel. (trục hoành)
		public int GetSquareByPixelX(int pixelX)
		{
			return pixelX / TileWidth;
		}

		// Lấy tọa độ ô trên map theo tọa độ Pixel. (trục tung)
		public int GetSquareByPixelY(int pixelY)
		{
			return pixelY / TileHeight;
		}

		// 
		public Point GetSquareAtPixel(Vector2 pixelLocation)
		{
			return new Point(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public Vector2 GetSquareCenter(int squareX, int squareY)
		{
			return new Vector2(
				(squareX * TileWidth) + (TileWidth / 2),
				(squareY * TileHeight) + (TileHeight / 2));
		}

		public Vector2 GetSquareCenter(Point square)
		{
			return GetSquareCenter(square.X, square.Y);
		}

		public Rectangle SquareWorldRectangle(int x, int y)
		{
			return new Rectangle(
				x * TileWidth + TileWidth / 2,
				y * TileHeight + TileWidth / 2,
				TileWidth,
				TileHeight);
		}

		public Rectangle SquareWorldRectangle(Point square)
		{
			return SquareWorldRectangle(square.X, square.Y);
		}

		public Rectangle SquareScreenRectangle(int x, int y)
		{
			return PathFinder.Camera.Transform(SquareWorldRectangle(x, y));
		}

		public Rectangle SquareScreenRectangle(Point square)
		{
			return SquareScreenRectangle((int)square.X, (int)square.Y);
		}

		public int GetTileAtSquare(int tileX, int tileY)
		{
			if ((tileX >= 0) && (tileX < MapWidth) &&
				(tileY >= 0) && (tileY < MapHeight))
			{
				return MapSquares[tileX, tileY];
			}
			else
			{
				return -1;
			}
		}

		public void SetTileAtSquare(int tileX, int tileY, int tile)
		{
			if ((tileX >= 0) && (tileX < MapWidth) &&
				(tileY >= 0) && (tileY < MapHeight))
			{
				MapSquares[tileX, tileY] = tile;
			}
		}

		public int GetTileAtPixel(int pixelX, int pixelY)
		{
			return GetTileAtSquare(
				GetSquareByPixelX(pixelX),
				GetSquareByPixelY(pixelY));
		}

		public int GetTileAtPixel(Vector2 pixelLocation)
		{
			return GetTileAtPixel(
				(int)pixelLocation.X,
				(int)pixelLocation.Y);
		}

		public bool IsWallTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			return tileIndex == 0;
		}

		public bool IsWallTile(Point square)
		{
			return IsWallTile((int)square.X, (int)square.Y);
		}

		public bool IsWallTileByPixel(Vector2 pixelLocation)
		{
			return IsWallTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public bool IsGroundTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			return tileIndex > 0;
		}

		public bool IsGroundTile(Point square)
		{
			return IsGroundTile((int)square.X, (int)square.Y);
		}

		public bool IsGroundTileByPixel(Vector2 pixelLocation)
		{
			return IsGroundTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public bool IsHoveredTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			MouseState mouseState = Mouse.GetState();

			return SquareScreenRectangle(tileX, tileY).Contains
				(new Point(mouseState.X + 8, mouseState.Y + 8));
		}

		public bool IsHoveredTile(Point square)
		{
			return IsHoveredTile((int)square.X, (int)square.Y);
		}

		public bool IsHoveredTile(Vector2 pixelLocation)
		{
			return IsHoveredTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public bool IsClickedTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			MouseState mouseState = Mouse.GetState();

			return	IsHoveredTile(tileX, tileY)
					&&
					mouseState.LeftButton == ButtonState.Pressed;
		}

		public bool IsClickedTile(Point square)
		{
			return IsClickedTile((int)square.X, (int)square.Y);
		}

		public bool IsClickedTile(Vector2 pixelLocation)
		{
			return IsClickedTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public bool IsInitiationTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			return	tileX == PathFinder.Initiation.X
					&&
					tileY == PathFinder.Initiation.Y;
		}

		public bool IsInitiationTile(Point square)
		{
			return IsInitiationTile((int)square.X, (int)square.Y);
		}

		public bool IsInitiationTile(Vector2 pixelLocation)
		{
			return IsInitiationTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public bool IsDestinationTile(int tileX, int tileY)
		{
			int tileIndex = GetTileAtSquare(tileX, tileY);

			if (tileIndex == -1)
			{
				return false;
			}

			return tileX == PathFinder.Destination.X
					&&
					tileY == PathFinder.Destination.Y;
		}

		public bool IsDestinationTile(Point square)
		{
			return IsDestinationTile((int)square.X, (int)square.Y);
		}

		public bool IsDestinationTile(Vector2 pixelLocation)
		{
			return IsDestinationTile(
				GetSquareByPixelX((int)pixelLocation.X),
				GetSquareByPixelY((int)pixelLocation.Y));
		}

		public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
		{
			Camera camera = PathFinder.Camera;
			int startX =
				GetSquareByPixelX((int)camera.CurrentWorldPosition.X);
			int endX =
				GetSquareByPixelX((int)camera.CurrentWorldPosition.X + camera.ViewportWidth);

			int startY =
				GetSquareByPixelY((int)camera.CurrentWorldPosition.Y);
			int endY =
				GetSquareByPixelY((int)camera.CurrentWorldPosition.Y + camera.ViewportHeight);

			Color TileColor;

			for (int x = startX; x <= endX; x++)
				for (int y = startY; y <= endY; y++)
				{
					Rectangle currentSquare = SquareScreenRectangle(x, y);
					if ((x >= 0) && (y >= 0) && (x < MapWidth) && (y < MapHeight))
					{
						TileColor = Color.White;

						if (IsClickedTile(x, y) && !PathFinder.IsRunning)
						{
							switch (PathFinder.SelectedTool)
							{
								case SelectedTool.Start:
									PathFinder.Initiation = new Point(x, y);
									PathFinder.Reset();
									break;
								case SelectedTool.End:
									PathFinder.Destination = new Point(x, y);
									PathFinder.Reset();
									break;
								case SelectedTool.Wall:
									MapSquares[x, y] = 0;
									break;
								case SelectedTool.D5:
									MapSquares[x, y] = 5;
									break;
								case SelectedTool.D10:
									MapSquares[x, y] = 10;
									break;
								case SelectedTool.D20:
									MapSquares[x, y] = 20;
									break;
								case SelectedTool.D40:
									MapSquares[x, y] = 40;
									break;
								case SelectedTool.Erase:
									MapSquares[x, y] = 1;
									break;
								default:
									break;
							}
						}

						switch (MapSquares[x, y])
						{
							case 0:
								TileColor = Color.Black;
								break;
							case 5:
								TileColor = new Color(160, 160, 160);
								break;
							case 10:
								TileColor = new Color(120, 120, 120);
								break;
							case 20:
								TileColor = new Color(80, 80, 80);
								break;
							case 40:
								TileColor = new Color(40, 40, 40);
								break;
							default:
								break;
						}

						if (PathFinder.openedSet != null)
						{
							if (PathFinder.openedSet.Contains(new Point(x, y)))
							{
								TileColor = Color.Green;
							}
							else if (PathFinder.closedSet.Contains(new Point(x, y)))
							{
								TileColor = Color.Yellow;
							}
						}

						if (PathFinder.IsFinished && PathFinder.prev != null)
						{
							int currentNode = PathFinder.Node(PathFinder.Destination);
							while (currentNode != PathFinder.Node(PathFinder.Initiation))
							{
								if (currentNode == PathFinder.Node(new Point(x, y)))
								{
									TileColor = Color.DeepPink;
									break;
								}
								currentNode = PathFinder.prev[currentNode];
							}
						}

						if (IsInitiationTile(x, y))
						{
							TileColor = Color.Blue;
						}
						else if (IsDestinationTile(x, y))
						{
							TileColor = Color.Red;
						}
						else if (IsHoveredTile(x, y) && !PathFinder.IsRunning)
						{
							TileColor = Color.Gray;
						}
						

						spriteBatch.Draw
							(
							tileSheet, camera.Transform(new Vector2
								(x * TileWidth + TileWidth / 2,
								y * TileHeight + TileHeight / 2)),
							new Rectangle(0, 0, 16, 16),
							TileColor,
							0,
							new Vector2(TileWidth / 2, TileHeight / 2), 1,
							SpriteEffects.None, 0.0f
							);
					}
				}
		}
		#endregion
	}
}