using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    public class HealAbility : Ability
    {
        float[] healAmount = { .5f, .6f, .7f, .8f, .9f, 1f };

        public HealAbility(PlayerSprite player)
            : base("Heal", player, 14)
        {
            manaCost[0] = 40;
            manaCost[1] = 70;
            manaCost[2] = 100;
            manaCost[3] = 130;
            manaCost[4] = 160;
            manaCost[5] = 200;
            elapsed = duration = 30000;
            isMagic = true;
            abilityType = AbilityType.Heal;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                player.Heal(healAmount[level -1] * player.MaxHealth, true);
                AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());

                // reset timer
                elapsed = 0;
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.Starburst, color: Color.Red, sizeScale:2.0f, numParticlesScale:2.0f);
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
                float heal = 0;
                if (level == 0)
                    heal = healAmount[0];
                else
                    heal = healAmount[level - 1];

                return "Cast a spell to instantly rejuvenate " + (heal * 100).ToString("F0") + "% of your health";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increased health gain to " + (healAmount[level] * 100).ToString("F0") + "%";
            }
        }

        public override void Select()
        {
            // use heal
            Activate(Vector2.Zero);
        }

    }
}
