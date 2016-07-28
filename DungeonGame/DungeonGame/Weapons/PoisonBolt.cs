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
    class PoisonBolt : WeaponSprite
    {
        public PoisonBolt(CharacterSprite owner)
            : base("PoisonBall", new Point(25, 25), new Point(13, 13), 1, new Vector2(13f, 13f), Vector2.Zero)
        {
            movementSpeed = 15f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 10;

            AddAnimation(new Animation("Fire", 1, 8, 70, true, SpriteEffects.None, Color.White));
            SetParticle(ParticleType.ExplosionWhite, Color.Green);
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            base.CollisionAction(otherSprite);

            Expire();

            if (owner is PlayerSprite && otherSprite is EnemySprite)
            {
                EnemySprite enemy = (EnemySprite)otherSprite;
                enemy.Poison(damage[0] / 10, 5000, (CharacterSprite)otherSprite);
            }
            else if (owner is EnemySprite && otherSprite is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)otherSprite;
                player.Poison(damage[0] / 5, 5000, (CharacterSprite)otherSprite);
            }
        }


        public override void Expire()
        {
            base.Expire();
            isCollisionable = false;
            ParticleSystem.AddParticles(this.Position, ParticleType.ExplosionWhite, color:Color.Green, numParticlesScale: 2.0f);
            AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
        }

    }
}
