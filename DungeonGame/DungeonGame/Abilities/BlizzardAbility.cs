using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class BlizzardAbility : Ability
    {
        IceBolt[] icebolts = new IceBolt[50];
        int[] damage = new int[MaxLevel];
        int attackDuration = 1000;

        public BlizzardAbility(PlayerSprite player)
            : base("Blizzard", player, 24)
        {
            manaCost[0] = 17;
            manaCost[1] = 21;
            manaCost[2] = 25;
            manaCost[3] = 30;
            manaCost[4] = 35;
            manaCost[5] = 40;
            duration = 1700;
            isMagic = true;
            for (int i = 0; i < icebolts.Length; i++)
            {
                icebolts[i] = new IceBolt(player);
            }
            abilityType = AbilityType.Blizzard;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 45;
            abilityPointCost[3] = 120;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;

            requiredAbilityType = AbilityType.IceBolt;
            requiredLevel = 2;

            damage[0] = BalanceManager.GetBaseDamage(3);
            damage[1] = BalanceManager.GetBaseDamage(6);
            damage[2] = BalanceManager.GetBaseDamage(9);
            damage[3] = BalanceManager.GetBaseDamage(16);
            damage[4] = BalanceManager.GetBaseDamage(32);
            damage[5] = BalanceManager.GetBaseDamage(50);
        }


        public override WeaponSprite[] GetCollisionableSet()
        {
            return icebolts;
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
            int boltNum = level + 1;
            player.UseMana(manaCost[level - 1]);

            for (int j = 0; j < boltNum; j++)
            {
                float angleAdjust = Util.GetAngleAdjust(boltNum, j);

                for (int i = 0; i < icebolts.Length; i++)
                {
                    if (!icebolts[i].IsActive)
                    {
                        float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                        angle += angleAdjust;
                        icebolts[i].SetDamage(Damage, DamageType.Ice);
                        icebolts[i].Fire(angle, new Vector2(player.CenteredPosition.X - icebolts[i].FrameDimensions.X / 2, player.CenteredPosition.Y - icebolts[i].FrameDimensions.Y / 2));
                        icebolts[i].AutoCrit = autoCrit;
                        break;
                    }
                }
            }

            base.ActivateFinish(direction);
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

            for (int i = 0; i < icebolts.Length; i++)
            {
                icebolts[i].Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < icebolts.Length; i++)
            {
                icebolts[i].Draw(spriteBatch, 3);
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
                int bolts = 2;
                if (level == 0)
                    currentDamage = damage[0];
                else
                {
                    currentDamage = damage[level - 1];
                    bolts = level + 1;
                }

                return "Shoot " + bolts.ToString() + " ice bolts, freezing the enemy and dealing " + currentDamage.ToString() + " damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase length of frozen effect, the number of bolts by 1 and increase damage to " + damage[level].ToString();
            }
        }

    }
}
