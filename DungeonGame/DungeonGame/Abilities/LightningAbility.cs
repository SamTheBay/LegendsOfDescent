using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class LightningAbility : Ability
    {
        Lightning[] bolts = new Lightning[15];
        int[] damage = new int[MaxLevel];

        public LightningAbility(PlayerSprite player)
            : base("Lightning", player, 5)
        {
            manaCost[0] = 7;
            manaCost[1] = 10;
            manaCost[2] = 14;
            manaCost[3] = 18;
            manaCost[4] = 22;
            manaCost[5] = 28;
            duration = 600;
            isMagic = true;
            for (int i = 0; i < bolts.Length; i++)
            {
                bolts[i] = new Lightning(player);
            }
            abilityType = AbilityType.Lightning;

            damage[0] = BalanceManager.GetBaseDamage(2);
            damage[1] = BalanceManager.GetBaseDamage(4);
            damage[2] = BalanceManager.GetBaseDamage(7);
            damage[3] = BalanceManager.GetBaseDamage(20);
            damage[4] = BalanceManager.GetBaseDamage(40);
            damage[5] = BalanceManager.GetBaseDamage(60);
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
                float durAdjust = (480f / (float)Duration) - 1f;
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
            // shoot
            for (int i = 0; i < bolts.Length; i++)
            {
                if (!bolts[i].IsActive)
                {
                    player.UseMana(manaCost[level - 1]);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    bolts[i].SetDamage(Damage, DamageType.Lightning);
                    bolts[i].Fire(angle, new Vector2(player.CenteredPosition.X - bolts[i].FrameDimensions.X / 2, player.CenteredPosition.Y - bolts[i].FrameDimensions.Y / 2));
                    bolts[i].AutoCrit = autoCrit;
                    break;
                }
            }

            base.ActivateFinish(direction);
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
                if (level == 0)
                    currentDamage = damage[0];
                else
                    currentDamage = damage[level - 1];
                return "Shoot a super fast lightning bolt that deals " + currentDamage + " lightning damage to the enemy it hits";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increases the damage dealt to " + damage[level];
            }
        }
    }
}
