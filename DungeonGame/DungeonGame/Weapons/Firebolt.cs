using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Firebolt : WeaponSprite
    {

        public Firebolt(CharacterSprite owner)
            : base("Firebolt", new Point(25, 25), new Point(13, 13), 1, new Vector2(13f, 13f), Vector2.Zero)
        {
            movementSpeed = 20f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 1;
            centeredReduce = 10;

            AddAnimation(new Animation("Fire", 1, 8, 70, true, SpriteEffects.None, Color.White));
            SetParticle(ParticleType.Explosion, Color.White);
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
            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Explosion, sizeScale: 1f, lifetimeScale: 1f, numParticlesScale: 2.0f);
            AudioManager.audioManager.PlaySFX("SmallExplosion" + Util.Random.Next(1, 4).ToString());
        }

    }
}
