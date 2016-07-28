using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class ExplosionTrap : WeaponSprite
    {
        public int Radius = 1;
        Timer expireTimer = new Timer(60000, TimerState.Stopped, TimerType.Manual);

        public ExplosionTrap(CharacterSprite owner)
            : base("Traps", new Point(48, 48), new Point(24, 24), 2, new Vector2(24f, 24f), Vector2.Zero)
        {
            movementSpeed = 0f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 10;
            shouldRotate = false;

            AddAnimation(new Animation("Fire", 1, 1, 70, true, SpriteEffects.None, Color.White));

            SetParticle(ParticleType.Explosion, Color.White, 3);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            if (otherSprite is EnemySprite && owner is PlayerSprite)
            {
                Expire();
            }
            else if (otherSprite is PlayerSprite && owner is EnemySprite)
            {
                Expire();
            }
        }

        public override void Expire()
        {
            base.Expire();

            if (owner is PlayerSprite)
            {
                foreach (var enemy in GameplayScreen.Instance.GetEnemiesInRadius(this.CenteredPosition, Radius * TileSprite.tileDimension.X))
                {
                    float closeness = 1f - (Vector2.Distance(CenteredPosition, enemy.CenteredPosition) / (Radius * TileSprite.tileDimension.X));
                    enemy.Damage(damage, damageTypes, owner, closeness);
                }
            }
            else if (owner is EnemySprite)
            {
                SaveGameManager.CurrentPlayer.Damage(damage, damageTypes, owner);
            }

            isCollisionable = false;

            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Explosion, sizeScale: Radius * 1.2f, lifetimeScale: .8f + .2f * Radius);
            AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
            AudioManager.audioManager.PlaySFX("TrapSpring" + Util.Random.Next(1, 5).ToString());

            expireTimer.State = TimerState.Stopped;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (expireTimer.Update(gameTime))
            {
                Expire();
            }
        }


        public override void Fire(float angle, Vector2 position)
        {
            base.Fire(angle, position);

            expireTimer.ResetTimerAndRun();
        }
    }
}
