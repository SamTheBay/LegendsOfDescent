using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class ExplosionTrapAbility : Ability
    {
        ExplosionTrap[] traps = new ExplosionTrap[10];
        int[] damage = new int[MaxLevel];

        public ExplosionTrapAbility(PlayerSprite player)
            : base("Explosion Trap", player, 28)
        {
            manaCost[0] = 15;
            manaCost[1] = 20;
            manaCost[2] = 25;
            manaCost[3] = 30;
            manaCost[4] = 35;
            manaCost[5] = 40;
            duration = 10000;
            isMagic = true;
            for (int i = 0; i < traps.Length; i++)
            {
                traps[i] = new ExplosionTrap(player);
            }
            abilityType = AbilityType.ExplosionTrap;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 100;
            abilityPointCost[4] = 290;
            abilityPointCost[5] = 850;


            damage[0] = BalanceManager.GetBaseDamage(10);
            damage[1] = BalanceManager.GetBaseDamage(16);
            damage[2] = BalanceManager.GetBaseDamage(24);
            damage[3] = BalanceManager.GetBaseDamage(35);
            damage[4] = BalanceManager.GetBaseDamage(60);
            damage[5] = BalanceManager.GetBaseDamage(80);
        }


        public override WeaponSprite[] GetCollisionableSet()
        {
            return traps;
        }

        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                // reset timer
                base.Activate(direction);
                activateDelayElapsed = 0;
                elapsed = 0;

                AudioManager.audioManager.PlaySFX("TrapSet" + Util.Random.Next(1, 6).ToString());
                return true;
            }
            return false;
        }


        public override void ActivateFinish(Vector2 direction)
        {
            base.ActivateFinish(direction);

            // shoot
            for (int i = 0; i < traps.Length; i++)
            {
                if (!traps[i].IsActive)
                {
                    player.UseMana(manaCost[level - 1]);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    traps[i].Radius = level + 1;
                    traps[i].SetDamage(Damage, DamageType.Fire);
                    traps[i].Fire(angle, new Vector2(player.CenteredPosition.X - traps[i].FrameDimensions.X / 2, player.CenteredPosition.Y - traps[i].FrameDimensions.Y / 2));
                    break;
                }
            }
        }


        int Damage
        {
            get
            {
                return (int)(damage[level - 1] + player.SpellDamage);
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < traps.Length; i++)
            {
                traps[i].Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < traps.Length; i++)
            {
                traps[i].Draw(spriteBatch, 3);
            }
        }

        public override void Select()
        {
            Activate(Vector2.Zero);
            base.Select();
        }

        public override string Description
        {
            get
            {
                int currentDamage = 0;
                if (level == 0)
                    currentDamage = damage[0];
                else
                    currentDamage = damage[level - 1];

                return "Place a trap that explodes when an enemy touches it dealing " + currentDamage.ToString() + " damage to all enemies in the explosion radius. The further from the explosion an enemy is, the less damage they take.";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase explosion radius and deal " + damage[level].ToString() + " damage";
            }
        }


    }
}
