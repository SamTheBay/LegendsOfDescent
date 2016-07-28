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
    class CreditsScreen : MenuScreen
    {
        public CreditsScreen()
            : base()
        {
            if (DungeonGame.pcMode)
            {
                ActivateBackButton(new Vector2(20, 140));
            }
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            int currentx = DungeonGame.ScreenSize.Y + 60;
            int groupSpace = 60;
            int nameSpace = 40;

            if (!DungeonGame.pcMode)
            {
                currentx += ((DungeonGame.ScreenSize.Height - 720) / 2);

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Developers", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Sam Bayless", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Ian Obermiller", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Artists", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Luke Brubaker", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Kurt Taylor", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Agata Wiejak", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Reiner \"Tiles\" Prokein", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Sound Effects", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Matt Webster", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Music", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Dustin Howie", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Ben Roland", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;
            }
            else
            {
                currentx += (DungeonGame.ScreenSize.Height - 600) / 2 + DungeonGame.ScreenSize.Y;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Developers", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Sam Bayless", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Ian Obermiller", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Artists", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Luke Brubaker", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Kurt Taylor", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Agata Wiejak", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Reiner \"Tiles\" Prokein", new Vector2(DungeonGame.ScreenSize.Width / 2, currentx), Color.White);
                currentx += groupSpace;

                int splitHeight = currentx;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Sound FX", new Vector2(DungeonGame.ScreenSize.Width / 4, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Matt Webster", new Vector2(DungeonGame.ScreenSize.Width / 4, currentx), Color.White);
                currentx += groupSpace;

                currentx = splitHeight;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Music", new Vector2(DungeonGame.ScreenSize.Width / 4 * 3, currentx), Color.Red);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Dustin Howie", new Vector2(DungeonGame.ScreenSize.Width / 4 * 3, currentx), Color.White);
                currentx += nameSpace;
                Fonts.DrawCenteredText(spriteBatch, Fonts.ButtonFont, "Ben Roland", new Vector2(DungeonGame.ScreenSize.Width / 4 * 3, currentx), Color.White);
                currentx += groupSpace;
            }

            spriteBatch.End();
        }



    }
}