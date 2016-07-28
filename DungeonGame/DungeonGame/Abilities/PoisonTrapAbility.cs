using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class PoisonTrapAbility : Ability
    {
        PoisonTrap[] traps = new PoisonTrap[10];
        int[] damage = new int[MaxLevel];
        int[] poisonDuration = { 5, 7, 9, 11, 13, 15 };

        public PoisonTrapAbility(PlayerSprite player)
            : base("Poison Trap", player, 27)
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
                traps[i] = new PoisonTrap(player);
            }
            abilityType = AbilityType.PoisonTrap;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 40;
            abilityPointCost[3] = 110;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 950;

            damage[0] = BalanceManager.GetBaseDamage(4);
            damage[1] = BalanceManager.GetBaseDamage(7);
            damage[2] = BalanceManager.GetBaseDamage(10);
            damage[3] = BalanceManager.GetBaseDamage(15);
            damage[4] = BalanceManager.GetBaseDamage(20);
            damage[5] = BalanceManager.GetBaseDamage(30);
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
                    traps[i].SetDamage(Damage, DamageType.Poison);
                    traps[i].PoisonDuration = poisonDuration[level - 1];
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
                int offset = 0;
                if (level != 0)
                    offset = level - 1;
                return "Place a trap that creates a cloud that poisons enemies dealing " + damage[offset].ToString() + " poison damaged every second for " + poisonDuration[offset].ToString() + " seconds";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Deals " + damage[level].ToString() + " poison damage every second for " + poisonDuration[level].ToString() + " seconds";
            }
        }

    }
}
