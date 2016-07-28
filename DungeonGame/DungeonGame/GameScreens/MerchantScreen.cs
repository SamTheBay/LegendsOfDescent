using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    class ItemSlot
    {
        public bool PlayerOwned = true;
        public int X = 0;
        public int Y = 0;

        public ItemSlot()
        {
        }

        public ItemSlot(int X, int Y, bool PlayerOwned)
        {
            this.X = X;
            this.Y = Y;
            this.PlayerOwned = PlayerOwned;
        }

        public Point ToPoint()
        {
            return new Point(X, Y);
        }
    }

    class MerchantScreen : PopUpScreen
    {
        static int boarderWidth = 6;
        PlayerSprite player;
        MerchantSprite merchant;
        Texture2D blankSlot;
        Vector2 inventorySlotsCorner = new Vector2();
        Vector2 merchantSlotsCorner = new Vector2();
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



        public MerchantScreen(PlayerSprite player, MerchantSprite merchant)
            : base()
        {
            this.merchant = merchant;
            CloseOnTapOutOfDimension = true;
            this.player = player;
            blankSlot = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/blankSlot");
            goldTex = InternalContentManager.GetTexture("ItemIcons");
            EnabledGestures = GestureType.DragComplete | GestureType.FreeDrag | GestureType.Tap;
            GameplayScreen.Instance.Pause();

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + ((DungeonGame.ScreenSize.Height - 720) / 2), DungeonGame.ScreenSize.Width - boarderWidth * 2, 720 - boarderWidth * 2);
                goldVec = new Vector2(50, dimension.Height - 75);

                helpButton = new HelpButton(HelpScreens.Merchants, new Vector2(WindowCorner.X + 400, dimension.Height + WindowCorner.Y - 65));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                inventorySlotsCorner.Y = 370 + WindowCorner.Y;

                inventory = merchant.Inventory;
                merchantSlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                merchantSlotsCorner.Y = 70 + WindowCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                equipmentSlotsCorner.Y = 370 + WindowCorner.Y - blankSlot.Height;

                titleVector = new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35);
            }
            else
            {
                if (DungeonGame.pcMode)
                    dimension = new Rectangle((DungeonGame.ScreenSize.Width - 800) / 2 + boarderWidth, (DungeonGame.ScreenSize.Height - 400) / 2 + DungeonGame.ScreenSize.Y + boarderWidth, 800 - boarderWidth * 2, 400 - boarderWidth * 2);
                else
                    dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + 80, DungeonGame.ScreenSize.Width - boarderWidth * 2, 400 - boarderWidth * 2);
                goldVec = new Vector2(70, dimension.Height - 75);

                helpButton = new HelpButton(HelpScreens.Merchants, new Vector2(WindowCorner.X + 10, dimension.Height + WindowCorner.Y - 65));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                inventorySlotsCorner.Y = blankSlot.Height + 30 + WindowCorner.Y;

                inventory = merchant.Inventory;
                merchantSlotsCorner.X = 20 + WindowCorner.X;
                merchantSlotsCorner.Y = inventorySlotsCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                equipmentSlotsCorner.Y = 30 + WindowCorner.Y;

                titleVector = new Vector2(merchantSlotsCorner.X + blankSlot.Width * 2.5f, WindowCorner.Y + 50);
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
                    if (selectedItem.IsPlayerOwned)
                    {
                        if (!selectedItem.CannotBeSoldOrDestroyed)
                        {
                            descriptionScreen.AddSpace(15);
                            descriptionScreen.AddButton("Sell");
                        }
                    }
                    else if (player.Gold >= selectedItem.Value)
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Buy");

                        if (selectedItem.Name.Contains("Health Potion") || selectedItem.Name.Contains("Mana Potion") || selectedItem.Name.Contains("Warp Scroll"))
                        {
                            // add a buy 5 button for these
                            descriptionScreen.AddSpace(10);
                            descriptionScreen.AddButton("Buy 5");
                        }
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
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, merchant.Name, titleVector, Color.Red);

            // draw inventory background
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, true);
            DrawInventory(spriteBatch, merchant.Inventory, merchantSlotsCorner, true);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, true);

            // draw particles
            spriteBatch.End();
            ParticleSystem.Draw(spriteBatch, gameTime, this, false);
            spriteBatch.Begin();

            // draw icons
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, false);
            DrawInventory(spriteBatch, merchant.Inventory, merchantSlotsCorner, false);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, false);

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

            found = GetSlotAtLocation(merchant.Inventory, dragPosition - merchantSlotsCorner, ref item, ref slot);
            if (found)
            {
                slot.PlayerOwned = false;
            }
            else
            {
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
                }
            }

            return found;
        }


        public bool GetSlotAtLocation(ItemSprite[,] inventory, Vector2 dragPosition, ref ItemSprite item, ref ItemSlot slot)
        {
            bool found = false;

            // check merchant
            if (dragPosition.X > 0 && dragPosition.Y > 0 && dragPosition.X < blankSlot.Width * inventory.GetLength(0) && dragPosition.Y < blankSlot.Height * inventory.GetLength(1))
            {
                slot.X = (int)(dragPosition.X / blankSlot.Width);
                slot.Y = (int)(dragPosition.Y / blankSlot.Height);
                item = inventory[slot.X, slot.Y];
                found = true;
            }

            return found;
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
                if (currentlyDragged.CannotBeSoldOrDestroyed)
                {
                    GameplayScreen.Instance.Pause();
                    ScreenManager.Message("", "You cannot sell this item.");
                    return;
                }

                // sell item
                player.AddGold(currentlyDragged.Value);
                SaveGameManager.CurrentPlayer.QuestLog.ItemSold(currentlyDragged);
                player.SetItem(null, slot1.ToPoint());
                merchant.AddItem(currentlyDragged);
                AudioManager.audioManager.PlaySFX("Gold" + Util.Random.Next(1, 4).ToString());
            }
            else if (slot1.PlayerOwned == false && slot2.PlayerOwned == true)
            {
                // buy item
                int cost = currentlyDragged.Value;
                if (player.Gold >= cost)
                {
                    if (currentlyDragged.Name.Contains("Health Potion") ||
                        currentlyDragged.Name.Contains("Mana Potion") ||
                        currentlyDragged.Name.Contains("Warp Scroll"))
                    {
                        bool result = player.AddItem(currentlyDragged.CopyItem());
                        if (result)
                        {
                            player.AddGold(-1 * cost);
                            AudioManager.audioManager.PlaySFX("Gold" + Util.Random.Next(1, 4).ToString());
                        }
                    }
                    else
                    {
                        bool result = player.AddItem(currentlyDragged);
                        if (result)
                        {
                            player.AddGold(-1 * cost);
                            merchant.Inventory[slot1.X, slot1.Y] = null;
                            AudioManager.audioManager.PlaySFX("Gold" + Util.Random.Next(1, 4).ToString());
                        }
                    }
                }
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
                else if (!selectedItem.IsPlayerOwned && descriptionScreen.ButtonPressed(0))
                {
                    currentlyDragged = selectedItem;
                    SwapItems(new ItemSlot(selectedItem.Slot.X, selectedItem.Slot.Y, false), new ItemSlot(0, 0, true));
                    currentlyDragged = null;
                }
                else if (!selectedItem.IsPlayerOwned && descriptionScreen.ButtonPressed(1))
                {
                    currentlyDragged = selectedItem;
                    for (int i = 0; i < 5; i++)
                    {
                        SwapItems(new ItemSlot(selectedItem.Slot.X, selectedItem.Slot.Y, false), new ItemSlot(0, 0, true));
                    }
                    currentlyDragged = null;
                }
                selectedItem = null;
            }

            if (particleTimer.Update(gameTime))
            {
                UpdateInventoryParticles(player.Inventory, inventorySlotsCorner);
                UpdateInventoryParticles(merchant.Inventory, merchantSlotsCorner);
                UpdateInventoryParticles(player.EquippedItems, equipmentSlotsCorner);
            }

            ParticleSystem.Update(gameTime, this);
        }

    }
}
