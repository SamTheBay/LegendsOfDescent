using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegendsOfDescent
{
    public enum DamageType
    {
        Physical = 0,
        Fire,
        Ice,
        Lightning,
        Poison,
        None
    }


    public class WeaponSprite : AnimatedSprite
    {
        protected CharacterSprite owner;
        public List<float> damage = new List<float>();
        public List<DamageType> damageTypes = new List<DamageType>();
        protected bool isProjectile;
        protected float projectileAngle;
        protected float movementSpeed;
        protected int collisionSize = 40;
        protected bool autoCrit = false;
        protected bool shouldRotate = true;

        // timers
        protected bool isExpired = false;
        protected int expiredTime = 300;
        protected int expiredElapsed = 0;

        // attaching a particle generator
        protected bool hasParticle = false;
        protected ParticleType particleType;
        protected Color particleColor;
        protected int particleFreq = 1;
        protected int particleFreqCount = 0;


        public WeaponSprite(string nTextureName, Point nFrameDimensions, Point nFrameOrigin, int nFramesPerRow, Vector2 nSourceOffset, Vector2 nPosition)
            : base(nTextureName, nFrameDimensions, nFrameOrigin, nFramesPerRow, nSourceOffset, nPosition)
        {
            isFlying = true;
        }


        Vector2 movement = new Vector2();
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isExpired)
            {
                expiredElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (expiredElapsed >= expiredTime)
                {
                    Deactivate();
                }
            }


            // update position
            if (isProjectile && isActive && !isExpired)
            {
                movement.Y = (float)Math.Sin(projectileAngle) * movementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f * 1.2f;
                movement.X = (float)Math.Cos(projectileAngle) * movementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f * 1.2f;

                if (CanMove(ref movement, GameplayScreen.Dungeon, collisionSize:collisionSize, rightAllowance:0, downAllowance:0))
                {
                    MovePosition(movement.X, movement.Y);
                }
                else
                {
                    Expire();
                }
            }

            // check if we are far enough off screen to deactivate
            if (Position.X > GameplayScreen.viewportCorner.X + DungeonGame.ScreenSize.Width + 100 ||
                Position.X < GameplayScreen.viewportCorner.X - 100 ||
                Position.Y > GameplayScreen.viewportCorner.Y + DungeonGame.ScreenSize.Height + 100 ||
                Position.Y < GameplayScreen.viewportCorner.Y - 100)
            {
                Deactivate();
            }
                
        }


        public void SetParticle(ParticleType particleType, Color particleColor, int frequency = 1)
        {
            hasParticle = true;
            this.particleType = particleType;
            this.particleColor = particleColor;
            this.particleFreq = frequency;
        }

        public void ResetParticle()
        {
            hasParticle = false;
        }

        public virtual void Fire(float angle, Vector2 position)
        {
            isExpired = false;
            expiredElapsed = 0;
            projectileAngle = angle;
            this.Position = position;

            // adjust position in direction to avoid wall collision
            MovePosition((float)Math.Cos(projectileAngle) * 30, (float)Math.Sin(projectileAngle) * 15);

            isCollisionable = true;
            Activate();
            ResetAnimation();
            if (shouldRotate)
                PlayAnimation("Fire", angle + (float)(Math.PI / 2));
            else
                PlayAnimation("Fire", 0f);

        }


        public virtual float MoveSpeed
        {
            set { movementSpeed = value; }
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);

            if (owner is PlayerSprite && otherSprite is EnemySprite)
            {
                EnemySprite enemy = (EnemySprite)otherSprite;
                enemy.Damage(damage, damageTypes, owner, autoCrit: this.autoCrit);
            }
            else if (owner is EnemySprite && otherSprite is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)otherSprite;
                player.Damage(damage, damageTypes, owner, autoCrit: this.autoCrit);
            }
        }



        public virtual void Expire()
        {
            isExpired = true;
            expiredElapsed = 0;
        }



        public void SetDamage(List<float> damage, List<DamageType> damageTypes)
        {
            this.damage = damage;
            this.damageTypes = damageTypes;
        }

        public void SetDamage(float damage, DamageType damageType)
        {
            this.damage.Clear();
            this.damageTypes.Clear();
            AddDamage(damage, damageType);
        }


        public void AddDamage(float damage, DamageType damageType)
        {
            this.damage.Add(damage);
            this.damageTypes.Add(damageType);
        }


        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            if (DungeonGame.TestMode)
            {
                DrawBoundingBox(spriteBatch);
            }

            particleFreqCount++;
            if (isActive && hasParticle && particleFreqCount % particleFreq == 0)
            {
                ParticleSystem.AddParticles(this.CenteredPosition, particleType, sizeScale: .6f, lifetimeScale: 1f, numParticlesScale: .1f, color: particleColor);
            }

            base.Draw(spriteBatch, position, segment, spriteEffect);
        }

        public bool AutoCrit
        {
            get { return autoCrit; }
            set { autoCrit = value; }
        }
    }
}
