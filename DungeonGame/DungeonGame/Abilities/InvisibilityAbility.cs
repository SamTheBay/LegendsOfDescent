using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class InvisibilityAbility : Ability
    {
        int[] invisiDuration = { 10, 15, 20, 25, 30, 35 };

        public InvisibilityAbility(PlayerSprite player)
            : base("Invisibility", player, 17)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            elapsed = duration = 10000;
            isMagic = true;
            abilityType = AbilityType.Invisibility;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 8;
            abilityPointCost[2] = 20;
            abilityPointCost[3] = 60;
            abilityPointCost[4] = 150;
            abilityPointCost[5] = 700;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                player.BecomeInvisible(invisiDuration[level - 1] * 1000);

                AudioManager.audioManager.PlaySFX("Invis" + Util.Random.Next(1, 6).ToString());

                // reset timer
                elapsed = 0;
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray, numParticlesScale: 2.0f);
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
                    dur = invisiDuration[0];
                else
                    dur = invisiDuration[level - 1];

                return "Turn invisible for " + dur.ToString() + " seconds or until your next attack. All attacks are automatically critical hits while invisible.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increased invisible time to " + invisiDuration[level].ToString() + " seconds";
            }
        }

        public override void Select()
        {
            // use heal
            Activate(Vector2.Zero);

            base.Select();
        }

        public override int GetActiveIconTime()
        {
            return player.InvisibleRemaining;
        }

    }
}
