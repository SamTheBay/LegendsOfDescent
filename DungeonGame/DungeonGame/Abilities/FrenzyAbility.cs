using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class FrenzyAbility : Ability
    {
        
        float[] increaseAttack = {30, 35, 40, 45, 50, 55};
        float[] increaseMoveSpeed = { 30, 35, 40, 45, 50, 55 };
        float[] increaseAttackSpeed = { 30, 35, 40, 45, 50, 55 };
        float increaseDamageTaken = 50;
        int[] effectDuration = {6, 8, 10, 12, 14, 16};
        Timer effectTimer = new Timer(10, TimerState.Stopped, TimerType.Manual);

        public FrenzyAbility(PlayerSprite player)
            : base("Frenzy", player, 20)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            elapsed = duration = 20000;
            speedAdjustable = false;
            abilityType = AbilityType.Frenzy;

            abilityPointCost[0] = 2;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 100;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);
                
                AudioManager.audioManager.PlaySFX("Shout" + Util.Random.Next(1, 4).ToString());

                effectTimer.ResetTimerAndRun(effectDuration[level - 1] * 1000);

                // reset timer
                elapsed = 0;
                return true;
            }
            return false;
        }


        public float GetDamageAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
                return (increaseAttack[level - 1] + 100f) / 100f;
            }
            return 1f;
        }

        public float GetMoveSpeedAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
                return increaseMoveSpeed[level - 1];
            }
            return 0f;
        }

        public float GetAttackSpeedAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
               return increaseAttackSpeed[level - 1];
            }
            return 0f;
        }


        public float GetDamageTakenAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
                return (increaseDamageTaken + 100f) / 100f;
            }
            return 1f;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            effectTimer.Update(gameTime);

            if (effectTimer.State == TimerState.Running)
            {
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Red, sizeScale: 1f, numParticlesScale: .1f);
            }
        }


        public override void ResetOnDeath()
        {
            base.ResetOnDeath();

            effectTimer.State = TimerState.Stopped;
        }


        public override string Description
        {
            get
            {
                int currentOffset = 0;
                if (level != 0)
                    currentOffset = level - 1;

                return "You go into a frenzy of aggression making you a force to be reckoned with. Increase melee damage by " + increaseAttack[currentOffset].ToString() + 
                    "%, attack speed by " + increaseAttackSpeed[currentOffset].ToString() + 
                    "%, and movement speed by " + increaseMoveSpeed[currentOffset].ToString() +
                    "% for " + effectDuration[currentOffset].ToString() + " seconds. While frenzy is active you will take " + increaseDamageTaken.ToString() + " more damage from enemies";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increase melee damage by " + increaseAttack[level].ToString() +
                    "%, attack speed by " + increaseAttackSpeed[level].ToString() +
                    "%, and movement speed by " + increaseMoveSpeed[level].ToString() +
                    "% for " + effectDuration[level].ToString() + " seconds.";
            }
        }


        public override int GetActiveIconTime()
        {
            return effectTimer.RemainingDuration;
        }

        public override void Select()
        {
            // use nova
            Activate(Vector2.Zero);

            base.Select();
        }
    }
}
