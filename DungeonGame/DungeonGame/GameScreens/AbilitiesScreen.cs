using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class AbilitiesScreen : PopUpScreen
    {
        int AbilityRowCount = 5;
        PopUpScreen owner;
        PlayerSprite player;
        Texture2D star;
        Texture2D arrowUp;
        int topIndex = 0;
        Rectangle[] abilityDragSpots = new Rectangle[5];
        Ability currentlyDragged;
        Vector2 currentlyDraggedLocation = new Vector2();
        Ability selectedAbility = null;
        Vector2 tapPosition = new Vector2();
        Color unusedStarColor = new Color(20, 20, 20, 100);
        HelpButton helpButton;
        DescriptionScreen descriptionScreen = new DescriptionScreen();
        List<Ability> abilities = new List<Ability>();

        public AbilitiesScreen(PopUpScreen owner, PlayerSprite player, Rectangle dimension)
            : base()
        {
            this.dimension = dimension;
            IsPopup = true;
            this.owner = owner;
            this.player = player;
            star = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/star");
            arrowUp = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/ArrowUp");

            RefreshAbilityList();
            Vector2 buttonPos = new Vector2(295, 70 + 85 / 2 - star.Height / 2);
            MenuEntry entry;

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(dimension.Width / 2 - 50 - arrowUp.Width, 55 + 95 * AbilityRowCount);
            entry.Texture = arrowUp;
            entry.OwningPopup = this;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(dimension.Width / 2 + 50, 55 + 95 * AbilityRowCount);
            entry.Texture = arrowUp;
            entry.OwningPopup = this;
            entry.SpriteEffect = SpriteEffects.FlipVertically;
            MenuEntries.Add(entry);

            abilityDragSpots[0] = new Rectangle(dimension.X + 20, dimension.Y + 100, 430, 80);
            for (int i = 1; i < abilityDragSpots.Length; i++)
            {
                abilityDragSpots[i] = abilityDragSpots[i - 1];
                abilityDragSpots[i].Y += 95;
            }

            helpButton = new HelpButton(HelpScreens.Abilities, Vector2.Zero);

            UpdateOrientation();
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0 && topIndex > 0)
            {
                topIndex--;             
            }
            else if (selectorIndex == 1 && topIndex < abilities.Count - AbilityRowCount)
            {
                topIndex++;
            }
        }


        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(0, 40);

                MenuEntries[0].Position = new Vector2(480 / 2 - 50 - arrowUp.Width, 540);
                MenuEntries[1].Position = new Vector2(480 / 2 + 50 - arrowUp.Width, 540);

                abilityDragSpots[0] = new Rectangle(dimension.X + 20, dimension.Y + 100, 430, 80);
                for (int i = 1; i < abilityDragSpots.Length; i++)
                {
                    abilityDragSpots[i] = abilityDragSpots[i - 1];
                    abilityDragSpots[i].Y += 93;
                }
                AbilityRowCount = 5;

                while (topIndex > abilities.Count - AbilityRowCount && topIndex > 0)
                {
                    topIndex--;
                }

                helpButton.Location = new Vector2(WindowCorner.X + 400, 660);
            }
            else
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, borderWidth, 800 - 80 - borderWidth * 2 - 50, 400 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(70, 0);

                MenuEntries[0].Position = new Vector2(550 - 25 - arrowUp.Width, dimension.Height - 50);
                MenuEntries[1].Position = new Vector2(550 + 25, dimension.Height - 50);

                abilityDragSpots[0] = new Rectangle(borderWidth + 20 + (int)WindowCorner.X, (int)WindowCorner.Y + borderWidth + 10, 430, 80);
                for (int i = 1; i < abilityDragSpots.Length; i++)
                {
                    abilityDragSpots[i] = abilityDragSpots[i - 1];
                    abilityDragSpots[i].Y += 93;
                }
                AbilityRowCount = 4;

                helpButton.Location = new Vector2(WindowCorner.X + dimension.Width - 60, WindowCorner.Y + 20);
            }
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
                // check if we are on an ability pick it up if it is selectable
                GetAbilityAtLocation(currentlyDraggedLocation, ref currentlyDragged);
                if (currentlyDragged != null && currentlyDragged.IsSelectable() == false)
                {
                    currentlyDragged = null;
                }
            }
            else if (isFinished && currentlyDragged != null)
            {
                if (currentlyDragged.IsSelectable())
                {
                    // check if the drop location is a button
                    int buttonIndex = 0;
                    bool isButton = GameplayScreen.ui.IsButtonLocation(currentlyDraggedLocation, ref buttonIndex);
                    if (isButton)
                    {
                        if (currentlyDragged.IsSelectable())
                        {
                            player.SetMacroButton(currentlyDragged.GetMacroButton(), buttonIndex);
                        }
                    }
                }

                // no longer dragging
                currentlyDragged = null;
            }

            bool isTapped = InputManager.GetTapPoint(ref tapPosition);
            if (isTapped)
            {
                GetAbilityAtLocation(tapPosition, ref selectedAbility);
                if (selectedAbility != null)
                {
                    descriptionScreen.Initialize(selectedAbility);
                    if (selectedAbility.Level == 0 && selectedAbility.CanLevelUp())
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Learn");
                    }
                    else if (selectedAbility.Level < Ability.MaxLevel && selectedAbility.CanLevelUp())
                    {
                        descriptionScreen.AddSpace(15);
                        descriptionScreen.AddButton("Upgrade");
                    }
                    descriptionScreen.SetFinalize();
                    AddNextScreen(descriptionScreen);
                }
            }

            helpButton.HandleInput();
        }



        private void GetAbilityAtLocation(Vector2 location, ref Ability ability)
        {
            for (int i = 0; i < AbilityRowCount; i++)
            {
                if (abilityDragSpots[i].Contains(location.ToPoint()))
                {
                    ability = abilities[topIndex + i];
                    return;
                }
            }
            ability = null;
        }


        public void RefreshAbilityList()
        {
            abilities.Clear();
            player.PlayerClass.GetAbilityList(player, abilities);
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            UpdateOrientation();

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Color fontColor = Color.White;

            Rectangle border;
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "Ability Points: " + player.AbilityPoints.ToString(), new Vector2(480 / 2, WindowCorner.Y + 45), fontColor);
                border = new Rectangle((int)WindowCorner.X + 20, (int)WindowCorner.Y + 70, 430, 85);
            }
            else
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, "AP: " + player.AbilityPoints.ToString(), new Vector2(550 + WindowCorner.X, WindowCorner.Y + 200), fontColor);
                border = new Rectangle((int)WindowCorner.X + 20, (int)WindowCorner.Y + 10, 430, 85);
            }


            for (int i = 0; i < AbilityRowCount; i++)
            {
                DrawBorder(border, InternalContentManager.GetTexture("Blank"), Color.Gray, 3, Color.Black, spriteBatch);

                if (topIndex + i < abilities.Count)
                {
                    Ability ability = abilities[topIndex + i];
                    ability.DrawIcon(spriteBatch, new Vector2(border.X, border.Y), new Vector2(80, 85));
                    spriteBatch.DrawString(Fonts.DescriptionFont, ability.Name, new Vector2(border.X + 80, border.Y + 85 / 2 - 15), fontColor);
                    for (int starIndex = 0; starIndex < Ability.MaxLevel - 3; starIndex++)
                    {
                        Vector2 loc = new Vector2(WindowCorner.X + 350 + (star.Width + 5) * starIndex, border.Y + 85 / 2 - star.Height - 5 );
                        Color tint = ability.Level >= (starIndex + 1) ? Color.White : unusedStarColor;
                        spriteBatch.Draw(star, loc, tint);
                    }
                    for (int starIndex = 3; starIndex < Ability.MaxLevel; starIndex++)
                    {
                        Vector2 loc = new Vector2(WindowCorner.X + 350 + (star.Width + 5) * (starIndex - 3), border.Y + 85 / 2 + 5);
                        Color tint = ability.Level >= (starIndex + 1) ? Color.White : unusedStarColor;
                        spriteBatch.Draw(star, loc, tint);
                    }
                }

                border.Y += 93;
            }

            helpButton.Draw(spriteBatch);

            // draw dragged ability
            if (currentlyDragged != null)
            {
                currentlyDragged.DrawIcon(spriteBatch, currentlyDraggedLocation, Vector2.Zero);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }



        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            RefreshAbilityList();

            if (!descriptionScreen.ActionHandled && descriptionScreen.IsClosed)
            {
                descriptionScreen.ActionHandled = true;
                if (descriptionScreen.ButtonPressed(0))
                {
                    selectedAbility.LevelUp();
                }
                selectedAbility = null;
            }
        }
    }
}
