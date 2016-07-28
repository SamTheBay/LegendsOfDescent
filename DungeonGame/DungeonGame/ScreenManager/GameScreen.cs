
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{
    public enum ScreenState
    {
        Active,
        Hidden
    }


    public abstract class GameScreen
    {
        public bool IsPopup
        {
            get { return isPopup; }
            protected set { isPopup = value; }
        }

        bool isPopup = false;


        public bool IsMasterControllerSensitive
        {
            get { return isMasterControllerSensitive; }
            set { isMasterControllerSensitive = value; }
        }

        bool isMasterControllerSensitive = false;


        public ScreenState ScreenState
        {
            get { return screenState; }
            protected set { screenState = value; }
        }
        ScreenState screenState = ScreenState.Active;

        public virtual void AddNextScreen(GameScreen nextScreen)
        {
            screenManager.AddScreen(nextScreen);
        }

        public virtual void AddNextScreenAndExit(GameScreen nextScreen)
        {
            AddNextScreen(nextScreen);
            ExitScreen();
        }


        public bool IsActive
        {
            get
            {
                return !otherScreenHasFocus;
            }
        }

        protected bool otherScreenHasFocus;

        public ScreenManager ScreenManager
        {
            get { return screenManager; }
            internal set { screenManager = value; }
        }

        ScreenManager screenManager;

        public virtual void LoadContent() { }

        public virtual void UnloadContent() { }

        public virtual void TopFullScreenAcquired() { }

        public virtual void OnRemoval() { }


        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                      bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;

            // set state of whether we are hidden or not
            if (coveredByOtherScreen)
                screenState = ScreenState.Hidden;
            else
                screenState = ScreenState.Active;


        }

        public GestureType EnabledGestures
        {
            get { return enabledGestures; }
            protected set
            {
                enabledGestures = value;

                // the screen manager handles this during screen changes, but
                // if this screen is active and the gesture types are changing,
                // we have to update the TouchPanel ourself.
                if (ScreenState == ScreenState.Active)
                {
                    TouchPanel.EnabledGestures = InputManager.ConvertGestureType(value);
                }
            }
        }

        GestureType enabledGestures = GestureType.None;

        public virtual void HandleInput() { }

        public virtual void Draw(GameTime gameTime) { }


        public virtual void ExitScreen()
        {
                OnRemoval();
                ScreenManager.RemoveScreen(this);
        }


        static public void DrawBorder(Rectangle windowLocation, Texture2D texture, int boarderwidth, Color centerTint, SpriteBatch spriteBatch)
        {
            DrawBorder(windowLocation, texture, Color.White, boarderwidth, centerTint, spriteBatch);
        }


        static public void DrawBorder(Rectangle windowLocation, Texture2D texture, Color textureTint, int boarderwidth, Color centerTint, SpriteBatch spriteBatch)
        {
            // fill will boarder texture
            spriteBatch.Draw(texture, windowLocation, null, textureTint);

            // cover center with blank
            Texture2D blank = InternalContentManager.GetTexture("Blank");
            windowLocation.X += boarderwidth;
            windowLocation.Y += boarderwidth;
            windowLocation.Width -= boarderwidth * 2;
            windowLocation.Height -= boarderwidth * 2;
            spriteBatch.Draw(blank, windowLocation, null, centerTint);
        }

        static public PlayerIndex IntToPlayerIndex(int index)
        {
            if (index == 0)
                return PlayerIndex.One;
            else if (index == 1)
                return PlayerIndex.Two;
            else if (index == 2)
                return PlayerIndex.Three;
            else if (index == 3)
                return PlayerIndex.Four;
            return PlayerIndex.One;
        }


    }
}
