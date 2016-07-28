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
    class GoldItem : ItemSprite
    {
        int amount;

        public GoldItem(int amount)
            : base(amount.ToString() + " Gold", "ItemIcons", 10, 10, 1, 1, "", 0)
        {
            isSelectable = false;
            this.amount = amount;
            this.value = amount;
        }


        public int Gold
        {
            get { return amount; }
        }


        public override bool ActivateItem(PlayerSprite player)
        {
            bool returnVal = base.ActivateItem(player);

            // add text sprite into the gameplay screen
            GameplayScreen.Instance.AddTextSpriteFromPool(amount.ToString(), Position, Color.DarkGoldenrod, Fonts.DescriptionFont, new Vector2(0, -4), 1000);
            AudioManager.audioManager.PlaySFX("Gold" + Util.Random.Next(1, 4).ToString());

            return returnVal;
        }


        public override void PlayDroppedSoundEffect()
        {
            AudioManager.audioManager.PlaySFX("Gold" + Util.Random.Next(1, 4).ToString());
        }

    }
}
