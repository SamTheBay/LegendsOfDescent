using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LegendsOfDescent.Quests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    public class DungeonLevel : IMap, ISaveable
    {
        bool[,] occupiedMap;
        bool[,] itemOccupiedMap;
        TileSprite[,] tiles;
        Point dimension;
        Point startLocation;
        TileSet tileSet;
        SpatialAStar<TileSprite, SearchData> AStar;
        List<NPCSprite> npcs = new List<NPCSprite>();
        List<IEnvItem> envItems = new List<IEnvItem>();
        LevelConfig config;
#if WIN8
        ContentManager contentManager = new ContentManager(DungeonGame.Instance.Services, "Assets");
#else
        ContentManager contentManager = new ContentManager(DungeonGame.Instance.Services);
#endif
        public Point StairsUpLocation { get; set; }
        public Point StairsDownLocation { get; set; }
        public int Level { get { return config.Floor; } }
        public bool ForceReload { get; set; }
        public bool StartDialogueShown = false;

        public Vector2 StairsDownPosition { get { return new Vector2(StairsDownLocation.X * TileDimension, StairsDownLocation.Y * TileDimension); } }

        // debugging stuff
        Texture2D occupiedSpaceTex;

        private bool goingDown = false;
        public bool GoingDown 
        {
            get { return goingDown; }
            set 
            {
                goingDown = value;
                if (goingDown && StairsUpLocation != Point.Zero)
                {
                    StartLocation = new Point(StairsUpLocation.X + 2, StairsUpLocation.Y + 2);
                }
                else if (StairsDownLocation != Point.Zero)
                {
                    StartLocation = new Point(StairsDownLocation.X + 2, StairsDownLocation.Y + 2);
                }
            }
        }

        public DungeonLevel(LevelConfig config, bool goingDown)
            : this()
        {
            this.config = config;
            this.tileSet = TileSet.GetTileSet(config.TileSet.Name);
            this.GoingDown = goingDown;
            if (config.Type == "Dungeon")
            {
                dimension = new Point(100, 100);
            }
            else if (config.Type == "Boss")
            {
                dimension = new Point(25, 25);
            }
            else if (config.Type == "Town")
            {
                dimension = new Point(65, 35);
            }
            else
            {
                dimension = new Point(100, 100);
            }
            this.tiles = new TileSprite[dimension.X, dimension.Y];
            ForceReload = false;
            this.Initialize();
        }


        public DungeonLevel()
        {
            ForceReload = false;
            contentManager.RootDirectory = DungeonGame.ContentRoot;
        }


        public void Initialize()
        {
            occupiedMap = new bool[dimension.X, dimension.Y];
            for (int x = 0; x < dimension.X; x++)
            {
                for (int y = 0; y < dimension.Y; y++)
                {
                    occupiedMap[x, y] = false;
                }
            }

            occupiedSpaceTex = InternalContentManager.GetSolidColorTexture(new Color(255, 0, 0, 1));
        }



        public void InitializeItemMap()
        {
            itemOccupiedMap = new bool[dimension.X, dimension.Y];
            for (int x = 0; x < dimension.X; x++)
            {
                for (int y = 0; y < dimension.Y; y++)
                {
                    if (tiles[x,y].IsWalkable(null))
                    {
                        itemOccupiedMap[x, y] = false;
                    }
                    else
                    {
                        itemOccupiedMap[x, y] = true;
                    }
                }
            }
        }


        public void Persist(BinaryWriter writer)
        {
            writer.Write(dimension);
            writer.Write(startLocation);
            writer.Write(StairsUpLocation);
            writer.Write(StairsDownLocation);
            writer.Write(config);

            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    writer.Write((Byte)tiles[x, y].TileTypes[0]);
                    writer.Write((Byte)tiles[x, y].TileTypes[1]);
                    writer.Write((Byte)tiles[x, y].TileTypes[2]);
                    writer.Write(tiles[x, y].HasBeenSeen);
                    writer.Write(tiles[x, y].IsDoorOpen);
                }
            }

            // persist items
            writer.Write(envItems.OfType<ItemSprite>());

            // persist merchants
            writer.Write(envItems.OfType<MerchantSprite>());

            // persist chests
            writer.Write(envItems.OfType<Chest>());

            // persist Enemies
            writer.Write(npcs.OfType<EnemySprite>());

            // persist blacksmiths
            writer.Write(envItems.OfType<BlacksmithSprite>());

            // persist enchanters
            writer.Write(envItems.OfType<EnchanterSprite>());

            writer.Write(envItems.OfType<QuesterSprite>());

            writer.Write(StartDialogueShown);

            // persist teleporters
            writer.Write(envItems.OfType<TeleporterSprite>());

            // persist reset sprites
            writer.Write(envItems.OfType<ResetSprite>());
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            dimension = reader.ReadPoint();
            startLocation = reader.ReadPoint();
            StairsUpLocation = reader.ReadPoint();
            StairsDownLocation = reader.ReadPoint();
            config = reader.Read<LevelConfig>(dataVersion);
            tileSet = TileSet.GetTileSet(config.TileSet.Name);

            tiles = new TileSprite[dimension.X, dimension.Y];
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    TileType type1, type2, type3;
                    type1 = (TileType)reader.ReadByte();
                    type2 = (TileType)reader.ReadByte();
                    type3 = (TileType)reader.ReadByte();
                    tiles[x, y] = new TileSprite(type1, type2, type3, new Vector2(x * tileSet.TileDimension, y * tileSet.TileDimension), tileSet);
                    tiles[x, y].HasBeenSeen = reader.ReadBoolean();

                    if (dataVersion >= 7)
                    {
                        tiles[x, y].IsDoorOpen = reader.ReadBoolean();
                    }
                }
            }

            int envItemsCount = reader.ReadInt32();
            for (int i = 0; i < envItemsCount; i++)
            {
                ItemSprite item = ItemSprite.LoadItem(reader, dataVersion);
                item.Activate();
                envItems.Add(item);
            }

            // load merchant
            int merchantCount = reader.ReadInt32();
            for (int i = 0; i < merchantCount; i++)
            {
                MerchantSprite merchant = new MerchantSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, MerchantType.General);
                merchant.Load(reader, dataVersion);
                envItems.Add(merchant);
            }

            // load chests
            int chestCount = reader.ReadInt32();
            for (int i = 0; i < chestCount; i++)
            {
                Chest chest = new Chest(Vector2.Zero, 1, ChestType.small);
                chest.Load(reader, dataVersion);
                envItems.Add(chest);
            }

            Initialize();

            int enemySpriteCount = reader.ReadInt32();
            for (int i = 0; i < enemySpriteCount; i++)
            {
                NPCSprite npc = EnemySprite.LoadEnemy(reader, dataVersion);
                AddNpc(npc, generatePosition:false);
            }

            if (dataVersion >= 3)
            {
                int blacksmithCount = reader.ReadInt32();
                for (int i = 0; i < blacksmithCount; i++)
                {
                    BlacksmithSprite blacksmith = new BlacksmithSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, Level);
                    blacksmith.Load(reader, dataVersion);
                    envItems.Add(blacksmith);
                }

                int enchanterCount = reader.ReadInt32();
                for (int i = 0; i < enchanterCount; i++)
                {
                    EnchanterSprite enchanter = new EnchanterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer, Level);
                    enchanter.Load(reader, dataVersion);
                    envItems.Add(enchanter);
                }
            }

            if (dataVersion >= 2 && dataVersion <= 6)
            {
                reader.Read<QuestLog>(dataVersion);
            }

            if (dataVersion >= 7)
            {
                int questerCount = reader.ReadInt32();
                for (int i = 0; i < questerCount; i++)
                {
                    var quester = new QuesterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    quester.Load(reader, dataVersion);
                    envItems.Add(quester);
                }

                StartDialogueShown = reader.ReadBoolean();

                int teleporterCount = reader.ReadInt32();
                for (int i = 0; i < teleporterCount; i++)
                {
                    var teleporter = new TeleporterSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    teleporter.Load(reader, dataVersion);
                    envItems.Add(teleporter);
                }

                int resetCount = reader.ReadInt32();
                for (int i = 0; i < resetCount; i++)
                {
                    var reseter = new ResetSprite(Vector2.Zero, SaveGameManager.CurrentPlayer);
                    reseter.Load(reader, dataVersion);
                    envItems.Add(reseter);
                }
            }

            GenerationComplete();
            CreateAndAddStairs();

            // reassign start location to make sure it is valid
            StartLocation = startLocation;

            return true;
        }


        public void InitializeEnemyTextures()
        {
            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i] is EnemySprite)
                {
                    npcs[i].InitializeTextures();
                }
            }
        }

        public void InitializeTileTextures()
        {
            for (int i = 0; i < tiles.GetLength(0); i++)
            {
                for (int j = 0; j < tiles.GetLength(1); j++)
                {
                    tiles[i, j].InitializeTextures();
                }
            }

            for (int i = 0; i < envItems.Count; i++)
            {
                if (envItems[i] is Stairs)
                {
                    Stairs stairs = (Stairs)envItems[i];
                    stairs.InitializeTextures();
                }
            }
        }

        public TileSprite GetTile(Point location)
        {
            return GetTile(location.X, location.Y);
        }

        public TileSprite GetTile(int x, int y)
        {
            if (x >= 0 && x < dimension.X && y >= 0 && y < dimension.Y)
                return tiles[x, y];
            else
                return null;
        }

        public TileSprite GetTileAtPosition(Point point)
        {
            return GetTileAtPosition(point.X, point.Y);
        }

        public TileSprite GetTileAtPosition(Vector2 vector)
        {
            return GetTileAtPosition((int)vector.X, (int)vector.Y);
        }

        public TileSprite GetTileAtPosition(int x, int y)
        {
            x /= TileDimension;
            y /= TileDimension;
            return GetTile(x, y);
        }

        public void SetTile(Point location, TileSprite tile)
        {
            tiles[location.X, location.Y] = tile;
        }

        public void SetTile(int x, int y, TileType tileType, TileType tileType2 = TileType.Empty, TileType tileType3 = TileType.Empty)
        {
            // preserve old tile data
            if (tiles[x, y] != null)
            {
                if (tileType == TileType.Empty)
                {
                    tileType = tiles[x, y].TileTypes[0];
                }
                if (tileType2 == TileType.Empty)
                {
                    tileType2 = tiles[x, y].TileTypes[1];
                }
                if (tileType3 == TileType.Empty)
                {
                    tileType3 = tiles[x, y].TileTypes[2];
                }
            }

            tiles[x, y] = new TileSprite(tileType, tileType2, tileType3, new Vector2(x * tileSet.TileDimension, y * tileSet.TileDimension), tileSet);
            tiles[x, y].TileLocation = new Point(x, y);
        }

        Texture2D minimapTexture;
        Color[] colorMap;

        public void Draw(SpriteBatch spriteBatch)
        {
            Point corner = GameplayScreen.viewportCorner.ToPoint();
            for (int y = (int)(corner.Y / TileDimension - 1); y < (corner.Y / TileDimension) + (DungeonGame.ScreenSize.Height / TileDimension + 4); y++)
            {
                for (int x = (int)(corner.X / TileDimension - 1); x < (corner.X / TileDimension) + (DungeonGame.ScreenSize.Width / TileDimension + 3); x++)
                {
                    TileSprite tile = GetTile(x, y);
                    if (tile != null)
                    {
                        tile.HasBeenSeen = true;
                        tile.Draw(spriteBatch);
                        // Note: Add in to debug pathfinding
                        //if (DungeonGame.TestMode)
                        //{
                        //    if (occupiedMap[x, y])
                        //    {
                        //        spriteBatch.Draw(occupiedSpaceTex, new Rectangle((int)tile.Position.X - corner.X, (int)tile.Position.Y - corner.Y, 64, 64), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
                        //    }
                        //}
                    }
                }
            }

            if (DungeonGame.TestMode)
            {
                // Note: enable to debug pathfinding
                //TileSprite tile = GetTileAtPosition(SaveGameManager.CurrentPlayer.CenteredPosition);
                //spriteBatch.Draw(occupiedSpaceTex, new Rectangle((int)tile.Position.X - corner.X, (int)tile.Position.Y - corner.Y, 64, 64), null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);

            }
        }


        public DialogueScreen GetIntroDialogueScreen()
        {
            DialogueScreen screen;
            if (config.OpeningText.Name == "Player")
            {
                screen = new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(0, 0, 110, 148), SaveGameManager.CurrentPlayer.Name, config.OpeningText.Text);
            }
            else if (config.OpeningText.Name == DwarfBoss.DwarfBossName)
            {
                screen = new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, config.OpeningText.Text);
            }
            else
            {
                screen = new DialogueScreen(null, Rectangle.Empty, config.OpeningText.Name, config.OpeningText.Text);
            }
            return screen;
        }


        public bool HasIntroDialogueScreen
        {
            get { return config.OpeningText != null; }
        }


        public void DrawMiniMap(Point position, Vector2 playerPosition, SpriteBatch spriteBatch, int scale = 1)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            Rectangle drawingPosition = new Rectangle();
            int playerX = (int)(playerPosition.X / tileSet.TileDimension);
            int playerY = (int)(playerPosition.Y / tileSet.TileDimension);

            if (minimapTexture == null)
            {
                colorMap = new Color[dimension.X * dimension.Y];

                minimapTexture = new Texture2D(
                        DungeonGame.Instance.GraphicsDevice,
                        dimension.X,
                        dimension.Y,
                        false,
                        SurfaceFormat.Color);
            }

            for (int x = 0; x < dimension.X; x++)
            {
                for (int y = 0; y < dimension.Y; y++)
                {
                    TileSprite tile = GetTile(x, y); 
                    Color color = Color.Transparent;

                    if (tile.HasBeenSeen)
                    {
                        if (tile.NotAWall)
                        {
                            color = Color.Gray;
                        }
                        else if (!tile.IsEmpty())
                        {
                            color = Color.Red;
                        }
                    }

                    color.A = Math.Min(color.A, (byte)100);

                    colorMap[x + dimension.X * y] = color;
                }
            }

            minimapTexture.SetData(colorMap);

            spriteBatch.Draw(InternalContentManager.GetTexture("Grayout"), new Rectangle(position.X, position.Y, minimapTexture.Width * scale, minimapTexture.Height * scale), Color.White);

            drawingPosition.X = position.X;
            drawingPosition.Y = position.Y;
            drawingPosition.Width = minimapTexture.Width;
            drawingPosition.Height = minimapTexture.Height;
            spriteBatch.Draw(minimapTexture, new Rectangle(position.X, position.Y, minimapTexture.Width * scale, minimapTexture.Height * scale), new Rectangle(0, 0, minimapTexture.Width, minimapTexture.Height), Color.White);

            drawingPosition.X = position.X + playerX * scale - 2;
            drawingPosition.Y = position.Y + playerY * scale - 2;
            drawingPosition.Width = 5;
            drawingPosition.Height = 5;

            spriteBatch.Draw(InternalContentManager.GetTexture("Blank"), drawingPosition, Color.Green);

            if (Level > 0)
            {
                TileSprite stairsUpTile = GetTile(StairsUpLocation);
                if (stairsUpTile != null && stairsUpTile.HasBeenSeen)
                {

                    drawingPosition.X = position.X + StairsUpLocation.X * scale - 2;
                    drawingPosition.Y = position.Y + StairsUpLocation.Y * scale - 2;
                    drawingPosition.Width = 5;
                    drawingPosition.Height = 5;

                    spriteBatch.Draw(InternalContentManager.GetTexture("Blank"), drawingPosition, Color.Yellow);
                }
            }

            if (GetTile(StairsDownLocation).HasBeenSeen)
            {
                drawingPosition.X = position.X + StairsDownLocation.X * scale - 2;
                drawingPosition.Y = position.Y + StairsDownLocation.Y * scale - 2;
                drawingPosition.Width = 5;
                drawingPosition.Height = 5;

                spriteBatch.Draw(InternalContentManager.GetTexture("Blank"), drawingPosition, Color.Yellow);
            }

            // draw friendly NPC's on mini map
            for (int i = 0; i < envItems.Count; i++)
            {
                if (envItems[i] != null &&
                    (envItems[i] is MerchantSprite ||
                    envItems[i] is BlacksmithSprite ||
                    envItems[i] is EnchanterSprite ||
                    envItems[i] is QuesterSprite ||
                    envItems[i] is ResetSprite ||
                    envItems[i] is TeleporterSprite))
                {
                    var merchantTile = GetTileAtPosition(envItems[i].Position);
                    if (merchantTile.HasBeenSeen)
                    {
                        playerX = (int)(merchantTile.Position.X / tileSet.TileDimension);
                        playerY = (int)(merchantTile.Position.Y / tileSet.TileDimension);
                        drawingPosition.X = position.X + playerX * scale - 2;
                        drawingPosition.Y = position.Y + playerY * scale - 2;
                        drawingPosition.Width = 5;
                        drawingPosition.Height = 5;

                        spriteBatch.Draw(InternalContentManager.GetTexture("Blank"), drawingPosition, Color.Blue);
                    }
                }
                else if (envItems[i] != null && envItems[i] is Chest)
                {
                    Chest chest = (Chest)envItems[i];
                    if (chest.IsStash)
                    {
                        var stashTile = GetTileAtPosition(envItems[i].Position);
                        if (stashTile.HasBeenSeen)
                        {
                            playerX = (int)(stashTile.Position.X / tileSet.TileDimension);
                            playerY = (int)(stashTile.Position.Y / tileSet.TileDimension);
                            drawingPosition.X = position.X + playerX * scale - 2;
                            drawingPosition.Y = position.Y + playerY * scale - 2;
                            drawingPosition.Width = 5;
                            drawingPosition.Height = 5;

                            spriteBatch.Draw(InternalContentManager.GetTexture("Blank"), drawingPosition, Color.Orange);
                        }
                    }
                }
            }

            spriteBatch.End();
        }


        public void GenerationComplete()
        {
            AStar = new SpatialAStar<TileSprite, SearchData>(tiles);
            InitializeItemMap();

            // Handle special stairs tiles
            TileSprite tile = tiles[StairsDownLocation.X + 1, StairsDownLocation.Y + 1];
            tile.ForceNotWalkable = true;
            tile.IsFlyable = true;
            itemOccupiedMap[StairsDownLocation.X + 1, StairsDownLocation.Y + 1] = true;
            if (Level > 0)
            {
                for (int x = 0; x <= 1; x++)
                {
                    for (int y = 0; y <= 1; y++)
                    {
                        tile = tiles[StairsUpLocation.X + x, StairsUpLocation.Y + y];
                        tile.ForceNotWalkable = true;
                        itemOccupiedMap[StairsUpLocation.X + x, StairsUpLocation.Y + y] = true;
                    }
                }
            }

            // Make sure pillars don't cover stairs
            for (int x = 0; x <= 2 && x < dimension.X; x++)
            {
                for (int y = 0; y <= 2 && y < dimension.Y; y++)
                {
                    tile = tiles[StairsDownLocation.X + x, StairsDownLocation.Y + y];
                    tile.ForceNotColumn();

                    if (Level > 0)
                    {
                        tile = tiles[StairsUpLocation.X + x, StairsUpLocation.Y + y];
                        tile.ForceNotColumn();
                    }
                }
            }

            // make chests not walk over able
            foreach (var chest in envItems.OfType<Chest>())
            {
                tile = GetTileAtPosition(chest.CenteredPosition);
                tile.ForceNotWalkable = true;
                tile.IsFlyable = true;
            }

            // add tiles into map
            for (int x = 0; x < dimension.X; x++)
            {
                for (int y = 0; y < dimension.Y; y++)
                {
                    if (tiles[x, y].IsDoor)
                    {
                        envItems.Add(tiles[x, y]);
                    }
                }
            }
        }


        public LinkedList<Vector2> GetPath(Vector2 inStartVec, Vector2 inEndVec, SearchData searchData)
        {
            // convert the input vectors to tile points
            Point startNode = new Point((int)inStartVec.X / TileDimension, (int)inStartVec.Y / TileDimension);
            Point endNode = new Point((int)inEndVec.X / TileDimension, (int)inEndVec.Y / TileDimension);

            // get path
            LinkedList<TileSprite> path;
            LinkedList<Vector2> processedPath = new LinkedList<Vector2>();
            path = AStar.Search(startNode, endNode, searchData);
            if (path != null && path.Count > 0)
            {
                // translate the path into a vector path
                //path.RemoveFirst(); // remove first node (we are already there)
                while (path.Count > 0) //while (path.Count > 1)
                {
                    // TODO: avoid allocations here
                    TileSprite tile = path.First();
                    processedPath.AddLast(new Vector2(tile.CenteredPosition.X, tile.CenteredPosition.Y));
                    path.RemoveFirst();
                }

                // replace last node with actual destination (won't be exactly on a node)
                //processedPath.AddLast(inEndVec);
                if (processedPath.Count > 0)
                    processedPath.RemoveFirst();
                if (processedPath.Count > 0)
                    processedPath.RemoveLast();
            }

            return processedPath;
        }


        public bool HasLineOfSight(Vector2 pos1, Vector2 pos2)
        {
            bool los = true;

            // check all tiles between these two and make sure they are walkable


            return los;
        }

        public TileSprite GetRandomOpenTile(SearchData searchData = null, int minimumDistanceFromPlayer = 0)
        {
            TileSprite tile;
            bool invalidTile;
            do
            {
                invalidTile = false;
                tile = GetTile(Util.Random.Next(0, Dimension.X - 1), Util.Random.Next(0, Dimension.Y - 1));

                // make sure that adjacent tiles are not walls so that think object can be navigated around
                for (int x = tile.TileLocation.X - 1; x <= tile.TileLocation.X + 1; x++)
                {
                    for (int y = tile.TileLocation.Y - 1; y <= tile.TileLocation.Y + 1; y++)
                    {
                        if (x > 0 && x < dimension.X && y > 0 && y < dimension.Y)
                        {
                            if (!GetTile(new Point(x, y)).IsWalkable(null))
                            {
                                invalidTile = true;
                                break;
                            }
                        }
                    }
                    if (invalidTile)
                    {
                        break;
                    }
                }
            } while (invalidTile || !tile.IsWalkable(searchData) || IsPositionOccupied(tile.Position.ToPoint()) || startLocation.DistanceTo(tile.TileLocation) < minimumDistanceFromPlayer);
            return tile;
        }


        public TileSprite GetClosestOpenTile(Vector2 position, bool checkOccupiedMap = true)
        {
            // move the location until we are sure that it is valid
            Point location = new Point((int)position.X / TileDimension, (int)position.Y / TileDimension);
            Point center = new Point(location.X, location.Y);
            int distance = 1;
            bool found = false;

            // we can only do this if the map tiles have been filled in
            if (GetTile(location) == null)
                return null;

            SearchData sd = null;
            if (checkOccupiedMap)
            {
                sd = new SearchData();
                sd.occupiedMap = occupiedMap;
            }

            while (!GetTile(location).IsWalkable(sd))
            {
                // find the closest open space
                for (int x = Math.Max(center.X - distance, 0); x <= center.X + distance && x < dimension.X; x++)
                {
                    for (int y = Math.Max(center.Y - distance, 0); y <= center.Y + distance && y < dimension.Y; y++)
                    {
                        if (GetTile(x, y).IsWalkable(sd))
                        {
                            location.X = x;
                            location.Y = y;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                distance++;
            }

            return GetTile(location);
        }


        public bool AttemptToOccupyTile(ref Point location, Point newLocation)
        {
            if (IsPositionOccupied(newLocation) == false)
            {
                occupiedMap[newLocation.X / TileDimension, newLocation.Y / TileDimension] = true;
                occupiedMap[location.X, location.Y] = false;
                location.X = newLocation.X / TileDimension;
                location.Y = newLocation.Y / TileDimension;
                return true;
            }

            return false;
        }


        public void ReleaseOccupiedPosition(Point location)
        {
            occupiedMap[location.X, location.Y] = false;
        }


        private Point itemCenter = new Point();
        public Point ItemOccupyPosition(Point location)
        {
            location.X = location.X / TileDimension;
            location.Y = location.Y / TileDimension;

            if (location.X < 0 || location.Y < 0 || location.X > Dimension.X || location.Y > dimension.Y)
            {
                location.X = 0;
                location.Y = 0;
            }

            itemCenter.X = location.X;
            itemCenter.Y = location.Y;
            int distance = 1;
            bool found = false;
            while (itemOccupiedMap[location.X, location.Y])
            {
                // find the closest open space
                for (int x = Math.Max(itemCenter.X - distance, 0); x <= itemCenter.X + distance && x < itemOccupiedMap.GetLength(0); x++)
                {
                    for (int y = Math.Max(itemCenter.Y - distance, 0); y <= itemCenter.Y + distance && y < itemOccupiedMap.GetLength(1); y++)
                    {
                        if (false == itemOccupiedMap[x, y])
                        {
                            location.X = x;
                            location.Y = y;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                distance++;
            }

            itemOccupiedMap[location.X, location.Y] = true;
            return location;
        }


        public void ReleaseItemPosition(Point location)
        {
            itemOccupiedMap[location.X, location.Y] = false;
        }


        public Point OccupyPosition(Point location)
        {
            location.X = location.X / TileDimension;
            location.Y = location.Y / TileDimension;
            occupiedMap[location.X, location.Y] = true;
            return location;
        }


        public bool IsPositionOccupied(Point location)
        {
            return occupiedMap[location.X / TileDimension, location.Y / TileDimension];
        }

        public bool IsPositionOccupied(Point location, Point previousLocation)
        {
            if (GetTileAtPosition(location) == GetTileAtPosition(previousLocation))
            {
                return false;
            }

            return occupiedMap[location.X / TileDimension, location.Y / TileDimension];
        }


        public void PopulateDungeon(PlayerSprite player)
        {
            foreach (var enemyConfig in config.Enemies)
            {
                EnemySprite.PlaceEnemies(enemyConfig.Type, enemyConfig.Count, this, BalanceManager.DungeonLevelToMonsterLevel(Level), player);
            }


            if (config.Type == "Dungeon")
            {
                // promote a few NPC's to champions
                int champNum = Util.Random.Next(3, 6);
                for (int i = 0; i < champNum; i++)
                {
                    EnemySprite enemy = npcs[Util.Random.Next(0, npcs.Count)] as EnemySprite;
                    if (enemy != null && !enemy.IsChampion)
                    {
                        enemy.SetChampion();
                    }
                }

                // Add chests
                int chestNum;
                if (DungeonGame.TestMode)
                {
                    chestNum = 100;
                }
                else
                {
                    chestNum = Util.Random.Next(5, 10);
                }

                TileSprite tile;
                for (int i = 0; i < chestNum; i++)
                {
                    Chest chest = new Chest(Vector2.Zero, Level, (ChestType)Util.Random.Next(0, 2));
                    AddEnvItem(chest, generatePosition: true);
                    tile = GetTileAtPosition(chest.CenteredPosition);
                    tile.ForceNotWalkable = true;
                    tile.IsFlyable = true;
                }
            }
            else if (config.Type == "Town")
            {
                // make sure chests added aren't walkable
                for (int i = 0; i < envItems.Count; i++)
                {
                    if (envItems[i] is Chest)
                    {
                        TileSprite tile = GetTileAtPosition(envItems[i].CenteredPosition);
                        tile.ForceNotWalkable = true;
                        tile.IsFlyable = true;
                        
                    }
                }
            }

        }

        public void AddNpc(NPCSprite npc, int minDistance = 0, bool generatePosition = true)
        {
            if (generatePosition)
            {
                TileSprite tile = GetRandomOpenTile(minimumDistanceFromPlayer: minDistance);
                npc.SetNPCPosition(tile.Position);
            }
            else if (!npc.IsDead)
            {
                // find the closest open position
                TileSprite tile = GetClosestOpenTile(npc.CenteredPosition);
                npc.SetNPCPosition(tile.Position, this);
            }

            // generate an enemy for that location
            npcs.Add(npc);
            npc.Activate();
        }


        public void AddEnvItem(IEnvItem envItem, int minDistance = 0, bool generatePosition = false)
        {
            if (generatePosition)
            {
                TileSprite tile = GetRandomOpenTile(minimumDistanceFromPlayer: minDistance);
                envItem.Position = tile.Position;
            }

            envItems.Add(envItem);
        }


        public void CreateAndAddStairs()
        {
            Stairs stairs = new Stairs(false, TileSet, StairsDownPosition, false);
            stairs.Activate();
            AddEnvItem(stairs);

            if (Level > 0)
            {
                stairs = new Stairs(true, TileSet, new Vector2(StairsUpLocation.X * TileSet.TileDimension, StairsUpLocation.Y * TileSet.TileDimension), false);
                stairs.Activate();
                AddEnvItem(stairs);
            }
        }

        public Point Dimension
        {
            get { return dimension; }
        }

        public Point StartLocation
        {
            get { return startLocation; }
            set 
            {
                startLocation = value;

                // move the location until we are sure that it is valid
                Point center = new Point(startLocation.X, startLocation.Y);
                int distance = 1;
                bool found = false;

                // we can only do this if the map tiles have been filled in
                if (GetTile(startLocation) == null)
                    return;

                while (!GetTile(startLocation).IsWalkable())
                {
                    // find the closest open space
                    for (int x = Math.Max(center.X - distance, 0); x <= center.X + distance && x < dimension.X; x++)
                    {
                        for (int y = Math.Max(center.Y - distance, 0); y <= center.Y + distance && y < dimension.Y; y++)
                        {
                            if (GetTile(x, y).IsWalkable())
                            {
                                startLocation.X = x;
                                startLocation.Y = y;
                                found = true;
                                break;
                            }
                        }
                        if (found)
                            break;
                    }
                    distance++;
                }
            }
        }

        public int TileDimension
        {
            get { return tileSet.TileDimension; }
        }

        public bool[,] OccupiedMap
        {
            get { return occupiedMap; }
        }

        public IEnumerable<EnemyType> EnemyTypes
        {
            get { return config.Enemies.Select(e => e.Type); }
        }

        public ContentManager Content
        {
            get { return contentManager; }
            set { contentManager = value; }
        }

        public List<NPCSprite> NPCs
        {
            get { return npcs; }
        }

        public TileSet TileSet
        {
            get { return tileSet; }
        }

        public List<IEnvItem> EnvItems
        {
            get { return envItems; }
        }

        public LevelConfig Config
        {
            get { return config; }
        }

        public string MapType 
        {
            get { return config.Type; }
        }
    }
}
