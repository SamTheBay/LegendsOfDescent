using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class ChainLightning : WeaponSprite
    {
        int maxBounceCount = 3;
        float maxBounceDistance = 300f;
        int currentBounceCount = 0;
        List<EnemySprite> alreadyHit = new List<EnemySprite>();

        public ChainLightning(CharacterSprite owner)
            : base("Lightning", new Point(24, 48), new Point(12, 24), 1, new Vector2(12f, 24f), Vector2.Zero)
        {
            movementSpeed = 20f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 0;
            centeredReduce = 10;

            AddAnimation(new Animation("Fire", 1, 3, 70, true, SpriteEffects.None, Color.White));
        }


        public void SetCharacteristics(int maxBounce, int damage)
        {
            maxBounceCount = maxBounce;
            SetDamage(damage, DamageType.Lightning);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            // check if we have already hit this enemy
            EnemySprite enemy = (EnemySprite)otherSprite;
            for (int i = 0; i < alreadyHit.Count; i++)
            {
                if (alreadyHit[i] != null && enemy.Equals(alreadyHit[i]))
                {
                    return;
                }
            }

            base.CollisionAction(otherSprite);
            currentBounceCount++;

            if (currentBounceCount >= maxBounceCount)
            {
                Expire();
            }
            else
            {
                // find the next closest enemy if there is one and head for them
                alreadyHit.Add(enemy);
                float distance = 0;
                EnemySprite nextEnemy = GameplayScreen.Instance.GetClosestEnemy(otherSprite.CenteredPosition, ref distance, alreadyHit);
                if (nextEnemy != null && distance < maxBounceDistance)
                {
                    // Show sparks on this enemy
                    ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Starburst, color: Color.Yellow, numParticlesScale:0.3f);
                    
                    // Go to the next enemy
                    projectileAngle = (float)Math.Atan2(nextEnemy.CenteredPosition.Y - CenteredPosition.Y, nextEnemy.CenteredPosition.X - CenteredPosition.X);
                    PlayAnimation("Fire", projectileAngle + (float)(Math.PI / 2));
                    AudioManager.audioManager.PlaySFX("Spark");

                    // reduce damage for next hop
                    for (int i = 0; i < damage.Count; i++)
                    {
                        damage[i] = (int)(damage[i] * .66f);
                    }
                }
                else
                {
                    Expire();
                }

            }

        }


        public override void Expire()
        {
            base.Expire();
            isCollisionable = false;
            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Starburst, color:Color.Yellow);
            AudioManager.audioManager.PlaySFX("Spark");
            AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
        }


        public override void Activate()
        {
            alreadyHit.Clear();
            currentBounceCount = 0;
            base.Activate();
        }

    }
}
