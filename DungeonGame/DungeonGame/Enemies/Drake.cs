using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Drake : EnemySprite
    {

        public static ElementalType ElementalType;
        WeaponSprite[] weapons;
        int boltNum = 3;

        public Drake(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(128 / 2, 128 / 2), Vector2.Zero, player, level)
        {
            Name = "Drake";

            // TODO: add drake death sound effect
            //enemySoundEffectName = "Skel";
            //enemySoundEffectVariations = 4;

            // add in other textures
            AddTexture("DragonWalk", new Point(128, 128), 8);
            AddTexture("DragonAttack", new Point(128, 128), 8);
            AddTexture("DragonDie", new Point(128, 128), 8);

            AddAnimationSet("Idle", 9, 1, 100, false, 1);
            AddAnimationSet("Walk", 8, 0, 100, true);
            AddAnimationSet("Attack", 9, 1, 80, false);
            AddAnimationSet("Die", 11, 2, 60, false);


            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 30;
            movementRange = 0;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability(BalanceManager.GetBaseEnemyLife(level)) * 1.5f);
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level));
            killExp = (int)((float)killExp * 1.5f);

            InitializeDrakeType();

            SetTarget(player);
        }

        public void InitializeDrakeType()
        {
            if (ElementalType == ElementalType.Fire)
            {
                if (!isChampion)
                    Name = "Fire Drake";
                weapons = new Firebolt[boltNum * 2];
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i] = new Firebolt(this);
                    weapons[i].SetDamage(attackDamage, attackDamageTypes);
                    weapons[i].MoveSpeed = 10f;
                }
                attackDamageTypes.Add(DamageType.Fire);
                SetResistance(DamageType.Fire, .5f);
            }
            else if (ElementalType == ElementalType.Ice)
            {
                if (!isChampion)
                    Name = "Ice Drake";
                weapons = new IceBolt[boltNum * 2];
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i] = new IceBolt(this);
                    weapons[i].SetDamage(attackDamage, attackDamageTypes);
                    weapons[i].MoveSpeed = 10f;
                }
                attackDamageTypes.Add(DamageType.Ice);
                SetResistance(DamageType.Ice, .5f);
            }
            else if (ElementalType == ElementalType.Lightning)
            {
                if (!isChampion)
                    Name = "Lightning Drake";
                weapons = new Lightning[boltNum * 2];
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i] = new Lightning(this);
                    weapons[i].SetDamage(attackDamage, attackDamageTypes);
                    weapons[i].MoveSpeed = 13f;
                }
                attackDamageTypes.Add(DamageType.Lightning);
                SetResistance(DamageType.Lightning, .5f);
            }
            else if (ElementalType == ElementalType.Poison)
            {
                if (!isChampion)
                    Name = "Poison Drake";
                weapons = new PoisonBolt[boltNum * 2];
                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i] = new PoisonBolt(this);
                    weapons[i].SetDamage(attackDamage, attackDamageTypes);
                    weapons[i].MoveSpeed = 13f;
                }
                attackDamageTypes.Add(DamageType.Poison);
                SetResistance(DamageType.Poison, .5f);
            }
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {

        }


        public override void Attack()
        {
            //AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());

            base.Attack();
        }


        public override void AttackFinish()
        {
            base.AttackFinish();

            float playerDistance = Vector2.Distance(CenteredPosition, target.CenteredPosition);

            if (target is PlayerSprite)
            {
                if (playerDistance <= 130)
                {
                    PlayerSprite player = (PlayerSprite)target;
                    player.Damage(attackDamage, attackDamageTypes, this);
                    //AudioManager.audioManager.PlaySFX("Sword" + Util.Random.Next(1, 7).ToString());
                }
                else
                {
                    for (int j = 0; j < boltNum; j++)
                    {
                        float angleAdjust = Util.GetAngleAdjust(boltNum, j);

                        for (int i = 0; i < weapons.Length; i++)
                        {
                            if (!weapons[i].IsActive)
                            {
                                float angle = (float)Math.Atan2(target.CenteredPosition.Y - CenteredPosition.Y, target.CenteredPosition.X - CenteredPosition.X);
                                angle += angleAdjust;
                                weapons[i].SetDamage(attackDamage[0], DamageType.Fire);
                                weapons[i].Fire(angle, new Vector2(CenteredPosition.X - weapons[i].FrameDimensions.X / 2, CenteredPosition.Y - weapons[i].FrameDimensions.Y / 2));
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override WeaponSprite[] GetCollisionableWeaponSet()
        {
            return weapons;
        }


        public override void Persist(System.IO.BinaryWriter writer)
        {
            base.Persist(writer);

            writer.Write((UInt32)ElementalType);
        }


        public override bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            ElementalType = (ElementalType)reader.ReadUInt32();
            InitializeDrakeType();

            return true;
        }





        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isDead)
                return;

            float playerDistance = Vector2.Distance(CenteredPosition, target.CenteredPosition);

            if (playerDistance > 200)
            {
                attackRange = 350;
                movementRange = 300;
                pathFindType = PathFindType.Ranged;
            }
            else
            {
                attackRange = 130;
                movementRange = 0;
                pathFindType = PathFindType.Melee;
            }
                
        }

        


    }
}
