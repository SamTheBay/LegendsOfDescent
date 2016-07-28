using System;
using System.Collections.Generic;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum ItemType : byte
    {
        Equipable,
        Usable,
        Gold
    }


    public enum EquipSlot : byte
    {
        Head = 0,

        Chest,

        Legs,

        Hand,

        Augment,

        [Description("Off Hand")]
        OffHand,

        [Description("Main Hand")]
        MainHand,

        [Description("Two Hand")]
        TwoHand,

        Num
    }

    public enum Property
    {
        Armor = 0,
        Damage,
        Health,
        Mana,
        MoveSpeed,
        BaseMoveSpeed,
        AttackSpeed,
        BaseAttackSpeed,
        CastSpeed,
        BaseCastSpeed,
        FireDamage,
        ColdDamage,
        LightningDamage,
        SpellDamage,
        HealthRegen,
        ManaRegen,
        CriticalHitChance,
        BaseManaCost,
        RangedDamage,
        MeleeDamage,
        MagicResistance,
        BaseRangedAttackSpeed,
        PoisonDamage,
        GoldDrop,
        MagicDrop,
        BleedDamage,
        BurnDamage,
        SlowChance,
        StunChance,
        FearChance,
        LifeSteal,
        ManaSteal,
        ReflectDamage,
        Num
    }

    public struct PropertyModifier
    {
        public Property property;
        public int amount;

        public PropertyModifier(PropertyModifier other)
        {
            this.property = other.property;
            this.amount = other.amount;
        }

        public PropertyModifier(Property property, int amount)
        {
            this.property = property;
            this.amount = amount;
        }

        public override string ToString()
        {
            string modifierString = ItemManager.PropertyToString(property) + ": ";
            if (amount > 0 && 
                property != Property.Damage && 
                property != Property.Armor && 
                property != Property.PoisonDamage &&
                property != Property.BurnDamage &&
                property != Property.BleedDamage)
            {
                modifierString += "+";
            }
            modifierString += amount.ToString();
            if (property == Property.CastSpeed ||
                property == Property.AttackSpeed ||
                property == Property.MoveSpeed ||
                property == Property.CriticalHitChance ||
                property == Property.MagicResistance ||
                property == Property.SlowChance ||
                property == Property.StunChance ||
                property == Property.FearChance ||
                property == Property.MagicDrop ||
                property == Property.GoldDrop ||
                property == Property.LifeSteal ||
                property == Property.ManaSteal ||
                property == Property.ReflectDamage)
            {
                modifierString += "%";
            }
            if (property == Property.PoisonDamage || property == Property.BurnDamage || property == Property.BleedDamage)
            {
                modifierString += "/s for 5s";
            }
            return modifierString;
        }
    }

    public class ItemSprite : EnvItemSprite, ISelectable, ISaveable
    {
        public const int EquipSlotNum = 6;

        protected Texture2D iconTexture;
        protected static Point iconDimensions = new Point(48,48);
        protected int iconFramesPerRow;
        protected int iconOffset;
        protected Rectangle iconSource;

        protected string iconTextureName;
        protected Point slot;
        protected PlayerSprite owner;

        protected string name;
        protected string description;
        protected int value = 0;
        protected int level = 0;
        protected ItemClass itemClass = ItemClass.Normal;
        protected List<PropertyModifier> modifiers = new List<PropertyModifier>();
        protected float valueAdjust = 1f;
        protected int upgradeCount = 0;
        protected MerchantType merchantType = MerchantType.General;

        protected bool isUsable = false;
        protected bool isEquipable = false;
        protected bool isSelectable = false;

        protected float tossAcc = .8f;
        protected float tossVel = 0f;
        protected float tossRotation = 0f;
        protected float tossRotationSpeed = 1.4f;
        protected Vector2 origonalPosition;
        protected float initTossVel = -12.5f;
        protected int tossTime = 10000;
        protected int tossTimeElapsed = 10000;

        protected TextSprite nameSprite = new TextSprite();

        // info for particle glow
        protected bool showParticles = false;
        protected ParticleType particleType;
        protected Color particleColor;

        public ItemSprite(string name, string texture, int framesPerRow, int offset, int level, float valueAdjust, string description, int value)
            : base(Vector2.Zero, texture, framesPerRow, offset, new Point(48,48))
        {
            this.name = name;
            this.level = level;
            this.valueAdjust = valueAdjust;
            this.description = description;
            this.value = value;
            nameSprite.BorderColor = Color.Black;
        }



        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (IsTossing)
            {
                tossTimeElapsed += gameTime.ElapsedGameTime.Milliseconds;
                tossVel += tossAcc * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;
                tossRotation += tossRotationSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;
                SetPosition(Position.X, Position.Y + tossVel * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f);
                if (tossVel >= -1 * initTossVel)
                {
                    tossTimeElapsed = tossTime;
                }
                if (!IsTossing)
                {
                    PlayDroppedSoundEffect();
                    Position = origonalPosition;
                    if (!(this is GoldItem))
                    {
                        ShowName();
                    }
                }
            }
            else
            {
                tossRotation = 0;
            }
        }


        public override void ShowName()
        {
            bool shouldAdd = !nameSprite.IsActive;

            nameSprite.SetDetails(name, Position + new Vector2(0, -35), ClassColor(), Fonts.DescriptionFont, Vector2.Zero, 5000);
            nameSprite.Activate();
            if (shouldAdd)
            {
                GameplayScreen.Instance.AddTextSprite(nameSprite);
            }
        }


        public void SetIcon(string iconTexture, int iconFramesPerRow, int iconOffset)
        {
            this.iconTexture = InternalContentManager.GetTexture(iconTexture);
            this.iconTextureName = iconTexture;
            this.iconFramesPerRow = iconFramesPerRow;
            this.iconOffset = iconOffset;

            int column = (iconOffset) / this.iconFramesPerRow;
            iconSource = new Rectangle(
                (iconOffset - (column * iconFramesPerRow)) * iconDimensions.X,
                column * iconDimensions.Y,
                iconDimensions.X, iconDimensions.Y);
        }


        public void SetIcon(int iconOffset)
        {
            SetIcon(iconTextureName, iconFramesPerRow, iconOffset);
        }



        public void SetParticleGlow(ParticleType particleType, Color particleColor)
        {
            showParticles = true;
            this.particleType = particleType;
            this.particleColor = particleColor;
        }

        public virtual void PlayDroppedSoundEffect()
        {
            AudioManager.audioManager.PlaySFX("Drop");
        }


        public virtual void AddParticles(object owner, Vector2 position)
        {
            if (itemClass == ItemClass.Magic)
                ParticleSystem.AddParticles(position, ParticleType.Glow, color: Color.LightSkyBlue, owner: owner);


            if (showParticles)
            {
                if (particleType == ParticleType.Starburst)
                {
                    ParticleSystem.AddParticles(position, ParticleType.Starburst, color: particleColor, owner: owner, sizeScale: .9f, lifetimeScale: .8f, velocityScale: .4f, numParticlesScale: .5f);
                }
                else if (particleType == ParticleType.ExplosionWhite)
                {
                    ParticleSystem.AddParticles(position, ParticleType.ExplosionWhite, color: particleColor, owner: owner, sizeScale: .6f, lifetimeScale: .8f, velocityScale: .5f, numParticlesScale: .7f);
                }
                else if (particleType == ParticleType.Explosion)
                {
                    ParticleSystem.AddParticles(position, ParticleType.Explosion, color: particleColor, owner: owner, sizeScale: .6f, lifetimeScale: .8f, velocityScale: .5f, numParticlesScale: .7f);
                }
                else if (particleType == ParticleType.Glow)
                {
                    ParticleSystem.AddParticles(position, ParticleType.Glow, color: particleColor, owner: owner);
                }
            }
        }


        public virtual bool IsSelectable()
        {
            return isSelectable;
        }


        public virtual void Select()
        {
            // implemented by item types
        }


        public virtual MacroButton GetMacroButton(PlayerSprite player = null)
        {
            // implemented by different item types
            return new MacroButton(); // return empty macro button
        }


        public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position, Vector2 frame)
        {
            position.X += (frame.X - iconSource.Width) / 2;
            position.Y += (frame.Y - iconSource.Height) / 2;
            spriteBatch.Draw(iconTexture, position, iconSource, Color.White);
        }

        public override bool ActivateItem(PlayerSprite player)
        {
            bool result = player.AddItem(this);
            if (result)
            {
                Deactivate();
                SaveGameManager.CurrentPlayer.QuestLog.ItemPickedUp(this);
                nameSprite.Duration = 1;
            }
            else
            {
                Toss();
            }
            return result;
        }


        public override bool CanItemActivate()
        {
            return true;
        }


        public override Rectangle GetBoundingRectangle(ref Rectangle rect)
        {
            rect.X = (int)Position.X - FrameDimensions.X / 2 + centeredReduce / 2;
            rect.Y = (int)Position.Y - FrameDimensions.Y / 2 + centeredReduce / 2;
            rect.Width = (int)(FrameDimensions.X) - centeredReduce;
            rect.Height = (int)(FrameDimensions.Y) - centeredReduce;
            return rect;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            // items are below characters unless they are being tossed, in which case they should be on top
            int layerdepth = 2;
            if (IsTossing)
            {
                layerdepth = 4;
            }

            if (isActive)
            {
                spriteBatch.Draw(Texture, Position.Trunc() - GameplayScreen.viewportCorner.Trunc(), sourceRectangle, Color.White, tossRotation, new Vector2(24, 24), 1f, SpriteEffects.None, GetLayerDepth(layerdepth));
            }
        }



        public virtual void AddDescriptionDetails(DescriptionScreen screen)
        {
            screen.AddTexture(iconTexture, iconSource);
            screen.AddLine(name, Fonts.HeaderFont, ClassColor());

            for (int i = 0; i < modifiers.Count; i++)
            {
                screen.AddLine(modifiers[i].ToString(), Fonts.DescriptionFont, Color.White);
            }

            if (owner is PlayerSprite)
            {
                screen.AddLine("Sell For: " + Value.ToString(), Fonts.DescriptionFont, Color.Gold);
            }
            else
            {
                screen.AddLine("Buy For: " + Value.ToString(), Fonts.DescriptionFont, Color.Gold);
            }

            screen.AddSpace(15);
            screen.AddLine(description, Fonts.DescriptionItFont, Color.SandyBrown);

            screen.ShowSS = true;
        }



        public void Toss()
        {
            if (!IsTossing)
            {
                tossTimeElapsed = 0;
                tossVel = initTossVel;
                tossRotation = 0;
                origonalPosition = Position;
                nameSprite.Duration = 1;
            }
        }


        public Color ClassColor()
        {
            if (this is GoldItem)
            {
                return Color.Goldenrod;
            }
            else if (itemClass == ItemClass.Magic)
            {
                return Color.Blue;
            }
            else if (itemClass == ItemClass.Rare)
            {
                return Color.Green;
            }
            else if (itemClass == ItemClass.Heroic)
            {
                return Color.Red;
            }
            else if (itemClass == ItemClass.Legendary)
            {
                return Color.Purple;
            }
            return Color.White;
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basics
            writer.Write(name);
            writer.Write(TextureName);
            writer.Write((Int32)FramesPerRow);
            writer.Write((Int32)offset);
            writer.Write((Int32)level);
            writer.Write((Int32)value);
            writer.Write(description);
            if (this is EquipableItem)
                writer.Write((Int32)ItemType.Equipable);
            else if (this is UsableItem)
                writer.Write((Int32)ItemType.Usable);
            else if (this is GoldItem)
                writer.Write((Int32)ItemType.Gold);
            writer.Write((Int32)upgradeCount);

            if (!(this is GoldItem))
            {
                // write icon
                writer.Write(iconTextureName);
                writer.Write((Int32)iconFramesPerRow);
                writer.Write((Int32)iconOffset);

                // write generic info
                writer.Write(slot);
                writer.Write((Int32)itemClass);

                // write modifiers
                writer.Write((Int32)modifiers.Count);
                for (int i = 0; i < modifiers.Count; i++)
                {
                    writer.Write((Int32)modifiers[i].property);
                    writer.Write((Int32)modifiers[i].amount);
                }
            }

            // save position in case this is on the map
            writer.Write(Position);

            // save particle info
            writer.Write(showParticles);
            writer.Write((Int16)particleType);
            particleColor.Persist(writer);

            writer.Write(this.CannotBeSoldOrDestroyed);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            if (!(this is GoldItem))
            {
                SetIcon(reader.ReadString(), reader.ReadInt32(), reader.ReadInt32());

                slot = reader.ReadPoint();
                itemClass = (ItemClass)reader.ReadInt32();

                int modNum = reader.ReadInt32();
                for (int i = 0; i < modNum; i++)
                {
                    modifiers.Add(new PropertyModifier((Property)reader.ReadInt32(), reader.ReadInt32()));
                }
            }

            Position = reader.ReadVector2();

            // new in data version 2: particles, undestroyable items
            if (dataVersion >= 2)
            {
                // load particle info
                showParticles = reader.ReadBoolean();
                particleType = (ParticleType)reader.ReadInt16();
                particleColor = reader.ReadColor();
                CannotBeSoldOrDestroyed = reader.ReadBoolean();
            }

            return true;
        }


        public static ItemSprite LoadItem(BinaryReader reader, int dataVersion)
        {
            ItemSprite item = null;

            // read in basics
            string name, texture, description;
            int framesPerRow, offset, level, value, upgradeCount = 0;
            ItemType itemType;
            name = reader.ReadString();
            texture = reader.ReadString();
            framesPerRow = reader.ReadInt32();
            offset = reader.ReadInt32();
            level = reader.ReadInt32();
            value = reader.ReadInt32();
            description = reader.ReadString();
            itemType = (ItemType)reader.ReadInt32();
            if (dataVersion >= 3)
                upgradeCount = reader.ReadInt32();


            if (dataVersion == 1)
            {
                // do backward compat translations to bridge the gap between 0.1 and 0.2. Can be deleted for v0.3
                if (offset == 28) // health potion
                {
                    texture = "RedPotions";
                    offset = 0;
                    framesPerRow = 5;
                }
                else if (offset == 27) // mana potion
                {
                    texture = "BluePotions";
                    offset = 0;
                    framesPerRow = 5;
                }
                else if (offset == 29) // speed potion
                {
                    texture = "GreenPotions";
                    offset = 0;
                    framesPerRow = 5;
                }
                else if (offset == 30) // Str potion
                {
                    texture = "OrangePotions";
                    offset = 0;
                    framesPerRow = 5;
                }
            }

            // generate the item
            if (itemType == ItemType.Equipable)
            {
                item = new EquipableItem(name, texture, framesPerRow, offset, level, 0, description, value);
            }
            else if (itemType == ItemType.Usable)
            {
                item = new UsableItem(name, texture, framesPerRow, offset, level, 0, description, value);
            }
            else if (itemType == ItemType.Gold)
            {
                item = new GoldItem(value);
            }
            else
            {
                throw new Exception("Tried to load an invalid item type" + itemType.ToString());
            }

            // load up the specifics
            item.upgradeCount = upgradeCount;
            item.Load(reader, dataVersion);

            if (dataVersion == 1)
            {
                item.SetIcon(texture, framesPerRow, offset);
            }

            return item;
        }

        public virtual int GetPropertyValue(Property property)
        {
            int value = 0;
            for (int j = 0; j < Modifiers.Count; j++)
            {
                if (Modifiers[j].property == property)
                {
                    value += Modifiers[j].amount;
                }
            }
            return value;
        }


        public virtual ItemSprite CopyItem()
        {
            return null;
        }

        public void AddModifier(PropertyModifier modifier)
        {
            modifiers.Add(modifier);
        }


        public void AdjustModifier(Property property, int adjust, bool isUpgrade = true)
        {
            bool found = false;
            for (int i = 0; i < Modifiers.Count; i++)
            {
                if (Modifiers[i].property == property)
                {
                    PropertyModifier modifer = new PropertyModifier(property, Modifiers[i].amount + adjust);
                    Modifiers[i] = modifer;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                PropertyModifier modifer = new PropertyModifier(property, adjust);
                AddModifier(modifer);
            }

            if (isUpgrade)
                upgradeCount++;
        }


        public string ModifierToString(PropertyModifier modifier)
        {
            string modifierString = ItemManager.PropertyToString(modifier.property) + ": ";
            if (modifier.amount > 0 &&
                modifier.property != Property.Damage &&
                modifier.property != Property.Armor &&
                modifier.property != Property.PoisonDamage &&
                modifier.property != Property.BurnDamage &&
                modifier.property != Property.BleedDamage)
            {
                modifierString += "+";
            }
            modifierString += GetPropertyValue(modifier.property).ToString();
            if (modifier.property == Property.CastSpeed ||
                modifier.property == Property.AttackSpeed ||
                modifier.property == Property.MoveSpeed ||
                modifier.property == Property.CriticalHitChance ||
                modifier.property == Property.MagicResistance ||
                modifier.property == Property.SlowChance ||
                modifier.property == Property.StunChance ||
                modifier.property == Property.FearChance ||
                modifier.property == Property.MagicDrop ||
                modifier.property == Property.GoldDrop ||
                modifier.property == Property.LifeSteal ||
                modifier.property == Property.ManaSteal ||
                modifier.property == Property.ReflectDamage)
            {
                modifierString += "%";
            }
            if (modifier.property == Property.PoisonDamage || modifier.property == Property.BurnDamage || modifier.property == Property.BleedDamage)
            {
                modifierString += "/s for 5s";
            }
            return modifierString;
        }


        public bool IsTossing
        {
            get { return tossTimeElapsed < tossTime; }
        }


        public Point Slot
        {
            get { return slot; }
            set { slot = value; }
        }

        public void SetSlot(int x, int y)
        {
            slot.X = x;
            slot.Y = y;
        }

        public List<PropertyModifier> Modifiers
        {
            get { return modifiers; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public PlayerSprite Owner
        {
            set { owner = value; }
        }

        public int Level
        {
            get { return level; }
            set { level = value; }
        }

        public ItemClass ItemClass
        {
            get { return itemClass; }
            set { itemClass = value; }
        }

        public virtual int Value
        {
            get 
            {
                int retValue = value;
                // adjust for sell price vs buy price
                if (owner is PlayerSprite)
                {
                    retValue /= 8;
                    if (itemClass == LegendsOfDescent.ItemClass.Normal)
                    {
                        retValue /= 4;
                    }
                }


                return retValue;
            }
            set { this.value = value; }
        }


        public float ValueAdjust
        {
            get { return valueAdjust; }
            set { valueAdjust = value; }
        }


        public bool IsPlayerOwned
        {
            get { return owner != null; }
        }

        public int UpgradeCount
        {
            get { return upgradeCount; }
        }

        public MerchantType MerchantType
        {
            get { return merchantType; }
            set { merchantType = value; }
        }

        public bool CannotBeSoldOrDestroyed { get; set; }
    }

    public static class InventoryExtensions
    {
        public static bool Contains(this ItemSprite[,] inventory, Func<ItemSprite, bool> predicate)
        {
            foreach (var item in inventory)
            {
                if (item != null && predicate(item))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool Remove(this ItemSprite[,] inventory, Func<ItemSprite, bool> predicate)
        {
            for (int x = 0; x < inventory.GetLength(0); x++)
            {
                for (int y = 0; y < inventory.GetLength(1); y++)
                {
                    var item = inventory[x, y];

                    if (item != null && predicate(item))
                    {
                        inventory[x, y] = null;
                        return true;
                    }
                }
            }

            return false;
        }


       
    }
}
