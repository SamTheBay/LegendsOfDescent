using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class HelpButton
    {
        HelpScreens helpScreenIndex;
        Vector2 buttonLocation;
        Texture2D buttonTexture;
        Texture2D buttonPressedTexture;
        Rectangle buttonTapLocation;

        public HelpButton(HelpScreens helpScreenIndex, Vector2 buttonLocation)
        {
            this.helpScreenIndex = helpScreenIndex;
            this.buttonLocation = buttonLocation;
            buttonTexture = InternalContentManager.GetTexture("Help");
            buttonPressedTexture = InternalContentManager.GetTexture("HelpSelect");
            buttonTapLocation = new Rectangle((int)buttonLocation.X, (int)buttonLocation.Y, buttonTexture.Width, buttonTexture.Height);
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            if (!InputManager.IsLocationPressed(buttonTapLocation))
            {
                spriteBatch.Draw(buttonTexture, buttonLocation, null, Color.White);
            }
            else
            {
                spriteBatch.Draw(buttonPressedTexture, buttonLocation, null, Color.White);
            }
        }

        public void HandleInput()
        {
            if (InputManager.IsLocationTapped(buttonTapLocation))
            {
                HelpScreenManager.Instance.ShowHelpScreen(helpScreenIndex);
            }
        }

        public Vector2 Location
        {
            set 
            { 
                buttonLocation = value;
                buttonTapLocation.X = (int)buttonLocation.X;
                buttonTapLocation.Y = (int)buttonLocation.Y;
            }
        }
    }
}
