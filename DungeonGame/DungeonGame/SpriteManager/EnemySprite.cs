using System;
using System.IO;
using System.Collections.Generic;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum EnemyType
    {
        Ogre = 0,

        [Description("Skeleton Knight")]
        SkeletonKnight = 1,

        [Description("Skeleton Archer")]
        SkeletonArcher = 2,

        [Description("Elemental")]
        LavaTroll = 3,

        Ghost = 4,

        Goblin = 5,

        [Description(LegendsOfDescent.DwarfBoss.DwarfBossName)]
        DwarfBoss = 6,

        [Description(LegendsOfDescent.DwarfBoss.DwarfBossName)]
        DwarfBoss2 = 7,

        [Description(LegendsOfDescent.DwarfBoss.DwarfBossName)]
        DwarfBoss3 = 8,

        Drake = 9,

        Count = 10

    }


    public enum ChampionTier
    {
        None = 0,
        Strong = 1,
        Fearsome = 2,
        Dreaded = 3
    }

    public class EnemySprite : NPCSprite, ISaveable
    {
        // variability parameters
        private const float baseHealthVariability = .25f;

        protected int killExp;
        protected DamageType resistanceDamageType = DamageType.None;

        float itemDropChance = .2f;
        float goldDropChance = .5f;
        float potionDropChance = .1f;


        // play enemy sound effects
        protected string enemySoundEffectName = "";
        protected int enemySoundEffectVariations = 1;

        // variables used for champion characters
        protected bool isBoss = false;
        protected bool isChampion = false;
        protected ChampionTier championTier = ChampionTier.None;
        protected int h = 0, s = 0, l = 0; // how to adjust the color of this champion character
        const float adjustHealthMax = 10.0f;
        const float adjustHealthMin = 5.0f;
        const float adjustDamageMax = 2.5f;
        const float adjustDamageMin = 1.8f;

        // Add items here that should always be dropped when the enemy dies
        private List<ItemSprite> specialLoot = new List<ItemSprite>();

        public EnemySprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset, PlayerSprite player, int level)
            : base(nPosition, nFrameOrigin, nSourceOffset, player)
        {
            this.level = level;
            killExp = BalanceManager.GetExperienceForEnemyKill(level);
        }


        public int ApplyHealthVariability(int health)
        {
            return (int)((float)health * Util.Random.Between(1f - baseHealthVariability, 1f + baseHealthVariability));
        }


        public override void DropItem(ItemSprite item)
        {
            base.DropItem(item);
            if (item is GoldItem == false)
            {
                SaveGameManager.CurrentPlayer.StatsManager.AddItemDrop(item);
            }
        }


        public override void Die(CharacterSprite killer)
        {
            base.Die(killer);

            SaveGameManager.CurrentPlayer.QuestLog.EnemyDied(this, killer is PlayerSprite);
            SaveGameManager.CurrentPlayer.StatsManager.AddEnemyDied(GetEnemyType());

            if (killer is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)killer;
                player.AddExperience(killExp);

                // drop items/gold/potions based on chances
                int itemDrops = 1;
                if (championTier == ChampionTier.Fearsome)
                    itemDrops = 2;
                else if (championTier == ChampionTier.Dreaded)
                    itemDrops = 3;

                foreach (var loot in specialLoot)
                {
                    DropItem(loot);
                }

                for (int i = 0; i < itemDrops; i++)
                {
                    ItemSprite item;
                    if (Util.Random.Between(0f, 1f) < itemDropChance || isChampion || isBoss)
                    {
                        item = ItemManager.Instance.GetItemForDrop(this);
                        DropItem(item);
                    }

                    if (Util.Random.Between(0f, 1f) < goldDropChance || isChampion || isBoss)
                    {
                        int goldAmount = (int)((float)BalanceManager.GetBaseGoldDrop(level) * Util.Random.Between(.75f, 1.25f));
                        if (isChampion)
                            goldAmount *= Util.Random.Next(4, 8);
                        goldAmount = (int)((float)goldAmount * (((float)player.GetEquippedPropertyValue(Property.GoldDrop) + 100f) / 100f));
                        item = new GoldItem(goldAmount);
                        DropItem(item);
                    }

                    if (Util.Random.Between(0f, 1f) < potionDropChance)
                    {
                        if (Util.Random.Next(0, 2) == 0)
                        {
                            item = ItemManager.Instance.GetItem("Health Potion", level);
                        }
                        else
                        {
                            item = ItemManager.Instance.GetItem("Mana Potion", level);
                        }
                        DropItem(item);
                    }
                }
            }

            // play death sound
            AudioManager.audioManager.PlaySFX(enemySoundEffectName + Util.Random.Next(1, enemySoundEffectVariations).ToString());

            
            // make sure we are not stacked exactly on another enemy
            bool goodPosition = false;
            while (goodPosition == false)
            {
                goodPosition = true;
                IEnumerable<EnemySprite> enemies = GameplayScreen.Instance.Enemies;
                foreach (EnemySprite enemy in enemies)
                {
                    if (enemy.Equals(this))
                        continue;

                    if (enemy.isDead && enemy.Position == Position)
                    {
                        // we are exactly on another enemy, we need to shift
                        MovePosition(1f, 0f);
                        goodPosition = false;
                        break;
                    }
                }
            }

        }




        static public EnemySprite GetEnemy(int level, Vector2 position, PlayerSprite player, EnemyType enemyType)
        {
            EnemySprite enemy = null;

            int random = Util.Random.Next(0, 100);

            if (enemyType == EnemyType.Ogre)
            {
                enemy = new Ogre(position, player, level);
            }
            else if (enemyType == EnemyType.LavaTroll)
            {
                enemy = new LavaTroll(position, player, level);
            }
            else if (enemyType == EnemyType.SkeletonArcher)
            {
                enemy = new SkeletonArcher(position, player, level);
            }
            else if (enemyType == EnemyType.Ghost)
            {
                enemy = new Ghost(position, player, level);
            }
            else if (enemyType == EnemyType.Goblin)
            {
                enemy = new Goblin(position, player, level);
            }
            else if (enemyType == EnemyType.DwarfBoss || enemyType == EnemyType.DwarfBoss2 || enemyType == EnemyType.DwarfBoss3)
            {
                enemy = new DwarfBoss(position, player, level);
            }
            else if (enemyType == EnemyType.Drake)
            {
                enemy = new Drake(position, player, level);
            }
            else
            {
                enemy = new SkeletonKnight(position, player, level);
            }

            return enemy;
        }



        public static void PlaceEnemies(EnemyType enemyType, int count, DungeonLevel dungeon, int enemyLevel, PlayerSprite player)
        {
            switch (enemyType)
            {
                case EnemyType.Ogre:
                    {
                        Ogre.PlaceEnemiesLocal(enemyType, count, dungeon, enemyLevel, player);
                        break;
                    }
                case EnemyType.Goblin:
                    {
                        Goblin.PlaceEnemiesLocal(enemyType, count, dungeon, enemyLevel, player);
                        break;
                    }
                case EnemyType.SkeletonArcher:
                    {
                        SkeletonArcher.PlaceEnemiesLocal(enemyType, count, dungeon, enemyLevel, player);
                        break;
                    }
                case EnemyType.SkeletonKnight:
                    {
                        SkeletonKnight.PlaceEnemiesLocal(enemyType, count, dungeon, enemyLevel, player);
                        break;
                    }
                default:
                    {
                        EnemySprite.PlaceEnemiesLocal(enemyType, count, dungeon, enemyLevel, player);
                        break;
                    }
            }
        }


        public static void PlaceEnemiesLocal(EnemyType enemyType, int count, DungeonLevel dungeon, int enemyLevel, PlayerSprite player)
        {
            // default is complete rand
            for (int i = 0; i < count; i++)
            {
                // get a random open location in the dungeon
                NPCSprite npc = EnemySprite.GetEnemy(enemyLevel, Vector2.Zero, player, enemyType);
                dungeon.AddNpc(npc, 10);
            }
        }


        public void SetChampion()
        {
            isChampion = true;
            Name = NameGenerator.GetName(NameGenerator.Style.OrcTolkien);

            // adjust stats to make this guy strong
            float attackIncrease = Util.Random.Between(adjustDamageMin, adjustDamageMax);
            float healthIncrease = Util.Random.Between(adjustHealthMin, adjustHealthMax);

            // adjust for tier
            int roll = Util.Random.Next(0, 100);
            if (roll > 95)
            {
                championTier = ChampionTier.Dreaded;
                attackIncrease *= 2f;
                healthIncrease *= 5f;
            }
            else if (roll > 80)
            {
                championTier = ChampionTier.Fearsome;
                attackIncrease *= 1.5f;
                healthIncrease *= 2.5f;
            }
            else
            {
                championTier = ChampionTier.Strong;
            }
         
            health = maxHealthBase = maxHealthBase * healthIncrease;
            killExp = (int)(killExp * (attackIncrease + healthIncrease));

            for (int i = 0; i < attackDamage.Count; i++)
            {
                attackDamage[i] = attackDamage[i] * attackIncrease;
            }

            // adjust to use the champion textures
            if (!DungeonGame.lowMemoryMode)
            {
                for (int i = 0; i < textureName.Count; i++)
                {
                    textureName[i] = textureName[i] + "Champ";
                }
            }
        }


        public Color GetColorForChampionTier()
        {
            if (championTier == ChampionTier.Strong)
            {
                return Color.Orange;
            }
            else if (championTier == ChampionTier.Fearsome)
            {
                return Color.Red;
            }
            else if (championTier == ChampionTier.Dreaded)
            {
                return Color.Purple;
            }
            else
            {
                return Color.White;
            }


        }



        public static void AdjustTexture(Texture2D texture, EnemyType enemyType, int level, string name)
        {
            switch (enemyType)
            {
                case EnemyType.Ogre:
                    Ogre.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.SkeletonKnight:
                    SkeletonKnight.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.SkeletonArcher:
                    SkeletonArcher.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.LavaTroll:
                    LavaTroll.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.Ghost:
                    Ghost.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.Goblin:
                    Goblin.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.DwarfBoss:
                    DwarfBoss.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.DwarfBoss2:
                    DwarfBoss.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.DwarfBoss3:
                    DwarfBoss.AdjustTextureLocal(texture, level, name);
                    break;
                case EnemyType.Drake:
                    Drake.AdjustTextureLocal(texture, level, name);
                    break;
            }
        }


        public static void AdjustChampionTexture(Texture2D texture, EnemyType enemyType, int level)
        {
            TextureModifier.AdjustHSL(texture, hueAdjust: level * 100, satAdjust: 50);
        }


        public virtual EnemyType GetEnemyType()
        {
            if (this is Ghost)
                return EnemyType.Ghost;
            else if (this is Ogre)
                return EnemyType.Ogre;
            else if (this is LavaTroll)
                return EnemyType.LavaTroll;
            else if (this is SkeletonArcher)
                return EnemyType.SkeletonArcher;
            else if (this is SkeletonKnight)
                return EnemyType.SkeletonKnight;
            else if (this is Goblin)
                return EnemyType.Goblin;
            else if (this is DwarfBoss)
                return EnemyType.DwarfBoss;
            else if (this is Drake)
                return EnemyType.Drake;

            throw new Exception("invalid enemy type observed");
        }

        public virtual void Persist(BinaryWriter writer)
        {
            writer.Write((Int32)GetEnemyType());
            writer.Write((Int32)level);
            writer.Write(Position);
            writer.Write((Int32)health);
            writer.Write((Int32)maxHealthBase);
            writer.Write(Name);
            writer.Write(isChampion);

            // new to 0.2 to support champion scaling and special loot
            writer.Write((Int32)killExp);
            writer.Write((Int32)attackDamage.Count);
            for (int i = 0; i < attackDamage.Count; i++)
            {
                writer.Write(attackDamage[i]);
                writer.Write((Int16)attackDamageTypes[i]);
            }
            writer.Write((Int16)championTier);

            writer.Write((Int32)specialLoot.Count);
            foreach (var loot in specialLoot)
            {
                loot.Persist(writer);
            }

            writer.Write((Int32)lastDirection);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // load any special enemy data here in inherited classes

            return true;
        }


        public static EnemySprite LoadEnemy(BinaryReader reader, int dataVersion)
        {
            EnemySprite enemy = null;

            // read and construct the basic enemy
            EnemyType enemyType = (EnemyType)reader.ReadInt32();
            int level = reader.ReadInt32();
            Vector2 position = reader.ReadVector2();
            enemy = GetEnemy(level, position, SaveGameManager.CurrentPlayer, enemyType);
            enemy.health = reader.ReadInt32();
            enemy.maxHealthBase = reader.ReadInt32();
            enemy.Name = reader.ReadString();
            enemy.isChampion = reader.ReadBoolean();
            
            // new to v0.2 to support champion scaling and special loot
            if (dataVersion >= 2)
            {
                enemy.killExp = reader.ReadInt32();
                enemy.attackDamage.Clear();
                enemy.attackDamageTypes.Clear();
                int damages = reader.ReadInt32();
                for (int i = 0; i < damages; i++)
                {
                    enemy.attackDamage.Add(reader.ReadSingle());
                    enemy.attackDamageTypes.Add((DamageType)reader.ReadInt16());
                }
                enemy.championTier = (ChampionTier)reader.ReadInt16();

                int specialLootCount = reader.ReadInt32();
                for (int i = 0; i < specialLootCount; i++)
                {
                    enemy.specialLoot.Add(ItemSprite.LoadItem(reader, dataVersion));
                }
            }

            if (dataVersion >= 7)
            {
                enemy.lastDirection = (Direction)reader.ReadInt32();
            }

            // load up the specifics
            enemy.Load(reader, dataVersion);

            if (!DungeonGame.lowMemoryMode)
            {
                if (enemy.isChampion)
                {
                    // adjust to use the champion textures
                    for (int i = 0; i < enemy.textureName.Count; i++)
                    {
                        enemy.textureName[i] = enemy.textureName[i] + "Champ";
                    }
                }
            }


            if (enemy.health <= 0)
            {
                enemy.isDead = true;
                enemy.isCollisionable = false;
                enemy.PlayAnimation("Die", enemy.lastDirection);
                enemy.currentFrame = enemy.currentAnimation.EndingFrame;
            }


            return enemy;
        }



        public void SetResistance(DamageType damageType, float amount = 0.7f, bool setColor = true)
        {
            // clear out any other resistance set
            for (int i = 0; i < resistances.Length; i++)
            {
                resistances[i] = 0f;
            }

            resistanceDamageType = damageType;
            Color tint = Color.White;
            if (damageType == DamageType.Fire)
            {
                resistances[(int)DamageType.Fire] = amount;
                tint = new Color(255, 81, 90); // light reds
            }
            else if (damageType == DamageType.Ice)
            {
                resistances[(int)DamageType.Ice] = amount;
                tint = new Color(112, 116, 255); // light blue
            }
            else if (damageType == DamageType.Lightning)
            {
                resistances[(int)DamageType.Lightning] = amount;
                tint = new Color(255, 232, 86); // light yellow
            }
            else if (damageType == DamageType.Poison)
            {
                resistances[(int)DamageType.Poison] = amount;
                tint = new Color(151, 255, 122); // light green
            }

            if (setColor)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    animations[i].Tint = tint;
                }
            }
        }


        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            if (!isDead && isChampion && !IsInvisible)
            {
                // draw the champions name
                nameSprite.Color = GetColorForChampionTier();
                nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, -10) + GameplayScreen.viewportCorner;
                nameSprite.Draw(spriteBatch);
            }

        }


        public bool IsBoss
        {
            get { return isBoss; }
        }

        public bool IsChampion
        {
            get { return isChampion; }
        }

        public ChampionTier ChampionTier
        {
            get { return championTier; }
        }

        public void AddSpecialLoot(ItemSprite item)
        {
            specialLoot.Add(item);
        }

        public int KillExp
        {
            get { return killExp; }
        }
    }
}
