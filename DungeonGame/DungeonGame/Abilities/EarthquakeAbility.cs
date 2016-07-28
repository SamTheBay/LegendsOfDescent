using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class EarthquakeAbility : Ability
    {
        int[] stunDuration = { 3, 4, 5, 6, 7, 8 };
        int[] damage = { 2, 5, 9 };
        float range = 120;

        public EarthquakeAbility(PlayerSprite player)
            : base("Earthquake", player, 13)
        {
            manaCost[0] = 20;
            manaCost[1] = 25;
            manaCost[2] = 30;
            manaCost[3] = 35;
            manaCost[4] = 40;
            manaCost[5] = 45;
            elapsed = duration = 10000;
            speedAdjustable = false;
            abilityType = AbilityType.Earthquake;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 8;
            abilityPointCost[2] = 20;
            abilityPointCost[3] = 90;
            abilityPointCost[4] = 230;
            abilityPointCost[5] = 700;
        }



        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                // reset timer
                base.Activate(direction);
                elapsed = 0;
                player.UseMana(manaCost[level - 1]);

                AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
                ParticleSystem.AddParticles(direction, ParticleType.ExplosionWhite, sizeScale: 1, lifetimeScale: 1, numParticlesScale: 2, color: Color.Gray);

                // stun all enemies nearby
                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(direction, range))
                {
                    enemy.Stun(stunDuration[level - 1]  * 1000, player);
                }

                return true;
            }
            return false;
        }


        public override void Select()
        {
            // set the players active ability to this one
            Activate(SaveGameManager.CurrentPlayer.CenteredPosition);
        }

        public override string Description
        {
            get
            {
                int offset = 0;
                if (level != 0)
                    offset = level - 1;
                return "Smash the ground creating an earthquake that stuns the enemies around you for " + stunDuration[offset].ToString() + " seconds.";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase stun duration to " + stunDuration[level].ToString() + " seconds";
            }
        }
    }
}
