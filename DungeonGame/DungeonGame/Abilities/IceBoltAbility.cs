using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class IceBoltAbility : Ability
    {
        IceBolt[] icebolts = new IceBolt[15];
        int[] damage = new int[MaxLevel];


        public IceBoltAbility(PlayerSprite player)
            : base("IceBolt", player, 4)
        {
            manaCost[0] = 7;
            manaCost[1] = 10;
            manaCost[2] = 14;
            manaCost[3] = 18;
            manaCost[4] = 22;
            manaCost[5] = 28;
            duration = 800;
            isMagic = true;
            for (int i = 0; i < icebolts.Length; i++)
            {
                icebolts[i] = new IceBolt(player);
            }
            abilityType = AbilityType.IceBolt;

            damage[0] = BalanceManager.GetBaseDamage(1);
            damage[1] = BalanceManager.GetBaseDamage(3);
            damage[2] = BalanceManager.GetBaseDamage(6);
            damage[3] = BalanceManager.GetBaseDamage(18);
            damage[4] = BalanceManager.GetBaseDamage(35);
            damage[5] = BalanceManager.GetBaseDamage(55);
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
            for (int i = 0; i < icebolts.Length; i++)
            {
                if (!icebolts[i].IsActive)
                {
                    player.UseMana(manaCost[level - 1]);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    icebolts[i].SetDamage(Damage, DamageType.Ice);
                    icebolts[i].Fire(angle, new Vector2(player.CenteredPosition.X - icebolts[i].FrameDimensions.X / 2, player.CenteredPosition.Y - icebolts[i].FrameDimensions.Y / 2));
                    icebolts[i].AutoCrit = autoCrit;
                    break;
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
                if (level == 0)
                    currentDamage = damage[0];
                else
                    currentDamage = damage[level - 1];

                return "Shoot an ice bolt, freezing the enemy and dealing " + currentDamage.ToString() + " damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase length of frozen effect and increase damage to " + damage[level].ToString();
            }
        }
    }
}
