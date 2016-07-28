using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendsOfDescent.Quests;

#if !SILVERLIGHT
    using Microsoft.Xna.Framework.GamerServices;
using System.Diagnostics;
#endif

namespace LegendsOfDescent
{
    class NewGameScreen : MenuScreen
    {
        bool gettingName = false;
        bool shownShouldDelete = false;
        ContinueQuestion shouldDelete = new ContinueQuestion("Are you sure you want to delete this save game?");
        string saveGameName = "";
        IAsyncResult result = null;
        TextInputScreen inputScreen;
        bool gettingDifficulty = false;
        DifficultyScreen difficultyScreen = null;
        StringBuilder adjustedName;
        int titleHeight;

        public NewGameScreen()
            : base()
        {
            int centeredOffset = (DungeonGame.ScreenSize.Height - 600) / 2 + DungeonGame.ScreenSize.Y;
            int buttonStartHeight = centeredOffset + 150;
            int buttonHeight = 60;
            titleHeight = centeredOffset + 85;

            if (DungeonGame.pcMode)
            {
                ActivateBackButton(new Vector2(20, 140));
            }

            int buttonSpace = buttonHeight + 10;
            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStartHeight, 450, buttonHeight);


            for (int i = 0; i < SaveGameManager.maxSaveGameNum; i++)
            {
                // start the animations
                MenuEntry entry = new MenuEntry(SaveGameManager.SaveName(i), location);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
                location.Y += buttonSpace;
            }

        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (gettingName == false)
            {
                if (SaveGameManager.IsValidSave(selectorIndex))
                {
                    // ask if they want to delete the previous owner of this slot
                    AddNextScreen(shouldDelete);
                    shownShouldDelete = true;
                }
                else
                {
                    // get the name of the new player
                    BeginGetName();
                }

            }
        }

        private void BeginGetName()
        {
            shouldDelete = null;
            gettingName = true;

            if (DungeonGame.pcMode)
            {
                inputScreen = new TextInputScreen(NameGenerator.GetName(NameGenerator.Style.MaleDwarvenTolkien), "Enter your new character's name");
                AddNextScreen(inputScreen);
            }
            else
            {
#if !SILVERLIGHT
                result = Guide.BeginShowKeyboardInput(
                    PlayerIndex.One,
                    "New Player Name",
                    "The name of your new characer",
                    NameGenerator.GetName(NameGenerator.Style.MaleDwarvenTolkien), null, null);
#endif
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // check if the should delete question is finished
            if (gettingName == false && shownShouldDelete == true && shouldDelete != null)
            {
                if (shouldDelete.IsFinished)
                {
                    if (shouldDelete.Result == true)
                    {
                        shouldDelete.ExitScreen();  
                        BeginGetName();
                    }
                    else
                    {
                        ExitScreen();
                        return;
                    }
                }
            }

            // check if screen input is finished
            else if (gettingName == true && ((result != null && result.IsCompleted == true) || (inputScreen != null && inputScreen.IsFinished)))
            {
                // get name
                if (!DungeonGame.pcMode)
                {
#if !SILVERLIGHT
                    saveGameName = Guide.EndShowKeyboardInput(result);
#endif
                    result = null;
                }
                else
                {
                    if (inputScreen.IsSuccessful)
                        saveGameName = inputScreen.Input;
                    else
                        saveGameName = null;
                    inputScreen = null;
                }

                adjustedName = new StringBuilder();

                if (saveGameName == null)
                {
                    ExitScreen();
                    return;
                }


                // crunch the save game name to remove any invalid characters
                for (int i = 0; i < saveGameName.Length && i < 15; i++)
                {
                    if (Fonts.HeaderFont.Characters.Contains(saveGameName[i]))
                    {
                        adjustedName.Append(saveGameName[i]);
                    }
                }

                GetDifficulty();
            }

            else if (gettingDifficulty == true && difficultyScreen.IsClosed)
            {
                gettingDifficulty = false;

                if (difficultyScreen.ActionHandled)
                {
                    // create new game
                    CreateGame(adjustedName, difficultyScreen.Difficulty);
                }
                else
                {
                    ExitScreen();
                }
            }
        }


        public void GetDifficulty()
        {
            gettingDifficulty = true;
            difficultyScreen = new DifficultyScreen();
            AddNextScreen(difficultyScreen);
        }


        public void CreateGame(StringBuilder adjustedName, GameDifficulty difficulty)
        {
            PlayerSprite player = new PlayerSprite(Vector2.Zero);
            player.Name = adjustedName.ToString();
            player.GameDifficulty = difficulty;
            
            if (player.Name.Equals("i am a cheater") || (Debugger.IsAttached && player.Name.EndsWith("xz")))
            {
                MakeSuperPlayer(player);
            }

            // reload the fixed quests since the objects get edited during game play.
            QuestLog.LoadFixedQuests();

            player.QuestLog.Quests.Add(QuestLog.FixedQuests[0]);
            SaveGameManager.RegisterNewPlayer(player, selectorIndex);
            LevelOrchestrator.Load();
            DungeonLevel dungeon = new DungeonLevel(LevelOrchestrator.GetConfig(0), true);
            GameScreen screen = new InitialLoadScreen(dungeon);
            if (shownShouldDelete)
            {
                ScreenManager.AddScreen(screen);
                ExitScreen();
            }
            else
            {
                AddNextScreenAndExit(screen);
            }

#if WIN8
            DungeonGame.productsManager.RefreshLicenseState();
#endif
        }
        
        private static void MakeSuperPlayer(PlayerSprite player)
        {
            HelpScreenManager.Instance.MarkAsShown(HelpScreens.LevelUp);
            //player.AddExperience(BalanceManager.GetCumulativeExpNeeded(player.Level, 100));
            player.AddGold(1000000);
            player.EquippedItems[(int)EquipSlot.Head, 0].AddModifier(new PropertyModifier(Property.MoveSpeed, 400));
            player.Heal(player.MaxHealth);
            player.AddMana(player.MaxMana);
        }

        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the title
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFontLarge, "New Game", new Vector2(DungeonGame.ScreenSize.Width / 2, titleHeight), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
