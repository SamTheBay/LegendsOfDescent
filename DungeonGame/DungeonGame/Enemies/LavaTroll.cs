using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum ElementalType
    {
        Fire,
        Ice,
        Lightning,
        Poison
    }

    class LavaTroll : EnemySprite
    {
        WeaponSprite weapon;
        public static ElementalType ElementalType;

        public LavaTroll(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(90 / 2, 120 / 2), Vector2.Zero, player, level)
        {
            enemySoundEffectName = "Element";
            enemySoundEffectVariations = 4;

            if (DungeonGame.isRT)
            {
                AddTexture("LavaTrollWalk" + ElementalType.ToString(), new Point(96, 96), 16);
                AddTexture("LavaTrollAttack" + ElementalType.ToString(), new Point(96, 96), 16);
                AddTexture("LavaTrollDie" + ElementalType.ToString(), new Point(96, 96), 16);
            }
            else
            {
                AddTexture("LavaTrollWalk", new Point(96, 96), 16);
                AddTexture("LavaTrollAttack", new Point(96, 96), 16);
                AddTexture("LavaTrollDie", new Point(96, 96), 16);
            }

            AddAnimationSet("Idle", 13, 1, 100, false, 1);
            AddAnimationSet("Walk", 8, 0, 100, true);
            AddAnimationSet("Attack", 13, 1, 65, false);
            AddAnimationSet("Die", 17, 2, 50, false);

            PlayAnimation("Idle", Direction.Left);
            lastDirection = Direction.Left;
            Activate();
            centeredReduce = 30;
            attackRange = 350;
            attackDelayTime = 350;
            attackDelayElapsed = 350;
            movementRange = 300;
            pathFindType = PathFindType.Ranged;
            maxHealthBase = Math.Max(1f, ApplyHealthVariability((int)(BalanceManager.GetBaseEnemyLife(level) * .7f)));
            health = maxHealthBase;
            attackDamage.Add(BalanceManager.GetBaseEnemeyDamage(level) * 1.5f);

            InitializeTrollType();

            SetTarget(player);
        }


        public void InitializeTrollType()
        {
            if (ElementalType == ElementalType.Fire)
            {
                if (!isChampion)
                    Name = "Fire Elemental";
                weapon = new Firebolt(this);
                attackDamageTypes.Add(DamageType.Fire);
                weapon.SetDamage(attackDamage, attackDamageTypes);
                weapon.MoveSpeed = 10f;
                resistances[(int)DamageType.Fire] = .5f;
            }
            else if (ElementalType == ElementalType.Ice)
            {
                if (!isChampion)
                    Name = "Ice Elemental";
                weapon = new IceBolt(this);
                attackDamageTypes.Add(DamageType.Ice);
                weapon.SetDamage(attackDamage, attackDamageTypes);
                weapon.MoveSpeed = 10f;
                resistances[(int)DamageType.Ice] = .5f;
            }
            else if (ElementalType == ElementalType.Lightning)
            {
                if (!isChampion)
                    Name = "Lightning Elemental";
                weapon = new Lightning(this);
                attackDamageTypes.Add(DamageType.Lightning);
                weapon.SetDamage(attackDamage, attackDamageTypes);
                weapon.MoveSpeed = 13f;
                resistances[(int)DamageType.Lightning] = .5f;
            }
            else if (ElementalType == ElementalType.Poison)
            {
                if (!isChampion)
                    Name = "Poison Elemental";
                weapon = new PoisonBolt(this);
                attackDamageTypes.Add(DamageType.Poison);
                weapon.SetDamage(attackDamage, attackDamageTypes);
                weapon.MoveSpeed = 10f;
                resistances[(int)DamageType.Poison] = .5f;
            }
        }


        public static void AdjustTextureLocal(Texture2D texture, int level, string name)
        {
            if (ElementalType == ElementalType.Ice)
            {
                TextureModifier.AdjustHSLInBand(texture, new Color(208, 142, 61), 220, hueAdjust: 180);
            }
            else if (ElementalType == ElementalType.Lightning)
            {
                TextureModifier.AdjustHSLInBand(texture, new Color(208, 142, 61), 220, hueAdjust: 35);
            }
            else if (ElementalType == ElementalType.Poison)
            {
                TextureModifier.AdjustHSLInBand(texture, new Color(208, 142, 61), 220, hueAdjust: 90);
            }
        }


        public override WeaponSprite[] GetCollisionableWeaponSet()
        {
            WeaponSprite[] weapons = new WeaponSprite[1];
            weapons[0] = weapon;
            return weapons;
        }



        public override void AttackFinish()
        {
            base.AttackFinish();

            if (weapon.IsActive == false)
            {
                float angle = (float)Math.Atan2(target.CenteredPosition.Y - CenteredPosition.Y, target.CenteredPosition.X - CenteredPosition.X);
                weapon.Fire(angle, new Vector2(CenteredPosition.X - weapon.FrameDimensions.X / 2, CenteredPosition.Y - weapon.FrameDimensions.Y / 2));
            }
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
            InitializeTrollType();

            return true;
        }


        public override void InitializeTextures()
        {
            if (DungeonGame.isRT)
            {
                textureName[0] = "LavaTrollWalk" + ElementalType.ToString();
                textureName[1] = "LavaTrollAttack" + ElementalType.ToString();
                textureName[2] = "LavaTrollDie" + ElementalType.ToString();
            }

            base.InitializeTextures();
        }
    }
}
