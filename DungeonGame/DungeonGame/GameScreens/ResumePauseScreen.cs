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
    class ResumePauseScreen : PopUpScreen
    {

        public ResumePauseScreen()
            : base()
        {
            GameplayScreen.Instance.Pause();
        }


        public override void ExitScreen()
        {
            base.ExitScreen();

            GameplayScreen.Instance.UnPause();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if ((InputManager.IsBackTriggered() || InputManager.IsLocationTapped(DungeonGame.ScreenSize)))
            {
                ExitScreen();
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw text
            Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Tap to Resume", new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y), Color.Red);

            spriteBatch.End();
        }
    }
}
