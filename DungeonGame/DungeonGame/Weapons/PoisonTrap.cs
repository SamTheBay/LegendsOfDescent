using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class PoisonTrap : WeaponSprite
    {
        int poisonDuration = 5;
        Timer cloudTimer = new Timer(5000, TimerState.Stopped, TimerType.Manual);
        Vector2 cloudCenter;
        float cloudRange = 150;
        Timer expireTimer = new Timer(60000, TimerState.Stopped, TimerType.Manual);

        public PoisonTrap(CharacterSprite owner)
            : base("Traps", new Point(48, 48), new Point(24, 24), 2, new Vector2(24f, 24f), Vector2.Zero)
        {
            movementSpeed = 0f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 10;
            shouldRotate = false;

            AddAnimation(new Animation("Fire", 2, 2, 70, true, SpriteEffects.None, Color.White));

            SetParticle(ParticleType.ExplosionWhite, Color.Green, 2);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            if (otherSprite is EnemySprite && owner is PlayerSprite)
            {
                Expire();
            }
        }

        public override void Expire()
        {
            base.Expire();

            cloudCenter = CenteredPosition;
            cloudTimer.ResetTimerAndRun();

            expireTimer.State = TimerState.Stopped;

            AudioManager.audioManager.PlaySFX("TrapSpring" + Util.Random.Next(1, 5).ToString());
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (expireTimer.Update(gameTime))
            {
                Expire();
            }

            cloudTimer.Update(gameTime);

            if (cloudTimer.State == TimerState.Running)
            {
                ParticleSystem.AddParticles(cloudCenter, ParticleType.ExplosionWhite, color: Color.DarkGreen, sizeScale: 3f, numParticlesScale: .05f);

                // find enemies within range
                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(cloudCenter, cloudRange))
                {
                    enemy.Poison(damage[0], poisonDuration * 1000, SaveGameManager.CurrentPlayer);
                }
            }
            
        }


        public override void Fire(float angle, Vector2 position)
        {
            base.Fire(angle, position);

            expireTimer.ResetTimerAndRun();
            expiredTime = poisonDuration * 1000;
        }


        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            if (!isExpired)
            {
                base.Draw(spriteBatch, position, segment, spriteEffect);
            }
        }


        public int PoisonDuration
        {
            get { return poisonDuration; }
            set { poisonDuration = value; }
        }

    }
}
