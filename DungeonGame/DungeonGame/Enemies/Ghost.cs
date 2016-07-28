using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Ghost : EnemySprite
    {
        bool lostInvisible = false;

        public Ghost(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(96/2, 96/2), Vector2.Zero, player, level)
        {
            Name = "Ghost";
            enemySoundEffectName = "Ghost";
            enemySoundEffectVariations = 5;

            // add in other textures
            AddTexture("GhostWalk", new Point(96, 96), 16);
            AddTexture("GhostAttack", new Point(96, 96), 16);
            AddTexture("GhostDie", new Point(96, 96), 16);


            AddAnimationSet("Idle", 13, 1, 100, false, 1);
            AddAnimationSet("Walk", 8, 0, 100, true);
            AddAnimationSet("Attack", 13, 1, 80, false);
            AddAnimationSet("Die", 9, 2, 70, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 30;
            movementRange = 0;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability(BalanceManager.GetBaseEnemyLife(level)));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level) * .7f);
            attackDamageTypes.Add(DamageType.Physical);
            resistances[(int)DamageType.Physical] = .25f;

            SetTarget(player);

            BecomeInvisible(Int32.MaxValue);
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {

        }


        public override void Attack()
        {
            AudioManager.audioManager.PlaySFX("Swing" + Util.Random.Next(1, 3).ToString());
            base.Attack();
        }


        public override void AttackFinish()
        {
            base.AttackFinish();

            if (target is PlayerSprite && Vector2.Distance(CenteredPosition, target.CenteredPosition) < attackRange)
            {
                PlayerSprite player = (PlayerSprite)target;
                player.Damage(attackDamage, attackDamageTypes, this);
                AudioManager.audioManager.PlaySFX("Hit" + Util.Random.Next(1, 4).ToString());
                LoseInvisible();
            }
        }


        public override void Damage(List<float> damage, List<DamageType> damageType, CharacterSprite attacker, float extraAdjust = 1, bool autoCrit = false, bool allowReflect = true)
        {
            base.Damage(damage, damageType, attacker, extraAdjust, autoCrit, allowReflect);
            LoseInvisible();
        }


        public void LoseInvisible()
        {
            if (!lostInvisible)
            {
                EndInvisible();
                ParticleSystem.AddParticles(CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray, numParticlesScale: 2.0f);
                lostInvisible = true;
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // remove the ghost when dead (it has no corpse)
            if (isDead && IsPlaybackComplete)
            {
                Deactivate();
            }
        }


        public override bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            if (dataVersion >= 2)
            {
                lostInvisible = reader.ReadBoolean();
                if (lostInvisible)
                {
                    EndInvisible();
                }
            }

            return true;
        }


        public override void Persist(System.IO.BinaryWriter writer)
        {
            base.Persist(writer);

            writer.Write(lostInvisible);
        }

    }
}
