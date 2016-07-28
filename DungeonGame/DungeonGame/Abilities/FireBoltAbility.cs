using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class FireBoltAbility : Ability
    {
        Firebolt[] fireballs = new Firebolt[15];
        int[] damage = new int[MaxLevel];

        public FireBoltAbility(PlayerSprite player)
            : base("FireBolt", player, 3)
        {
            manaCost[0] = 5;
            manaCost[1] = 8;
            manaCost[2] = 11;
            manaCost[3] = 15;
            manaCost[4] = 20;
            manaCost[5] = 25;
            duration = 600;
            isMagic = true;
            for (int i = 0; i < fireballs.Length; i++)
            {
                fireballs[i] = new Firebolt(player);
            }
            abilityType = AbilityType.FireBolt;

            damage[0] = BalanceManager.GetBaseDamage(2);
            damage[1] = BalanceManager.GetBaseDamage(4);
            damage[2] = BalanceManager.GetBaseDamage(7);
            damage[3] = BalanceManager.GetBaseDamage(20);
            damage[4] = BalanceManager.GetBaseDamage(40);
            damage[5] = BalanceManager.GetBaseDamage(60);
        }


        public override WeaponSprite[] GetCollisionableSet()
        {
            return fireballs;
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
                AudioManager.audioManager.PlaySFX("Fire" + Util.Random.Next(1, 4).ToString());
                return true;
            }
            return false;
        }


        public override void ActivateFinish(Vector2 direction)
        {
            // shoot
            for (int i = 0; i < fireballs.Length; i++)
            {
                if (!fireballs[i].IsActive)
                {
                    player.UseMana(manaCost[level - 1]);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    fireballs[i].SetDamage(Damage, DamageType.Fire);
                    fireballs[i].Fire(angle, new Vector2(player.CenteredPosition.X - fireballs[i].FrameDimensions.X / 2, player.CenteredPosition.Y - fireballs[i].FrameDimensions.Y / 2));
                    fireballs[i].AutoCrit = autoCrit;
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

            for (int i = 0; i < fireballs.Length; i++)
            {
                fireballs[i].Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < fireballs.Length; i++)
            {
                fireballs[i].Draw(spriteBatch, 3);
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
                return "Shoot a firebolt that deals " + currentDamage + " fire damage to the enemy it hits";
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
