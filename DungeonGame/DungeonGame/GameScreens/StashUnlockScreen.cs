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
    class StashUnlockScreen : DescriptionScreen
    {
        PlayerSprite player;

        public StashUnlockScreen(PlayerSprite player)
            : base()
        {
            this.player = player;
            exitOnTouch = false;

#if WIN8
            AddLine("Would you like to unlock stash number " + (player.StashesUnlocked + 1).ToString() + " for all of your characters and any new characters you make?", Fonts.HeaderFont, Color.White);
#else
            AddLine("Would you like to unlock stash number " + (player.StashesUnlocked + 1).ToString() + " for " + player.NextStashCost.ToString() + " gold?", Fonts.HeaderFont, Color.White);
#endif
            AddSpace(20f);
            AddButton("Purchase");
            AddSpace(20f);
            AddButton("No");

            SetFinalize();
        }

       
        public override void OnRemoval()
        {
            base.OnRemoval();
            if (ButtonPressed(0))
            {
#if WIN8
                string productId = "Stash Slot " + (player.StashesUnlocked + 1).ToString();
                DungeonGame.productsManager.PurchaseLicense(productId);
#else
                player.UnlockStash();
#endif
            }
        }
    }
}
