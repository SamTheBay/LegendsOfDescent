using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class FrostArmorAbility : Ability
    {
        int[] damage = { 4, 7, 12, 20, 40, 80 };
        Timer armorTimer = new Timer(12000, TimerState.Stopped, TimerType.Manual);

        public FrostArmorAbility(PlayerSprite player)
            : base("Frost Armor", player, 18)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            elapsed = duration = 30000;
            speedAdjustable = false;
            abilityType = AbilityType.FrostArmor;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 15;
            abilityPointCost[2] = 45;
            abilityPointCost[3] = 120;
            abilityPointCost[4] = 350;
            abilityPointCost[5] = 1000;

            requiredAbilityType = AbilityType.IceBolt;
            requiredLevel = 1;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                // reset timer
                base.Activate(direction);
                elapsed = 0;
                player.UseMana(manaCost[level - 1]);

                // TODO: add frost armor noise
                //AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());

                armorTimer.ResetTimerAndRun();


                return true;
            }
            return false;
        }



        public override void ResetOnDeath()
        {
            base.ResetOnDeath();

            armorTimer.State = TimerState.Stopped;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            armorTimer.Update(gameTime);

            if (armorTimer.State == TimerState.Running)
            {
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.DarkBlue, sizeScale: 1f, numParticlesScale: .1f);
            }
        }

        public override int GetActiveIconTime()
        {
            return armorTimer.RemainingDuration;
        }

        public override void Select()
        {
            // set the players active ability to this one
            Activate(Vector2.Zero);

            base.Select();
        }


        public bool IsFrostArmorActive
        {
            get { return armorTimer.State == TimerState.Running; }
        }


        public float FrostDamage
        {
            get { return damage[level - 1]; }
        }


        public override string Description
        {
            get
            {
                int offset = 0;
                if (level != 0)
                    offset = level - 1;
                return "Surrounds you in frost armor which deals " + damage[offset].ToString() + " ice damage and slows any enemy that hits you with a melee weapon.";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Increase the damage dealt to " + damage[level].ToString() + ".";
            }
        }
    }
}
