using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{

    public abstract class MenuScreen : GameScreen
    {

        List<MenuEntry> menuEntries = new List<MenuEntry>();
        protected int selectedEntry = 0;
        protected bool isPlayerTuned = false;
        protected int controllerIndex = 0;
        protected int selectorIndex = 0;
        protected bool isActionable = true;
        private MenuEntry backButton;
        private bool backButtonActive = false;

        public bool OtherScreenHasFocus { get; set; }


        protected IList<MenuEntry> MenuEntries
        {
            get { return menuEntries; }
        }


        protected MenuEntry SelectedMenuEntry
        {
            get
            {
                if ((selectedEntry < 0) || (selectedEntry >= menuEntries.Count))
                {
                    return null;
                }
                return menuEntries[selectedEntry];
            }
        }


        public bool IsActionable
        {
            get { return isActionable; }
            set { isActionable = value; }
        }


        public MenuScreen()
        {
            EnabledGestures = GestureType.Tap;
        }


        public void ActivateBackButton(Vector2 position)
        {
            backButton = new MenuEntry("");
            backButton.Texture = InternalContentManager.GetTexture("Back");
            backButton.PressedTexture = InternalContentManager.GetTexture("BackSelect");
            backButton.Position = position;
            backButtonActive = true;
        }



        public override void HandleInput()
        {
            if (!isActionable)
                return;

            for (int i = 0; i < menuEntries.Count; i++)
            {
                if (InputManager.IsLocationTapped(menuEntries[i].Location))
                {
                    if (menuEntries[i].IsActive)
                        AudioManager.audioManager.PlaySFX("MenuSelect");
                    selectedEntry = i;
                    selectorIndex = i;
                    OnSelectEntry(i);
                }
            }
            if (InputManager.IsBackTriggered() || (backButtonActive && InputManager.IsLocationTapped(backButton.Location)))
            {
                OnCancel();
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            OtherScreenHasFocus = otherScreenHasFocus;
        }


        protected virtual void OnSelectEntry(int entryIndex)
        {
            menuEntries[entryIndex].OnSelectEntry();
        }


        protected virtual void OnCancel()
        {
            ExitScreen();
        }




        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            spriteBatch.Begin();

            // Draw each menu entry in turn.
            for (int i = 0; i < menuEntries.Count; i++)
            {
                MenuEntry menuEntry = menuEntries[i];

                bool isSelected = (i == selectedEntry);

                menuEntry.Draw(this, isSelected, gameTime);
            }

            if (backButtonActive)
            {
                backButton.Draw(this, false, gameTime);
            }

            spriteBatch.End();
        }

    }
}
