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
    class StashScreen : PopUpScreen
    {
        static int boarderWidth = 6;
        PlayerSprite player;
        Texture2D blankSlot;
        int stashIndex = 0;
        Vector2 inventorySlotsCorner = new Vector2();
        Vector2 stashSlotsCorner = new Vector2();
        Vector2 equipmentSlotsCorner = new Vector2();
        ItemSprite currentlyDragged = null;
        ItemSprite currentlyDropped = null;
        Vector2 currentlyDraggedLocation = new Vector2();
        ItemSlot dragSlot = new ItemSlot();
        ItemSlot dropSlot = new ItemSlot();
        ItemSprite selectedItem = null;
        Vector2 tapPosition = new Vector2();
        ItemSlot tapSlot = new ItemSlot();
        Rectangle goldSourceRect = new Rectangle(0, 48, 48, 48);
        HelpButton helpButton;
        Vector2 titleVector;
        DescriptionScreen descriptionScreen = new DescriptionScreen();
        Timer particleTimer = new Timer(200, TimerState.Running, TimerType.Auto);


        public StashScreen(PlayerSprite player)
            : base()
        {
            CloseOnTapOutOfDimension = true;
            this.player = player;
            blankSlot = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/blankSlot");
            InputManager.ClearInputForPeriod(300);

            EnabledGestures = GestureType.DragComplete | GestureType.FreeDrag | GestureType.Tap;

            GameplayScreen.Instance.Pause();

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth, DungeonGame.ScreenSize.Width - boarderWidth * 2, DungeonGame.ScreenSize.Height - boarderWidth * 2);
                helpButton = new HelpButton(HelpScreens.Stash, new Vector2(WindowCorner.X + 400, WindowCorner.Y + 10));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                inventorySlotsCorner.Y = 420 + WindowCorner.Y;

                inventory = player.GetStash(stashIndex);
                stashSlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                stashSlotsCorner.Y = 120 + WindowCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = ((dimension.Width - (blankSlot.Width * inventory.GetLength(0))) / 2) + WindowCorner.X;
                equipmentSlotsCorner.Y = 420 + WindowCorner.Y - blankSlot.Height;

                titleVector = new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35);

                // add in buttons for swapping between stash screens
                MenuEntry entry;
                Rectangle location = new Rectangle(((dimension.Width - (blankSlot.Width * 5)) / 2) + (int)WindowCorner.X, 70 + (int)WindowCorner.Y, blankSlot.Width, blankSlot.Height / 2);
                for (int i = 0; i < 5; i++)
                {
                    entry = new MenuEntry((i + 1).ToString(), location);
                    entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                    MenuEntries.Add(entry);
                    location.X += location.Width;
                    if (i >= player.StashesUnlocked)
                    {
                        entry.IsActive = false;
                    }
                }
            }
            else
            {
                if (DungeonGame.pcMode)
                    dimension = new Rectangle((DungeonGame.ScreenSize.Width - 800) / 2 + boarderWidth, (DungeonGame.ScreenSize.Height - 400) / 2 + DungeonGame.ScreenSize.Y + boarderWidth, 800 - boarderWidth * 2, 400 - boarderWidth * 2);
                else
                    dimension = new Rectangle(boarderWidth, DungeonGame.ScreenSize.Y + boarderWidth + 80, DungeonGame.ScreenSize.Width - boarderWidth * 2, 400 - boarderWidth * 2);
                
                helpButton = new HelpButton(HelpScreens.Stash, new Vector2(WindowCorner.X + 10, WindowCorner.Y + 20));

                ItemSprite[,] inventory = player.Inventory;
                inventorySlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                inventorySlotsCorner.Y = blankSlot.Height + 30 + WindowCorner.Y;

                inventory = player.EquippedItems;
                equipmentSlotsCorner.X = dimension.Width - (blankSlot.Width * inventory.GetLength(0)) - 20 + WindowCorner.X;
                equipmentSlotsCorner.Y = 30 + WindowCorner.Y;

                inventory = player.GetStash(stashIndex);
                stashSlotsCorner.X = 20 + WindowCorner.X;
                stashSlotsCorner.Y = inventorySlotsCorner.Y + blankSlot.Height;

                titleVector = new Vector2(stashSlotsCorner.X + blankSlot.Width * 2.5f, WindowCorner.Y + 50);

                // add in buttons for swapping between stash screens
                MenuEntry entry;
                Rectangle location = new Rectangle(20 + (int)WindowCorner.X, (int)inventorySlotsCorner.Y, blankSlot.Width, blankSlot.Height / 2);
                for (int i = 0; i < 5; i++)
                {
                    entry = new MenuEntry((i + 1).ToString(), location);
                    entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                    MenuEntries.Add(entry);
                    location.X += location.Width;
                    if (i >= player.StashesUnlocked)
                    {
                        entry.IsActive = false;
                    }
                }
            }
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectedEntry < player.StashesUnlocked)
            {
                stashIndex = selectedEntry;
            }
            else
            {
                // display stash unlock screen
                AddNextScreen(new StashUnlockScreen(player));
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
                    // TODO: add a swap button as an alternative to drag and drop
                    //descriptionScreen.AddSpace(15);
                    //descriptionScreen.AddButton("Swap");
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
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Stash", titleVector, Color.Red);

            // draw inventory background
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, true);
            DrawInventory(spriteBatch, player.GetStash(stashIndex), stashSlotsCorner, true);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, true);

            // draw particles
            spriteBatch.End();
            ParticleSystem.Draw(spriteBatch, gameTime, this, false);
            spriteBatch.Begin();

            // draw icons
            DrawInventory(spriteBatch, player.Inventory, inventorySlotsCorner, false);
            DrawInventory(spriteBatch, player.GetStash(stashIndex), stashSlotsCorner, false);
            DrawInventory(spriteBatch, player.EquippedItems, equipmentSlotsCorner, false);


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

            found = GetSlotAtLocation(player.GetStash(stashIndex), dragPosition - stashSlotsCorner, ref item, ref slot);
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

            if (dragPosition.X > 0 && dragPosition.Y > 0 && dragPosition.X < blankSlot.Width * inventory.GetLength(0) && dragPosition.Y < blankSlot.Height * inventory.GetLength(1))
            {
                slot.X = (int)(dragPosition.X / blankSlot.Width);
                slot.Y = (int)(dragPosition.Y / blankSlot.Height);
                item = inventory[slot.X, slot.Y];
                found = true;
            }

            return found;
        }


        ItemSprite GetItemFromSlot(ItemSlot slot)
        {
            if (slot.PlayerOwned)
            {
                return player.GetItem(slot.ToPoint());
            }
            else
            {
                return player.GetStash(stashIndex)[slot.X, slot.Y];
            }
        }


        void SetItemToSlot(ItemSlot slot, ItemSprite item)
        {
            if (slot.PlayerOwned)
            {
                ItemSprite displacedItem = player.SetItem(item, slot.ToPoint());
                if (null != displacedItem && !player.AddItem(displacedItem, true))
                {
                    player.DropItem(displacedItem);
                }
            }
            else
            {
                player.GetStash(stashIndex)[slot.X, slot.Y] = item;
                if (item != null)
                    item.Slot = slot.ToPoint();
            }
        }


        // we make the assumption that slot1 will never be null
        public void SwapItems(ItemSlot slot1, ItemSlot slot2)
        {
            ItemSprite item1 = GetItemFromSlot(slot1);
            ItemSprite item2 = GetItemFromSlot(slot2);

            // check for stack combining
            if (item1 is UsableItem && item2 is UsableItem)
            {
                UsableItem usable1 = (UsableItem)item1;
                UsableItem usable2 = (UsableItem)item2;
                bool combined = usable2.CombineStacks(usable1);
                if (combined)
                {
                    if (usable1.IsStackEmpty)
                    {
                        SetItemToSlot(slot1, null);
                    }
                    return;
                }
            }


            // do the swap if it is allowed
            if ((item2 == null || slot1.PlayerOwned == false || player.IsValidSlot(item2, slot1.ToPoint())) &&
                (item1 == null || slot2.PlayerOwned == false || player.IsValidSlot(item1, slot2.ToPoint())))
            {
                SetItemToSlot(slot1, item2);
                SetItemToSlot(slot2, item1);
            }

        }


      

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            for (int i = 0; i < 5; i++)
            {
                MenuEntry entry = MenuEntries[i];
                if (i >= player.StashesUnlocked)
                {
                    entry.IsActive = false;
                }
                else
                {
                    entry.IsActive = true;   
                }
            }

            if (!descriptionScreen.ActionHandled && descriptionScreen.IsClosed)
            {
                descriptionScreen.ActionHandled = true;
                if (selectedItem.IsPlayerOwned && descriptionScreen.ButtonPressed(0))
                {
                    //currentlyDragged = selectedItem;
                    //SwapItems(new ItemSlot(selectedItem.Slot.X, selectedItem.Slot.Y, true), new ItemSlot(0, 0, false));
                    //currentlyDragged = null;
                }
                else if (!selectedItem.IsPlayerOwned && descriptionScreen.ButtonPressed(0))
                {
                    //currentlyDragged = selectedItem;
                    //SwapItems(new ItemSlot(selectedItem.Slot.X, selectedItem.Slot.Y, false), new ItemSlot(0, 0, true));
                    //currentlyDragged = null;
                }
                selectedItem = null;
            }

            if (particleTimer.Update(gameTime))
            {
                UpdateInventoryParticles(player.Inventory, inventorySlotsCorner);
                UpdateInventoryParticles(player.GetStash(stashIndex), stashSlotsCorner);
                UpdateInventoryParticles(player.EquippedItems, equipmentSlotsCorner);
            }

            ParticleSystem.Update(gameTime, this);
        }

    }
}
