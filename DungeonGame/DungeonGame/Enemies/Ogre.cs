using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Ogre : EnemySprite
    {

        public Ogre(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(96/2, 96/2), Vector2.Zero, player, level)
        {
            Name = "Ogre";
            enemySoundEffectName = "Ork";
            enemySoundEffectVariations = 3;

            // add in other textures
            AddTexture("OgreRun", new Point(96, 96), 16);
            AddTexture("OgreAttack", new Point(128, 128), 12);
            AddTexture("OgreDie", new Point(128, 128), 12);

            AddAnimationSet("Idle", 11, 1, 100, false, 1);
            AddAnimationSet("Walk", 8, 0, 100, true);
            AddAnimationSet("Attack", 11, 1, 80, false);
            AddAnimationSet("Die", 11, 2, 70, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            attackDelayTime = 450;
            centeredReduce = 30;
            movementRange = 0;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability((int)(BalanceManager.GetBaseEnemyLife(level))));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level));
            attackDamageTypes.Add(DamageType.Physical);

            SetTarget(player);
        }


        public override void Attack()
        {
            AudioManager.audioManager.PlaySFX("Swing" + Util.Random.Next(1, 3).ToString());
            base.Attack();
        }

        public override void AttackFinish()
        {
            base.AttackFinish();

            // hit the player if he is still in range
            if (target is PlayerSprite && Vector2.Distance(CenteredPosition, target.CenteredPosition) < attackRange)
            {
                PlayerSprite player = (PlayerSprite)target;
                player.Damage(attackDamage, attackDamageTypes, this);
                AudioManager.audioManager.PlaySFX("Hit" + Util.Random.Next(1, 4).ToString());
            }
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {
            if (DungeonGame.isRT)
                return;

            TextureModifier.AdjustHSL(texture, hueAdjust: level * 100);
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
