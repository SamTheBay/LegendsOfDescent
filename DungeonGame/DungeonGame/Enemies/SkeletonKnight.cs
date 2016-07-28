using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class SkeletonKnight : EnemySprite
    {

        public SkeletonKnight(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(96 / 2, 96 / 2), Vector2.Zero, player, level)
        {
            Name = "Skeleton Knight";
            enemySoundEffectName = "Skel";
            enemySoundEffectVariations = 4;

            // add in other textures
            AddTexture("SkelKnightWalk", new Point(96, 96), 16);
            AddTexture("SkelKnightAttack", new Point(96, 96), 16);
            AddTexture("SkelKnightDie", new Point(96, 96), 16);

            AddAnimationSet("Idle", 10, 1, 100, false, 1);
            AddAnimationSet("Walk", 9, 0, 100, true);
            AddAnimationSet("Attack", 10, 1, 80, false);
            AddAnimationSet("Die", 9, 2, 70, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 30;
            movementRange = 0;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability(BalanceManager.GetBaseEnemyLife(level)));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level));
            attackDamageTypes.Add(DamageType.Physical);

            // give a resistance
            int roll = Util.Random.Next(0, 6);
            if (roll == 0)
            {
                SetResistance(DamageType.Fire);
            }
            if (roll == 1)
            {
                SetResistance(DamageType.Ice);
            }
            if (roll == 2)
            {
                SetResistance(DamageType.Lightning);
            }
            if (roll == 3)
            {
                SetResistance(DamageType.Poison);
            }

            SetTarget(player);
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
                AudioManager.audioManager.PlaySFX("Sword" + Util.Random.Next(1, 7).ToString());
            }
        }


        public override void Persist(System.IO.BinaryWriter writer)
        {
            base.Persist(writer);

            writer.Write((Byte)resistanceDamageType);
        }


        public override bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            SetResistance((DamageType)reader.ReadByte());

            return true;
        }



        public static new void PlaceEnemiesLocal(EnemyType enemyType, int count, DungeonLevel dungeon, int enemyLevel, PlayerSprite player)
        {
            Vector2 lastPosition = Vector2.Zero;

            // default is complete rand
            for (int i = 0; i < count;)
            {
                int groupSize = Util.Random.Next(2, 5);

                // get a random open location in the dungeon
                for (int j = 0; j < groupSize && i < count; j++)
                {
                    NPCSprite npc = EnemySprite.GetEnemy(enemyLevel, Vector2.Zero, player, enemyType);
                    if (j == 0)
                    {
                        dungeon.AddNpc(npc, 10);
                        lastPosition = npc.Position;
                    }
                    else
                    {
                        npc.Position = lastPosition;
                        dungeon.AddNpc(npc, 10, false);
                    }
                    i++;
                }
            }
        }


    }
}
