using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{
    class TeleportScreen : PopUpScreen
    {
        static int boarderWidth = 6;
        PlayerSprite player;
        HelpButton helpButton;
        Vector2 titleVector;
        Texture2D arrowUp;

        int dungeonStartLevel = 1;
        int buttonNumPerRow = 5;
        int buttonRows = 5;


        public TeleportScreen(PlayerSprite player)
            : base()
        {
            CloseOnTapOutOfDimension = true;
            this.player = player;
            arrowUp = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/ArrowUp");

            EnabledGestures = GestureType.Tap;

            GameplayScreen.Instance.Pause();

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + ((DungeonGame.ScreenSize.Height - 720) / 2), DungeonGame.ScreenSize.Width - boarderWidth * 2, 720 - boarderWidth * 2);

                helpButton = new HelpButton(HelpScreens.Teleport, new Vector2(WindowCorner.X + dimension.Width - 60, WindowCorner.Y + dimension.Height - 60));

                titleVector = new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35);

                // add scroll buttons
                MenuEntry entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Position = new Vector2(WindowCorner.X + dimension.Width / 2 - 100 - arrowUp.Width / 2, dimension.Height - 20 - arrowUp.Height);
                entry.Texture = arrowUp;
                entry.OwningPopup = this;
                MenuEntries.Add(entry);

                entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Position = new Vector2(WindowCorner.X + dimension.Width / 2 + 100 - arrowUp.Width / 2, dimension.Height - 20 - arrowUp.Height);
                entry.Texture = arrowUp;
                entry.OwningPopup = this;
                entry.SpriteEffect = SpriteEffects.FlipVertically;
                MenuEntries.Add(entry);

                // add buttons for each warp location
                buttonNumPerRow = 3;
                buttonRows = 9;
                int level = dungeonStartLevel;
                int buttonStartX = (int)WindowCorner.X + 40;
                Rectangle locaiton = new Rectangle(buttonStartX, (int)WindowCorner.Y + 75, 120, 50);
                for (int i = 0; i < buttonRows; i++)
                {
                    for (int j = 0; j < buttonNumPerRow; j++)
                    {
                        entry = new MenuEntry(level.ToString(), locaiton);
                        entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                        entry.IsActive = false;
                        MenuEntries.Add(entry);

                        level++;
                        locaiton.X += locaiton.Width + 10;
                    }
                    locaiton.Y += locaiton.Height + 10;
                    locaiton.X = buttonStartX;
                }
            }
            else
            {
                if (DungeonGame.pcMode)
                    dimension = new Rectangle((DungeonGame.ScreenSize.Width - 800) / 2 + boarderWidth, (DungeonGame.ScreenSize.Height - 400) / 2 + DungeonGame.ScreenSize.Y + boarderWidth, 800 - boarderWidth * 2, 400 - boarderWidth * 2);
                else
                    dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + 80, DungeonGame.ScreenSize.Width - boarderWidth * 2, 400 - boarderWidth * 2);

                helpButton = new HelpButton(HelpScreens.Teleport, new Vector2(WindowCorner.X + dimension.Width - 60, WindowCorner.Y + 10));

                titleVector = new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35);

                // add scroll buttons
                MenuEntry entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Position = new Vector2(dimension.Width - 100, 60);
                entry.Texture = arrowUp;
                entry.OwningPopup = this;
                MenuEntries.Add(entry);

                entry = new MenuEntry("");
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.Position = new Vector2(dimension.Width - 100, dimension.Height - 30 - arrowUp.Height);
                entry.Texture = arrowUp;
                entry.OwningPopup = this;
                entry.SpriteEffect = SpriteEffects.FlipVertically;
                MenuEntries.Add(entry);

                // add buttons for each warp location
                buttonNumPerRow = 5;
                buttonRows = 5;
                int level = dungeonStartLevel;
                int buttonStartX = (int)WindowCorner.X + 30;
                Rectangle locaiton = new Rectangle(buttonStartX, (int)WindowCorner.Y + 65, 120, 50);
                for (int i = 0; i < buttonRows; i++)
                {
                    for (int j = 0; j < buttonNumPerRow; j++)
                    {
                        entry = new MenuEntry(level.ToString(), locaiton);
                        entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                        entry.IsActive = false;
                        MenuEntries.Add(entry);

                        level++;
                        locaiton.X += locaiton.Width + 10;
                    }
                    locaiton.Y += locaiton.Height + 10;
                    locaiton.X = buttonStartX;
                }
            }
        }

        void entry_Selected(object sender, EventArgs e)
        {
            // teleport to the selected level
            if (selectedEntry == 0)
            {
                if (dungeonStartLevel > 1)
                {
                    dungeonStartLevel -= buttonNumPerRow * buttonRows;
                }
            }
            else if (selectedEntry == 1)
            {
                if (dungeonStartLevel + buttonNumPerRow * buttonRows <= player.DungeonLevelsGenerated)
                {
                    dungeonStartLevel += buttonNumPerRow * buttonRows;
                }
            }
            else
            {
                // teleport
                int level = dungeonStartLevel + selectedEntry - 2;

                if (level <= player.DungeonLevelsGenerated)
                {
                    DungeonLevel newDungeon = new DungeonLevel();
                    player.DungeonLevel = level;
                    SaveGameManager.LoadDungeon(ref newDungeon);
                    newDungeon.GoingDown = true;
                    player.Position = Vector2.Zero;
                    DungeonGame.ScreenManager.AddScreen(new InitialLoadScreen(newDungeon));
                    DungeonGame.ScreenManager.RemoveScreen(GameplayScreen.Instance);
                    DungeonGame.ScreenManager.RemoveScreen(this);
                    player.StopMoving();
                }
            }
        }


        public void SetButtonActiveStatus()
        {
            for (int i = 2; i < MenuEntries.Count; i++)
            {
                if (dungeonStartLevel + i - 2 <= player.DungeonLevelsGenerated)
                {
                    MenuEntries[i].IsActive = true;
                }
                else
                {
                    MenuEntries[i].IsActive = false;
                }

                MenuEntries[i].Text = (dungeonStartLevel + i - 2).ToString();
            }
        }


        public override void OnRemoval()
        {
            base.OnRemoval();

            GameplayScreen.Instance.UnPause();
        }



        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // draw boarder
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            DrawBorder(new Rectangle((int)WindowCorner.X - boarderWidth, (int)WindowCorner.Y - boarderWidth, dimension.Width + boarderWidth * 2, dimension.Height + boarderWidth * 2),
                InternalContentManager.GetTexture("Blank"), Color.Gray, boarderWidth, Color.Black, spriteBatch);

            // draw title
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Teleport", titleVector, Color.Red);

            helpButton.Draw(spriteBatch);

            spriteBatch.End();
            base.Draw(gameTime);
        }

      


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            SetButtonActiveStatus();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            helpButton.HandleInput();
        }

    }
}
