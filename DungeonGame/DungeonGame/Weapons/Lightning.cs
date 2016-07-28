using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Lightning : WeaponSprite
    {

        public Lightning(CharacterSprite owner)
            : base("Lightning", new Point(24, 48), new Point(12, 24), 1, new Vector2(12f, 24f), Vector2.Zero)
        {
            movementSpeed = 30f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 0;
            centeredReduce = 10;

            AddAnimation(new Animation("Fire", 1, 8, 70, true, SpriteEffects.None, Color.White));
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);

            Expire();
        }


        public override void Expire()
        {
            base.Expire();
            isCollisionable = false;
            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Starburst, color:Color.Yellow);
            AudioManager.audioManager.PlaySFX("Spark");
            AudioManager.audioManager.PlaySFX("Boom");
        }

    }
}
