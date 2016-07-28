using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

#if WIN8
using System.Threading.Tasks;
#endif

namespace LegendsOfDescent
{
    class FeedBackScreen : MenuScreen
    {
        String text;
        String[] buttonNames = { "Forums", "Facebook", "Back" };
        int titleHeight;
        int textHeight;

        public FeedBackScreen()
            : base()
        {
            text = "Join the community! Get the latest LoD news and give us feedback on the game.";
            int centeredOffset = (DungeonGame.ScreenSize.Height - 600) / 2 + DungeonGame.ScreenSize.Y;

            int buttonStart = centeredOffset + 300;
            int buttonHeight = 60;
            titleHeight = centeredOffset + 75;
            textHeight = centeredOffset + 200;

            if (DungeonGame.pcMode)
            {
                ActivateBackButton(new Vector2(20, 140));
            }

            int buttonOffset = buttonHeight + 10;
            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStart + buttonOffset, 450, buttonHeight);


            for (int i = 0; i < buttonNames.Length; i++)
            {
                MenuEntry entry = new MenuEntry(buttonNames[i], location);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
                location.Y += buttonOffset;
            }
        }



        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFontLarge, "Alpha", new Vector2(DungeonGame.ScreenSize.Width / 2, titleHeight), Color.Red);
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFontLarge, "Feedback", new Vector2(DungeonGame.ScreenSize.Width / 2, titleHeight + 60), Color.Red);

            SpriteFont font = Fonts.ButtonFont;
            List<String> lines = Fonts.BreakTextIntoList(text, font, 400);
            float lineHeight = font.MeasureString(lines[0]).Y;

            Vector2 position = new Vector2(DungeonGame.ScreenSize.Width / 2, textHeight);

            for (int i = 0; i < lines.Count; i++)
            {
                Fonts.DrawCenteredText(spriteBatch, font, lines[i], position, Color.White);
                position.Y += lineHeight;
            }

            spriteBatch.End();
        }



        void entry_Selected(object sender, EventArgs e)
        {

            if (selectedEntry == 0)
            {
#if WINDOWS_PHONE
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri("http://legendsofdescent.com");
                task.Show();
#else
#if !WIN8
                System.Diagnostics.Process.Start("http://legendsofdescent.com");
#else
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://legendsofdescent.com/board"));
#endif
#endif
            }
            else if (selectedEntry == 1)
            {
#if WINDOWS_PHONE
                WebBrowserTask task = new WebBrowserTask();
                task.Uri = new Uri("http://www.facebook.com/pages/Legends-Of-Descent/110521525719792");
                task.Show();
#else
#if !WIN8
                System.Diagnostics.Process.Start("http://www.facebook.com/pages/Legends-Of-Descent/110521525719792");
#else
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/pages/Legends-Of-Descent/110521525719792"));
#endif
#endif
            }

            else if (selectedEntry == 2)
            {
                ExitScreen();
            }
        }
    }
}
