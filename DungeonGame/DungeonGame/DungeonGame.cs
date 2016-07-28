using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Diagnostics;
using System.IO;

#if !SILVERLIGHT
    using Microsoft.Xna.Framework.GamerServices;
#endif

#if WINDOWS_PHONE
    using Microsoft.Phone.Shell;
    using Microsoft.Phone.Info;
#endif


namespace LegendsOfDescent
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DungeonGame : Game
    {
        // Global Objects
        public static DungeonGame Instance;
        public static GraphicsDeviceManager graphics;
        public static ScreenManager ScreenManager;
        public static bool IsTrialModeCached = false;
        public static ItemManager itemManager;
        public static bool TestMode = false;
        public static AdControlManager adControlManager;
        public static bool freezeScreen = false;
        public static bool previousPDownState = false;
        public static bool portraitMode = true;
        public static bool currentlyPortraitMode = true;
        public static bool pcMode = false;
        public static bool initialLoadComplete = false;
        public static bool takeScreenShot = false;
        public static bool paid = false;
        public static bool fullScreen = false;
        public static bool lowMemoryMode = false;
        public static bool touchEnabled = true;
        public static bool joystickEnabled = true;
        public static bool touchSupported = true;
        public static bool isRT = true;
#if WIN8
        public static string ContentRoot = "Assets";
#else
        public static string ContentRoot = "Content";
#endif

        // Screen settings
        public static Rectangle ScreenSize = new Rectangle(0, 80, 480, 720);

        public DungeonGame()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);



#if WINDOWS_PHONE
            pcMode = false;

            // Disable screen sleep
            PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;

            // check if we are running on low memory
            long deviceTotalMemory = (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory");
            long deviceTotalMemoryMB = deviceTotalMemory / (1024 * 1024);
            if (deviceTotalMemoryMB <= 256)
            {
                lowMemoryMode = true;
            }
#else
            pcMode = true;
#endif

#if PAID
            paid = true;
#endif

#if !SILVERLIGHT
            IsTrialModeCached = Guide.IsTrialMode;
            Deactivated += new EventHandler<EventArgs>(DungeonGame_Deactivated);
#endif

#if !WIN8
            AnalyticsManager.Initialize(this);
#endif



            if (pcMode)
            {
                // Frame rate is 40 fps by default for PC.
                //TargetElapsedTime = TimeSpan.FromTicks(250000);
                // because we aren't using elapsed time in our calculations, speeding up frame rate speeds up the game.
                TargetElapsedTime = TimeSpan.FromTicks(333333);

                //graphics.PreferredBackBufferWidth = 800;
                //graphics.PreferredBackBufferHeight = 600;
                IsMouseVisible = true;
                ScreenSize = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            }
            else
            {
                // Frame rate is 30 fps by default for Windows Phone.
                TargetElapsedTime = TimeSpan.FromTicks(333333);

                graphics.PreferredBackBufferWidth = 480;
                graphics.PreferredBackBufferHeight = 800;
                graphics.IsFullScreen = true;
                graphics.SupportedOrientations = DisplayOrientation.Portrait;

                if (paid)
                {
                    ScreenSize = new Rectangle(0, 0, 480, 800);
                }
            }

            Content.RootDirectory = ContentRoot;

            // we aren't using hte accelerometer right now, but the code is in the engine if we want it
            //Accelerometer.Initialize();

            // add the screen manager
            var screenManagerComponent = new ScreenManagerComponent(this);
            ScreenManager = screenManagerComponent.ScreenManager;
            Components.Add(screenManagerComponent);

            adControlManager = new AdControlManager(Components, false);

            if (!pcMode && !paid)
            {
                adControlManager.ShowAds = true;
            }
            else
            {
                // TODO: find a way to monetize PC version (ads or should we make it paid?)
                adControlManager.ShowAds = false;
            }

            TileSet.InitializeTileSets();
        }



        // Catch the exiting event and save the players game
        void DungeonGame_Deactivated(object sender, EventArgs e)
        {
            if (GameplayScreen.Instance != null && SaveGameManager.CurrentPlayer != null)
            {
                SaveGameManager.PersistPlayer();
                SaveGameManager.PersistDungeon(GameplayScreen.Dungeon);
            }

            if (adControlManager != null)
            {
                adControlManager.UnLoad();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

#if SILVERLIGHT
            graphics.GraphicsDevice.HintClipEnable();
#endif
            Fonts.LoadContent(DungeonGame.Instance.Content);

            GameScreen screen = new InitialLoadScreen();
            ScreenManager.AddScreen(screen);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            base.LoadContent();

            adControlManager.Load();
        }


        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            Fonts.UnloadContent();

            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        //int loop = 0;
        protected override void Update(GameTime gameTime)
        {
            // Handle freeze screen logic for screen captures
            //if (Keyboard.GetState().IsKeyDown(Keys.P))
            //{
            //    if (!previousPDownState)
            //    {
            //        freezeScreen = !freezeScreen;
            //    }
            //}
            //previousPDownState = Keyboard.GetState().IsKeyDown(Keys.Home);
            //if (freezeScreen)
            //    return;

            // turn on to debug memory growth
            //loop++;
            //if (loop % 100 == 0)
            //{
            //    long deviceTotalMemory = (long)DeviceExtendedProperties.GetValue("DeviceTotalMemory");
            //    long applicationCurrentMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationCurrentMemoryUsage");
            //    long applicationPeakMemoryUsage = (long)DeviceExtendedProperties.GetValue("ApplicationPeakMemoryUsage");
            //    Debug.WriteLine("using " + ((float)applicationCurrentMemoryUsage / 1024f / 1024f).ToString() + " of " + ((float)deviceTotalMemory / 1024f / 1024f).ToString() +
            //        " with a peak of " + ((float)applicationPeakMemoryUsage / 1024f / 1024f).ToString());
            //}

            base.Update(gameTime);

            adControlManager.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            bool endShot = false;
            if (takeScreenShot)
            {
                ScreenShot.StartScreenShot();
                endShot = true;
            }

            graphics.GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            if (endShot)
            {
                ScreenShot.EndScreenShot();
                takeScreenShot = false;
            }


        }


#if !WIN8
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

            if (adControlManager != null)
            {
                adControlManager.Load();
            }

            // check if we have a game currently running
            if (GameplayScreen.Instance != null)
            {
                GameScreen[] screens = ScreenManager.GetScreens();

                // check if the gameplay screen is the active one
                if (screens[screens.Length - 1].Equals(GameplayScreen.Instance))
                {
                    // gameplay is active so we need to put on a resume screen to prevent jumping strait into action too fast
                    ScreenManager.AddScreen(new ResumePauseScreen());
                }
            }
        }
#endif



        public void SetPortraitMode()
        {
            if (!currentlyPortraitMode && !pcMode) 
            {
                graphics.PreferredBackBufferWidth = 480;
                graphics.PreferredBackBufferHeight = 800;
                if (paid)
                {
                    ScreenSize = new Rectangle(0, 0, 480, 800);
                }
                else
                {
                    ScreenSize = new Rectangle(0, 80, 480, 720);
                }
                graphics.SupportedOrientations = DisplayOrientation.Portrait;
                currentlyPortraitMode = true;
                graphics.ApplyChanges();
            }
        }

        public void SetLandscapeMode()
        {
            if (currentlyPortraitMode && !pcMode)
            {
                graphics.PreferredBackBufferWidth = 800;
                graphics.PreferredBackBufferHeight = 480;
                ScreenSize = new Rectangle(0, 0, 800, 480);
                graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
                currentlyPortraitMode = false;
                graphics.ApplyChanges();
            }
        }

        public void SetResolution(Vector2 resolution)
        {
            if (pcMode)
            {
                graphics.PreferredBackBufferWidth = (int)resolution.X;
                graphics.PreferredBackBufferHeight = (int)resolution.Y;
                ScreenSize = new Rectangle(0, 0, (int)resolution.X, (int)resolution.Y);
                graphics.IsFullScreen = fullScreen;
                graphics.ApplyChanges();
            }

        }

        public bool PortraitMode
        {
            get { return portraitMode; }
            set { portraitMode = value; }
        }

        public static new bool IsActive
        {
            get
            {
                if (Instance != null)
                    return ((Game)Instance).IsActive;
                else
                    return false;
            }
        }
    }
}
