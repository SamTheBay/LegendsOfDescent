using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;



namespace LegendsOfDescent
{
    class SprintAbility : Ability
    {

        float[] increase = {30, 35, 40, 45, 50, 55};
        int[] effectDuration = {15, 20, 25, 30, 35, 40};
        Timer effectTimer = new Timer(10, TimerState.Stopped, TimerType.Manual);

        public SprintAbility(PlayerSprite player)
            : base("Sprint", player, 19)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            duration = 40000;
            isMagic = true;
            abilityType = AbilityType.Sprint;

            abilityPointCost[0] = 2;
            abilityPointCost[1] = 6;
            abilityPointCost[2] = 20;
            abilityPointCost[3] = 50;
            abilityPointCost[4] = 150;
            abilityPointCost[5] = 500;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);

                AudioManager.audioManager.PlaySFX("SpeedUp" + Util.Random.Next(1, 4).ToString());
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.Starburst, color: Color.Green, sizeScale: 2.0f, numParticlesScale: 2.0f);
                effectTimer.ResetTimerAndRun(effectDuration[level - 1] * 1000);

                // reset timer
                elapsed = 0;
                return true;
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            effectTimer.Update(gameTime);
        }

        public override void ResetOnDeath()
        {
            base.ResetOnDeath();

            effectTimer.State = TimerState.Stopped;
        }

        public override int GetActiveIconTime()
        {
            return effectTimer.RemainingDuration;
        }


        public float GetSpeedAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
                return increase[level - 1];
            }
            return 0;
        }


        public override string Description
        {
            get
            {
                int currentOffset = 0;
                if (level != 0)
                    currentOffset = level - 1;

                return "Increase movement speed by " + increase[currentOffset].ToString() +"% for " + effectDuration[currentOffset].ToString() + " seconds.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increase movement speed to " + increase[level].ToString() +"% and duration to " + effectDuration[level].ToString() + " seconds.";
            }
        }

        public override void Select()
        {
            // use nova
            Activate(Vector2.Zero);

            base.Select();
        }
    }

}
