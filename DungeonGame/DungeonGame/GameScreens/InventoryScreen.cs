using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    class InventoryScreen : PopUpScreen
    {
        PlayerSprite player;
        Texture2D blankSlot;
        Texture2D trashSlotTexture;
        Texture2D dropSlotTexture;
        Texture2D swapTexture;
        Texture2D swapPressTexture;
        Texture2D townTexture;
        Texture2D townPressTexture;
        Vector2 inventorySlotsCorner = new Vector2();
        Vector2 inventorySlotsCornerBase;
        ItemSprite currentlyDragged = null;
        ItemSprite currentlyDropped = null;
        Vector2 currentlyDraggedLocation = new Vector2();
        Point dragSlot = new Point();
        Point dropSlot = new Point();
        Vector2[] equipSlotLocations;
        FloatRectangle trashSlot;
        FloatRectangle dropItemSlot;
        FloatRectangle swapSlot;
        FloatRectangle townSlot;
        PopUpScreen owner;
        ItemSprite selectedItem = null;
        Vector2 tapPosition = new Vector2();
        Point tapSlot = new Point();
        Texture2D goldTex;
        Vector2 goldVec;
        Rectangle goldSourceRect = new Rectangle(0, 48, 48, 48);
        HelpButton helpButton;
        DescriptionScreen descriptionScreen = new DescriptionScreen();
        Timer particleTimer = new Timer(300, TimerState.Running, TimerType.Auto);
        bool draggingSwap = false;
        bool draggingTown = false;


        public InventoryScreen(PopUpScreen owner, PlayerSprite player, Rectangle dimension)
            : base()
        {
            this.dimension = dimension;
            IsPopup = true;
            this.owner = owner;
            this.player = player;
            blankSlot = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/blankSlot");
            trashSlotTexture = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/trashSlot");
            dropSlotTexture = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/dropSlot");
            swapTexture = InternalContentManager.GetTexture("Swap");
            swapPressTexture = InternalContentManager.GetTexture("SwapSelected");
            townTexture = InternalContentManager.GetTexture("TownButton");
            townPressTexture = InternalContentManager.GetTexture("TownSelected");
            goldTex = InternalContentManager.GetTexture("ItemIcons");

            helpButton = new HelpButton(HelpScreens.Inventory, Vector2.Zero);
            UpdateOrientation();
        }



        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(0, 50);
                inventorySlotsCornerBase = new Vector2(((dimension.Width - (blankSlot.Width * player.Inventory.GetLength(0))) / 2) + WindowCorner.X, 230 + WindowCorner.Y);
                goldVec = new Vector2(50, dimension.Height - 125);

                const int virtOffset = 20;
                equipSlotLocations = new Vector2[ItemSprite.EquipSlotNum];
                equipSlotLocations[(int)EquipSlot.Head] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2, virtOffset);
                equipSlotLocations[(int)EquipSlot.Chest] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2, virtOffset + blankSlot.Height);
                equipSlotLocations[(int)EquipSlot.Legs] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2, virtOffset + blankSlot.Height * 2);
                equipSlotLocations[(int)EquipSlot.Augment] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2 + blankSlot.Width, virtOffset);
                equipSlotLocations[(int)EquipSlot.OffHand] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2 + blankSlot.Width, virtOffset + blankSlot.Height);
                equipSlotLocations[(int)EquipSlot.Hand] = new Vector2(dimension.Width / 2 - blankSlot.Width / 2 - blankSlot.Width, virtOffset + blankSlot.Height);

                trashSlot = new FloatRectangle(
                    dimension.Width - virtOffset * 2 - blankSlot.Width,
                    dimension.Height - 60 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                dropItemSlot = new FloatRectangle(
                    dimension.Width - virtOffset * 2 - blankSlot.Width * 2 - 10,
                    dimension.Height - 60 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                swapSlot = new FloatRectangle(
                    dimension.Width / 2 - blankSlot.Width / 2 - blankSlot.Width * 5 / 2,
                    virtOffset + blankSlot.Height + blankSlot.Height / 2 - swapTexture.Height / 2,
                    blankSlot.Width,
                    blankSlot.Height);

                townSlot = new FloatRectangle(
                    dimension.Width / 2 - blankSlot.Width / 2 - blankSlot.Width * 5 / 2,
                    virtOffset + blankSlot.Height * 2 + blankSlot.Height / 2 - townTexture.Height / 2,
                    blankSlot.Width,
                    blankSlot.Height);

                helpButton.Location = new Vector2(WindowCorner.X + 400, WindowCorner.Y + 20);
            }
            else
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, borderWidth, 800 - 80 - borderWidth * 2 - 50, 400 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(60, 0);

                inventorySlotsCornerBase = new Vector2((dimension.Width - (blankSlot.Width * player.Inventory.GetLength(0)) - 20) + WindowCorner.X,
                     WindowCorner.Y + 20);
                goldVec = new Vector2(260, dimension.Height - 80);

                const int virtOffset = 52, horizOffset = 20;
                equipSlotLocations = new Vector2[ItemSprite.EquipSlotNum];
                equipSlotLocations[(int)EquipSlot.Head] = new Vector2(horizOffset + blankSlot.Width, virtOffset);
                equipSlotLocations[(int)EquipSlot.Chest] = new Vector2(horizOffset + blankSlot.Width, virtOffset + blankSlot.Height);
                equipSlotLocations[(int)EquipSlot.Legs] = new Vector2(horizOffset + blankSlot.Width, virtOffset + blankSlot.Height * 2);
                equipSlotLocations[(int)EquipSlot.Augment] = new Vector2(horizOffset + blankSlot.Width * 2, virtOffset);
                equipSlotLocations[(int)EquipSlot.OffHand] = new Vector2(horizOffset + blankSlot.Width * 2, virtOffset + blankSlot.Height);
                equipSlotLocations[(int)EquipSlot.Hand] = new Vector2(horizOffset, virtOffset + blankSlot.Height);

                trashSlot = new FloatRectangle(
                    dimension.Width - 20 - blankSlot.Width,
                    dimension.Height + borderWidth - 20 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                dropItemSlot = new FloatRectangle(
                    dimension.Width - 20 - blankSlot.Width * 2 - 10,
                    dimension.Height + borderWidth - 20 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                swapSlot = new FloatRectangle(
                    100,
                    dimension.Height + borderWidth - 20 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                townSlot = new FloatRectangle(
                    160,
                    dimension.Height + borderWidth - 20 - blankSlot.Height,
                    blankSlot.Width,
                    blankSlot.Height);

                helpButton.Location = new Vector2(WindowCorner.X + 40, WindowCorner.Y + dimension.Height - 78);
            }


            EnabledGestures = GestureType.DragComplete | GestureType.FreeDrag | GestureType.Tap;
        }


        public override void HandleInput()
        {
            base.HandleInput();

            // detect drag and drop of items
            ItemSprite[,] inventory = player.Inventory;
            inventorySlotsCorner = inventorySlotsCornerBase;


            // handle drag and drop
            bool isFinished = false;
            bool isStarted = false;
            bool isDragging = InputManager.GetDrag(ref currentlyDraggedLocation, ref isStarted, ref isFinished);
            if (isStarted)
            {
                // check if we are on a tile and pick it up if so
                bool isSlot = GetSlotAtLocation(currentlyDraggedLocation, ref currentlyDragged, ref dragSlot);
                if (!isSlot)
                {
                    // check if we are dragging the swap macro
                    if (swapSlot.Contains(currentlyDraggedLocation - WindowCorner))
                    {
                        draggingSwap = true;
                    }
                    else if (townSlot.Contains(currentlyDraggedLocation - WindowCorner))
                    {
                        draggingTown = true;
                    }
                }
            }
            else if (isFinished && currentlyDragged != null)
            {
                bool isSlot = GetSlotAtLocation(currentlyDraggedLocation, ref currentlyDropped, ref dropSlot);
                if (isSlot)
                {
                    player.SwapItems(dragSlot, dropSlot);
                }
                else if (trashSlot.Contains(currentlyDraggedLocation - WindowCorner))
                {
                    var item = player.GetItem(dragSlot);
                    {
                        if (item != null && !item.CannotBeSoldOrDestroyed)
                        {
                            player.SetItem(null, dragSlot);
                        }
                    }
                }
                else if (dropItemSlot.Contains(currentlyDraggedLocation - WindowCorner))
                {
                    currentlyDragged.Position = player.Position;
                    player.SetItem(null, dragSlot);
                    GameplayScreen.Instance.AddEnvItem(currentlyDragged);
                    currentlyDragged.Toss();
                }
                else if (currentlyDragged.IsSelectable())
                {
                    // check if the drop location is a button
                    int buttonIndex = 0;
                    bool isButton = GameplayScreen.ui.IsButtonLocation(currentlyDraggedLocation, ref buttonIndex);
                    if (isButton)
                    {
                        // if this is a weapon then equip it
                        if (currentlyDragged is EquipableItem)
                        {
                            EquipableItem equipableItem = (EquipableItem)currentlyDragged;
                            player.SwapItems(dragSlot, new Point(-1, (int)equipableItem.BasicEquipSlot));
                        }

                        player.SetMacroButton(currentlyDragged.GetMacroButton(), buttonIndex);
                    }
                }

                // no longer dragging
                currentlyDragged = null;
                currentlyDropped = null;
            }
            else if (isFinished && draggingSwap)
            {
                // check if the drop location is a button
                int buttonIndex = 0;
                bool isButton = GameplayScreen.ui.IsButtonLocation(currentlyDraggedLocation, ref buttonIndex);
                if (isButton)
                {
                    // set macro button to swap
                    MacroButton macro = new MacroButton(type: MacroButtonType.Swap, player: player);
                    macro.SetIcon("Swap", new Rectangle(0, 0, 48, 48));
                    player.SetMacroButton(macro, buttonIndex);
                }

                draggingSwap = false;
            }
            else if (isFinished && draggingTown)
            {
                // check if the drop location is a button
                int buttonIndex = 0;
                bool isButton = GameplayScreen.ui.IsButtonLocation(currentlyDraggedLocation, ref buttonIndex);
                if (isButton)
                {
                    // set macro button to swap
                    MacroButton macro = new MacroButton(type: MacroButtonType.TownPortal, player: player);
                    macro.SetIcon("TownButton", new Rectangle(0, 0, 48, 48));
                    player.SetMacroButton(macro, buttonIndex);
                }

                draggingTown = false;
            }

            bool isTapped = InputManager.GetTapPoint(ref tapPosition);
            if (isTapped)
            {
                GetSlotAtLocation(tapPosition, ref selectedItem, ref tapSlot);
                if (selectedItem != null)
                {
                    descriptionScreen.Initialize(selectedItem);
                    if (selectedItem is EquipableItem && selectedItem.Slot.X != -1)
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Equip");
                    }
                    else if (selectedItem is UsableItem)
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Use");
                    }
                    descriptionScreen.SetFinalize();
                    AddNextScreen(descriptionScreen);
                }
                else if (swapSlot.Contains(tapPosition - WindowCorner))
                {
                    player.SwapItem();
                }
                else if (townSlot.Contains(tapPosition - WindowCorner) && player.DungeonLevel != 0)
                {
                    player.ActivateTownPortal();
                    owner.ExitScreen();
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
            UpdateOrientation();

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw inventory slots
            ItemSprite[,] inventory = player.Inventory;
            inventorySlotsCorner.X = inventorySlotsCornerBase.X;
            for (int x = 0; x < inventory.GetLength(0); x++)
            {
                inventorySlotsCorner.Y = inventorySlotsCornerBase.Y;
                for (int y = 0; y < inventory.GetLength(1); y++)
                {
                    spriteBatch.Draw(blankSlot, inventorySlotsCorner, Color.White);
                    inventorySlotsCorner.Y += blankSlot.Height;
                }
                inventorySlotsCorner.X += blankSlot.Width;
            }

            EquipableItem[,] equiped = player.EquippedItems;
            for (int i = 0; i < equipSlotLocations.Length; i++)
            {
                spriteBatch.Draw(blankSlot, equipSlotLocations[i] + WindowCorner, Color.White);
            }


            // draw particles
            spriteBatch.End();
            ParticleSystem.Draw(spriteBatch, gameTime, this, false);
            spriteBatch.Begin();


            // draw items
            inventorySlotsCorner.X = inventorySlotsCornerBase.X;
            for (int x = 0; x < inventory.GetLength(0); x++)
            {
                inventorySlotsCorner.Y = inventorySlotsCornerBase.Y;
                for (int y = 0; y < inventory.GetLength(1); y++)
                {
                    if (inventory[x, y] != null && !inventory[x, y].Equals(currentlyDragged))
                    {
                        inventory[x, y].DrawIcon(spriteBatch, inventorySlotsCorner, blankSlot.GetDimensions());
                    }

                    inventorySlotsCorner.Y += blankSlot.Height;
                }
                inventorySlotsCorner.X += blankSlot.Width;
            }

            // special slots
            spriteBatch.Draw(trashSlotTexture, trashSlot.Corner + WindowCorner, Color.White);
            spriteBatch.Draw(dropSlotTexture, dropItemSlot.Corner + WindowCorner, Color.White);

            if (!InputManager.IsLocationPressed(new Rectangle((int)(swapSlot.Corner.X + WindowCorner.X), (int)(swapSlot.Corner.Y + WindowCorner.Y), 48, 48)))
            {
                spriteBatch.Draw(swapTexture, swapSlot.Corner + WindowCorner, Color.White);
            }
            else
            {
                spriteBatch.Draw(swapPressTexture, swapSlot.Corner + WindowCorner, Color.White);
            }

            if (player.DungeonLevel == 0)
            {
                spriteBatch.Draw(townTexture, townSlot.Corner + WindowCorner, Color.Gray);
            }
            else if (!InputManager.IsLocationPressed(new Rectangle((int)(townSlot.Corner.X + WindowCorner.X), (int)(townSlot.Corner.Y + WindowCorner.Y), 48, 48)))
            {
                spriteBatch.Draw(townTexture, townSlot.Corner + WindowCorner, Color.White);
            }
            else
            {
                spriteBatch.Draw(townPressTexture, townSlot.Corner + WindowCorner, Color.White);
            }

            for (int i = 0; i < equipSlotLocations.Length; i++)
            {
                if (equiped[i, 0] != null && !equiped[i, 0].Equals(currentlyDragged))
                {
                    equiped[i, 0].DrawIcon(spriteBatch, equipSlotLocations[i] + WindowCorner, blankSlot.GetDimensions());
                }
                else
                {
                    DrawEmptyIcon(spriteBatch, equipSlotLocations[i] + WindowCorner + new Vector2(blankSlot.Width / 2 - 24, blankSlot.Height / 2 - 24), (EquipSlot)i);
                }
            }


            // draw gold
            spriteBatch.Draw(goldTex, goldVec + WindowCorner, goldSourceRect, Color.White);
            spriteBatch.DrawString(Fonts.HeaderFont, player.Gold.ToString("N0"), goldVec + WindowCorner + new Vector2(50, 5), Color.White);

            helpButton.Draw(spriteBatch);

            // draw the currently dragged item
            if (currentlyDragged != null)
            {
                currentlyDragged.DrawIcon(spriteBatch, currentlyDraggedLocation, Vector2.Zero);
            }
            else if (draggingSwap)
            {
                spriteBatch.Draw(swapTexture, currentlyDraggedLocation - new Vector2(swapTexture.Width / 2, swapTexture.Height / 2), Color.White);
            }
            else if (draggingTown)
            {
                spriteBatch.Draw(townTexture, currentlyDraggedLocation - new Vector2(townTexture.Width / 2, townTexture.Height / 2), Color.White);
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }



        public static void DrawEmptyIcon(SpriteBatch spriteBatch, Vector2 location, EquipSlot slot)
        {
            if (slot == EquipSlot.Hand)
            {
                spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 9, 48 * 9, 48, 48), Color.White);
            }
            else if (slot == EquipSlot.Head)
            {
                spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 4, 48 * 9, 48, 48), Color.White);
            }
            else if (slot == EquipSlot.Chest)
            {
                spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 5, 48 * 9, 48, 48), Color.White);
            }
            else if (slot == EquipSlot.Legs)
            {
                spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 6, 48 * 9, 48, 48), Color.White);
            }
            else if (slot == EquipSlot.Augment)
            {
                spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 7, 48 * 9, 48, 48), Color.White);
            }
            else if (slot == EquipSlot.OffHand)
            {
                if (SaveGameManager.CurrentPlayer.EquippedItems[(int)EquipSlot.Hand, 0] != null && SaveGameManager.CurrentPlayer.EquippedItems[(int)EquipSlot.Hand, 0].EquipSlot == EquipSlot.TwoHand)
                {
                    spriteBatch.Draw(InternalContentManager.GetTexture("InUse"), location, new Rectangle(0, 0, 48, 48), Color.White);
                }
                else
                {
                    spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), location, new Rectangle(48 * 8, 48 * 9, 48, 48), Color.White);
                }
            }
        }



        public bool GetSlotAtLocation(Vector2 dragPosition, ref ItemSprite item, ref Point slot)
        {
            bool found = false;
            ItemSprite[,] inventory = player.Inventory;
            inventorySlotsCorner = inventorySlotsCornerBase;


            // check equipped
            for (int i = 0; i < equipSlotLocations.Length; i++)
            {
                if (dragPosition.X - WindowCorner.X > equipSlotLocations[i].X &&
                    dragPosition.X - WindowCorner.X < equipSlotLocations[i].X + blankSlot.Width &&
                    dragPosition.Y - WindowCorner.Y > equipSlotLocations[i].Y &&
                    dragPosition.Y - WindowCorner.Y < equipSlotLocations[i].Y + blankSlot.Height)
                {
                    slot.X = -1; // -1 means equipped
                    slot.Y = i;
                    item = player.EquippedItems[i,0];
                    found = true;
                }
            }


            if (!found)
            {
                // check inventory
                dragPosition.X -= inventorySlotsCornerBase.X;
                dragPosition.Y -= inventorySlotsCornerBase.Y;
                if (dragPosition.X > 0 && dragPosition.Y > 0 && dragPosition.X < blankSlot.Width * inventory.GetLength(0) && dragPosition.Y < blankSlot.Height * inventory.GetLength(1))
                {
                    slot.X = (int)(dragPosition.X / blankSlot.Width);
                    slot.Y = (int)(dragPosition.Y / blankSlot.Height);
                    item = inventory[slot.X, slot.Y];
                    found = true;
                }
            }

            return found;
        }



        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (!descriptionScreen.ActionHandled && descriptionScreen.IsClosed)
            {
                descriptionScreen.ActionHandled = true;
                if (selectedItem is EquipableItem && descriptionScreen.ButtonPressed(0))
                {
                    // equip the item
                    EquipableItem item = (EquipableItem)selectedItem;
                    player.SwapItems(selectedItem.Slot, new Point(-1, (int)item.BasicEquipSlot));
                }
                else if (selectedItem is UsableItem && descriptionScreen.ButtonPressed(0))
                {
                    UsableItem item = (UsableItem)selectedItem;
                    item.Select();
                }
                selectedItem = null;
            }

            if (particleTimer.Update(gameTime))
            {
                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = inventorySlotsCornerBase.X;
                for (int x = 0; x < inventory.GetLength(0); x++)
                {
                    inventorySlotsCorner.Y = inventorySlotsCornerBase.Y;
                    for (int y = 0; y < inventory.GetLength(1); y++)
                    {
                        if (inventory[x, y] != null && !inventory[x, y].Equals(currentlyDragged))
                        {
                            inventory[x, y].AddParticles(this, inventorySlotsCorner + new Vector2(blankSlot.Width / 2, blankSlot.Height / 2));
                        }

                        inventorySlotsCorner.Y += blankSlot.Height;
                    }
                    inventorySlotsCorner.X += blankSlot.Width;
                }

                EquipableItem[,] equiped = player.EquippedItems;
                for (int i = 0; i < equipSlotLocations.Length; i++)
                {
                    if (equiped[i, 0] != null && !equiped[i,0].Equals(currentlyDragged))
                    {
                        equiped[i, 0].AddParticles(this, equipSlotLocations[i] + new Vector2(blankSlot.Width / 2, blankSlot.Height / 2) + WindowCorner);
                    }
                }

            }

            ParticleSystem.Update(gameTime, this);
        }

    }
}
