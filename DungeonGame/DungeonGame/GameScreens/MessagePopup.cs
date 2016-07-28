using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{
    class MessagePopup : PopUpScreen
    {
        static SpriteFont font = Fonts.DescriptionFont;
        const int width = 560;
        int height = 0;
        string message;

        public void Show(string message)
        {
            this.message = message.Wrap(font, width);
            height = (int)font.MeasureString(this.message).Y;
            GameplayScreen.Instance.Pause();
            GameplayScreen.Instance.ScreenManager.AddScreen(this);
        }

        public override void HandleInput()
        {
            base.HandleInput();
            if (InputManager.GetTouchCollection().Any(t => t.State == TouchLocationState.Pressed))
            {
                ExitScreen();
                GameplayScreen.Instance.UnPause();
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            int x = 90;
            int y = 240;
            int padding = 20;
            int borderWidth = 12;
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            DrawBorder(
                new Rectangle(
                    x - borderWidth, 
                    y - borderWidth, 
                    width + borderWidth * 2 + padding * 2, 
                    height + borderWidth * 2 + padding * 2),
                InternalContentManager.GetTexture("metalTex"), borderWidth, Color.White, spriteBatch);

            spriteBatch.Draw(
                InternalContentManager.GetTexture("Blank"),
                new Rectangle(
                    x,
                    y,
                    width + padding * 2,
                    height + padding * 2),
                Color.White);

            spriteBatch.DrawString(
                font,
                message,
                new Vector2(x + padding, y + padding),
                Color.Black);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
