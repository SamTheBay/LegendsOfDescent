using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendsOfDescent.Quests;

namespace LegendsOfDescent
{
    class LoadGameScreen : MenuScreen
    {
        int titleHeight;

        public LoadGameScreen()
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
                if (entry.Text == "Empty")
                {
                    entry.IsActive = false;
                }
                MenuEntries.Add(entry);
                location.Y += buttonSpace;
            }

        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (SaveGameManager.IsValidSave(selectorIndex))
            {
                // reload the fixed quests since the objects get edited during game play.
                QuestLog.LoadFixedQuests();

                PlayerSprite.loadedInstance = selectorIndex;
                PlayerSprite player = new PlayerSprite(Vector2.Zero);
                SaveGameManager.LoadPlayer(selectorIndex, ref player);
                DungeonLevel dungeon;
                dungeon = new DungeonLevel();
                bool result = SaveGameManager.LoadDungeon(ref dungeon);

#if !DEBUG
                // failsafe for corrupt dungeon files
                if (!result)
                {
                    dungeon = new DungeonLevel(LevelOrchestrator.GetConfig(player.DungeonLevel), true);
                    dungeon.ForceReload = true;
                    player.Position = Vector2.Zero;
                }
#endif

                GameScreen screen = new InitialLoadScreen(dungeon);
                AddNextScreenAndExit(screen);
            }
        }



        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the title
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFontLarge, "Load Game", new Vector2(DungeonGame.ScreenSize.Width / 2, titleHeight), Color.Red);

            spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
