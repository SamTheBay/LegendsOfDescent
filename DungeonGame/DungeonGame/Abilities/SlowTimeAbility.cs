using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{


    class SlowTimeAbility : Ability
    {
        int[] slowDuration = { 5, 7, 9, 11, 13, 15 };

        public SlowTimeAbility(PlayerSprite player)
            : base("Slow Time", player, 12)
        {
            manaCost[0] = 30;
            manaCost[1] = 45;
            manaCost[2] = 60;
            manaCost[3] = 75;
            manaCost[4] = 90;
            manaCost[5] = 105;
            elapsed = duration = 20000;
            isMagic = true;
            abilityType = AbilityType.SlowTime;

            abilityPointCost[0] = 4;
            abilityPointCost[1] = 12;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 95;
            abilityPointCost[4] = 290;
            abilityPointCost[5] = 850;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                GameplayScreen.Instance.SlowTime(slowDuration[level - 1] * 1000);

                AudioManager.audioManager.PlaySFX("Slow" + Util.Random.Next(1, 4).ToString());

                // reset timer
                elapsed = 0;
                return true;
            }
            return false;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override string Description
        {
            get
            {
                int dur = 0;
                if (level == 0)
                    dur = slowDuration[0];
                else
                    dur = slowDuration[level - 1];

                return "Slow down time for " + dur.ToString() + " seconds.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increase duration of slow time to " + slowDuration[level].ToString() + " seconds";
            }
        }

        public override void Select()
        {
            Activate(Vector2.Zero);
            base.Select();
        }


        public override int GetActiveIconTime()
        {
            return GameplayScreen.Instance.SlowTimeRemaining;
        }

    }
}
