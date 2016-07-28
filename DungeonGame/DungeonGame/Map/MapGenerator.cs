using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using LegendsOfDescent;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    /// <summary>
    /// Based on http://roguebasin.roguelikedevelopment.org/index.php/Basic_BSP_Dungeon_generation
    /// </summary>
    public class MapGenerator
    {
        const int MapSize = 100;
        const int MinRoomContainerSize = 15;
        const int MaxRoomSize = MapSize / 4;
        const int MinRoomSize = 10;
        const int MaxHallWidth = 6;
        const int MinHallWidth = 3;
        const int MaxIterations = 5;
        const double ColumnRatio = 1.0 / 100;

        private MapTileType[,] tiles;
        private IMap map;

        public MapGenerator(IMap map)
        {
            this.map = map;
        }

        public void GenerateMap()
        {
            tiles = new MapTileType[map.Dimension.X, map.Dimension.Y];
            GenerateMap(map.Dimension);
            if (map.MapType != "Town")
            {
                TileToDungeonMap();
                AddStairs();
            }
            else
            {
                map.StartLocation = new Point(56, 4);
                map.CreateAndAddStairs();
            }
            map.GenerationComplete();
            map.StartLocation = map.StartLocation;
        }

        private bool IsInsideRoom(Point point)
        {
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (GetTile(point.X + i, point.Y + i) != MapTileType.RoomInside || !IsWalkable(point))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private Point GetRandomPointInsideRoom()
        {
            Point point = new Point();

            do
            {
                point.X = Util.Random.Next(0, tiles.GetLength(0));
                point.Y = Util.Random.Next(0, tiles.GetLength(1));
            } while (!IsInsideRoom(point));

            return point;
        }

        private bool IsWalkable(Point point)
        {
            var tile = GetTile(point.X, point.Y);
            return tile == MapTileType.HallInside || tile == MapTileType.RoomInside || tile == MapTileType.Portal;
        }

        private void AddStairs()
        {
            // add stairs down
            Point point = GetRandomPointInsideRoom();

            map.StairsDownLocation = point;
            if (!map.GoingDown)
            {
                map.StartLocation = point.Offset(1, 0);
            }

            // add stairs up
            if (map.Level > 0)
            {
                point = GetRandomPointInsideRoom();

                map.StairsUpLocation = point;
                if (map.GoingDown)
                {
                    map.StartLocation = point.Offset(1, 2);
                }
            }
            else
            {
                map.StairsUpLocation = new Point(-100, -100);
            }

            map.CreateAndAddStairs();
        }

        public void AddWalls()
        {
            for (int x = 0; x < map.Dimension.X; x++)
            {
                for (int y = 0; y < map.Dimension.Y; y++)
                {
                    if (tiles[x, y] == MapTileType.None)
                    {
                        var newTile = MapTileType.None;
                        bool top = GetTile(x, y - 1) == MapTileType.RoomInside;
                        bool right = GetTile(x + 1, y) == MapTileType.RoomInside;
                        bool bottom = GetTile(x, y + 1) == MapTileType.RoomInside;
                        bool left = GetTile(x - 1, y) == MapTileType.RoomInside;

                        if (top && right && bottom && left)
                            newTile = MapTileType.Column;

                        else if (!top && right && bottom && left)
                            newTile = MapTileType.RoomWallRight;
                        else if (top && !right && bottom && left)
                            newTile = MapTileType.RoomWallTop;
                        else if (top && right && !bottom && left)
                            newTile = MapTileType.RoomWallRight;
                        else if (top && right && bottom && !left)
                            newTile = MapTileType.RoomWallTop;

                        else if (!top && !right && bottom && left)
                            newTile = MapTileType.RoomCornerBottomLeft;
                        else if (!top && right && !bottom && left)
                            newTile = MapTileType.RoomWallRight;
                        else if (!top && right && bottom && !left)
                            newTile = MapTileType.RoomCornerBottomRight;

                        else if (top && !right && !bottom && left)
                            newTile = MapTileType.RoomCornerTopLeft;
                        else if (top && !right && bottom && !left)
                            newTile = MapTileType.RoomWallTop;

                        else if (top && right && !bottom && !left)
                            newTile = MapTileType.RoomCornerTopRight;
                        
                        else if (!top && !right && !bottom && left)
                            newTile = MapTileType.RoomWallRight;
                        else if (top && !right && !bottom && !left)
                            newTile = MapTileType.RoomWallTop;
                        else if (!top && right && !bottom && !left)
                            newTile = GetTile(x - 1, y) == MapTileType.RoomWallTop ? MapTileType.RoomCornerBottomRight : MapTileType.RoomWallRight;
                        else if (!top && !right && bottom && !left)
                            newTile = GetTile(x, y - 1) == MapTileType.RoomWallRight ? MapTileType.RoomCornerBottomRight : MapTileType.RoomWallTop;

                        else // covered corner
                        {
                            if (GetTile(x - 1, y - 1) == MapTileType.RoomInside)
                                newTile = MapTileType.RoomCornerBottomRight;
                            else if (GetTile(x - 1, y + 1) == MapTileType.RoomInside)
                                newTile = GetTile(x, y - 1) == MapTileType.RoomWallRight ? MapTileType.RoomCornerBottomRight : MapTileType.RoomCornerTopRight;
                            else if (GetTile(x + 1, y + 1) == MapTileType.RoomInside)
                                newTile = GetTile(x, y - 1) == MapTileType.RoomWallRight ? MapTileType.RoomWallRight : MapTileType.RoomCornerTopLeft;
                            else if (GetTile(x + 1, y - 1) == MapTileType.RoomInside)
                                newTile = MapTileType.RoomCornerBottomLeft;
                        }

                        tiles[x, y] = newTile;
                    }
                }
            }
        }

        public void TileToDungeonMap()
        {
            AddWalls();
            for (int x = 0; x < map.Dimension.X; x++)
            {
                for (int y = 0; y < map.Dimension.Y; y++)
                {
                    TileType firstTileType = TileType.Empty;
                    TileType secondTileType = TileType.Empty;

                    switch (tiles[x, y])
                    {
                        case MapTileType.None:
                            firstTileType = TileType.Empty;
                            break;
                        case MapTileType.Column:
                            firstTileType = TileType.Column;
                            break;
                        case MapTileType.HallWallTop:
                        case MapTileType.RoomWallTop:
                        case MapTileType.HallWallBottom:
                        case MapTileType.RoomWallBottom:
                            secondTileType = (TileType)Util.Random.Next((int)TileType.WallHorizontal, (int)TileType.WallHorizontal3 + 1);
                            firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            if (GetTile(x + 1, y) == MapTileType.RoomInside)
                            {
                                firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            }
                            break;
                        case MapTileType.HallWallRight:
                        case MapTileType.RoomWallRight:
                        case MapTileType.HallWallLeft:
                        case MapTileType.RoomWallLeft:
                            secondTileType = (TileType)Util.Random.Next((int)TileType.WallVertical, (int)TileType.WallVertical3 + 1);
                            firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            break;
                        case MapTileType.HallCornerTopLeft:
                        case MapTileType.RoomCornerTopLeft:
                            secondTileType = TileType.WallCornerTopLeft;
                            break;
                        case MapTileType.HallCornerTopRight:
                        case MapTileType.RoomCornerTopRight:
                            secondTileType = TileType.WallCornerTopRight;
                            if (y > 0 && (
                                tiles[x, y - 1] == MapTileType.HallInside ||
                                tiles[x, y - 1] == MapTileType.RoomInside ||
                                tiles[x, y - 1] == MapTileType.Portal))
                            {
                                firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            }
                            break;
                        case MapTileType.HallCornerBottomLeft:
                        case MapTileType.RoomCornerBottomLeft:
                            secondTileType = TileType.WallCornerBottomLeft;
                            if (x > 0 && (
                                tiles[x - 1, y] == MapTileType.HallInside ||
                                tiles[x - 1, y] == MapTileType.RoomInside ||
                                tiles[x - 1, y] == MapTileType.Portal))
                            {
                                firstTileType = TileType.Open;
                            }
                            break;
                        case MapTileType.HallCornerBottomRight:
                        case MapTileType.RoomCornerBottomRight:
                            secondTileType = TileType.WallCornerBottomRight;
                            break;
                        case MapTileType.HallInside:
                        case MapTileType.RoomInside:
                        case MapTileType.Portal:
                            firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            if (tiles[x, y] == MapTileType.RoomInside &&
                                !IsNextTo(x, y, MapTileType.HallInside) &&
                                !IsNextTo(x, y, MapTileType.Portal) &&
                                Util.Random.NextDouble() < ColumnRatio && map.MapType != "Town")
                            {
                                secondTileType = TileType.Column;
                            }
                            break;
                        case MapTileType.MetalDoorVirtical:
                            firstTileType = TileType.MetalDoorVirtical;
                            secondTileType = TileType.Open;
                            break;
                        case MapTileType.MetalDoorHorizontal:
                            firstTileType = TileType.MetalDoorHorizontal;
                            secondTileType = TileType.Open;
                            if (GetTile(x + 1, y) == MapTileType.RoomInside)
                            {
                                firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            }
                            break;
                        case MapTileType.WoodenDoorVirtical:
                            firstTileType = TileType.WoodenDoorVirtical;
                            secondTileType = TileType.Open;
                            break;
                        case MapTileType.WoodenDoorHorizontal:
                            firstTileType = TileType.WoodenDoorHorizontal;
                            secondTileType = TileType.Open;
                            if (GetTile(x + 1, y) == MapTileType.RoomInside)
                            {
                                firstTileType = (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1);
                            }
                            break;
                    }

                    map.SetTile(x, y, firstTileType, secondTileType);
                }
            }
        }

        private bool IsNextTo(int x, int y, MapTileType mapTileType)
        {
            return new[] {
                GetTile(x + 1, y),
                GetTile(x - 1, y),
                GetTile(x, y + 1),
                GetTile(x, y - 1)
            }.Any(t => t == mapTileType);
        }

        private void GenerateMap(Point dimension)
        {
            if (map.MapType == "Boss")
            {
                GenerateOneRoomMap();
            }
            if (map.MapType == "Town")
            {
#if WIN8
                LoadMapFromFile("Assets/Data/townMap.xml");
#else
                LoadMapFromFile("Content/Data/townMap.xml");
#endif
            }
            else
            {
                Node root = Subdivide(new Rectangle(0, 0, dimension.X, dimension.Y), 1);
                ConnectRooms(root);
                FixEdges();
            }
        }


        private void GenerateOneRoomMap()
        {
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    tiles[x, y] = MapTileType.RoomInside;
                }
            }

            tiles[0, 0] = MapTileType.RoomCornerTopLeft;
            tiles[0, tiles.GetLength(1) - 1] = MapTileType.RoomCornerBottomLeft;
            tiles[tiles.GetLength(0) - 1, 0] = MapTileType.RoomCornerTopRight;
            tiles[tiles.GetLength(0) - 1, tiles.GetLength(1) - 1] = MapTileType.RoomCornerBottomRight;

            for (int x = 1; x < tiles.GetLength(0) - 1; x++)
            {
                tiles[x, 0] = MapTileType.RoomWallTop;
                tiles[x, tiles.GetLength(1) - 1] = MapTileType.RoomWallBottom;
            }
            for (int y = 1; y < tiles.GetLength(0) - 1; y++)
            {
                tiles[0, y] = MapTileType.RoomWallLeft;
                tiles[tiles.GetLength(0) - 1, y] = MapTileType.RoomWallRight;
            }
        }



        public void LoadMapFromFile(string file)
        {
            XDocument root = XmlHelper.Load(file);
            IEnumerable<XElement> childrenNodes = root.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Floor")
                {
                    ParseFloor(element.Value);
                }
                else if (element.Name == "Map")
                {
                    ParseMap(element.Value);
                }
            }
        }


        private void ParseMap(string mapChars)
        {
            int charOffset = 0;
            char currentChar = '-';
            int x = 0;
            int y = 0;

            // remove white space on top
            while (mapChars[charOffset] == ' ' || mapChars[charOffset] == '\n')
            {
                charOffset++;
                break;
            }


            while (charOffset < mapChars.Length && y < map.Dimension.Y)
            {
                currentChar = mapChars[charOffset];

                // add NPC's
                IEnvItem npc = null;
                if (currentChar == '1')
                {
                    MerchantSprite character = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.General);
                    character.Name = "Mirin";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '2')
                {
                    BlacksmithSprite character = new BlacksmithSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, 1);
                    character.Name = "Dagnar";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '3')
                {
                    EnchanterSprite character = new EnchanterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, 1);
                    character.Name = "Sagnus";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '4')
                {
                    QuesterSprite character = new QuesterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, true);
                    character.Name = QuesterSprite.MainQuesterName;
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '5')
                {
                    QuesterSprite character = new QuesterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    character.Name = "Vesil";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '6')
                {
                    Chest stash = new Chest(Vector2.Zero, 1, ChestType.stash);
                    stash.Position = new Vector2(x * map.TileDimension, y * map.TileDimension);
                    npc = stash;
                }
                else if (currentChar == '7')
                {
                    MerchantSprite character = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.Potion);
                    character.Name = "Popkins";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '8')
                {
                    MerchantSprite character = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.Warrior);
                    character.Name = "Risil";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '9')
                {
                    // Make this the alchemist
                    MerchantSprite character = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.Rogue);
                    character.Name = "Boli";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '(')
                {
                    TeleporterSprite character = new TeleporterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    //character.Name = "Boli";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == ')')
                {
                    ResetSprite character = new ResetSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    character.Name = "Gregor";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }
                else if (currentChar == '0')
                {
                    MerchantSprite character = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.Mage);
                    character.Name = "Stilgar";
                    character.Position = new Vector2(x * map.TileDimension - character.FrameDimensions.X / 2 + map.TileDimension / 2, y * map.TileDimension - character.FrameDimensions.Y / 2 + map.TileDimension / 2);
                    npc = character;
                }

                if (npc != null)
                {
                    map.AddEnvItem(npc, generatePosition: false);
                }

                // Handle stairs locations
                if (currentChar == 'S')
                {
                    map.StairsDownLocation = new Point(x, y);
                }

                // set tiles
                else if (currentChar == 'A')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WallCornerTopLeft);
                }
                else if (currentChar == 'B' || currentChar == 'F')
                {
                    map.SetTile(x, y, TileType.Empty, (TileType)Util.Random.Next((int)TileType.WallHorizontal, (int)TileType.WallHorizontal3 + 1));
                }
                else if (currentChar == 'C')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WallCornerTopRight);
                }
                else if (currentChar == 'D' || currentChar == 'H')
                {
                    map.SetTile(x, y, TileType.Empty, (TileType)Util.Random.Next((int)TileType.WallVertical, (int)TileType.WallVertical3 + 1));
                }
                else if (currentChar == 'E')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WallCornerBottomRight);
                }
                else if (currentChar == 'G')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WallCornerBottomLeft);
                }
                else if (currentChar == 'I')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallCornerTopLeft);
                }
                else if (currentChar == 'J')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallHorizontal);
                }
                else if (currentChar == 'N')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallHorizontal2);
                }
                else if (currentChar == 'Q')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallHorizontalBroken);
                }
                else if (currentChar == 'R')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallHorizontalBroken2);
                }
                else if (currentChar == 'K')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallCornerTopRight);
                }
                else if (currentChar == 'L')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallVertical);
                }
                else if (currentChar == 'P')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallVertical2);
                }
                else if (currentChar == 'T')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallVerticalBroken);
                }
                else if (currentChar == 'U')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallVerticalBroken2);
                }
                else if (currentChar == 'M')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallCornerBottomRight);
                }
                else if (currentChar == 'O')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallCornerBottomLeft);
                }
                else if (currentChar == '$')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallColumn);
                }
                else if (currentChar == '+')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichColumn);
                }
                else if (currentChar == '|')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.Column);
                }
                else if (currentChar == 'i')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichCornerTopLeft);
                }
                else if (currentChar == 'j')
                {
                    map.SetTile(x, y, TileType.Empty, (TileType)Util.Random.Next((int)TileType.InnerWallRichHorizontal, (int)TileType.InnerWallRichHorizontal4 + 1));
                }
                else if (currentChar == 'k')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichCornerTopRight);
                }
                else if (currentChar == 'l')
                {
                    map.SetTile(x, y, TileType.Empty, (TileType)Util.Random.Next((int)TileType.InnerWallRichVertical, (int)TileType.InnerWallRichVertical4 + 1));
                }
                else if (currentChar == 'm')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichCornerBottomRight);
                }
                else if (currentChar == 'o')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichCornerBottomLeft);
                }
                else if (currentChar == 'n')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.InnerWallRichCornerBottomLeft);
                }
                else if (currentChar == 'V')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.Anvil);
                }
                else if (currentChar == '^')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.Torch);
                }
                else if (currentChar == '#')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.Vent);
                }
                else if (currentChar == 'W')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.MetalDoorVirtical);
                }
                else if (currentChar == 'X')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.MetalDoorHorizontal);
                }
                else if (currentChar == 'Y')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WoodenDoorVirtical);
                }
                else if (currentChar == 'Z')
                {
                    map.SetTile(x, y, TileType.Empty, TileType.WoodenDoorHorizontal);
                }
                else if (currentChar == '\n')
                {
                    x = -1;
                    y++;
                }

                // advance to next tile
                if (currentChar != ' ')
                {
                    x++;
                }
                charOffset++;
            }
        }


        private void ParseFloor(string mapChars)
        {
            int charOffset = 0;
            char currentChar = '-';
            int x = 0;
            int y = 0;

            // remove white space on top
            while (mapChars[charOffset] == ' ' || mapChars[charOffset] == '\n')
            {
                charOffset++;
                break;
            }


            while (charOffset < mapChars.Length && y < map.Dimension.Y)
            {
                currentChar = mapChars[charOffset];

                // set tiles
                if (currentChar == '-')
                {
                    map.SetTile(x, y, (TileType)Util.Random.Next((int)TileType.Open, (int)TileType.Open3 + 1));
                }
                else if (currentChar == '.')
                {
                    map.SetTile(x, y, (TileType)Util.Random.Next((int)TileType.InnerInside, (int)TileType.InnerInside4 + 1));
                }
                else if (currentChar == ',')
                {
                    int roll = Util.Random.Next(0, 100);
                    if (roll < 70)
                    {
                        map.SetTile(x, y, TileType.InnerInsideRich);
                    }
                    else
                    {
                        map.SetTile(x, y, (TileType)Util.Random.Next((int)TileType.InnerInsideRich2, (int)TileType.InnerInsideRich6 + 1));
                    }
                }
                else if (currentChar == 'F')
                {
                    map.SetTile(x, y, (TileType)Util.Random.Next((int)TileType.ShroomForest, (int)TileType.ShroomForest4 + 1));
                }
                else if (currentChar == 'R')
                {
                    map.SetTile(x, y, TileType.Road);
                }
                else if (currentChar == 'V')
                {
                    map.SetTile(x, y, TileType.Vent);
                }
                else if (currentChar == 'D')
                {
                    map.SetTile(x, y, TileType.Dirt);
                }
                else if (currentChar == '*')
                {
                    map.SetTile(x, y, TileType.Pillar);
                }
                else if (currentChar == '\n')
                {
                    x = -1;
                    y++;
                }

                // advance to next tile
                if (currentChar != ' ')
                {
                    x++;
                }
                charOffset++;
            }
        }


        private void FixEdges()
        {
            for (int x = 0; x < map.Dimension.X; x++)
            {
                for (int y = 0; y < map.Dimension.Y; y++)
                {
                    if (x == 0 || y == 0 || x == map.Dimension.X - 1 || y == map.Dimension.Y - 1)
                    {
                        tiles[x, y] = MapTileType.None;
                    }
                }
            }
        }

        private void ConnectRooms(Node node)
        {
            if (node.Left != null && node.Right != null)
            {
                if (node.Left.IsLeaf && node.Right.IsLeaf)
                {
                    Rectangle a = node.Left.Rect;
                    Rectangle b = node.Right.Rect;
                    // We've reached a leaf, connect the rooms!

                    ConnectRects(node, a, b);
                }
                else
                {
                    ConnectRooms(node.Left);
                    ConnectRooms(node.Right);

                    Rectangle leftFound = new Rectangle();
                    Rectangle rightFound = new Rectangle();

                    node.GetClosestChildren(ref leftFound, ref rightFound);

                    ConnectRects(node, leftFound, rightFound);
                }
            }
            else
            {

            }
        }

        private void ConnectRects(Node node, Rectangle a, Rectangle b)
        {
            if (!TryConnectHorizontally(a, b, ref node.HallRect))
            {
                if (!TryConnectVertically(a, b, ref node.HallRect))
                {
                    ConnectWithAngle(a, b, ref node.HallRect, ref node.HallRect2);
                }
            }
        }

        private void ConnectWithAngle(
            Rectangle a,
            Rectangle b,
            ref Rectangle hallRect,
            ref Rectangle hallRect2)
        {
            int hallWidth = 4;

            //
            // ####
            // #  #...... A
            // ####     .
            //  .     ######
            //  ......#    #
            //  B     ######
            //
            if ((a.Y < b.Y && a.X < b.X) || (b.Y < a.Y && b.X < a.X))
            {
                if (b.Y < a.Y && b.X < a.X)
                {
                    // swap
                    Rectangle temp = a;
                    a = b;
                    b = temp;
                }
                hallRect.X = a.X + a.Width;
                hallRect.Y = a.Y + (a.Height / 2) - (hallWidth / 2);
                hallRect.Width = (b.X + (b.Width / 2)) - (hallRect.X) + (hallWidth / 2);
                hallRect.Height = hallWidth;

                DrawRect(hallRect, false);

                hallRect2.X = b.X + (b.Width / 2) - (hallWidth / 2);
                hallRect2.Y = hallRect.Y;
                hallRect2.Width = hallWidth;
                hallRect2.Height = b.Y - hallRect.Y;

                DrawRect(hallRect2, false);
            }
            //   C    ######
            //   .....#    #
            //   .    ######
            //   .      .
            // ####     .
            // #  #...... D
            // ####
            //
            else
            {
                if (b.X < a.X)
                {
                    // swap
                    Rectangle temp = a;
                    a = b;
                    b = temp;
                }

                hallRect.X = a.X + (a.Width / 2) - (hallWidth / 2);
                hallRect.Y = b.Y + (b.Height / 2) - (hallWidth / 2);
                hallRect.Width = hallWidth;
                hallRect.Height = a.Y - (b.Y + (b.Height / 2) - (hallWidth / 2));

                DrawRect(hallRect, false);

                hallRect2.X = hallRect.X;
                hallRect2.Y = hallRect.Y;
                hallRect2.Width = b.X - hallRect.X;
                hallRect2.Height = hallWidth;

                DrawRect(hallRect2, false);
            }
        }

        private bool TryConnectHorizontally(Rectangle a, Rectangle b, ref Rectangle hallRect)
        {
            if (b.X < a.X)
            {
                // swap
                Rectangle temp = a;
                a = b;
                b = temp;
            }

            Line aVert = new Line(a.Y, a.Y + a.Height);
            Line bVert = new Line(b.Y, b.Y + b.Height);
            Line overlap = aVert.Overlap(bVert);

            if (overlap != null)
            {
                int length = overlap.Length;
                if (length >= MinHallWidth)
                {
                    int height = Util.Random.Next(MinHallWidth, Math.Min(length + 1, MaxHallWidth + 1));
                    int y = Util.Random.Next(overlap.P1, overlap.P1 + (length - height));
                    // draw a hall
                    hallRect = new Rectangle(a.X + a.Width, y, b.X - (a.X + a.Width), height);
                    DrawRect(hallRect, false);
                    return true;
                }
            }
            return false;
        }

        private bool TryConnectVertically(Rectangle a, Rectangle b, ref Rectangle hallRect)
        {
            if (b.Y < a.Y)
            {
                // swap
                Rectangle temp = a;
                a = b;
                b = temp;
            }

            Line aHoriz = new Line(a.X, a.X + a.Width);
            Line bHoriz = new Line(b.X, b.X + b.Width);
            Line overlap = aHoriz.Overlap(bHoriz);

            if (overlap != null)
            {
                int length = overlap.Length;
                if (length >= MinHallWidth)
                {
                    int width = Util.Random.Next(MinHallWidth, Math.Min(length + 1, MaxHallWidth + 1));
                    int x = Util.Random.Next(overlap.P1, overlap.P1 + (length - width));
                    // draw a hall
                    hallRect = new Rectangle(x, a.Y + a.Height, width, b.Y - (a.Y + a.Height));
                    DrawRect(hallRect, false);
                    return true;
                }
            }
            return false;
        }

        public static bool RandomBool()
        {
            return Util.Random.Next(2) == 1;
        }

        private Node Subdivide(Rectangle rectangle, int iteration)
        {
            Node me = new Node();
            //me.Rect = rectangle;

            if (iteration > MaxIterations)
            {
                me.IsLeaf = true;
                me.Rect = AddRoom(rectangle);
                map.StartLocation = me.Rect.Location.Offset(2, 2);
                return me;
            }

            // choose a random direction (v or h)
            bool isVertical = RandomBool();

            bool widthTooSmall = rectangle.Width < MinRoomContainerSize * 2;
            bool heightToSmall = rectangle.Height < MinRoomContainerSize * 2;

            // constrain direction by min room size
            if (widthTooSmall && heightToSmall)
            {
                me.IsLeaf = true;
                me.Rect = AddRoom(rectangle);
                return me;
            }
            else if (widthTooSmall)
            {
                isVertical = false;
            }
            else if (heightToSmall)
            {
                isVertical = true;
            }

            // choose a Util.Random position (x for v, y for h)
            int min, max;
            int split = 0;
            if (isVertical)
            {
                min = rectangle.X + MinRoomContainerSize;
                max = rectangle.X + rectangle.Width - MinRoomContainerSize;
            }
            else
            {
                min = rectangle.Y + MinRoomContainerSize;
                max = rectangle.Y + rectangle.Height - MinRoomContainerSize;
            }

            split = Util.Random.Next(min, max);

            //int newIteration = Util.Random.Next(iteration, iteration + MaxIterations);
            int newIteration = iteration + 1;

            // call recursively
            if (isVertical)
            {
                me.Left = Subdivide(new Rectangle(rectangle.X, rectangle.Y, split - rectangle.X, rectangle.Height), newIteration);
                me.Right = Subdivide(new Rectangle(split, rectangle.Y, rectangle.X + rectangle.Width - split, rectangle.Height), newIteration);
            }
            else
            {
                me.Left = Subdivide(new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, split - rectangle.Y), newIteration);
                me.Right = Subdivide(new Rectangle(rectangle.X, split, rectangle.Width, rectangle.Y + rectangle.Height - split), newIteration);
            }
            return me;
        }

        private Rectangle AddRoom(Rectangle r)
        {
            int x, y, w, h;
            w = Util.Random.Next(MinRoomSize, Math.Min(r.Width - 2, MaxRoomSize));
            h = Util.Random.Next(MinRoomSize, Math.Min(r.Height - 2, MaxRoomSize));

            x = Util.Random.Next(r.X + 1, r.X + r.Width - w);
            y = Util.Random.Next(r.Y + 1, r.Y + r.Height - h);
            Rectangle newRect = new Rectangle(x, y, w, h);
            DrawRect(newRect, true);
            return newRect;
        }

        public void DrawRect(Rectangle r, bool isRoom)
        {
            for (int x = r.Left; x <= r.Right; x++)
            {
                for (int y = r.Top; y <= r.Bottom; y++)
                {
                    if (x == r.Left)
                    {
                        if (y == r.Y)
                            SetTile(x, y, isRoom ? MapTileType.RoomCornerTopLeft : MapTileType.HallCornerTopLeft);
                        else if (y == r.Bottom)
                            SetTile(x, y, isRoom ? MapTileType.RoomCornerBottomLeft : MapTileType.HallCornerBottomLeft);
                        else
                            SetTile(x, y, isRoom ? MapTileType.RoomWallLeft : MapTileType.HallWallLeft);

                    }
                    else if (x == r.Right)
                    {
                        if (y == r.Y)
                            SetTile(x, y, isRoom ? MapTileType.RoomCornerTopRight : MapTileType.HallCornerTopRight);
                        else if (y == r.Bottom)
                            SetTile(x, y, isRoom ? MapTileType.RoomCornerBottomRight : MapTileType.HallCornerBottomRight);
                        else
                            SetTile(x, y, isRoom ? MapTileType.RoomWallRight : MapTileType.HallWallRight);
                    }
                    else if (y == r.Top)
                    {
                        SetTile(x, y, isRoom ? MapTileType.RoomWallTop : MapTileType.HallWallTop);
                    }
                    else if (y == r.Bottom)
                    {
                        SetTile(x, y, isRoom ? MapTileType.RoomWallBottom : MapTileType.HallWallBottom);
                    }
                    else
                    {
                        SetTile(x, y, isRoom ? MapTileType.RoomInside : MapTileType.HallInside);
                    }
                }
            }
        }

        /// <summary>
        /// Get's a tile, with bounds checking. If the requested tile is out of bounds, returns MapTileType.None.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private MapTileType GetTile(int x, int y)
        {
            if (x < 0 || x >= tiles.GetLength(0) || y < 0 || y >= tiles.GetLength(1))
            {
                return MapTileType.None;
            }
            else
            {
                return tiles[x, y];
            }
        }

        private void SetTile(int x, int y, MapTileType newType)
        {
            switch (newType)
            {
                case MapTileType.None:
                case MapTileType.RoomInside:
                case MapTileType.HallInside:
                case MapTileType.Portal:
                case MapTileType.HallWallTop:
                case MapTileType.HallWallRight:
                case MapTileType.HallWallBottom:
                case MapTileType.HallWallLeft:
                case MapTileType.RoomWallTop:
                case MapTileType.RoomWallRight:
                case MapTileType.RoomWallBottom:
                case MapTileType.RoomWallLeft:
                case MapTileType.RoomCornerTopLeft:
                case MapTileType.RoomCornerTopRight:
                case MapTileType.RoomCornerBottomLeft:
                case MapTileType.RoomCornerBottomRight:
                case MapTileType.HallCornerTopLeft:
                case MapTileType.HallCornerTopRight:
                case MapTileType.HallCornerBottomLeft:
                case MapTileType.HallCornerBottomRight:
                    newType = MapTileType.RoomInside;
                    break;
            }

            tiles[x, y] = newType;
        }

        private class Node
        {
            public bool IsLeaf = false;
            public Rectangle Rect;
            public Rectangle HallRect;
            public Rectangle HallRect2;
            public Node Left;
            public Node Right;

            internal void GetClosestChildren(ref Rectangle leftFound, ref Rectangle rightFound)
            {
                List<Rectangle> leftRects = new List<Rectangle>();
                this.Left.GetAllRects(leftRects);
                List<Rectangle> rightRects = new List<Rectangle>();
                this.Right.GetAllRects(rightRects);

                Vector2 leftPoint = new Vector2();
                Vector2 rightPoint = new Vector2();
                float distance = float.MaxValue;
                foreach (var l in leftRects)
                {
                    foreach (var r in rightRects)
                    {
                        leftPoint.X = l.X;
                        leftPoint.Y = l.Y;
                        rightPoint.X = r.X;
                        rightPoint.Y = r.Y;
                        float d = Vector2.DistanceSquared(leftPoint, rightPoint);
                        if (d < distance)
                        {
                            distance = d;
                            leftFound = l;
                            rightFound = r;
                        }
                    }
                }
            }

            private void GetAllRects(List<Rectangle> list)
            {
                if (Rect != Rectangle.Empty) list.Add(Rect);
                if (Left != null) Left.GetAllRects(list);
                if (Right != null) Right.GetAllRects(list);
            }
        }
    }

    public class Line
    {
        public int P1;
        public int P2;

        public Line(int p1, int p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public override string ToString()
        {
            return P1 + ", " + P2;
        }

        public int Length { get { return P2 - P1; } }

        public Line Overlap(Line other)
        {
            return Overlap(other, false);
        }

        public Line Overlap(Line other, bool noRecursion)
        {
            if (this.P1 >= other.P1 && this.P1 <= other.P2)
            {
                return new Line(this.P1, Math.Min(this.P2, other.P2));
            }
            else if (this.P2 >= other.P1 && this.P2 <= other.P2)
            {
                return new Line(other.P1, Math.Min(this.P2, other.P2));
            }

            if (noRecursion)
            {
                return null;
            }

            return other.Overlap(this, true);
        }
    }

    /// <summary>
    /// Do not change the order of this enum, as the above code uses comparision operators
    /// </summary>
    public enum MapTileType
    {
        None = 0,

        RoomInside,
        HallInside,
        Portal,

        Column,

        RoomWallTop,
        RoomWallRight,
        RoomWallBottom,
        RoomWallLeft,

        RoomCornerTopLeft,
        RoomCornerTopRight,
        RoomCornerBottomLeft,
        RoomCornerBottomRight,

        HallWallTop,
        HallWallRight,
        HallWallBottom,
        HallWallLeft,

        HallCornerTopLeft,
        HallCornerTopRight,
        HallCornerBottomLeft,
        HallCornerBottomRight,

        WoodenDoorVirtical,
        WoodenDoorHorizontal,
        MetalDoorVirtical,
        MetalDoorHorizontal
    }
}
