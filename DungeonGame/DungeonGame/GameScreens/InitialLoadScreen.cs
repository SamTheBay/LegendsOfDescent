using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendsOfDescent.Quests;

#if WINDOWS_PHONE
    using Microsoft.Phone.Info;
#endif

namespace LegendsOfDescent
{

    class InitialLoadScreen : GameScreen
    {
        Vector2 textLocation;
        Vector2 displayStringLocation;
        int iteration = 0;
        Texture2D BlankBarHorz;
        Texture2D FullBarHorz;
        Texture2D celticBoarder;
        Vector2 BarCorner;
        Rectangle BarFill;
        static DungeonLevel lastDungeon = null;
        static DungeonLevel currentDungeon = null;
        GameplayScreen gamePlayScreen = null;
        string displayString = "";
        int detailIndex = 0;
        bool showStats = false;
        Int64[] statsIndexes = new Int64[4];
        string[] detailString = {"The level of an item is displayed in its title in parentheses. The higher the level of an item the more powerful and valuable it is.",
                                 "The color of an items title shows how rare it is. Blue is Magic, Green is Rare, Red is Heroic and Purple is Legendary.",
                                 "Abilities can be upgraded 6 times in order to increase their power.",
                                 "Remember to check the Update Notes section to find out what is new after downloading an update.",
                                 "Skeletons have magic resistances based on their color. Blue resists cold, Green resists poison, Yellow resists lightning and Red resists fire.",
                                 "Ghosts have resistance to physical weapons and can be killed easier using spells.",
                                 "Visit LegendsOfDescent.com to participate in the LoD community and help shape the future of the game.",
                                 "Use warp scrolls to avoid having to walk long distances when you hit a dead end in a dungeon",
                                 "You can turn the sound effects and music on and off from the options menu.",
                                 "Use leather or cloth armor and a wand or staff to make an effective mage character. This will make spells cast faster and use less mana.",
                                 "Champion creatures have a name that displays above them. They are much more difficult to defeat, but they also drop better loot and give more XP.",
                                 "You can change the game play screen to landscape mode in the options screen.",
                                 "Champion creatures with a yellow name are strong, red name are fearsome and purple name are dreaded.",
                                 "Goblins are weak and travel in packs. However, each pack includes one leader which is significantly stronger than the rest.",
                                 "Once you reach level 5 you can pick your tier 1 class of Warrior, Rogue or Mage in the stats menu.",
                                 "Enchanter NPCs will add magical effects to your gear for gold.",
                                 "Blacksmith NPCs can enhance your weapons and armor to make them more effective.",
                                 "If a rogue attacks while invisible it's enemy is unprepared for the blow and it is a guaranteed critical hit.",
                                 "Magic resistance reduces the amount of fire, ice, lightning and poison damage that your character will take.",
                                 "Every dungeon has several quests which can be completed for extra rewards. Look at the \"Quest\" tab in the options menu to view your current quests.",
                                 "A full set of mail armor will reduce your attack speed with bows by 10%. A full set of plate will reduce your attack speed with bows by 25%. Leather and cloth do not decrease ranged attack speed.",
                                 "You can cast a town portal from any dungeon to return to town and sell trade your loot. Simply go to the options menu to do this.",
                                 "You can use the teleporter NPC in town to get to any dungeon level you have previously visited instantly.",
                                 "Some of the merchants in town specialize in selling gear that is best for a particular class. You should find the one with gear that matches your class best.",
                                 "Items with a white title are of normal rarity and sell for only a small portion of gold. You might not want to pick these up to save room for more valuable items.",
                                 "In the settings you can toggle the touch input on and off as well as the touch joystick.",
                                 "Press 'Z' or tap the 'ABC' button in the top left corner to show the names of all the items on the ground."};

        public InitialLoadScreen()
        {
            displayString = "Loading LoD";
            Initialize(null);
        }


        public InitialLoadScreen(DungeonLevel dungeon)
        {
            displayString = "Loading Dungeon";
            Initialize(dungeon);
        }

        private void Initialize(DungeonLevel dungeon)
        {
            // handle screen rotation
            if (DungeonGame.currentlyPortraitMode && !DungeonGame.portraitMode && currentDungeon != null && lastDungeon != null)
            {
                DungeonGame.Instance.SetLandscapeMode();
            }
            else if (!DungeonGame.currentlyPortraitMode && DungeonGame.portraitMode)
            {
                DungeonGame.Instance.SetPortraitMode();
            }

            celticBoarder = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\celticBoarderLong");

            detailIndex = Util.Random.Next(0, detailString.Length);
            showStats = Util.Random.Next(0, 4) == 0;
            BlankBarHorz = DungeonGame.Instance.Content.Load<Texture2D>(System.IO.Path.Combine(@"Textures\PlayerDisplay", "emptybarhoriz"));
            FullBarHorz = DungeonGame.Instance.Content.Load<Texture2D>(System.IO.Path.Combine(@"Textures\PlayerDisplay", "blankfullbarhoriz"));
            BarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - FullBarHorz.Width / 2, DungeonGame.ScreenSize.Height/2 - FullBarHorz.Height / 2 + DungeonGame.ScreenSize.Y);
            BarFill = new Rectangle(0, 0, 0, FullBarHorz.Height);
            textLocation = new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2 - 80 + DungeonGame.ScreenSize.Y);
            displayStringLocation = new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2 + 20 + DungeonGame.ScreenSize.Y);
            currentDungeon = dungeon;

            if (SaveGameManager.CurrentPlayer != null)
            {
                SaveGameManager.PersistPlayer();
            }

            if (showStats)
            {
                for (int i = 0; i < statsIndexes.Length; i++)
                {
                    bool isUnique = false;
                    while (!isUnique)
                    {
                        statsIndexes[i] = Util.Random.Next(0, (int)StatType.num);
                        isUnique = true;
                        for (int j = 0; j < i; j++)
                        {
                            if (statsIndexes[i] == statsIndexes[j])
                            {
                                isUnique = false;
                                break;
                            }
                        }
                    }
                }
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
#if WINDOWS_PHONE
            // handle screen rotation
            if (DungeonGame.currentlyPortraitMode && !DungeonGame.portraitMode && currentDungeon != null && lastDungeon != null)
            {
                DungeonGame.Instance.SetLandscapeMode();
            }
            else if (!DungeonGame.currentlyPortraitMode && DungeonGame.portraitMode)
            {
                DungeonGame.Instance.SetPortraitMode();
            }
            long deviceTotalMemory = (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory");
            long applicationCurrentMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
            long applicationPeakMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage");
            Debug.WriteLine("using " + ((float)applicationCurrentMemoryUsage / 1024f / 1024f).ToString() + " of " + ((float)deviceTotalMemory / 1024f / 1024f).ToString() +
                " with a peak of " + ((float)applicationPeakMemoryUsage / 1024f / 1024f).ToString() + " on iteration " + iteration.ToString());
#endif
            Debug.WriteLine("Seed: " + Util.Seed);


            iteration++;

            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (currentDungeon == null && lastDungeon == null)
            {
                LoadInitial();
            }
            else if (lastDungeon != null)
            {
                UnloadDungeon();
                lastDungeon = null;
                iteration--;
                GC.Collect();
            }
            else
            {
                LoadDungeon();
            }

            if (iteration == 22)
                GC.Collect();

        }


        int sleepIterations = 0;
        public void LoadInitial()
        {
            if (sleepIterations < 20)
            {
                sleepIterations++;
                iteration--;
                return;
            }

            // load up content
            if (iteration == 2)
            {
                InternalContentManager.Load();
            }

            // load up one sound effect
            if (iteration == 4)
            {
                AudioManager.Initialize();
            }

            if (iteration == 6)
            {
                AudioManager.audioManager.LoadAllSFX();
            }

            if (iteration == 10)
            {
                DungeonGame.itemManager = new ItemManager();
                ItemManager.Instance.Load();
            }

            if (iteration == 12)
            {
                new HelpScreenManager();
            }

            if (iteration == 19)
            {
                QuestLog.LoadFixedQuests();
            }

            if (iteration == 20)
            {
                SaveGameManager.Initialize();

#if WIN8
                AnalyticsManagerWin8.Augment();
#else
                AnalyticsManager.Augment();
#endif

                // TODO: remove
                //SaveGameManager.SetTestPlayer();
            }

            if (iteration == 22)
            {
                AddNextScreenAndExit(new IntroScreen());

                // TODO: remove
                //AddNextScreenAndExit(new InitialLoadScreen(new Dungeon(new Point(100, 100), new TileSet("BrickTiles"), 1, true)));
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            BarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - FullBarHorz.Width / 2, DungeonGame.ScreenSize.Height / 2 - FullBarHorz.Height / 2 + DungeonGame.ScreenSize.Y);
            BarFill = new Rectangle(0, 0, 0, FullBarHorz.Height);
            textLocation = new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2 - 80 + DungeonGame.ScreenSize.Y);
            displayStringLocation = new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2 + 20 + DungeonGame.ScreenSize.Y);

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, displayString, textLocation, Color.Red);

            // draw progress bar
            //spriteBatch.Draw(BlankBarHorz, BarCorner, Color.White);
            BarFill.Width = (int)((float)FullBarHorz.Width * (float)iteration / 22f);
            spriteBatch.Draw(FullBarHorz,
                BarCorner,
                BarFill,
                Color.Red);

            spriteBatch.Draw(celticBoarder, new Vector2(BarCorner.X - 9, BarCorner.Y - 2), null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);

            // draw details
            List<String> list;
            if (showStats && SaveGameManager.CurrentPlayer != null)
            {
                StatsManager statsManager = SaveGameManager.CurrentPlayer.StatsManager;
                list = new List<string>();
                list.Add(SaveGameManager.CurrentPlayer.Name);
                for (int i = 0; i < statsIndexes.Length; i++)
                {
                    list.Add(statsManager.GetName((StatType)statsIndexes[i]) + ":   " + statsManager.Get((StatType)statsIndexes[i]).ToString("N0"));
                }
            }
            else
            {
                list = Fonts.BreakTextIntoList(detailString[detailIndex], Fonts.DescriptionFont, 400);
            }
            Vector2 loc = new Vector2(DungeonGame.ScreenSize.Width / 2, BarCorner.Y + 60);
            float heightAdjust = Fonts.DescriptionFont.MeasureString(list[0]).Y;
            for (int i = 0; i < list.Count; i++)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, list[i], loc, Color.White);
                loc.Y += heightAdjust;
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void UnloadDungeon()
        {
            InternalContentManager.UnloadDungeon(lastDungeon);
            lastDungeon.Content.Dispose();
            lastDungeon.Content = null;
        }

        private void LoadDungeon()
        {
            if (sleepIterations < 20)
            {
                sleepIterations++;
                iteration--;
                return;
            }

            // load the player textures if necessary
            else if (iteration == 6)
            {
                InternalContentManager.LoadPlayer(SaveGameManager.CurrentPlayer.PlayerClass);
                SaveGameManager.CurrentPlayer.InitializeTextures();
            }

            // load textures for the dungeon
            else if (iteration == 8)
            {
                InternalContentManager.LoadDungeon(currentDungeon);
                if (!IsFirstDungeonLoad)
                {
                    currentDungeon.InitializeEnemyTextures();
                    currentDungeon.InitializeTileTextures();
                }
            }

            // generate the dungeon
            else if (iteration == 16)
            {
                if (IsFirstDungeonLoad)
                {
                    new MapGenerator(currentDungeon).GenerateMap();
                    currentDungeon.InitializeTileTextures();
                }
                gamePlayScreen = new GameplayScreen(SaveGameManager.CurrentPlayer, currentDungeon);
            }


            // add in the characters
            else if (iteration == 20 && IsFirstDungeonLoad)
            {
                currentDungeon.PopulateDungeon(SaveGameManager.CurrentPlayer);
                currentDungeon.InitializeEnemyTextures();
            }

            // create new game
            else if (iteration == 22)
            {
                SaveGameManager.CurrentPlayer.QuestLog.Initialize(SaveGameManager.CurrentPlayer);
                gamePlayScreen.SetDungeon(currentDungeon);
                lastDungeon = currentDungeon;
                AddNextScreenAndExit(gamePlayScreen);

                if (IsFirstDungeonLoad)
                {
                    SaveGameManager.PersistDungeon(currentDungeon);

                    if (SaveGameManager.CurrentPlayer.DungeonLevelsGenerated < currentDungeon.Level)
                    {
                        SaveGameManager.CurrentPlayer.DungeonLevelsGenerated = currentDungeon.Level;
                        SaveGameManager.PersistPlayer();
                    }
                }

                currentDungeon.ForceReload = false;
            }
        }



        private bool IsFirstDungeonLoad
        {
            get { return SaveGameManager.CurrentPlayer.DungeonLevelsGenerated < currentDungeon.Level || currentDungeon.ForceReload;  }
        }

    }
}