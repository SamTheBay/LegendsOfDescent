using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class SnapScreen : GameScreen
    {

        public SnapScreen()
        {
            if (GameplayScreen.Instance != null)
            {
                GameplayScreen.Instance.Pause();
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Texture2D logo = InternalContentManager.GetTexture("Logo");
            spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(Color.Black), DungeonGame.ScreenSize, Color.Black);
            spriteBatch.Draw(logo, new Vector2((DungeonGame.ScreenSize.Width - logo.Width) / 2, (DungeonGame.ScreenSize.Height - logo.Height) / 2), Color.White);

            spriteBatch.End();
        }


        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (DungeonGame.ScreenSize.Width > 400)
            {
                ExitScreen();
            }
        }

        public override void OnRemoval()
        {
            if (GameplayScreen.Instance != null)
            {
                GameplayScreen.Instance.UnPause();
                base.OnRemoval();
            }
        }

    }
}
