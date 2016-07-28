using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class NovaAbility : Ability
    {
        float[] effectRadius = { 120, 160, 200, 240, 280, 320 };
        int[] damage = new int[MaxLevel];
        int stunDuration = 3;

        public NovaAbility(PlayerSprite player)
            : base("Nova", player, 6)
        {
            manaCost[0] = 14;
            manaCost[1] = 18;
            manaCost[2] = 22;
            manaCost[3] = 26;
            manaCost[4] = 30;
            manaCost[5] = 35;
            requiredLevel = 2;
            requiredAbilityType = AbilityType.Lightning;
            duration = 1000;
            isMagic = true;
            abilityType = AbilityType.Nova;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 100;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;

            damage[0] = BalanceManager.GetBaseDamage(5);
            damage[1] = BalanceManager.GetBaseDamage(10);
            damage[2] = BalanceManager.GetBaseDamage(15);
            damage[3] = BalanceManager.GetBaseDamage(25);
            damage[4] = BalanceManager.GetBaseDamage(45);
            damage[5] = BalanceManager.GetBaseDamage(65);
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                
                // flash bang
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Yellow, sizeScale: level);
                AudioManager.audioManager.PlaySFX("Spark");
                AudioManager.audioManager.PlaySFX("Boom");

                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(player.CenteredPosition, effectRadius[level - 1]))
                {
                    enemy.Damage(Damage, DamageType.Lightning, player);
                    enemy.Stun(stunDuration * 1000, player);
                }

                // reset timer
                elapsed = 0;
                return true;
            }
            return false;
        }


        int Damage
        {
            get
            {
                return (int)(damage[level - 1] + player.SpellDamage);
            }
        }


        public override string Description
        {
            get
            {
                int offset = 0;
                if (level != 0)
                    offset = level - 1;

                return "Release an explosion of energy dealing " + damage[offset].ToString() +" damage to all enemies around you and stunning them for " + stunDuration.ToString() + " seconds.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increased the range of the effect and the damage to " + damage[level].ToString();
            }
        }

        public override void Select()
        {
            // use nova
            Activate(Vector2.Zero);
        }
    }
}
