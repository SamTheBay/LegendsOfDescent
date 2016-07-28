using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Text;
using System.Threading;

#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    public class ScreenManager
    {
        List<GameScreen> screens = new List<GameScreen>();
        List<GameScreen> screensToUpdate = new List<GameScreen>();
        SpriteBatch spriteBatch;
        bool isInitialized;
        bool traceEnabled;
        GameScreen lastTopFullScreen = null;


        // framerate variables
        bool showFrameRate = false;
        int prevFrames = 0;
        int currFrames = 0;
        int lastSeconds = 0;

        int prevFramesDraw = 0;
        int currFramesDraw = 0;
        int lastSecondsDraw = 0;

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        public bool TraceEnabled
        {
            get { return traceEnabled; }
            set { traceEnabled = value; }
        }

        public ScreenManager()
        {
            // we must set EnabledGestures before we can query for them, but
            // we don't assume the game wants to read them.
            TouchPanel.EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.None;

#if DEBUG
            showFrameRate = true;
#endif 
        }


        public void Initialize()
        {
            isInitialized = true;
        }


        public void LoadContent()
        {
            // Load content belonging to the screen manager.
            ContentManager content = DungeonGame.Instance.Content;

            spriteBatch = new SpriteBatch(DungeonGame.Instance.GraphicsDevice);

            // Tell each of the screens to load their content.
            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }


        public void UnloadContent()
        {
            // Tell each of the screens to unload their content.
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }


        public void Update(GameTime gameTime)
        {
            if (showFrameRate)
            {
                // keep track of framerate
                if (lastSeconds != gameTime.TotalGameTime.Seconds)
                {
                    lastSeconds = gameTime.TotalGameTime.Seconds;
                    prevFrames = currFrames;
                    currFrames = 0;
                }
                currFrames++;
            }

            InputManager.Update(gameTime);

#if WIN8
            // check if we need to put up the snap screen
            if (DungeonGame.ScreenSize.Width < 400 && !(screens[screens.Count - 1] is SnapScreen))
            {
                AddScreen(new SnapScreen());
            }
#endif

            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !DungeonGame.IsActive;
            bool coveredByOtherScreen = false;


            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[screensToUpdate.Count - 1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // check if this screen has aquired the top full state
                if (!screen.IsPopup && !coveredByOtherScreen && screen != lastTopFullScreen)
                {
                    screen.TopFullScreenAcquired();
                    lastTopFullScreen = screen;
                }

                // Update the screen.
                screen.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.Active)
                {
                    // If this is the first active screen we came across,
                    // give it a chance to handle input.
                    if (!otherScreenHasFocus)
                    {
                        screen.HandleInput();
                        otherScreenHasFocus = true;
                    }

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }
        }


        public void Draw(GameTime gameTime)
        {
            if (showFrameRate)
            {
                // keep track of framerate
                if (lastSecondsDraw != gameTime.TotalGameTime.Seconds)
                {
                    lastSecondsDraw = gameTime.TotalGameTime.Seconds;
                    prevFramesDraw = currFramesDraw;
                    currFramesDraw = 0;
                }
                currFramesDraw++;
            }

            //try
            //{
                foreach (GameScreen screen in screens)
                {
                    if (screen.ScreenState == ScreenState.Hidden)
                        continue;

                    screen.Draw(gameTime);
                }
            //}
            //catch (ObjectDisposedException)
            //{
            //    // sometimes this happens when we exit the game screen, and I can't figure out why.
            //    // For the time being it shouldn't be a problem to just catch and ignore it.
            //    Debug.WriteLine("Attempted to draw a disposed object");
            //    spriteBatch.Dispose();
            //    spriteBatch = new SpriteBatch(GraphicsDevice);
            //}

            // draw the default ad
            //DungeonGame.adControlManager.Draw(gameTime);


            if (showFrameRate)
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(Fonts.DescriptionFont, "Update: " + prevFrames.ToString(), new Vector2(10, 200), Color.Red);
                spriteBatch.DrawString(Fonts.DescriptionFont, "Draw: " + prevFramesDraw.ToString(), new Vector2(10, 230), Color.Red);
                spriteBatch.End();
            }
        }


        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;

            // If we have a graphics device, tell the screen to load content.
            if (isInitialized)
            {
                screen.LoadContent();
            }
            
            screens.Add(screen);

            // update the TouchPanel to respond to gestures this screen is interested in
            TouchPanel.EnabledGestures = InputManager.ConvertGestureType(screen.EnabledGestures);

            InputManager.ClearInputForPeriod(500);
        }


        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (isInitialized)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);

            if (screens.Count > 0)
                TouchPanel.EnabledGestures = InputManager.ConvertGestureType(screens[screens.Count - 1].EnabledGestures);
            else
                TouchPanel.EnabledGestures = InputManager.ConvertGestureType(GestureType.None);
        }


        public GameScreen[] GetScreens()
        {
            return screens.ToArray();
        }

        public bool HasScreen(GameScreen screen)
        {
            return screens.Contains(screen);
        }

        public void Message(string title, string text)
        {
            InputManager.ClearInputForPeriod(500);
            DungeonGame.ScreenManager.AddScreen(new HelpScreen(title, "", text));
        }

        public void Dialogue(string name, string text)
        {
            InputManager.ClearInputForPeriod(500);
            DungeonGame.ScreenManager.AddScreen(new DialogueScreen(null, Rectangle.Empty, name, text));
        }

        public void PlayerDialogue(string text)
        {
            InputManager.ClearInputForPeriod(500);
            DungeonGame.ScreenManager.AddScreen(new DialogueScreen(InternalContentManager.GetTexture("Portraits"), new Rectangle(0, 0, 110, 148), SaveGameManager.CurrentPlayer.Name, text));
        }
    }

#if !WIN8
    public class ScreenManagerComponent : DrawableGameComponent
    {
        public ScreenManager ScreenManager = new ScreenManager();

        public ScreenManagerComponent(Game game)
            : base(game)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
            ScreenManager.Initialize();
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            ScreenManager.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
            ScreenManager.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            ScreenManager.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            ScreenManager.Draw(gameTime);
        }
    }
#endif
}
