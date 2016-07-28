using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    class HelpScreen : DescriptionScreen
    {

        public HelpScreen(string title, string helpTextureName, string helpScreenText)
            : base()
        {
            // note that we pause the game in the help screen manager before showing this screen

            AddLine(title, Fonts.HeaderFont, Color.White);
            AddSpace(10f);
            AddLine(helpScreenText, Fonts.DescriptionFont, Color.White);
            if (!String.IsNullOrEmpty(helpTextureName))
            {
                AddSpace(10f);
                Texture2D texture = DungeonGame.Instance.Content.Load<Texture2D>("Textures/Help/" + helpTextureName);
                AddTexture(texture, new Rectangle(0, 0, texture.Width, texture.Height));
            }
            SetFinalize();

            EnabledGestures = GestureType.Tap;
        }

       
        public override void OnRemoval()
        {
            base.OnRemoval();

            GameplayScreen.Instance.UnPause();
        }

    }
}