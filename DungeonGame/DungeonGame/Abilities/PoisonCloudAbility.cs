using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class PoisonCloudAbility : Ability
    {
        int[] poisonDuration = { 5, 8, 12 };
        int[] damage = { 2, 5, 9 };
        Timer cloudTimer = new Timer(5000, TimerState.Stopped, TimerType.Manual);
        Vector2 cloudCenter;
        float cloudRange = 150;

        public PoisonCloudAbility(PlayerSprite player)
            : base("Poison Cloud", player, 16)
        {
            manaCost[0] = 30;
            manaCost[1] = 45;
            manaCost[2] = 60;
            elapsed = duration = 10000;
            isMagic = true;
            abilityType = AbilityType.PoisonCloud;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 8;
            abilityPointCost[2] = 20;
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
                elapsed = 0;
                player.UseMana(manaCost[level - 1]);

                AudioManager.audioManager.PlaySFX("Poison" + Util.Random.Next(1, 4).ToString());

                cloudCenter = direction;
                cloudTimer.ResetTimerAndRun();

                return true;
            }
            return false;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            cloudTimer.Update(gameTime);

            if (cloudTimer.State == TimerState.Running)
            {
                ParticleSystem.AddParticles(cloudCenter, ParticleType.ExplosionWhite, color: Color.DarkGreen, sizeScale: 3f, numParticlesScale: .05f);

                // find enemies within range
                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(cloudCenter, cloudRange))
                {
                    enemy.Poison(damage[level - 1], poisonDuration[level - 1] * 1000, player);
                }
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
                int offset = 0;
                if (level != 0)
                    offset = level - 1;
                return "Create a cloud that poisons enemies dealing " + damage[offset].ToString() + " poison damaged every second for " + poisonDuration[offset].ToString() + " seconds";
            }
        }

        public override string NextDescription
        {
            get
            {
                if (level == MaxLevel)
                    return String.Empty;

                return "Deals" + damage[level].ToString() + " poison damage every second for " + poisonDuration[level].ToString() + " seconds";
            }
        }



    }
}
