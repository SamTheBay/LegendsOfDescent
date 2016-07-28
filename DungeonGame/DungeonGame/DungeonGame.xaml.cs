using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Input;
using Windows.ApplicationModel.Store;
using Windows.ApplicationModel;
using Windows.Storage;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.GamerServices;

namespace LegendsOfDescent
{

    public sealed partial class DungeonGame : SwapChainBackgroundPanel
    {
        static TimeSpan ThirtyFPS = TimeSpan.FromTicks(333333);
        static TimeSpan SixtyFPS = TimeSpan.FromTicks(166666);

        GameTimer gameTimer;
        GameTime gameTime = new GameTime();
        TimeSpan TargetElapsedTime = SixtyFPS;
        bool loaded = false;
        bool isActive = true;
        DateTime lostActiveTime = DateTime.Now;
        Vector2 screenSizePixel;

        DispatcherTimer dispatcherTimer;

        public static SharedGraphicsDeviceManager graphics;
        public ContentManager Content;
        public GraphicsDevice GraphicsDevice;
        public IServiceProvider Services;

        // Global Objects
        public static DungeonGame Instance;
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
        public static string ContentRoot = "Content";
        public static bool touchEnabled = true;
        public static bool joystickEnabled = true;
        public static bool touchSupported = true;
        public static bool isRT = false;
        public static ProductsManager productsManager;

        // Screen settings
        public static Rectangle ScreenSize = new Rectangle(0, 80, 480, 720);

        public DungeonGame()
        {
            this.Loaded += GamePage_Loaded;
            this.Unloaded += GamePage_Unloaded;
            this.InitializeComponent();
            Instance = this;
            pcMode = true;

            AnalyticsManagerWin8.Initialize();

#if WINRT
            isRT = true;

            // Currently RT must run in low memory mode to avoid crashing
            lowMemoryMode = true;
#else
            isRT = false;
#endif

            if (isRT)
            {
                TargetElapsedTime = ThirtyFPS;
            }

            // we aren't using the accelerometer right now, but the code is in the engine if we want it
            //Accelerometer.Initialize();

            // Check if we have a touch screen
            TouchCapabilities touchCap = new TouchCapabilities();
            touchSupported = touchCap.TouchPresent > 0;
            if (!touchSupported)
            {
                touchEnabled = false;
                joystickEnabled = false;
            }

            productsManager = new ProductsManager();

            adControlManager = new AdControlManager(false);
            adControlManager.AdGrid = grid;

            paid = productsManager.IsLiscenseValid("Disable Ads");
            adControlManager.ShowAds = false;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();
        }


        public async void dispatcherTimer_Tick(object sender, object e)
        {
            // store purchases must happen on the UI thread
            String productBuy = productsManager.GetNextPurchase();
            if (productBuy != null)
            {
                try
                {
#if DEBUG
                    await CurrentAppSimulator.RequestProductPurchaseAsync(productBuy, false);
#else
                    await CurrentApp.RequestProductPurchaseAsync(productBuy, false);
#endif
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }


        void GamePage_Unloaded(object sender, RoutedEventArgs e)
        {
            UnloadContent();
        }


        void GamePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!loaded)
            {
                graphics = SharedGraphicsDeviceManager.Current;
                graphics.PreferredBackBufferWidth = (int)this.ActualWidth;
                graphics.PreferredBackBufferHeight = (int)this.ActualHeight;
                graphics.SwapChainPanel = this;
                graphics.ApplyChanges();
                GraphicsDevice = graphics.GraphicsDevice;

                gameTimer = new GameTimer();
                gameTimer.UpdateInterval = TargetElapsedTime; // Variable frame rate

                gameTimer.Update += OnUpdate;
                gameTimer.Draw += OnDraw;

                this.SizeChanged += GamePage_SizeChanged;

                screenSizePixel = new Vector2((int)this.ActualWidth, (int)this.ActualHeight);
                if (pcMode)
                {
                    //IsMouseVisible = true;
                    ScreenSize = new Rectangle(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
                }
                else
                {
                    graphics.PreferredBackBufferWidth = 480;
                    graphics.PreferredBackBufferHeight = 800;

                    if (paid)
                    {
                        ScreenSize = new Rectangle(0, 0, 480, 800);
                    }
                }

                ScreenManager = new ScreenManager();
                ScreenManager.Initialize();

                TileSet.InitializeTileSets();

                LoadContent();

                loaded = true;
            }

        }

        private void LoadContent()
        {
            Content = ((App)App.Current).Content;
            Services = Content.ServiceProvider;

            ContentRoot = "Assets";
            Content.RootDirectory = ContentRoot;

            gameTimer.Start();

            Fonts.LoadContent(DungeonGame.Instance.Content);
            ScreenManager.LoadContent();

            adControlManager.Load();

            GameScreen screen = new InitialLoadScreen();
            ScreenManager.AddScreen(screen);
        }

        private void UnloadContent()
        {
            Debug.WriteLine("Unloading contents");
            gameTimer.Stop();

            ScreenManager.UnloadContent();
            Fonts.UnloadContent();
        }


        void GamePage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            screenSizePixel = new Vector2((int)this.ActualWidth, (int)this.ActualHeight);
            ScreenSize.Width = (int)this.ActualWidth;
            ScreenSize.Height = (int)this.ActualHeight;

            graphics.PreferredBackBufferWidth = (int)screenSizePixel.X;
            graphics.PreferredBackBufferHeight = (int)screenSizePixel.Y;
            graphics.ApplyChanges();
        }

        public void GamePage_Suspend()
        {
            Debug.WriteLine("Game Suspended");
            isActive = false;
            if (GameplayScreen.Instance != null && SaveGameManager.CurrentPlayer != null)
            {
                SaveGameManager.PersistPlayer();
                SaveGameManager.PersistDungeon(GameplayScreen.Dungeon);
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

            if (adControlManager != null)
            {
                adControlManager.UnLoad();
            }
        }

        public void GamePage_Resume()
        {
            if (isActive)
                return;

            Debug.WriteLine("Game Resumed");

            isActive = true;

            if (adControlManager != null)
            {
                adControlManager.Load();
            }

        }

        //Game Draw
        private void OnDraw(object sender, GameTimerEventArgs e)
        {
            bool endShot = false;
            if (takeScreenShot)
            {
                ScreenShot.StartScreenShot();
                endShot = true;
            }

            SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Color.Black);

            ScreenManager.Draw(gameTime);

            if (endShot)
            {
                ScreenShot.EndScreenShot();
                takeScreenShot = false;
            }

            adControlManager.Draw(gameTime);

        }

        //Game Update
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {
            gameTime.ElapsedGameTime = TargetElapsedTime;
            gameTime.TotalGameTime += TargetElapsedTime;
            ScreenManager.Update(gameTime);
            adControlManager.Update(gameTime);

            //if (InputManager.IsKeyTriggered(Keys.P))
            //    takeScreenShot = true;
        }



        public void Exit()
        {
            ((App)App.Current).Exit();
        }

        public static bool IsActive
        {
            get 
            {
                if (Instance != null)
                    return Instance.isActive & Window.Current.Visible;
                else
                    return false;
            }
        }

        public void SetPortraitMode()
        {
            // NO-OP in win8
        }

        public void SetLandscapeMode()
        {
            // NO-OP in win8
        }

        public void SetResolution(Vector2 resolution)
        {
            // NO-OP in win8
        }

        public bool PortraitMode
        {
            get { return portraitMode; }
            set { portraitMode = value; }
        }


        public TextBox TextInputBox
        {
            get { return TextInput; }
        }

    }
}
