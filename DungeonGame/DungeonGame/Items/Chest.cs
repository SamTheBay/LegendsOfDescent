using System;
using System.Collections.Generic;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum ChestType
    {
        small,
        large,
        stash
    }

    class Chest : AnimatedSprite, IEnvItem, ISaveable
    {
        bool isOpened = false;
        List<ItemSprite> contents = new List<ItemSprite>();
        ChestType type;

        public Chest(Vector2 position, int level, ChestType type)
            : base("Chests", new Point(40,40), new Point(0,0), 6, Vector2.Zero, position)
        {
            this.type = type;

            // generate items to place in the chest
            int itemNum = GetNumberOfItemsToDrop();
            for (int i = 0; i < itemNum; i++)
            {
                ItemSprite item;
                if (Util.Random.Next(0, 100) > 70)
                {
                    item = ItemManager.Instance.GetItemForDrop(level);
                }
                else
                {
                    int goldAmount = (int)((float)BalanceManager.GetBaseGoldDrop(level) * Util.Random.Between(.75f, 1.25f));
                    goldAmount = (int)((float)goldAmount * (((float)SaveGameManager.CurrentPlayer.GetEquippedPropertyValue(Property.GoldDrop) + 100f) / 100f));
                    item = new GoldItem(goldAmount);
                }
                contents.Add(item);
            }

            Initailize();
        }


        void Initailize()
        {
            animations.Clear();
            SetAnimations();

            if (isOpened)
            {
                PlayAnimation("Open");
            }
            else
            {
                PlayAnimation("Closed");
            }

            Activate();
        }


        void SetAnimations()
        {
            switch (type)
            {
                case ChestType.small:
                    {
                        AddAnimation(new Animation("Closed", 3, 3, 1, false, SpriteEffects.None));
                        AddAnimation(new Animation("Open", 4, 4, 1, false, SpriteEffects.None));
                        break;
                    }
                case ChestType.large:
                    {
                        AddAnimation(new Animation("Closed", 1, 1, 1, false, SpriteEffects.None));
                        AddAnimation(new Animation("Open", 2, 2, 1, false, SpriteEffects.None));
                        break;
                    }
                case ChestType.stash:
                    {
                        AddAnimation(new Animation("Closed", 5, 5, 1, false, SpriteEffects.None));
                        AddAnimation(new Animation("Open", 6, 6, 1, false, SpriteEffects.None));
                        break;
                    }
                default:
                    {
                        AddAnimation(new Animation("Closed", 3, 3, 1, false, SpriteEffects.None));
                        AddAnimation(new Animation("Open", 4, 4, 1, false, SpriteEffects.None));
                        break;
                    }
            }
        }


        int GetNumberOfItemsToDrop()
        {
            switch (type)
            {
                case ChestType.small:
                    {
                        return 4;
                    }
                case ChestType.large:
                    {
                        return 7;
                    }
                case ChestType.stash:
                    {
                        return 0;
                    }
                default:
                    {
                        return 4;
                    }
            }
        }

        public virtual bool ActivateItem(PlayerSprite player)
        {
            if (type != ChestType.stash)
            {
                if (!isOpened)
                {
                    for (int i = 0; i < contents.Count; i++)
                    {
                        contents[i].Position = this.Position;
                        GameplayScreen.Instance.AddEnvItem(contents[i]);
                        contents[i].Toss();
                    }
                    isOpened = true;
                    PlayAnimation("Open");
                    AudioManager.audioManager.PlaySFX("BoxOpen" + Util.Random.Next(1, 4).ToString());
                    contents.Clear();
                }
            }
            else
            {
                DungeonGame.ScreenManager.AddScreen(new StashScreen(player));
            }

            return false;
        }

        public bool CanItemActivate()
        {
            if (!isOpened)
                return true;
            else
                return false;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch, 2);
        }

        public Point OccupiedSlot { get; set; }


        public void Persist(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(isOpened);
            writer.Write((Int32)type);
            writer.Write((Int32)contents.Count);
            for (int i = 0; i < contents.Count; i++)
            {
                contents[i].Persist(writer);
            }
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            Position = reader.ReadVector2();
            isOpened = reader.ReadBoolean();
            type = (ChestType)reader.ReadInt32();

            contents.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                contents.Add(ItemSprite.LoadItem(reader, dataVersion));
            }

            Initailize();
    
            return true;
        }


        public bool IsStash
        {
            get { return type == ChestType.stash; }
        }
    }
}
