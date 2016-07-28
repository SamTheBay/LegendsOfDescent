using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{

    class FearAbility : Ability
    {
        float effectRadius = 140;
        int[] fearDuration = { 4, 6, 8, 10, 12, 14 };

        public FearAbility(PlayerSprite player)
            : base("Fear", player, 11)
        {
            manaCost[0] = 25;
            manaCost[1] = 35;
            manaCost[2] = 45;
            manaCost[3] = 55;
            manaCost[4] = 65;
            manaCost[5] = 75;
            duration = 15000;
            isMagic = true;
            abilityType = AbilityType.Fear;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 25;
            abilityPointCost[3] = 80;
            abilityPointCost[4] = 200;
            abilityPointCost[5] = 600;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                
                // flash bang
                GameplayScreen.Instance.Flash(Color.Red);
                AudioManager.audioManager.PlaySFX("Fear" + Util.Random.Next(1, 6).ToString());

                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(player.CenteredPosition, effectRadius))
                {
                    enemy.Fear(fearDuration[level - 1] * 1000, player);
                }

                // reset timer
                elapsed = 0;
                return true;
            }
            return false;
        }



        public override string Description
        {
            get
            {
                int dur = 0;
                if (level == 0)
                    dur = fearDuration[0];
                else
                    dur = fearDuration[level - 1];

                return "Make enemies near you run away in fear for " + dur.ToString() + " seconds.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increase fear duration to " + fearDuration[level].ToString() + " seconds";
            }
        }

        public override void Select()
        {
            // use nova
            Activate(Vector2.Zero);
        }




    }
}
