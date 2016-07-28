using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class DwarfBoss : EnemySprite
    {
        public enum DwarfBossPhase
        {
            Basic,
            Firebolts,
            SummonSkeletons,
            Icebolts,
            Frenzy,
            Invisibile,
            Traps,
        }

        public const string DwarfBossName = "Dragnor";

        DwarfBossPhase phase = DwarfBossPhase.Basic;
        DwarfBossPhase[] phaseSeq;
        Timer phaseChangeTimer = new Timer(12000, TimerState.Running, TimerType.Auto);
        Timer trapTimer = new Timer(4000, TimerState.Running, TimerType.Auto);
        Timer invisTimer = new Timer(4000, TimerState.Running, TimerType.Auto);
        int phaseNum = 0;
        Firebolt[] firebolts = new Firebolt[10];
        IceBolt[] icebolts = new IceBolt[10];
        ExplosionTrap[] explosionTraps = new ExplosionTrap[5];
        int boltNum = 3;
        int skelNum = 4;
        List<EnemySprite> enemies = new List<EnemySprite>();

        public DwarfBoss(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(196 / 2, 196 / 2), Vector2.Zero, player, level)
        {
            Name = DwarfBossName;
            isBoss = true;
            //enemySoundEffectName = "Skel";
            //enemySoundEffectVariations = 4;

            // add in other textures
            if (level <= 5)
            {
                AddTexture("DwarfBossWalk", new Point(196, 196), 10);
                AddTexture("DwarfBossAttack", new Point(196, 196), 10);
            }
            else if (level <= 15)
            {
                AddTexture("DwarfBossWalk2", new Point(196, 196), 10);
                AddTexture("DwarfBossAttack2", new Point(196, 196), 10);
            }
            else
            {
                AddTexture("DwarfBossWalk3", new Point(196, 196), 10);
                AddTexture("DwarfBossAttack3", new Point(196, 196), 10);
            }
            AddTexture("Clear", new Point(1, 1), 1);    // dwarf boss die

            AddAnimationSet("Idle", 12, 1, 100, false, 1, offsetToDir1:false);
            AddAnimationSet("Walk", 12, 0, 100, true, offsetToDir1: false);
            AddAnimationSet("Attack", 12, 1, 80, false, offsetToDir1: false);
            AddAnimationSet("Die", 1, 2, 100, false);

            for (int i = 0; i < firebolts.Length; i++)
            {
                firebolts[i] = new Firebolt(this);
            }
            for (int i = 0; i < icebolts.Length; i++)
            {
                icebolts[i] = new IceBolt(this);
            }
            for (int i = 0; i < explosionTraps.Length; i++)
            {
                explosionTraps[i] = new ExplosionTrap(this);
            }

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 80;
            movementRange = 0;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability(BalanceManager.GetBaseEnemyLife(level)) * 60);
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level) * 2f);
            attackDamageTypes.Add(DamageType.Physical);
            killExp *= 100;

            SetTarget(player);

            AdjustForLevel();
        }


        public override void Die(CharacterSprite killer)
        {
            // add smoke plumes
            ParticleSystem.AddParticles(player.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray, numParticlesScale: 4.0f, sizeScale: 2f);

            // add death text
            if (level <= 5)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, "You are not as weak as you look fool. Do not follow me. Next time we meet you will die."));
            }
            else if (level <= 10)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, "You are growing strong, but you are still a fool. Come after me if you dare!"));
            }
            else if (level <= 15)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, "My corruption is almost complete. You have merely slowed me down, but you cannot stop me!"));
            }
            else if (level <= 20)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, "Impossible!\nBut I am invincible... how can this happen...\nAHHHHHhhh!!!\n.. . ... ."));
            }
            else
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(110, 0, 110, 148), DwarfBoss.DwarfBossName, "My corruption is only driven deeper into the recesses of the earth! My soul cannot be destroyed!"));
            }

            base.Die(killer);
        }

        public override EnemyType GetEnemyType()
        {
            if (level <= 5)
            {
                return EnemyType.DwarfBoss;
            }
            else if (level <= 15)
            {
                return EnemyType.DwarfBoss2;
            }
            else
            {
                return EnemyType.DwarfBoss3;
            }
        }


        private void AdjustForLevel()
        {
            // adjust based on level
            if (level <= 5)
            {
                // first encounter
                boltNum = 1;
                skelNum = 2;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Basic, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else if (level <= 10)
            {
                // Second encounter
                boltNum = 1;
                skelNum = 3;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Basic, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else if (level <= 15)
            {
                // Third encounter
                boltNum = 3;
                skelNum = 4;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Traps, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Invisibile, DwarfBossPhase.Traps, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else if (level <= 20)
            {
                // Final encounter
                boltNum = 3;
                skelNum = 5;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Traps, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Invisibile, DwarfBossPhase.Traps, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else if (level <= 25)
            {
                // end game
                boltNum = 3;
                skelNum = 6;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Traps, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Invisibile, DwarfBossPhase.Traps, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else if (level <= 30)
            {
                // end game
                boltNum = 3;
                skelNum = 7;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Traps, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Invisibile, DwarfBossPhase.Traps, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
            else
            {
                // end game
                boltNum = 3;
                skelNum = 8;
                DwarfBossPhase[] phaseSeq = { DwarfBossPhase.Traps, DwarfBossPhase.Firebolts, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Icebolts, DwarfBossPhase.Frenzy, DwarfBossPhase.SummonSkeletons, DwarfBossPhase.Invisibile, DwarfBossPhase.Traps, DwarfBossPhase.SummonSkeletons };
                this.phaseSeq = phaseSeq;
            }
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {

        }


        public override void Attack()
        {
            if (phase == DwarfBossPhase.Basic || phase == DwarfBossPhase.Frenzy)
            {
                AudioManager.audioManager.PlaySFX("Swing" + Util.Random.Next(1, 3).ToString());
            }
            else
            {
                AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());
            }
            base.Attack();
        }


        public override void AttackFinish()
        {
            base.AttackFinish();

            if (target is PlayerSprite)
            {
                if (phase == DwarfBossPhase.Basic || phase == DwarfBossPhase.Frenzy || phase == DwarfBossPhase.Invisibile || phase == DwarfBossPhase.Traps)
                {
                    if (Vector2.Distance(CenteredPosition, target.CenteredPosition) < attackRange)
                    {
                        PlayerSprite player = (PlayerSprite)target;
                        player.Damage(attackDamage, attackDamageTypes, this);
                        AudioManager.audioManager.PlaySFX("Sword" + Util.Random.Next(1, 7).ToString());
                    }
                }
                else if (phase == DwarfBossPhase.Firebolts)
                {
                    for (int j = 0; j < boltNum; j++)
                    {
                        float angleAdjust = Util.GetAngleAdjust(boltNum, j);

                        for (int i = 0; i < firebolts.Length; i++)
                        {
                            if (!firebolts[i].IsActive)
                            {
                                float angle = (float)Math.Atan2(target.CenteredPosition.Y - CenteredPosition.Y, target.CenteredPosition.X - CenteredPosition.X);
                                angle += angleAdjust;
                                firebolts[i].SetDamage(attackDamage[0], DamageType.Fire);
                                firebolts[i].Fire(angle, new Vector2(CenteredPosition.X - icebolts[i].FrameDimensions.X / 2, CenteredPosition.Y - icebolts[i].FrameDimensions.Y / 2));
                                break;
                            }
                        }
                    }
                }
                else if (phase == DwarfBossPhase.Icebolts)
                {
                    for (int j = 0; j < boltNum; j++)
                    {
                        float angleAdjust = Util.GetAngleAdjust(boltNum, j);

                        for (int i = 0; i < icebolts.Length; i++)
                        {
                            if (!icebolts[i].IsActive)
                            {
                                float angle = (float)Math.Atan2(target.CenteredPosition.Y - CenteredPosition.Y, target.CenteredPosition.X - CenteredPosition.X);
                                angle += angleAdjust;
                                icebolts[i].SetDamage(attackDamage[0], DamageType.Ice);
                                icebolts[i].Fire(angle, new Vector2(CenteredPosition.X - icebolts[i].FrameDimensions.X / 2, CenteredPosition.Y - icebolts[i].FrameDimensions.Y / 2));
                                break;
                            }
                        }
                    }
                }
                else if (phase == DwarfBossPhase.SummonSkeletons)
                {
                    for (int i = 0; i < skelNum; i++)
                    {
                        EnemySprite enemy = new SkeletonKnight(Position, SaveGameManager.CurrentPlayer, Level);
                        enemy.InitializeTextures();
                        GameplayScreen.Dungeon.AddNpc(enemy, 0, false);
                        ParticleSystem.AddParticles(enemy.CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray);
                        enemies.Add(enemy);
                    }
                    attackTime = 500000000; // only do this once a phase
                }

            }
        }

        public override WeaponSprite[] GetCollisionableWeaponSet()
        {
            int totalweps = firebolts.Length + icebolts.Length + explosionTraps.Length;
            int offset = 0;
            WeaponSprite[] weapons = new WeaponSprite[totalweps];

            for (int i = 0; i < firebolts.Length; i++, offset++)
            {
                weapons[offset] = firebolts[i];
            }
            for (int i = 0; i < icebolts.Length; i++, offset++)
            {
                weapons[offset] = icebolts[i];
            }
            for (int i = 0; i < explosionTraps.Length; i++, offset++)
            {
                weapons[offset] = explosionTraps[i];
            }

            return weapons;
        }


        public override void Persist(System.IO.BinaryWriter writer)
        {
            base.Persist(writer);

            writer.Write((UInt32)phase);
            phaseChangeTimer.Persist(writer);
            trapTimer.Persist(writer);
            invisTimer.Persist(writer);
            writer.Write((Int32)phaseNum);
            writer.Write((Int32)boltNum);
            writer.Write((Int32)skelNum);
        }


        public override bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            phase = (DwarfBossPhase)reader.ReadUInt32();
            phaseChangeTimer.Load(reader, dataVersion);
            trapTimer.Load(reader, dataVersion);
            invisTimer.Load(reader, dataVersion);
            phaseNum = reader.ReadInt32();
            boltNum = reader.ReadInt32();
            skelNum = reader.ReadInt32();

            return true;
        }


        public override void Damage(List<float> damage, List<DamageType> damageType, CharacterSprite attacker, float extraAdjust = 1f, bool autoCrit = false, bool allowReflect = true)
        {
            EndInvisible();

            base.Damage(damage, damageType, attacker, extraAdjust, autoCrit, allowReflect);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isDead)
                return;

            if (phaseChangeTimer.Update(gameTime))
            {
                if (phase == DwarfBossPhase.SummonSkeletons)
                {
                    AbsorbEnemies();
                    phaseChangeTimer.ExpireDuration = 12000;
                }

                // time to change the phase
                phaseNum++;
                if (phaseNum > phaseSeq.Length - 1)
                    phaseNum = 0;
                phase = phaseSeq[phaseNum];


                // change our behavior based on new state
                if (phase == DwarfBossPhase.Firebolts || phase == DwarfBossPhase.Icebolts)
                {
                    attackRange = 350;
                    movementRange = 300;
                    baseMovementSpeed = 5f;
                    attackTime = 2000;
                    pathFindType = PathFindType.Ranged;
                }
                else if (phase == DwarfBossPhase.Basic || phase == DwarfBossPhase.Traps)
                {
                    attackRange = 130;
                    movementRange = 0;
                    baseMovementSpeed = 5f;
                    attackTime = 2000;
                    pathFindType = PathFindType.Melee;
                }
                else if (phase == DwarfBossPhase.Frenzy)
                {
                    attackRange = 130;
                    movementRange = 0;
                    baseMovementSpeed = 7f;
                    attackTime = 1300;
                    pathFindType = PathFindType.Melee;
                    // TODO: increase damage taken during this time
                }
                else if (phase == DwarfBossPhase.SummonSkeletons)
                {
                    attackRange = 10000000000; // stand still
                    movementRange = 3000000;
                    baseMovementSpeed = 5f;
                    attackTime = 4000;
                    pathFindType = PathFindType.Ranged;
                    phaseChangeTimer.ExpireDuration = 18000;
                }
                else if (phase == DwarfBossPhase.Invisibile)
                {
                    attackRange = 130;
                    movementRange = 0;
                    baseMovementSpeed = 5f;
                    attackTime = 2000;
                    pathFindType = PathFindType.Melee;
                    BecomeInvisible(20000);
                    ParticleSystem.AddParticles(CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray);
                }
            }

            if (phase == DwarfBossPhase.Frenzy)
            {
                ParticleSystem.AddParticles(CenteredPosition, ParticleType.ExplosionWhite, color: Color.Red, sizeScale: 1f, numParticlesScale: .1f);
            }
            else if (phase == DwarfBossPhase.Traps)
            {
                if (trapTimer.Update(gameTime))
                {
                    for (int i = 0; i < explosionTraps.Length; i++)
                    {
                        if (!explosionTraps[i].IsActive)
                        {
                            explosionTraps[i].SetDamage(attackDamage[0] * 3, DamageType.Fire);
                            explosionTraps[i].Fire(0f, new Vector2(CenteredPosition.X - explosionTraps[i].FrameDimensions.X / 2, CenteredPosition.Y - explosionTraps[i].FrameDimensions.Y / 2));
                            break;
                        }
                    }
                }
            }
            else if (phase == DwarfBossPhase.Invisibile)
            {
                if (invisTimer.Update(gameTime))
                {
                    if (!IsInvisible)
                    {
                        BecomeInvisible(20000);
                        ParticleSystem.AddParticles(CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray);
                    }
                }
            }

        }

        private void AbsorbEnemies()
        {
            bool anyAbsorbed = false;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] != null && enemies[i].IsActive && !enemies[i].IsDead)
                {
                    ParticleSystem.AddParticles(enemies[i].CenteredPosition, ParticleType.ExplosionWhite, color: Color.Gray);
                    health += enemies[i].Health * 5;
                    if (health > MaxHealth)
                    {
                        health = MaxHealth;
                    }
                    enemies[i].Die(this);
                    anyAbsorbed = true;
                }
            }
            if (anyAbsorbed)
            {
                ParticleSystem.AddParticles(CenteredPosition, ParticleType.Starburst, color: Color.Red, numParticlesScale:3f);
            }
            enemies.Clear();
        }



    }
}
