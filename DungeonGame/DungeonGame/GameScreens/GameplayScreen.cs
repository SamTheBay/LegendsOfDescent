using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{

    class GameplayScreen : GameScreen
    {
        const int pickupDistance = 130;

        Texture2D greyout;
        static public DungeonLevel Dungeon;
        static public Vector2 viewportCorner = new Vector2();
        static public GameplayScreen Instance;
        static public bool takeScreenShot = false;
        static public bool screenClear = false;
        PlayerSprite player;
        List<NPCSprite> npcs = new List<NPCSprite>();
        static public List<NPCSprite> npcSprites = new List<NPCSprite>();
        static public UserInterface ui;
        int currentPathfinder = 0;
        List<IEnvItem> items = new List<IEnvItem>();
        List<WeaponSprite[]> playerWeps = new List<WeaponSprite[]>(); // weapons that must be collision detected with
        List<WeaponSprite[]> enemyWeps = new List<WeaponSprite[]>();
        List<TextSprite> textSprites = new List<TextSprite>();
        TextSprite[] textSpritePool = new TextSprite[50];
        int pausedCount = 0;
        PauseScreen pauseScreen;

        // input management variables
        bool haveMoveTouch = false;
        TouchLocation moveTouch;
        IEnvItem itemTargeted = null;
        Rectangle holdRectangle = new Rectangle(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 100, 100, 100);
        Rectangle holdRectangleLeft = new Rectangle(DungeonGame.ScreenSize.Width - 100, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 100, 100, 100);


        // slow time variables
        Timer slowTimeTimer = new Timer(1000, TimerState.Stopped, TimerType.Manual);
        bool lastFrameSkipped = false;
        Timer flashTimer = new Timer(300, TimerState.Stopped, TimerType.Manual);
        Color flashColor = Color.White;

        int deathDuration = 5000;
        int deathElapsed = 0;
        Texture2D deadTex;

        public GameplayScreen(PlayerSprite player, DungeonLevel dungeon)
            : base()
        {
            this.player = player;
            Instance = this;
            greyout = InternalContentManager.GetTexture("Grayout");
            deadTex = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/Dead");
            EnabledGestures = GestureType.Tap;

            SetDungeon(dungeon);
            AddPlayer(player);

            ParticleSystem.Initialize(1000);

            for (int i = 0; i < textSpritePool.Length; i++)
            {
                textSpritePool[i] = new TextSprite();
            }

            ui = new UserInterface();
            ui.Initialize(player);
        }


        public void SetDungeon(DungeonLevel dungeon)
        {
            GameplayScreen.Dungeon = dungeon;
            npcs = dungeon.NPCs;
            items = dungeon.EnvItems;
            for (int i = 0; i < dungeon.EnvItems.Count; i++)
            {
                items[i].OccupiedSlot = Dungeon.ItemOccupyPosition(items[i].CenteredPosition.ToPoint());
            }
            enemyWeps.Clear();

            // add npcs projectiles into the weapon sets
            for (int i = 0; i < npcs.Count; i++)
            {
                WeaponSprite[] weapons = npcs[i].GetCollisionableWeaponSet();
                if (weapons != null)
                {
                    AddCollisonableWeaponSet(weapons, npcs[i]);
                }
            }

            // refresh merchants and stuff
            items.OfType<NPCSprite>().ForEach(n => n.Refresh());

            // adjust music
            if (dungeon.MapType == "Town")
            {
                AudioManager.audioManager.PlaySFX("MainMusic");
            }
            else if (dungeon.MapType == "Boss")
            {
                AudioManager.audioManager.PlaySFX("MainMusic3");
            }
            else
            {
                AudioManager.audioManager.PlaySFX("MainMusic2");
            }

            player.QuestLog.SetDungeon(dungeon);
        }



        private void AddPlayer(PlayerSprite player)
        {
            if (player.Position == Vector2.Zero)
            {
                player.SetPosition(
                    Dungeon.StartLocation.X * Dungeon.TileDimension + (Dungeon.TileDimension / 2) - (player.FrameDimensions.X / 2),
                    Dungeon.StartLocation.Y * Dungeon.TileDimension + (Dungeon.TileDimension / 2) - (player.FrameDimensions.Y / 2));
            }
            player.StopMoving();

            player.ValidatePosition();

            // add in the players weapon sprites
            for (int i = 0; i < player.Abilities.Length; i++)
            {
                WeaponSprite[] weaponSet = player.Abilities[i].GetCollisionableSet();
                if (weaponSet != null)
                {
                    for (int j = 0; j < weaponSet.Length; j++)
                    {
                        weaponSet[j].Deactivate();
                    }

                    AddCollisonableWeaponSet(weaponSet, player);
                }
            }

            // Add in the town portal
            items.Add(player.TownPortal);
            player.TownPortal.SetPosition();

            pauseScreen = new PauseScreen(player);
        }



        Rectangle updateRectangle = new Rectangle(0, 0, DungeonGame.ScreenSize.Width + 400, DungeonGame.ScreenSize.Height + 400);
        List<NPCSprite> updatingNpcs = new List<NPCSprite>();
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            screenClear = !otherScreenHasFocus;

            if (IsActive && !coveredByOtherScreen)
            {
                if (!Dungeon.StartDialogueShown && Dungeon.HasIntroDialogueScreen)
                {
                    ScreenManager.AddScreen(Dungeon.GetIntroDialogueScreen());
                    Dungeon.StartDialogueShown = true;
                    return;
                }

                ui.Update(gameTime);

                // handle screen rotation
                if (DungeonGame.currentlyPortraitMode && !DungeonGame.portraitMode)
                {
                    DungeonGame.Instance.SetLandscapeMode();
                }
                else if (!DungeonGame.currentlyPortraitMode && DungeonGame.portraitMode)
                {
                    DungeonGame.Instance.SetPortraitMode();
                }


                // help screen logic
                if (DungeonGame.joystickEnabled)
                {
                    if (HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.HowToMoveJoystick))
                        return;
                }
                else
                {
                    if (HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.HowToMove))
                        return;
                }

                if (!HelpScreenManager.Instance.HasBeenShownBefore(HelpScreens.Stairs) && Vector2.Distance(player.CenteredPosition, Dungeon.StairsDownPosition) < 200)
                {
                    HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.Stairs);
                    return;
                }

                if (Dungeon.Level == 5)
                {
                    HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.Dragnor);
                }


                if (player.IsDead)
                {
                    deathElapsed += gameTime.ElapsedGameTime.Milliseconds;
                    if (deathElapsed > deathDuration)
                    {
                        ResurrectPlayer();
                    }
                }

                // update timers
                if (slowTimeTimer.Update(gameTime))
                {
                    AudioManager.audioManager.PlaySFX("SpeedUp" + Util.Random.Next(1, 4).ToString());
                }
                flashTimer.Update(gameTime);

                // get the enemies that are in the updateable zone
                updateRectangle.X = (int)(viewportCorner.X - 200);
                updateRectangle.Y = (int)(viewportCorner.Y - 200);
                updateRectangle.Width = DungeonGame.ScreenSize.Width + 400;
                updateRectangle.Height = DungeonGame.ScreenSize.Height + 400;
                updatingNpcs.Clear();
                for (int i = 0; i < npcs.Count; i++)
                {
                    if (updateRectangle.Contains((int)npcs[i].CenteredPosition.X, (int)npcs[i].CenteredPosition.Y))
                    {
                        updatingNpcs.Add(npcs[i]);
                    }
                }

                // update the player
                player.Update(gameTime);

                SaveGameManager.CurrentPlayer.QuestLog.PlayerUpdated(player);

                // update the tiles
                Point corner = GameplayScreen.viewportCorner.ToPoint();
                for (int y = (int)(corner.Y / Dungeon.TileDimension - 1); y < (corner.Y / Dungeon.TileDimension) + (DungeonGame.ScreenSize.Height / Dungeon.TileDimension + 4); y++)
                {
                    for (int x = (int)(corner.X / Dungeon.TileDimension - 1); x < (corner.X / Dungeon.TileDimension) + (DungeonGame.ScreenSize.Width / Dungeon.TileDimension + 3); x++)
                    {
                        if (x > 0 && y > 0 && x < Dungeon.Dimension.X && y < Dungeon.Dimension.Y)
                        {
                            Dungeon.GetTile(x, y).Update(gameTime);
                        }
                    }
                }

                if (itemTargeted != null && Vector2.Distance(itemTargeted.CenteredPosition, player.CenteredPosition) < pickupDistance)
                {
                    bool result = itemTargeted.ActivateItem(player);
                    if (result)
                    {
                        items.Remove(itemTargeted);
                    }
                    itemTargeted = null;
                }


                // auto pick up gold that is close
                for (int i = 0; i < items.Count; i++)
                {
                    if (items[i] != null && items[i] is GoldItem)
                    {
                        if (Vector2.Distance(player.CenteredPosition, items[i].CenteredPosition) < 50)
                        {
                            bool result = items[i].ActivateItem(player);
                            if (result)
                            {
                                items.RemoveAt(i);
                            }
                        }
                    }
                }


                if (!IsTimeSlow || !lastFrameSkipped)
                {
                    // update enemies
                    for (int i = 0; i < updatingNpcs.Count; i++)
                    {
                        updatingNpcs[i].Update(gameTime);
                        if (i == currentPathfinder)
                            updatingNpcs[i].UpdatePathfind(updatingNpcs, player);
                    }
                    if (updatingNpcs.Count > 0)
                    {
                        currentPathfinder = (currentPathfinder + 1) % updatingNpcs.Count;
                    }


                    // Update enemies weapons
                    for (int i = 0; i < enemyWeps.Count; i++)
                    {
                        for (int j = 0; j < enemyWeps[i].Length; j++)
                        {
                            enemyWeps[i][j].Update(gameTime);
                        }
                    }


                    // Update the particle system
                    ParticleSystem.Update(gameTime);


                    // update items
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] != null)
                        {
                            items[i].Update(gameTime);
                        }
                    }


                    // update text sprites
                    for (int i = 0; i < textSprites.Count; i++)
                    {
                        if (textSprites[i] != null)
                        {
                            textSprites[i].Update(gameTime);
                            if (!textSprites[i].IsActive)
                            {
                                textSprites.RemoveAt(i);
                            }
                        }
                    }

                }
                lastFrameSkipped = !lastFrameSkipped;

                // do collision detection player weapons to enemies
                for (int i = 0; i < playerWeps.Count; i++)
                {
                    for (int j = 0; j < playerWeps[i].Length; j++)
                    {
                        for (int e = 0; e < updatingNpcs.Count; e++)
                        {
                            if (!playerWeps[i][j].IsCollisionable)
                            {
                                continue;
                            }
                            if (updatingNpcs[e].IsCollisionable)
                            {
                                if (playerWeps[i][j].CollisionDetect(updatingNpcs[e]))
                                {
                                    playerWeps[i][j].CollisionAction(updatingNpcs[e]);
                                    updatingNpcs[e].CollisionAction(playerWeps[i][j]);
                                }
                            }
                        }

                    }
                }

                // collision detect enemy weapons to player
                for (int i = 0; i < enemyWeps.Count; i++)
                {
                    for (int j = 0; j < enemyWeps[i].Length; j++)
                    {
                        if (!enemyWeps[i][j].IsCollisionable)
                        {
                            continue;
                        }
                        if (enemyWeps[i][j].CollisionDetect(player))
                        {
                            enemyWeps[i][j].CollisionAction(player);
                            player.CollisionAction(enemyWeps[i][j]);
                        }

                    }
                }

            }
        }

        private static void ProjectPoint(ref Vector2 newPosition, Vector2 diff, float length)
        {
            float angle = (float)Math.Atan2(diff.X, diff.Y);
            float xdiff = (float)Math.Sin(angle) * length;
            float ydiff = (float)Math.Cos(angle) * length;

            newPosition.X += xdiff;
            newPosition.Y += ydiff;
        }

        public override void TopFullScreenAcquired()
        {
            base.TopFullScreenAcquired();
            if (DungeonGame.portraitMode)
            {
                DungeonGame.Instance.SetPortraitMode();
            }
            else
            {
                DungeonGame.Instance.SetLandscapeMode();
            }
        }


        public void HandleMacros()
        {
            // handle hotkeys
            if (InputManager.IsBackTriggered())
            {
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Options))
            {
                pauseScreen.SelectedTab = 4;
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Quest))
            {
                pauseScreen.SelectedTab = 3;
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Inventory))
            {
                pauseScreen.SelectedTab = 0;
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Abilities))
            {
                pauseScreen.SelectedTab = 2;
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Stats))
            {
                pauseScreen.SelectedTab = 1;
                pauseScreen.Show();
                player.StopMoving();
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.HealthPot))
            {
                player.UseHealthPotion();
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.ManaPot))
            {
                player.UseManaPotion();
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.ShowItemNames))
            {
                ShowAllItemNames();
            }


            for (int i = (int)keyboardActionSet.H1; i < (int)keyboardActionSet.H1 + UserInterface.numMacroButtons; i++)
            {
                if (InputManager.GetKeyboardAction((keyboardActionSet)i))
                {
                    ui.ButtonSelected(i - (int)keyboardActionSet.H1);
                }
            }
        }


        public void CheckItemAndButtonTouch(TouchLocation touch, ref bool touchedButton, ref bool touchedItem)
        {
            touchedButton = ui.CheckButtonPress(touch.Position);

            if (!touchedButton)
            {
                if (ui.IsHoldButtonDown && !DungeonGame.joystickEnabled)
                {
                    player.UseActiveAbility(touch.Position + viewportCorner);
                }
                else
                {
                    // check if press was on an item
                    for (int j = 0; j < items.Count; j++)
                    {
                        if (items[j] != null && items[j].IsActive && items[j].CanItemActivate())
                        {
                            Rectangle loc = new Rectangle();
                            items[j].GetBoundingRectangle(ref loc);
                            if (loc.Contains((int)touch.Position.X + (int)viewportCorner.X, (int)touch.Position.Y + (int)viewportCorner.Y))
                            {
                                // pick up the item if player is close
                                if (Vector2.Distance(items[j].CenteredPosition, player.CenteredPosition) < pickupDistance)
                                {
                                    bool result = items[j].ActivateItem(player);
                                    if (result)
                                    {
                                        items.RemoveAt(j);
                                    }

                                }
                                else
                                {
                                    itemTargeted = items[j];
                                }

                                touchedItem = true;
                                break;
                            }
                        }
                    }
                }
            }
        }



        public void HandleTouchWithNoJoystick()
        {
            KeyboardState keyState = Keyboard.GetState();
            holdRectangle.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 100;
            holdRectangleLeft.X = DungeonGame.ScreenSize.Width - 100;
            holdRectangleLeft.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 100;
            if (keyState.IsKeyDown(Keys.Space) || keyState.IsKeyDown(Keys.LeftShift) ||
                (InputManager.IsLocationPressed(holdRectangle) && DungeonGame.touchEnabled) ||
                (DungeonGame.pcMode && InputManager.IsLocationPressed(holdRectangleLeft) && DungeonGame.touchEnabled))
            {
                if (ui.IsHoldButtonDown == false)
                {
                    // hold is triggered so stop moving
                    player.StopMoving();
                    haveMoveTouch = false;
                }
                ui.IsHoldButtonDown = true;
            }
            else
            {
                ui.IsHoldButtonDown = false;
            }

            TouchCollection touches = InputManager.GetTouchCollection();

            // if we have a move touch then try and find it and update it
            if (haveMoveTouch)
            {
                haveMoveTouch = false;
                for (int i = 0; i < touches.Count; i++)
                {
                    if (touches[i].Id == moveTouch.Id)
                    {
                        if (touches[i].State == TouchLocationState.Moved || touches[i].State == TouchLocationState.Pressed)
                        {
                            moveTouch = touches[i];
                            haveMoveTouch = true;
                        }
                        break;
                    }
                }
            }

            // check for new presses to trigger actions
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].State == TouchLocationState.Pressed)
                {
                    if (haveMoveTouch && moveTouch.Id == touches[i].Id)
                        continue;

                    // check if this press was on a button
                    bool touchedItem = false;
                    bool touchedButton = false;

                    CheckItemAndButtonTouch(touches[i], ref touchedButton, ref touchedItem);

                    // otherwise check if this is our new move
                    if (!touchedButton && !haveMoveTouch && (!touchedItem || itemTargeted != null))
                    {
                        if (!touchedButton && !ui.IsHoldButtonDown && !haveMoveTouch)
                        {
                            moveTouch = touches[i];
                            haveMoveTouch = true;
                        }
                        if (!touchedItem && itemTargeted != null)
                        {
                            itemTargeted = null;
                        }
                    }
                }

            }

            if (haveMoveTouch)
            {
                player.SetDestination(moveTouch.Position + viewportCorner);
                ParticleSystem.AddParticles(moveTouch.Position + viewportCorner, ParticleType.Starburst, numParticlesScale: .2f, lifetimeScale: .3f);
            }
        }



        public void HandleTouchWithJoystick()
        {
            TouchCollection touches = InputManager.GetTouchCollection();

            // check for new presses to trigger actions
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].State == TouchLocationState.Pressed)
                {
                    // check if this press was on a button
                    bool touchedItem = false;
                    bool touchedButton = false;

                    CheckItemAndButtonTouch(touches[i], ref touchedButton, ref touchedItem);
                }
            }

            // check if the player is moving
            if (ui.Joystick.HasDirection)
            {
                Vector2 direction = player.Position;
                direction += (ui.Joystick.Direction * 300);
                player.SetDestination(direction, false);
            }
            else
            {
                player.StopMoving();
            }

            // check if we should perform an action
            if (ui.ActionTriggered)
            {
                Vector2 direction = player.CenteredPosition;
                direction += ui.Joystick.Direction * 100;
                player.UseActiveAbility(direction);
            }
        }



        
        public override void HandleInput()
        {
            if (player.IsActive && player.IsDead == false)
            {
                HandleMacros();

                if (DungeonGame.touchEnabled && DungeonGame.joystickEnabled)
                {
                    HandleTouchWithJoystick();
                }
                else
                {
                    HandleTouchWithNoJoystick();
                }

                // secondary mouse actions for PC
                if (DungeonGame.pcMode)
                {
                    if (InputManager.SecondaryMousePressed && !player.IsAttacking)
                    {
                        player.StopMoving();
                        player.UseActiveAbility(InputManager.MousePosition + viewportCorner);
                    }
                }

            }
        }


        public override void Draw(GameTime gameTime)
        {
            if (screenClear && takeScreenShot)
            {
                DungeonGame.takeScreenShot = true;
                takeScreenShot = false;
            }

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;

            // determine the corner position of the viewport
            viewportCorner.X = player.Position.X + player.FrameDimensions.X / 2 - DungeonGame.ScreenSize.Width / 2;
            viewportCorner.Y = player.Position.Y + player.FrameDimensions.Y / 2 - DungeonGame.ScreenSize.Height / 2 - DungeonGame.ScreenSize.Y;

            if (DungeonGame.Instance.PortraitMode == false && DungeonGame.adControlManager.ShowAds == true)
            {
                // shift the viewport to accommodate the ad at the top
                viewportCorner.Y -= 40;
            }

            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);

            // draw the environment
            Dungeon.Draw(spriteBatch);

            // Draw items that are on the map
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {

                    if (items[i].Position.X > viewportCorner.X - 200 &&
                        items[i].Position.X < viewportCorner.X + DungeonGame.ScreenSize.Width + 200 &&
                        items[i].Position.Y > viewportCorner.Y - 200 &&
                        items[i].Position.Y < viewportCorner.Y + DungeonGame.ScreenSize.Height + 200)
                    {
                        items[i].Draw(spriteBatch);
                    }
                }
            }

            // draw enemies
            for (int i = 0; i < npcs.Count; i++)
            {
                if (npcs[i].Position.X > viewportCorner.X - 200 &&
                    npcs[i].Position.X < viewportCorner.X + DungeonGame.ScreenSize.Width + 200 &&
                    npcs[i].Position.Y > viewportCorner.Y - 200 &&
                    npcs[i].Position.Y < viewportCorner.Y + DungeonGame.ScreenSize.Height + 200)
                {
                    npcs[i].Draw(spriteBatch);
                }
            }

            // draw enemies weapons
            for (int i = 0; i < enemyWeps.Count; i++)
            {
                for (int j = 0; j < enemyWeps[i].Length; j++)
                {
                    enemyWeps[i][j].Draw(spriteBatch, 3);
                }
            }

            // draw the players
            player.Draw(spriteBatch);

            spriteBatch.End();

            // draw text sprites
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            for (int i = 0; i < textSprites.Count; i++)
            {
                if (textSprites[i] != null)
                {
                    textSprites[i].Draw(spriteBatch);
                }
            }
            spriteBatch.End();

            if (IsPaused)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                spriteBatch.Draw(greyout, DungeonGame.ScreenSize, null, Color.White, 0f, Vector2.Zero, SpriteEffects.None, .003f);
                spriteBatch.End();
            }

            ParticleSystem.Draw(spriteBatch, gameTime);

            // draw the minimap
            bool drawLargeMiniMap = DungeonGame.pcMode;
            Point miniMapPosition = DungeonGame.currentlyPortraitMode ? new Point(5, DungeonGame.ScreenSize.Y + 5) : new Point(25, DungeonGame.ScreenSize.Y + 50);
            if (DungeonGame.pcMode && DungeonGame.ScreenSize.Width < 1024)
            {
                miniMapPosition.Y += 90;
            }
            Dungeon.DrawMiniMap(miniMapPosition, player.Position, spriteBatch, drawLargeMiniMap ? 2 : 1);


            // draw the ui
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            ui.Draw(spriteBatch);
            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);


            // draw a flash on the screen
            if (flashTimer.State == TimerState.Running)
            {
                float a = 0;
                if (flashTimer.PercentComplete < .5f)
                {
                    a = flashTimer.PercentComplete * 2;
                }
                else
                {
                    a = 1f - ((flashTimer.PercentComplete - .5f) * 2);
                }

                Debug.WriteLine("alpha flash: " + a.ToString());
                Texture2D flash = InternalContentManager.GetSolidColorTexture(new Color(1f, 1f, 1f, a));
                spriteBatch.Draw(flash, DungeonGame.ScreenSize, flashColor);
            }

            if (IsTimeSlow)
            {
                spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(1f, 1f, 1f, .1f)), DungeonGame.ScreenSize, Color.White);
            }

            spriteBatch.End();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            if (player.IsDead)
            {
                Vector2 textLoc = new Vector2(DungeonGame.ScreenSize.Width / 2 - deadTex.Width / 2, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y - deadTex.Height / 2);
                spriteBatch.Draw(deadTex, textLoc, Color.White);
                textLoc.Y += deadTex.Height + 10;
                textLoc.X = DungeonGame.ScreenSize.Width / 2;
                spriteBatch.DrawCenteredText(Fonts.HeaderFont, "Death takes it's toll of", textLoc, Color.Red, borderColor: Color.Black);
                textLoc.Y += 40;
                spriteBatch.DrawCenteredText(Fonts.HeaderFont, (player.Gold / 20).ToString() + " gold", textLoc, Color.Red, borderColor: Color.Black);
            }

            spriteBatch.End();
        }


        public void ResurrectPlayer()
        {
            deathElapsed = 0;
            player.Resurrect(Dungeon.GetTile(15, 16).Position);

            // send the player to town
            SaveGameManager.PersistDungeon(Dungeon);
            DungeonLevel newDungeon = new DungeonLevel();
            player.DungeonLevel = 0;
            SaveGameManager.LoadDungeon(ref newDungeon);
            newDungeon.GoingDown = false;
            DungeonGame.ScreenManager.AddScreen(new InitialLoadScreen(newDungeon));
            DungeonGame.ScreenManager.RemoveScreen(this);
        }


        public void ShowAllItemNames()
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    if (items[i].Position.X > viewportCorner.X - 200 &&
                        items[i].Position.X < viewportCorner.X + DungeonGame.ScreenSize.Width + 200 &&
                        items[i].Position.Y > viewportCorner.Y - 200 &&
                        items[i].Position.Y < viewportCorner.Y + DungeonGame.ScreenSize.Height + 200)
                    {
                        items[i].ShowName();
                    }
                }
            }
        }

        public void AddTextSprite(TextSprite textSprite)
        {
            textSprites.Add(textSprite);
        }

        public void RemoveTextSprite(TextSprite textSprite)
        {
            try
            {
                textSprites.Remove(textSprite);
            }
            catch {}
        }


        public void AddTextSpriteFromPool(string text, Vector2 position, Color color, SpriteFont font, Vector2 movement, int duration)
        {
            for (int i = 0; i < textSpritePool.Length; i++)
            {
                TextSprite textSprite = textSpritePool[i];
                if (!textSprite.IsActive)
                {
                    textSprite.SetDetails(text, position, color, font, movement, duration);
                    textSprite.Activate();
                    textSprites.Add(textSprite);
                    return;
                }
            }
        }


        public void AddEnvItem(IEnvItem item)
        {
            // take a spot in the dungeon
            item.OccupiedSlot = Dungeon.ItemOccupyPosition(item.CenteredPosition.ToPoint());
            item.Position = new Vector2(item.OccupiedSlot.X * Dungeon.TileDimension + Dungeon.TileDimension / 2, item.OccupiedSlot.Y * Dungeon.TileDimension + Dungeon.TileDimension / 2);
            item.Activate();
            items.Add(item);
        }


        public void PlayerAtDestination()
        {

        }


        public void AddCollisonableWeaponSet(WeaponSprite[] weapons, CharacterSprite owner)
        {
            if (owner is PlayerSprite)
                playerWeps.Add(weapons);
            else if (owner is EnemySprite)
                enemyWeps.Add(weapons);
        }

        // TODO: make a variation that only returns the enemies on the screen currently
        public IEnumerable<EnemySprite> Enemies
        {
            get { return npcs.OfType<EnemySprite>(); }
        }


        public EnemySprite GetClosestEnemy(Vector2 position, ref float distance, List<EnemySprite> excludeList)
        {
            distance = float.MaxValue;
            EnemySprite closestEnemy = null;
            foreach (var enemy in Enemies)
            {
                if (enemy != null)
                {
                    bool skip = false;
                    for (int j = 0; j < excludeList.Count; j++)
                    {
                        if (excludeList[j] != null && enemy.Equals(excludeList[j]))
                        {
                            skip = true;
                            break;
                        }
                    }
                    if (skip == true)
                    {
                        continue;
                    }
                    else if (enemy.IsActive && !enemy.IsDead)
                    {
                        float newDistance = Vector2.Distance(position, enemy.CenteredPosition);
                        if (newDistance < distance)
                        {
                            distance = newDistance;
                            closestEnemy = enemy;
                        }
                    }
                }
            }
            return closestEnemy;
        }

        public IEnumerable<EnemySprite> GetEnemiesInRadius(Vector2 position, float radius)
        {
            foreach (var enemy in Enemies)
            {
                if (enemy != null && enemy.IsActive && !enemy.IsDead)
                {
                    if (Vector2.Distance(position, enemy.CenteredPosition) < radius)
                    {
                        yield return enemy;
                    }
                }
            }
        }

        public bool IsPaused 
        {
            get { return pausedCount > 0; }        
        }

        public void Pause()
        {
            pausedCount++;
        }

        public void UnPause()
        {
            pausedCount--;
        }

        public void SlowTime(int duration)
        {
            slowTimeTimer.ResetTimerAndRun(duration);
            Flash(Color.White);
        }

        public void EndSlowTime()
        {
            slowTimeTimer.State = TimerState.Stopped;
        }

        public int SlowTimeRemaining
        {
            get { return slowTimeTimer.RemainingDuration; }
        }


        public void Flash(Color color)
        {
            flashTimer.ResetTimerAndRun();
            flashColor = color;
        }


        public bool IsTimeSlow
        {
            get { return slowTimeTimer.State == TimerState.Running; }
        }

        MessagePopup messageBox = new MessagePopup();
        public void ShowMessage(string text)
        {
            messageBox.Show(text);
        }


        public PauseScreen PauseScreen
        {
            get { return pauseScreen; }
        }
    }
}
