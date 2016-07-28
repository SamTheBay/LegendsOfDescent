using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class StoneSkinAbility : Ability
    {
        float[] increase = { 40, 50, 60, 70, 80, 90 };
        int[] effectDuration = { 15, 20, 25, 30, 35, 40 };
        Timer effectTimer = new Timer(10, TimerState.Stopped, TimerType.Manual);

        public StoneSkinAbility(PlayerSprite player)
            : base("Stone Skin", player, 21)
        {
            manaCost[0] = 30;
            manaCost[1] = 40;
            manaCost[2] = 50;
            manaCost[3] = 60;
            manaCost[4] = 70;
            manaCost[5] = 80;
            duration = 40000;
            speedAdjustable = false;
            abilityType = AbilityType.StoneSkin;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 12;
            abilityPointCost[2] = 35;
            abilityPointCost[3] = 90;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.Mana >= manaCost[level - 1])
            {
                base.Activate(direction);
                player.UseMana(manaCost[level - 1]);

                AudioManager.audioManager.PlaySFX("Stone" + Util.Random.Next(1, 5).ToString());
                ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray, sizeScale: 1.0f, numParticlesScale: 2.0f);
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


        public float GetArmorAdjust()
        {
            if (level > 0 && effectTimer.State == TimerState.Running)
            {
                return (increase[level - 1] + 100f) / 100f;
            }
            return 1f;
        }


        public bool IsActive()
        {
            return effectTimer.State == TimerState.Running;
        }


        public override string Description
        {
            get
            {
                int currentOffset = 0;
                if (level != 0)
                    currentOffset = level - 1;

                return "Turn your skin as hard as stone to deflect damage. Increases armor by " + increase[currentOffset].ToString() + "% for " + effectDuration[currentOffset].ToString() + " seconds.";
            }
        }


        public override string NextDescription
        {
            get
            {
                return "Increase armor to " + increase[level].ToString() + "% and duration to " + effectDuration[level].ToString() + " seconds.";
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
