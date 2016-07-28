using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{
    class Goblin : EnemySprite
    {
        bool isLeader = false;


        public Goblin(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(128/2, 128/2), Vector2.Zero, player, level)
        {
            Name = "Goblin";

            // TODO: add goblin specific sound effects
            enemySoundEffectName = "Ork";
            enemySoundEffectVariations = 3;

            // add in other textures
            AddTexture("GoblinWalk", new Point(128, 128), 12);
            AddTexture("GoblinAttack", new Point(128, 128), 12);
            AddTexture("GoblinDie", new Point(128, 128), 12);

            AddAnimationSet("Idle", 12, 1, 100, false, 1);
            AddAnimationSet("Walk", 12, 0, 100, true);
            AddAnimationSet("Attack", 12, 1, 80, false);
            AddAnimationSet("Die", 12, 2, 100, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 60;
            movementRange = 0;

            // default to non-leader
            maxHealthBase = Math.Max(1f, ApplyHealthVariability((int)(BalanceManager.GetBaseEnemyLife(level) * .6f)));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level) * .6f);
            attackDamageTypes.Add(DamageType.Physical);
            killExp = (int)(killExp * .6f);
            

            SetTarget(player);
        }



        public void SetLeaderInitial()
        {
            // adjust stats
            isLeader = true;
            maxHealthBase *= 3f;
            health = maxHealthBase;
            attackDamage[0] *= 2.5f;
            killExp = (int)(killExp * 3f);

            SetLeader();
        }


        public void SetLeader()
        {
            // change textures for leader
            for (int i = 0; i < textureName.Count; i++)
            {
                textureName[i] = textureName[i] + "Leader";
            }
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
            }
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {
            // TODO: maybe use preset combo's here?

            if (DungeonGame.isRT)
                return;

            int sat = 0;
            if (level > 8)
                sat = 25;
            if (name.Contains("Leader"))
                TextureModifier.AdjustHSLInBand(texture, new Color(81, 101, 76), 40, hueAdjust: level * 100, lightAdjust:40, satAdjust:sat);
            else
                TextureModifier.AdjustHSLInBand(texture, new Color(81, 101, 76), 40, hueAdjust: level * 100 + 50, lightAdjust: 40, satAdjust:sat);
        }

        public override void Persist(System.IO.BinaryWriter writer)
        {
            base.Persist(writer);

            writer.Write(isLeader);
        }


        public override bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            isLeader = reader.ReadBoolean();
            if (isLeader)
                SetLeader();

            return true;
        }



        public static new void PlaceEnemiesLocal(EnemyType enemyType, int count, DungeonLevel dungeon, int enemyLevel, PlayerSprite player)
        {
            Vector2 lastPosition = Vector2.Zero;

            // default is complete rand
            for (int i = 0; i < count;)
            {
                int groupSize = Util.Random.Next(3, 6);

                // get a random open location in the dungeon
                for (int j = 0; j < groupSize && i < count; j++)
                {
                    Goblin npc = (Goblin)EnemySprite.GetEnemy(enemyLevel, Vector2.Zero, player, enemyType);
                    if (j == 0)
                    {
                        npc.SetLeaderInitial();
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
