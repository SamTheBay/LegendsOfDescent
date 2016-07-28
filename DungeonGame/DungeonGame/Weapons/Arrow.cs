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
    class Arrow : WeaponSprite
    {

        public Arrow(CharacterSprite owner)
            : base("Arrow", new Point(32, 32), new Point(16, 16), 1, new Vector2(16f, 16f), Vector2.Zero)
        {
            movementSpeed = 20f;
            isProjectile = true;
            this.owner = owner;
            expiredTime = 0;
            centeredReduce = 10;
            collisionSize = 20;

            AddAnimation(new Animation("Fire", 1, 1, 70, true, SpriteEffects.None, Color.White));
        }


        public override void CollisionAction(GameSprite otherSprite)
        {
            if (owner is PlayerSprite && otherSprite is EnemySprite)
            {
                EnemySprite enemy = (EnemySprite)otherSprite;
                PlayerSprite player = (PlayerSprite)owner;

                List<ITimedEffect> effects = player.GetAttackEffects(enemy);
                for (int i = 0; i < effects.Count; i++)
                {
                    enemy.AddTimedEffect(effects[i]);
                }

                enemy.Damage(damage, damageTypes, owner, autoCrit: this.autoCrit);
            }
            else if (owner is EnemySprite && otherSprite is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)otherSprite;
                player.Damage(damage, damageTypes, owner, autoCrit: this.autoCrit);
            }

            Expire();
        }

        public override void Expire()
        {
            base.Expire();
            isCollisionable = false;
            AudioManager.audioManager.PlaySFX("Hit4");
        }


    }
}
