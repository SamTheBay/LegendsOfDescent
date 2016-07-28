using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    public class PauseScreen : PopUpScreen
    {
        static int borderWidth = 6;
        PlayerSprite player;
        PopUpScreen[] tabs = new PopUpScreen[5];
        int selectedtab = 0;
        String[] tabNames = { "Inventory", "Stats", "Ability", "Quest", "Settings" };
        

        public PauseScreen(PlayerSprite player)
            : base()
        {
            IsPopup = true;
            this.player = player;

            int offset = 5;
            int buttonSpacing = 25;

            for (int i = 0; i < tabNames.Length; i++)
            {
                AddTab(tabNames[i], buttonSpacing, ref offset);
            }

            Dimension = new Rectangle(borderWidth + DungeonGame.ScreenSize.Width / 2 - 240, DungeonGame.ScreenSize.Y + borderWidth, 480 - borderWidth * 2, DungeonGame.ScreenSize.Height - 80 - borderWidth * 2);

            // create tabs
            tabs[0] = new InventoryScreen(this, player, dimension);
            tabs[1] = new StatsScreen(this, player, dimension);
            tabs[2] = new AbilitiesScreen(this, player, dimension);
            tabs[3] = new QuestScreen(this, player, dimension);
            tabs[4] = new OptionScreen(this, player, dimension);

            foreach (var tab in tabs)
            {
                tab.ScreenManager = DungeonGame.ScreenManager;
            }

            CloseOnTapOutOfDimension = true;

            EnabledGestures = GestureType.DragComplete | GestureType.FreeDrag | GestureType.Tap;
        }

        private void AddTab(string name, int buttonSpacing, ref int offset)
        {
            MenuEntry entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(dimension.X + offset, dimension.Y);
            entry.Texture = InternalContentManager.GetTexture(name);
            entry.PressedTexture = InternalContentManager.GetTexture(name + "Selected");
            MenuEntries.Add(entry);
            offset += entry.Location.Width + buttonSpacing;
        }

        void entry_Selected(object sender, EventArgs e)
        {
            selectedtab = selectedEntry;
            InputManager.ClearInputForPeriod(300);

            if (selectedEntry == 2)
            {
                ((AbilitiesScreen)tabs[2]).RefreshAbilityList();
            }
        }

        public void Show()
        {
            InputManager.ClearInputForPeriod(500);
            GameplayScreen.Instance.Pause();
            GameplayScreen.Instance.ScreenManager.AddScreen(this);
            UpdateOrientation();
        }


        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2);
                int offset = 35;
                int buttonSpacing = 40;
                for (int i = 0; i < MenuEntries.Count; i++)
                {
                    MenuEntry entry = MenuEntries[i];
                    entry.Position = new Vector2(dimension.X + offset, dimension.Y + 5);
                    offset += entry.Location.Width + buttonSpacing;
                }
            }
            else
            {
                if (DungeonGame.pcMode)
                    dimension = new Rectangle((DungeonGame.ScreenSize.Width - 750) / 2 + borderWidth, (DungeonGame.ScreenSize.Height - 400) / 2 + DungeonGame.ScreenSize.Y + borderWidth, 750 - borderWidth * 2, 400 - borderWidth * 2);
                else
                    dimension = new Rectangle(borderWidth, borderWidth + 80, 800 - 80 - borderWidth * 2, 400 - borderWidth * 2);

                int offset = 30;
                int buttonSpacing = 25;
                for (int i = 0; i < MenuEntries.Count; i++)
                {
                    MenuEntry entry = MenuEntries[i];
                    entry.Position = new Vector2(dimension.X + 5, dimension.Y + offset);
                    offset += entry.Location.Height + buttonSpacing;
                }
            }
        }


        public override void OnRemoval()
        {
            GameplayScreen.Instance.UnPause();
            ParticleSystem.ClearParticles(tabs[0]);
            base.OnRemoval();
        }

        public override void HandleInput()
        {
            base.HandleInput();

            // handle macros
            // handle hotkeys
            if (InputManager.GetKeyboardAction(keyboardActionSet.Options))
            {
                if (selectedtab == 4)
                    ExitScreen();
                SelectedTab = 4;
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Inventory))
            {
                if (selectedtab == 0)
                    ExitScreen();
                SelectedTab = 0;
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Abilities))
            {
                if (selectedtab == 2)
                    ExitScreen();
                SelectedTab = 2;
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Stats))
            {
                if (selectedtab == 1)
                    ExitScreen();
                SelectedTab = 1;
                return;
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Quest))
            {
                if (selectedtab == 3)
                    ExitScreen();
                SelectedTab = 3;
                return;
            }
            else if ((InputManager.GetKeyboardAction(keyboardActionSet.Up) && (DungeonGame.pcMode || !DungeonGame.portraitMode)) ||
                     (InputManager.GetKeyboardAction(keyboardActionSet.Right) && (!DungeonGame.pcMode && DungeonGame.portraitMode)))
            {
                if (selectedtab > 0)
                    SelectedTab = selectedtab - 1;
                return;
            }
            else if ((InputManager.GetKeyboardAction(keyboardActionSet.Down) && (DungeonGame.pcMode || !DungeonGame.portraitMode)) ||
                     (InputManager.GetKeyboardAction(keyboardActionSet.Left) && (!DungeonGame.pcMode && DungeonGame.portraitMode)))
            {
                if (selectedtab < 4)
                    SelectedTab = selectedtab + 1;
                return;
            }

            tabs[selectedtab].HandleInput();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            UpdateOrientation();
            tabs[selectedtab].Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);
        }


        public override void Draw(GameTime gameTime)
        {
            // draw boarder
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            DrawBorder(
                new Rectangle(
                    (int)WindowCorner.X - borderWidth, 
                    (int)WindowCorner.Y - borderWidth, 
                    dimension.Width + borderWidth * 2, 
                    dimension.Height + borderWidth * 2),
                InternalContentManager.GetTexture("Blank"), 
                Color.Gray, 
                borderWidth, 
                Color.Black, 
                spriteBatch);

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(25, 25, 25)),
                                new Rectangle(
                                    (int)WindowCorner.X,
                                    (int)WindowCorner.Y,
                                    dimension.Width,
                                    58),
                                Color.White);
            }
            else
            {
                spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(25, 25, 25)),
                new Rectangle(
                    (int)WindowCorner.X,
                    (int)WindowCorner.Y,
                    58,
                    dimension.Height),
                Color.White);
            }

            spriteBatch.End();


            tabs[selectedtab].Draw(gameTime);
            base.Draw(gameTime);
        }


        public int SelectedTab
        {
            set 
            { 
                selectedtab = value;
                selectedEntry = value;
            }
        }


    }
}