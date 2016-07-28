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
    public class WarpScreen : PopUpScreen
    {

        static Rectangle dimension = new Rectangle(0, 0, 480 - 12, 400 - 12);

        DungeonLevel dungeon;
        PlayerSprite player;
        HelpButton helpButton;
        Point miniMapCorner;
        int minimapScale = 3;
        Point currentWarpLocation;
        bool canCancel = true;
        bool used = false;


        public WarpScreen(DungeonLevel dungeon, PlayerSprite player, bool canCancel = true)
            : base()
        {
            GameplayScreen.Instance.Pause();
            this.dungeon = dungeon;
            this.player = player;
            WindowCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - dimension.Width / 2, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y - dimension.Height / 2);
            this.canCancel = canCancel;

            if (!DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                MoveWindowCorner(0, 40);
            }


            currentWarpLocation = new Point((int)player.Position.X / dungeon.TileDimension, (int)player.Position.Y / dungeon.TileDimension);

            int buttonStartHeight = (int)WindowCorner.Y + dimension.Height - 60;
            int buttonSpacing = 235;
            Rectangle location = new Rectangle((DungeonGame.ScreenSize.Width - 450) / 2, buttonStartHeight, 215, 50);

            MenuEntry entry = new MenuEntry("Warp", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.X += buttonSpacing;

            entry = new MenuEntry("Cancel", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);

            if (!this.canCancel)
                entry.IsActive = false;

            helpButton = new HelpButton(HelpScreens.Warping, new Vector2(WindowCorner.X + dimension.Width - 60, WindowCorner.Y + 10));

            miniMapCorner = new Point((int)DungeonGame.ScreenSize.Width / 2 - dungeon.Dimension.X * minimapScale / 2, (int)WindowCorner.Y + dimension.Height / 2 - dungeon.Dimension.Y * minimapScale / 2 - 50);
        }


        public override void OnRemoval()
        {
            base.OnRemoval();

            if (used == false && canCancel == true)
                player.AddItem(ItemManager.Instance.GetItem("Warp Scroll", 1));

            GameplayScreen.Instance.UnPause();
        }


        void entry_Selected(object sender, EventArgs e)
        {
            Point newPosition = Point.Zero;
            bool newPositionSet = false;
            if (selectedEntry == 0 && dungeon.GetTile(currentWarpLocation).HasBeenSeen)
            {
                newPosition = currentWarpLocation;
                newPositionSet = true;
                used = true;
                ExitScreen();
            }
            else if (selectedEntry == 1 && canCancel)
            {
                ExitScreen();
            }


            if (newPositionSet)
            {
                player.SetPosition(newPosition.X * dungeon.TileDimension + dungeon.TileDimension / 2 - player.FrameDimensions.X / 2,
                    newPosition.Y * dungeon.TileDimension + dungeon.TileDimension / 2 - player.FrameDimensions.Y / 2);

                player.ValidatePosition();

                player.StopMoving();
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            // draw boarder
            int boarderWidth = 6;
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            DrawBorder(new Rectangle((int)WindowCorner.X - boarderWidth, (int)WindowCorner.Y - boarderWidth, dimension.Width + boarderWidth * 2, dimension.Height + boarderWidth * 2),
                InternalContentManager.GetTexture("Blank"), Color.Red, boarderWidth, Color.Black, spriteBatch);

            helpButton.Draw(spriteBatch);

            spriteBatch.End();

            // draw mini map
            dungeon.DrawMiniMap(miniMapCorner, new Vector2(currentWarpLocation.X * dungeon.TileDimension, currentWarpLocation.Y * dungeon.TileDimension), spriteBatch, 3);

            base.Draw(gameTime);
        }

        public override void HandleInput()
        {
            base.HandleInput();

            helpButton.HandleInput();

            Vector2 press = new Vector2();
            if (InputManager.GetPress(ref press))
            {
                if (new Rectangle(miniMapCorner.X, miniMapCorner.Y, dungeon.Dimension.X * minimapScale, dungeon.Dimension.Y * minimapScale).Contains(press.ToPoint()))
                {
                    currentWarpLocation.X = (int)((press.X - miniMapCorner.X) / minimapScale);
                    currentWarpLocation.Y = (int)((press.Y - miniMapCorner.Y) / minimapScale);
                }
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (dungeon.GetTile(currentWarpLocation).HasBeenSeen)
            {
                MenuEntries[0].IsActive = true;
            }
            else
            {
                MenuEntries[0].IsActive = false;
            }
        }

    }
}
