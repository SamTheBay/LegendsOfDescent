using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class SkeletonArcher : EnemySprite
    {
        Arrow arrow;

        public SkeletonArcher(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(90 / 2, 120 / 2), Vector2.Zero, player, level)
        {
            Name = "Skeleton Archer";
            enemySoundEffectName = "Skel";
            enemySoundEffectVariations = 4;

            AddTexture("SkelArcherWalk", new Point(96, 96), 16);
            AddTexture("SkelArcherAttack", new Point(96, 96), 16);
            AddTexture("SkelArcherDie", new Point(96, 96), 16);

            AddAnimationSet("Idle", 13, 1, 100, false, 1);
            AddAnimationSet("Walk", 8, 0, 100, true);
            AddAnimationSet("Attack", 13, 1, 65, false);
            AddAnimationSet("Die", 9, 2, 50, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 30;
            attackRange = 350;
            attackDelayTime = 350;
            attackDelayElapsed = 350;
            movementRange = 300;
            pathFindType = PathFindType.Ranged;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability(BalanceManager.GetBaseEnemyLife(level)));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level) * .7f);
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

            arrow = new Arrow(this);
            arrow.SetDamage(attackDamage, attackDamageTypes);
            SetTarget(player);
        }


        public override WeaponSprite[] GetCollisionableWeaponSet()
        {
            WeaponSprite[] arrows = new WeaponSprite[1];
            arrows[0] = arrow;
            return arrows;
        }



        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {

        }


        public override void AttackFinish()
        {
            base.AttackFinish();

            if (arrow.IsActive == false)
            {
                float angle = (float)Math.Atan2(target.CenteredPosition.Y - CenteredPosition.Y, target.CenteredPosition.X - CenteredPosition.X);
                arrow.Fire(angle, new Vector2(CenteredPosition.X - arrow.FrameDimensions.X / 2, CenteredPosition.Y - arrow.FrameDimensions.Y / 2));
                AudioManager.audioManager.PlaySFX("BowShoot");
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
