using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Fireball : WeaponSprite
    {
        public int Radius = 1;

        public Fireball(CharacterSprite owner)
            : base("Fireball", new Point(38, 38), new Point(19, 19), 1, new Vector2(19f, 19f), Vector2.Zero)
        {
            movementSpeed = 20f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 20;

            AddAnimation(new Animation("Fire", 1, 8, 70, true, SpriteEffects.None, Color.White));
            SetParticle(ParticleType.Explosion, Color.White);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            Expire();

            if (otherSprite is PlayerSprite)
            {
                base.CollisionAction(otherSprite);
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

            isCollisionable = false;

            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Explosion, sizeScale: Radius * 1.2f, lifetimeScale: .8f + .2f * Radius);
            AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
        }



    }
}
