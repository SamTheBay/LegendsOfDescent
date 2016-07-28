using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegendsOfDescent
{
    class StairsScreen : PopUpScreen
    {
        static Rectangle dimension = new Rectangle(0, 0, 450, 300);

        bool stairsUp;
        PlayerSprite player;
        DungeonLevel dungeon;

        public StairsScreen(bool stairsUp, PlayerSprite player, DungeonLevel dungeon)
        {
            this.stairsUp = stairsUp;
            this.player = player;
            this.dungeon = dungeon;
            GameplayScreen.Instance.Pause();

            WindowCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - dimension.Width / 2, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y - dimension.Height / 2);
            if (!DungeonGame.currentlyPortraitMode && !DungeonGame.pcMode && !DungeonGame.paid)
                MoveWindowCorner(0, 40);


            Rectangle location = new Rectangle((DungeonGame.ScreenSize.Width - 300) / 2, (int)WindowCorner.Y + 170, 300, 50);
            MenuEntry entry;
            entry = new MenuEntry("Yes", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Location = location;
            MenuEntries.Add(entry);

            location.Y += 60;
            entry = new MenuEntry("No", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Location = location;
            MenuEntries.Add(entry);

            // clear input since we can get added during an update call
            InputManager.ClearInputForPeriod(500);

        }


        public override void OnRemoval()
        {
            base.OnRemoval();

            GameplayScreen.Instance.UnPause();
        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (sender.Equals(MenuEntries[0]))
            {
                SaveGameManager.PersistDungeon(GameplayScreen.Dungeon);

                // load a new dungeon
                DungeonLevel newDungeon;
                if (stairsUp)
                {
                    AudioManager.audioManager.PlaySFX("StairsUp");
                    newDungeon = new DungeonLevel();
                    player.DungeonLevel--;
                    SaveGameManager.LoadDungeon(ref newDungeon);
                    newDungeon.GoingDown = false;
                }
                else
                {
                    AudioManager.audioManager.PlaySFX("StairsDown");
                    player.DungeonLevel++;
                    if (dungeon.Level + 1 <= player.DungeonLevelsGenerated)
                    {
                        newDungeon = new DungeonLevel();
                        SaveGameManager.LoadDungeon(ref newDungeon);
                    }
                    else
                    {
                        int level = dungeon.Level + 1;
                        newDungeon = new DungeonLevel(LevelOrchestrator.GetConfig(level), true);
                    }
                    newDungeon.GoingDown = true;

                    Quests.QuestLog.GiveUnassignedQuestsForLevel(player.DungeonLevel, player);
                }
                player.Position = Vector2.Zero;
                ScreenManager.AddScreen(new InitialLoadScreen(newDungeon));
                ScreenManager.RemoveScreen(this);
                ScreenManager.RemoveScreen(GameplayScreen.Instance);
            }
            else if (sender.Equals(MenuEntries[1]))
            {
                // quit / No
                ExitScreen();
            }
            
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            // draw boarder
            int boarderWidth = 6;
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            Color fontColor = Color.White;
            DrawBorder(new Rectangle((int)WindowCorner.X - boarderWidth, (int)WindowCorner.Y - boarderWidth, dimension.Width + boarderWidth * 2, dimension.Height + boarderWidth * 2),
                InternalContentManager.GetSolidColorTexture(Color.Gray), boarderWidth, Color.Black, spriteBatch);

            // draw text
            if (stairsUp)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Would you like to", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 50), fontColor, 1.5f);
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "take the stairs up", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 90), fontColor, 1.5f);
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "to the previous level?", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 130), fontColor, 1.5f);
            }
            else
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "Would you like to", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 50), fontColor, 1.5f);
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "take the stairs down", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 90), fontColor, 1.5f);
                Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, "to the next level?", new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 130), fontColor, 1.5f);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}
