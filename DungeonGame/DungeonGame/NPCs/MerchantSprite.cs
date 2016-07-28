using System;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum MerchantType
    {
        General,
        Equipable,
        Potion,
        Rogue,
        Warrior,
        Mage,
        Num
    }


    public class MerchantSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {

        ItemSprite[,] items;
        DateTime lastRefreshTime = DateTime.Now;
        int refreshMinInterval = 5;
        MerchantType type;

        public MerchantSprite(Vector2 nPosition, PlayerSprite player, MerchantType type)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = "Merchant";
            buttonText[0] = "Show me";
            buttonText[1] = "Not interested";
            hasDialogueAction = true;
            items = new ItemSprite[5, 3];
            this.type = type;
            centeredReduce = 60;

            SetMerchantType();
            GenerateInventory();
            
            AddAnimationSet("Idle", 1, 0, 100, false);

            InitializeTextures();
            PlayAnimation("IdleRight");

            Activate();
        }


        public void GenerateInventory()
        {
            // clear out old items
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    items[i,j] = null;
                }
            }

            if (type == MerchantType.General)
            {
                AddItem(ItemManager.Instance.GetItem("Health Potion", player.DungeonLevelsGenerated));
                AddItem(ItemManager.Instance.GetItem("Mana Potion", player.DungeonLevelsGenerated));
                AddItem(ItemManager.Instance.GetItem("Warp Scroll", player.DungeonLevelsGenerated));

                for (int i = 0; i < 10; i++)
                {
                    AddItem(ItemManager.Instance.GetItemForDrop(player.DungeonLevelsGenerated, DropType.Merchant, MerchantType.Equipable));
                }
            }
            else
            {
                for (int i = 0; i < 12; i++)
                {
                    AddItem(ItemManager.Instance.GetItemForDrop(player.DungeonLevelsGenerated, DropType.Merchant, type));
                }
            }
        }


        public void SetMerchantType()
        {
            ResetTextures();

            if (type == MerchantType.General)
            {
                text = "Everything can be acquired for a price. Have a look at my goods, I am sure I have something you need.";
                AddTexture("NPCCommonRed", new Point(128, 128), 1);
            }
            else if (type == MerchantType.Potion)
            {
                text = "It's amazing how you kind find everything in the mushroom forest that you need for lots of great potions. Want to have a look and my brews?";
                AddTexture("NPCMageYellow", new Point(128, 128), 1);
            }
            else if (type == MerchantType.Warrior)
            {
                text = "No one sells stronger, sharper, heavier equipment than me! Would you like to see my wares?";
                AddTexture("NPCCommonBlue", new Point(128, 128), 1);
            }
            else if (type == MerchantType.Rogue)
            {
                text = "Want some incredible items that won't slow you down? Your enemy won't even know you are there before you kill him with these. Have a look.";
                AddTexture("NPCCommonGray2", new Point(128, 128), 1);
            }
            else if (type == MerchantType.Mage)
            {
                text = "Who needs plate armor when you can burn your enemies to dust with fireballs? If you agree then I bet I have something for you.";
                AddTexture("NPCMageGreen", new Point(128, 128), 1);
            }
        }


        public override void Refresh()
        {
            base.Refresh();

            TimeSpan difference = DateTime.Now - lastRefreshTime;
            if (Math.Abs(difference.TotalMinutes) > refreshMinInterval)
            {
                GenerateInventory();
                lastRefreshTime = DateTime.Now;
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lastDirection = GetDirectionFromVector(SaveGameManager.CurrentPlayer.CenteredPosition - CenteredPosition);
            PlayAnimation("Idle", lastDirection);
        }



        public override void DialogueAction(int buttonSelected)
        {
            if (buttonSelected == 0)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new MerchantScreen(player, this));
                InputManager.ClearInputForPeriod(500);
            }
        }


        public bool AddItem(ItemSprite item)
        {
            for (int x = 0; x < items.GetLength(0); x++)
            {
                for (int y = 0; y < items.GetLength(1); y++)
                {
                    if (items[x, y] == null)
                    {
                        item.SetSlot(x, y);
                        items[x, y] = item;
                        item.Owner = null;
                        return true;
                    }
                }
            }

            return false;
        }


        public ItemSprite[,] Inventory
        {
            get { return items; }
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write((UInt32)maxHealthBase);
            writer.Write((UInt32)maxManaBase);
            writer.Write((UInt32)level);
            writer.Write(Position);

            // count the number of items we have
            int itemNum = 0;
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i, j] != null)
                    {
                        itemNum++;
                    }
                }
            }

            // write item info
            writer.Write((Int32)itemNum);
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i, j] != null)
                    {
                        items[i, j].Persist(writer);
                    }
                }
            }

            writer.Write(lastRefreshTime);

            writer.Write((Int32)type);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            maxHealthBase = reader.ReadInt32();
            maxManaBase = reader.ReadInt32();
            level = reader.ReadInt32();
            Position = reader.ReadVector2();

            // clear out old items on this player
            items = new ItemSprite[5, 3];

            // read items
            int itemNum = reader.ReadInt32();
            for (int i = 0; i < itemNum; i++)
            {
                ItemSprite item = ItemSprite.LoadItem(reader, dataVersion);
                if (item.Slot.X < 5 && item.Slot.Y < 3)
                    items[item.Slot.X, item.Slot.Y] = item;
            }

            if (dataVersion >= 7)
            {
                lastRefreshTime = reader.ReadDateTime();

                type = (MerchantType)reader.ReadInt32();
                SetMerchantType();
                InitializeTextures();
            }

            return true;
        }



        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            // draw the merchants name
            nameSprite.Color = friendlyNPCColor;
            nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, 0) + GameplayScreen.viewportCorner;
            nameSprite.Draw(spriteBatch);

        }



    }
}
