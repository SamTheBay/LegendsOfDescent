using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE
using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{
    class BlacksmithScreen : PopUpScreen
    {
        static int boarderWidth = 6;
        PlayerSprite player;
        BlacksmithSprite blacksmith;
        Texture2D blankSlot;
        Vector2 inventorySlotsCorner = new Vector2();
        Vector2 upgradeSlotsCorner = new Vector2();
        Vector2 equipmentSlotsCorner = new Vector2();
        ItemSprite currentlyDragged = null;
        ItemSprite currentlyDropped = null;
        Vector2 currentlyDraggedLocation = new Vector2();
        ItemSlot dragSlot = new ItemSlot();
        ItemSlot dropSlot = new ItemSlot();
        ItemSprite selectedItem = null;
        Vector2 tapPosition = new Vector2();
        ItemSlot tapSlot = new ItemSlot();
        Texture2D goldTex;
        Vector2 goldVec;
        Rectangle goldSourceRect = new Rectangle(0, 48, 48, 48);
        HelpButton helpButton;
        Vector2 titleVector;
        DescriptionScreen descriptionScreen = new DescriptionScreen();
        Timer particleTimer = new Timer(200, TimerState.Running, TimerType.Auto);
        ItemSprite[,] upgradeSlot = new ItemSprite[1, 1];
        List<String> upgradeDescription = new List<String>();
        int upgradeAmount = 0;
        int upgradeCost = 0;
        bool upgradeable = false;
        Property upgradeProperty;
        Vector2 descriptionLocation;


        public BlacksmithScreen(PlayerSprite player, BlacksmithSprite blacksmith)
            : base()
        {
            this.blacksmith = blacksmith;
            this.player = player;
            blankSlot = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/blankSlot");
            goldTex = InternalContentManager.GetTexture("ItemIcons");
            CloseOnTapOutOfDimension = true;

            EnabledGestures = GestureType.DragComplete | GestureType.FreeDrag | GestureType.Tap;

            GameplayScreen.Instance.Pause();

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + ((DungeonGame.ScreenSize.Height - 720) / 2), DungeonGame.ScreenSize.Width - boarderWidth * 2, 720 - boarderWidth * 2);
                goldVec = new Vector2(50, dimension.Height - 75);

                helpButton = new HelpButton(HelpScreens.Blacksmith, new Vector2(WindowCorner.X + 400, dimension.Height + WindowCorner.Y - 65));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                inventorySlotsCorner.Y = 370 + WindowCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                equipmentSlotsCorner.Y = 370 + WindowCorner.Y - blankSlot.Height;

                upgradeSlotsCorner.X = 70 + WindowCorner.X;
                upgradeSlotsCorner.Y = WindowCorner.Y + 140;

                titleVector = new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35);

                descriptionLocation = new Vector2(WindowCorner.X + dimension.Width - 270, WindowCorner.Y + 90);

                Rectangle locaiton = new Rectangle((int)WindowCorner.X + dimension.Width - 270, (int)WindowCorner.Y + 200, 200, 50);
                MenuEntry entry = new MenuEntry("Enhance", locaiton);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.IsActive = false;
                MenuEntries.Add(entry);
            }
            else
            {
                if (DungeonGame.pcMode)
                    dimension = new Rectangle((DungeonGame.ScreenSize.Width - 800) / 2 + boarderWidth, (DungeonGame.ScreenSize.Height - 400) / 2 + DungeonGame.ScreenSize.Y + boarderWidth, 800 - boarderWidth * 2, 400 - boarderWidth * 2);
                else
                    dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + 80, DungeonGame.ScreenSize.Width - boarderWidth * 2, 400 - boarderWidth * 2);
                goldVec = new Vector2(70, dimension.Height - 75);

                helpButton = new HelpButton(HelpScreens.Blacksmith, new Vector2(WindowCorner.X + 10, dimension.Height + WindowCorner.Y - 65));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                inventorySlotsCorner.Y = blankSlot.Height + 30 + WindowCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                equipmentSlotsCorner.Y = 30 + WindowCorner.Y;

                upgradeSlotsCorner.X = WindowCorner.X + 280;
                upgradeSlotsCorner.Y = WindowCorner.Y + dimension.Height / 2 - blankSlot.Height / 2;

                titleVector = new Vector2(WindowCorner.X + 20 + blankSlot.Width * 2.5f, WindowCorner.Y + 50);

                descriptionLocation = new Vector2(WindowCorner.X + 40, inventorySlotsCorner.Y + 20);

                Rectangle locaiton = new Rectangle((int)WindowCorner.X + 40, (int)descriptionLocation.Y + 110, 200, 50);
                MenuEntry entry = new MenuEntry("Enhance", locaiton);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.IsActive = false;
                MenuEntries.Add(entry);
            }
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (upgradeable && upgradeSlot[0, 0] != null && player.Gold >= upgradeCost)
            {
                ItemSprite item = upgradeSlot[0,0];
                item.AdjustModifier(upgradeProperty, upgradeAmount);
                player.AddGold(upgradeCost * -1);
                SetUpgradeDetails();
                item.Value = item.Value + upgradeCost;
            }
        }


        public void SetButtonActiveStatus()
        {
            MenuEntry entry = MenuEntries[0];
            if (upgradeSlot[0,0] != null && 
                player.Gold > upgradeCost && 
                upgradeSlot[0,0] is EquipableItem && 
                (upgradeSlot[0,0] as EquipableItem).EquipSlot != EquipSlot.Augment)
            {
                entry.IsActive = true;
            }
            else
            {
                entry.IsActive = false;
            }
        }


        public override void OnRemoval()
        {
            base.OnRemoval();

            GameplayScreen.Instance.UnPause();
            ParticleSystem.ClearParticles(this);
        }


        public override void HandleInput()
        {
            base.HandleInput();

            // handle drag and drop
            bool isFinished = false;
            bool isStarted = false;
            bool isDragging = InputManager.GetDrag(ref currentlyDraggedLocation, ref isStarted, ref isFinished);
            if (isStarted)
            {
                // check if we are on a tile and pick it up if so
                GetSlotAtLocation(currentlyDraggedLocation, ref currentlyDragged, ref dragSlot);
                if (dragSlot.PlayerOwned == false)
                {
                    currentlyDragged = null;
                }
            }
            else if (isFinished && currentlyDragged != null)
            {
                bool isSlot = GetSlotAtLocation(currentlyDraggedLocation, ref currentlyDropped, ref dropSlot);
                if (isSlot)
                {
                    SwapItems(dragSlot, dropSlot);
                }

                // no longer dragging
                currentlyDragged = null;
                currentlyDropped = null;
            }

            bool isTapped = InputManager.GetTapPoint(ref tapPosition);
            if (isTapped)
            {
                GetSlotAtLocation(tapPosition, ref selectedItem, ref tapSlot);
                if (selectedItem != null)
                {
                    descriptionScreen.Initialize(selectedItem);
                    if (tapSlot.PlayerOwned)
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Select");
                    }
                    descriptionScreen.SetFinalize();
                    AddNextScreen(descriptionScreen);
                }
            }

            helpButton.HandleInput();

            if (InputManager.GetTouchCollection().Count == 0 && currentlyDragged != null)
            {
                // correct floating items
                currentlyDragged = null;
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // draw boarder
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();
            DrawBorder(new Rectangle((int)WindowCorner.X - boarderWidth, (int)WindowCorner.Y - boarderWidth, dimension.Width + boarderWidth * 2, dimension.Height + boarderWidth * 2),
                InternalContentManager.GetTexture("Blank"), Color.Gray, boarderWidth, Color.Black, spriteBatch);

            // draw title
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, blacksmith.Name, titleVector, Color.Red);

            // draw inventory background
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, true);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, true);
            DrawInventory(spriteBatch, upgradeSlot, upgradeSlotsCorner, true);

            // draw particles
            spriteBatch.End();
            ParticleSystem.Draw(spriteBatch, gameTime, this, false);
            spriteBatch.Begin();

            // draw icons
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, false);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, false);
            DrawInventory(spriteBatch, upgradeSlot, upgradeSlotsCorner, false);

            for (int i = 0; i < player.EquippedItems.GetLength(0); i++)
            {
                if (player.EquippedItems[i, 0] == null || player.EquippedItems[i, 0].Equals(currentlyDragged))
                {
                    InventoryScreen.DrawEmptyIcon(spriteBatch, equipmentSlotsCorner + new Vector2(blankSlot.Width * i + blankSlot.Width / 2 - 24, blankSlot.Height / 2 - 24), (EquipSlot)i);
                }
            }

            // draw gold
            spriteBatch.Draw(goldTex, goldVec + WindowCorner, goldSourceRect, Color.White);
            spriteBatch.DrawString(Fonts.HeaderFont, player.Gold.ToString(), goldVec + WindowCorner + new Vector2(50, 5), Color.White);

            helpButton.Draw(spriteBatch);

            // draw the details
            DrawBorder(new Rectangle((int)descriptionLocation.X - 20, (int)descriptionLocation.Y - 20, 240, 200),
                InternalContentManager.GetTexture("Blank"), Color.Gray, 3, Color.Black, spriteBatch);
            if (upgradeSlot[0, 0] != null)
            {
                Vector2 location = descriptionLocation;
                for (int i = 0; i < upgradeDescription.Count; i++)
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, upgradeDescription[i], location, Color.White);
                    location.Y += 20;
                }
            }

            // draw the currently dragged item
            if (currentlyDragged != null)
            {
                currentlyDragged.DrawIcon(spriteBatch, currentlyDraggedLocation, Vector2.Zero);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }




        public void DrawInventory(SpriteBatch spriteBatch, ItemSprite[,] inventory, Vector2 slotCorner, bool background)
        {
            float yLoc = slotCorner.Y;
            for (int x = 0; x < inventory.GetLength(0); x++)
            {
                slotCorner.Y = yLoc;
                for (int y = 0; y < inventory.GetLength(1); y++)
                {
                    if (background)
                    {
                        spriteBatch.Draw(blankSlot, slotCorner, Color.White);
                    }
                    else
                    {
                        if (inventory[x, y] != null && !inventory[x, y].Equals(currentlyDragged))
                        {
                            inventory[x, y].DrawIcon(spriteBatch, slotCorner, blankSlot.GetDimensions());
                        }
                    }

                    slotCorner.Y += blankSlot.Height;
                }
                slotCorner.X += blankSlot.Width;
            }
        }


        public void UpdateInventoryParticles(ItemSprite[,] inventory, Vector2 slotCorner)
        {
            float yLoc = slotCorner.Y;
            for (int x = 0; x < inventory.GetLength(0); x++)
            {
                slotCorner.Y = yLoc;
                for (int y = 0; y < inventory.GetLength(1); y++)
                {
                    if (inventory[x, y] != null && !inventory[x, y].Equals(currentlyDragged))
                    {
                        inventory[x, y].AddParticles(this, slotCorner + new Vector2(blankSlot.Width / 2, blankSlot.Height / 2));
                    }

                    slotCorner.Y += blankSlot.Height;
                }
                slotCorner.X += blankSlot.Width;
            }
        }




        public bool GetSlotAtLocation(Vector2 dragPosition, ref ItemSprite item, ref ItemSlot slot)
        {
            bool found = false;

            found = GetSlotAtLocation(player.Inventory, dragPosition - inventorySlotsCorner, ref item, ref slot);
            if (found)
            {
                slot.PlayerOwned = true;
            }
            else
            {
                found = GetSlotAtLocation(player.EquippedItems, dragPosition - equipmentSlotsCorner, ref item, ref slot);
                if (found)
                {
                    slot.PlayerOwned = true;
                    slot.Y = slot.X;
                    slot.X = -1;
                }
                else
                {
                    found = GetSlotAtLocation(upgradeSlot, dragPosition - upgradeSlotsCorner, ref item, ref slot);
                    if (found)
                    {
                        slot.PlayerOwned = false;
                        slot.X = -1;
                        slot.Y = -1;
                    }
                }
            }

            return found;
        }


        public bool GetSlotAtLocation(ItemSprite[,] inventory, Vector2 dragPosition, ref ItemSprite item, ref ItemSlot slot)
        {
            bool found = false;

            if (dragPosition.X > 0 && dragPosition.Y > 0 && dragPosition.X < blankSlot.Width * inventory.GetLength(0) && dragPosition.Y < blankSlot.Height * inventory.GetLength(1))
            {
                slot.X = (int)(dragPosition.X / blankSlot.Width);
                slot.Y = (int)(dragPosition.Y / blankSlot.Height);
                item = inventory[slot.X, slot.Y];
                found = true;
            }

            return found;
        }



        public void SetUpgradeDetails()
        {
            ItemSprite item = upgradeSlot[0, 0];
            upgradeDescription.Clear();
            upgradeable = false;

            if (item is EquipableItem)
            {
                EquipableItem ei = (EquipableItem)item;
                if (ei.EquipSlot == EquipSlot.Hand || ei.EquipSlot == EquipSlot.TwoHand || ei.EquipSlot == EquipSlot.MainHand)
                {
                    upgradeAmount = Math.Max(1, (int)(item.GetPropertyValue(Property.Damage) * .03f)); 
                    upgradeCost = BalanceManager.GetBaseItemValue(ei.Level) * (5 + (ei.UpgradeCount * 3));
                    if (ei.ItemClass == ItemClass.Magic)
                    {
                        upgradeCost *= 2;
                    }
                    else if (ei.ItemClass == ItemClass.Rare)
                    {
                        upgradeCost *= 3;
                    }
                    else if (ei.ItemClass == ItemClass.Heroic)
                    {
                        upgradeCost *= 4;
                    }
                    else if (ei.ItemClass == ItemClass.Legendary)
                    {
                        upgradeCost *= 5;
                    }
                    upgradeProperty = Property.Damage;
                    upgradeDescription.Add("Damage +" + upgradeAmount.ToString());
                    upgradeDescription.Add("Cost: " + upgradeCost.ToString());
                    upgradeable = true;
                }
                else if (ei.EquipSlot == EquipSlot.Chest || ei.EquipSlot == EquipSlot.Head || ei.EquipSlot == EquipSlot.Legs || ei.EquipSlot == EquipSlot.OffHand)
                {
                    upgradeAmount = Math.Max(1, (int)(item.GetPropertyValue(Property.Armor) * .03f)); 
                    upgradeCost = BalanceManager.GetBaseItemValue(ei.Level) * (5 + (ei.UpgradeCount * 3));
                    if (ei.ItemClass == ItemClass.Magic)
                    {
                        upgradeCost *= 2;
                    }
                    else if (ei.ItemClass == ItemClass.Rare)
                    {
                        upgradeCost *= 3;
                    }
                    else if (ei.ItemClass == ItemClass.Heroic)
                    {
                        upgradeCost *= 4;
                    }
                    else if (ei.ItemClass == ItemClass.Legendary)
                    {
                        upgradeCost *= 5;
                    }
                    upgradeProperty = Property.Armor;
                    upgradeDescription.Add("Armor +" + upgradeAmount.ToString());
                    upgradeDescription.Add("Cost: " + upgradeCost.ToString());
                    upgradeable = true;
                }
            }
        }



        // we make the assumption that slot1 will never be null
        public void SwapItems(ItemSlot slot1, ItemSlot slot2)
        {
            // player item swap
            if (slot1.PlayerOwned == true && slot2.PlayerOwned == true)
            {
                player.SwapItems(slot1.ToPoint(), slot2.ToPoint());
            }
            else if (slot1.PlayerOwned == true && slot2.PlayerOwned == false)
            {
                // put this item into the augment spot
                ItemSprite item = player.GetItem(slot1.ToPoint());
                upgradeSlot[0, 0] = item;
                SetUpgradeDetails();

            }
        }


        


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!descriptionScreen.ActionHandled && descriptionScreen.IsClosed)
            {
                descriptionScreen.ActionHandled = true;
                if (selectedItem.IsPlayerOwned && descriptionScreen.ButtonPressed(0))
                {
                    currentlyDragged = selectedItem;
                    SwapItems(new ItemSlot(selectedItem.Slot.X, selectedItem.Slot.Y, true), new ItemSlot(0, 0, false));
                    currentlyDragged = null;
                }
                selectedItem = null;
            }

            if (particleTimer.Update(gameTime))
            {
                UpdateInventoryParticles(player.Inventory, inventorySlotsCorner);
                UpdateInventoryParticles(player.EquippedItems, equipmentSlotsCorner);
                UpdateInventoryParticles(upgradeSlot, upgradeSlotsCorner);
            }

            ParticleSystem.Update(gameTime, this);

            SetButtonActiveStatus();
        }



    }
}
