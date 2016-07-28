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
    class IceBolt : WeaponSprite
    {
        float baseSlowAmount = .5f;
        int baseSlowDuration = 2000;

        public IceBolt(CharacterSprite owner)
            : base("IceBolt", new Point(16, 27), new Point(8, 13), 1, new Vector2(8f, 13f), Vector2.Zero)
        {
            movementSpeed = 15f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 10;

            AddAnimation(new Animation("Fire", 1, 8, 70, true, SpriteEffects.None, Color.White));
            SetParticle(ParticleType.ExplosionWhite, Color.Blue);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);

            Expire();

            if (owner is PlayerSprite && otherSprite is EnemySprite)
            {
                EnemySprite enemy = (EnemySprite)otherSprite;
                enemy.SlowCharacter(baseSlowDuration, baseSlowAmount, owner);
            }
            else if (owner is EnemySprite && otherSprite is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)otherSprite;
                player.SlowCharacter(baseSlowDuration, baseSlowAmount, owner);
            }
        }


        public override void Expire()
        {
            base.Expire();
            isCollisionable = false;
            ParticleSystem.AddParticles(this.Position, ParticleType.ExplosionWhite, color:Color.Blue, numParticlesScale: 2.0f);
            AudioManager.audioManager.PlaySFX("Boom");
            AudioManager.audioManager.PlaySFX("Frozen" + Util.Random.Next(1,4).ToString());
        }

    }
}
