using System;
using System.IO;
using System.Text;
using LegendsOfDescent;
using Microsoft.Xna.Framework;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            int newSeed;
            string enteredSeed = string.Empty;
            while (true)
            {
                if (!int.TryParse(enteredSeed, out newSeed))
                    newSeed = Environment.TickCount;

                Util.Reseed(newSeed);
                StringBuilder builder = new StringBuilder();
                builder.AppendLine("Seed: " + Util.Seed);
                TestDungeon dungeon = new TestDungeon(new Point(100, 40));

                new MapGenerator(dungeon).GenerateMap();
                DrawTiles(dungeon.Tiles, builder);
                Console.Write(builder.ToString());
                File.WriteAllText(Util.Seed + ".txt", builder.ToString());
                enteredSeed = Console.ReadLine();
                Console.Clear();
            }
        }

        private static void DrawTiles(TileType[,] tiles, StringBuilder builder)
        {
            for (int y = 0; y < tiles.GetLength(1); y++)
            {
                for (int x = 0; x < tiles.GetLength(0); x++)
                {
                    builder.Append(TileTypeToChar(tiles[x, y]));
                }
                builder.AppendLine();
            }
        }

        static char TileTypeToChar(TileType tile)
        {
            switch (tile)
            {
                case TileType.Empty:
                    return '.';
                case TileType.Open:
                case TileType.Open2:
                case TileType.Open3:
                    return ' ';
                case TileType.WallVertical:
                case TileType.WallVertical2:
                case TileType.WallVertical3:
                    return '│';
                case TileType.WallHorizontal:
                case TileType.WallHorizontal2:
                case TileType.WallHorizontal3:
                    return '─';
                case TileType.WallCornerBottomRight:
                    return '┘';
                case TileType.WallCornerTopLeft:
                    return '┌';
                case TileType.WallCornerBottomLeft:
                    return '└';
                case TileType.WallCornerTopRight:
                    return '┐';
                case TileType.StairsUp:
                    return 'u';
                case TileType.StairsDown:
                    return 'd';
                case TileType.Column:
                    return 'o';
                default:
                    return ' ';
            }
        }
    }

    class TestDungeon : IMap
    {
        TileType[,] tiles;

        public TestDungeon(Point dimension)
        {
            Dimension = dimension;
            tiles = new TileType[Dimension.X, Dimension.Y];
            Level = 2; // so we have both stairs up and down
        }

        public Point Dimension { get; private set; }

        public Point StairsUpLocation { get; set; }

        public Point StairsDownLocation { get; set; }

        public bool GoingDown { get; set; }

        public Point StartLocation { get; set; }

        public int Level { get; set; }

        public TileType[,] Tiles { get { return tiles; } }

        public void GenerationComplete()
        {

        }

        public void SetTile(int x, int y, TileType firstType, TileType secondType, TileType thirdType)
        {
            tiles[x, y] = secondType == TileType.Empty ? firstType : secondType;
        }

        public void CreateAndAddStairs()
        {
            // Simply overwrite the existing tile
            tiles[StairsUpLocation.X, StairsUpLocation.Y] = TileType.StairsUp;
            tiles[StairsDownLocation.X, StairsDownLocation.Y] = TileType.StairsDown;
        }

        public string MapType { get { return "Dungeon"; } }

        public int TileDimension
        {
            get { return 64; }
        }

        public void AddEnvItem(IEnvItem envItem, int minDistance = 0, bool generatePosition = false)
        {
        }
    }
}
