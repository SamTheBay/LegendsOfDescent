using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class ChainLightningAbility : Ability
    {
        ChainLightning[] bolts = new ChainLightning[15];
        int[] damage = new int[MaxLevel];
        int attackDuration = 1000;

        public ChainLightningAbility(PlayerSprite player)
            : base("Chain Lightning", player, 2)
        {
            manaCost[0] = 20;
            manaCost[1] = 25;
            manaCost[2] = 30;
            manaCost[3] = 35;
            manaCost[4] = 40;
            manaCost[5] = 45;
            requiredLevel = 5;
            duration = 1200;
            isMagic = true;
            for (int i = 0; i < bolts.Length; i++)
            {
                bolts[i] = new ChainLightning(player);
            }
            abilityType = AbilityType.ChainLightning;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 100;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;

            requiredAbilityType = AbilityType.Lightning;
            requiredLevel = 2;

            damage[0] = BalanceManager.GetBaseDamage(4);
            damage[1] = BalanceManager.GetBaseDamage(8);
            damage[2] = BalanceManager.GetBaseDamage(12);
            damage[3] = BalanceManager.GetBaseDamage(24);
            damage[4] = BalanceManager.GetBaseDamage(45);
            damage[5] = BalanceManager.GetBaseDamage(70);
        }

        public override WeaponSprite[] GetCollisionableSet()
        {
            return bolts;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                // adjust speed of attack animation for this spell
                float durAdjust = (480f / ((float)attackDuration / (player.CastSpeedAdjust + 1f))) - 1f;
                player.SetAttackAnimationInterval(durAdjust);

                // reset timer
                base.Activate(direction);
                activateDelayElapsed = 0;
                elapsed = 0;
                AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());
                return true;
            }
            return false;
        }


        public override void ActivateFinish(Vector2 direction)
        {
            base.ActivateFinish(direction);

            // shoot
            for (int i = 0; i < bolts.Length; i++)
            {
                if (!bolts[i].IsActive)
                {
                    player.UseMana(manaCost[level - 1]);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    bolts[i].SetCharacteristics(level + 1, Damage + player.SpellDamage);
                    bolts[i].Fire(angle, new Vector2(player.CenteredPosition.X - bolts[i].FrameDimensions.X / 2, player.CenteredPosition.Y - bolts[i].FrameDimensions.Y / 2));
                    break;
                }
            }

        }

        int Damage
        {
            get
            {
                return damage[level - 1] + player.SpellDamage;
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < bolts.Length; i++)
            {
                bolts[i].Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < bolts.Length; i++)
            {
                bolts[i].Draw(spriteBatch, 3);
            }
        }

        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);
        }

        public override string Description
        {
            get
            {
                int currentDamage = 0;
                int enemies = 0;
                if (level == 0)
                {
                    currentDamage = damage[0];
                    enemies = 2;
                }
                else
                {
                    currentDamage = damage[level - 1];
                    enemies = level + 1;
                }

                return "Shoot a lightning bolt that bounces from enemy to enemy hitting up to " + enemies.ToString() + " enemies for " + currentDamage.ToString() + " damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Hit " + (level + 2).ToString() + " enemies for " + damage[level].ToString() + " damage";

            }
        }
    }
}
