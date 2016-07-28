using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE
using Microsoft.Advertising;
using Microsoft.Advertising.Mobile.Xna;
using WP7XNASDK;
using AdDuplex.Xna;
#endif

#if WIN8
using Microsoft.Advertising.WinRT.UI;
using AdDuplex.Controls;
using Windows.UI.Xaml.Controls;
#endif

namespace LegendsOfDescent
{
    public class AdControlManager
    {
        enum AdControlType
        {
            MSFT,
            Millenial,
            AdDuplex
        }

#if WINDOWS_PHONE 
        // MSFT Ad Control
        private DrawableAd bannerAd;
        
        // Millenial Ad Control
        MMAdView adView;

        // ad duplex
        AdManager adDuplex;

#endif

#if WIN8
        Grid adGrid = null;

        Microsoft.Advertising.WinRT.UI.AdControl w8AdControl = null;
        Microsoft.Advertising.WinRT.UI.AdControl w8AdControlSmall = null;
        Microsoft.Advertising.WinRT.UI.AdControl w8ActiveAdControl = null;  
        AdDuplex.Controls.AdControl w8AdDuplexAdControl = null;

        bool adInErrorState = false;
        
        const string w8AdUnit = "98832";
        const string w8TestAdUnit = "Image_728x90";
        const string w8AdUnitSmall = "103563";
        const string w8TestAdUnitSmall = "Image_292x60";
        const string w8AppID = "e7905d9d-87d4-4dbf-b3bd-61a7b68aa7c8";
        const string w8TestAppId = "test_client";

        const string w8AdDuplexID = "21295";

#endif

        bool anyAdServed = false;
        int adServeFailoverTime = 10000;
        AdControlType adType = AdControlType.MSFT;
        int adCycleDuration = 30000; // 30 seconds in the minimum based on the rules
        int adCycleErrorDuration = 3000;
        int adCycleElapsed = 0;
        DateTime startTime;
        TimeSpan delayAdsSeconds = TimeSpan.FromSeconds(0);

        bool testMode = false;
        bool showAds = true;
        Texture2D defaultAd;
        Texture2D defaultAd_292_60;
        Texture2D defaultAd_728_90;
        bool isLoaded = true;
        Rectangle adRect = new Rectangle();

        const string mainAdUnit = "27436";
        const string testAdUnit = "TextAd";
        static string[] trialAdUnits = { "77796", "77797", "77798", "77799", "77800", "77801", "77802", "77803", "77804",
                                         "77805", "77806", "77807", "77808", "77809", "77810", "77811", "77812", "77813",
                                         "77814", "77815", "77816", "77817", "77818"};


        public AdControlManager(bool testMode = true)
        {
            this.testMode = testMode;
            startTime = DateTime.Now;

            int adDuplexSplit = 10;
#if WIN8
            adDuplexSplit = 50;
#endif

            // roll for an ad engine
            int roll = Util.Random.Next(0, 100);
            if (roll < adDuplexSplit)
            {
                adType = AdControlType.AdDuplex;
            }
            else
            {
                adType = AdControlType.MSFT;
            }
        }

#if WINDOWS_PHONE
        public AdControlManager(GameComponentCollection Components, bool testMode = true) : this()
        {
            // Create an ad manager for this game
            if (adType == AdControlType.MSFT)
            {
                if (testMode)
                {
                    AdGameComponent.Initialize(DungeonGame.Instance, "test_client");
                }
                else
                {
                    AdGameComponent.Initialize(DungeonGame.Instance, "31ad7318-88d4-4c36-83e6-14b64f15ec9e");
                }
                Components.Add(AdGameComponent.Current);
            }
        }
#endif


        public void Load()
        {
            isLoaded = true;

#if WINDOWS_PHONE
            if (adType == AdControlType.MSFT)
            {
                if (testMode)
                {
                    bannerAd = AdGameComponent.Current.CreateAd(testAdUnit, new Rectangle(0, 0, 480, 80), false);
                }
                else
                {
                    // roll for trial ad unit
                    int roll = Util.Random.Next(0, 100);
                    if (roll < 5)
                    {
                        // we have one adunit for every category to keep tabs on which ones are giving the best cpms
                        bannerAd = AdGameComponent.Current.CreateAd(trialAdUnits[Util.Random.Next(0, trialAdUnits.Length)], new Rectangle(0, 0, 480, 80), false);
                    }
                    else
                    {
                        // this is our main ad unit which we will adjust for the best cpms
                        bannerAd = AdGameComponent.Current.CreateAd(mainAdUnit, new Rectangle(0, 0, 480, 80), false);
                    }
                }

                bannerAd.BorderEnabled = true;
                bannerAd.DropShadowEnabled = false;
                bannerAd.BorderColor = Color.White;
                bannerAd.Visible = true;
                bannerAd.ErrorOccurred += new EventHandler<AdErrorEventArgs>(bannerAd_ErrorOccurred);
                bannerAd.AdRefreshed += new EventHandler(bannerAd_AdRefreshed);
            }    
            else if (adType == AdControlType.Millenial)
            {
                adView = new MMAdView(DungeonGame.graphics.GraphicsDevice, new Vector2(0, 0), AdPlacement.BannerAdTop);
                adView.Apid = "60242";
                adView.RefreshTimer = 30;
                adView.DesiredAdHeight = 80;
                adView.DesiredAdWidth = 480;
            }
            else if (adType == AdControlType.AdDuplex)
            {
                adDuplex = new AdManager(DungeonGame.Instance, "4387");
                adDuplex.LoadContent();
                adDuplex.RefreshInterval = 30;
            }

#endif

            defaultAd = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/DefaultAd");
            defaultAd_292_60 = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/DefaultAd_292_60");
            defaultAd_728_90 = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/DefaultAd_728_90");
        }


        public void UnLoad()
        {
            isLoaded = false;
#if WINDOWS_PHONE

            // protect against ad control crash during resume
            if (adType == AdControlType.MSFT)
            {
                AdGameComponent.Current.RemoveAll();
            }

#endif
        }


#if WINDOWS_PHONE
        void bannerAd_AdRefreshed(object sender, EventArgs e)
        {
            anyAdServed = true;
        }


        void bannerAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            bannerAd.Refresh();
        }
#endif

        public void Update(GameTime gameTime)
        {
#if WINDOWS_PHONE
            if (adType == AdControlType.MSFT)
            {
                if (ShowAds && DungeonGame.takeScreenShot == false)
                {
                    bannerAd.Visible = true;
                }
                else
                {
                    bannerAd.Visible = false;
                }

                if (DungeonGame.currentlyPortraitMode)
                {
                    bannerAd.DisplayRectangle = new Rectangle(0, 0, 480, 80);
                }
                else
                {
                    bannerAd.DisplayRectangle = new Rectangle(800 / 2 - 480 / 2, 0, 480, 80);
                }

                adCycleElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (adCycleElapsed > adCycleDuration)
                {
                    adCycleElapsed = 0;
                    bannerAd.Refresh();
                }

                // handle failover
                if (!anyAdServed && gameTime.TotalGameTime.TotalMilliseconds > adServeFailoverTime)
                {
                    adType = AdControlType.AdDuplex;
                    adDuplex = new AdManager(DungeonGame.Instance, "4387");
                    adDuplex.LoadContent();
                    adDuplex.RefreshInterval = 30;
                    bannerAd.Visible = false;
                }
            }
            else if (adType == AdControlType.Millenial)
            {
                if (ShowAds)
                {
                    // Update the adView with the current game time
                    adView.Update(gameTime);
                }
            }
            else if (adType == AdControlType.AdDuplex)
            {
                if (ShowAds && DungeonGame.takeScreenShot == false)
                {
                    adDuplex.Visible = true;
                }
                else
                {
                    adDuplex.Visible = false;
                }

                adDuplex.Update(gameTime);
            }
#endif

#if WIN8

            if (adType == AdControlType.MSFT)
            {
                // swap to the correct ad control size
                if (DungeonGame.ScreenSize.Width < 728)
                {
                    w8ActiveAdControl = w8AdControlSmall;
                    w8AdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    w8ActiveAdControl = w8AdControl;
                    w8AdControlSmall.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }

                adCycleElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if ((adInErrorState && adCycleElapsed > adCycleErrorDuration) || adCycleElapsed > adCycleDuration)
                {
                    // there is a bug in refreshing msft ads, so switch to ad duplex after the refresh period
                    adCycleElapsed = 0;
                    w8ActiveAdControl.Refresh();
                    return;
                }

                if (w8AdDuplexAdControl != null)
                {
                    w8AdDuplexAdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                if (w8AdControl != null)
                {
                    if (ShowAds && DungeonGame.takeScreenShot == false)
                    {
                        w8ActiveAdControl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    }
                    else
                    {
                        w8ActiveAdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    }
                }   

                // handle failover
                if (!anyAdServed && gameTime.TotalGameTime.TotalMilliseconds > adServeFailoverTime)
                {
                    adType = AdControlType.AdDuplex;
                }

                if (DungeonGame.ScreenSize.Width < 1300 && DungeonGame.ScreenSize.Width >= 1024)
                {
                    w8ActiveAdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                }
                else
                {
                    w8ActiveAdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                }
            }
            else if (adType == AdControlType.AdDuplex)
            {
                // swap to the correct ad control size
                if (DungeonGame.ScreenSize.Width < 728)
                {
                    if (w8AdDuplexAdControl.Width != 292 || w8AdDuplexAdControl.Height != 60 || w8AdDuplexAdControl.Size != "292x60")
                    {
                        w8AdDuplexAdControl.Width = 292;
                        w8AdDuplexAdControl.Height = 60;
                        w8AdDuplexAdControl.Size = "292x60";
                    }
                }
                else
                {
                    if (w8AdDuplexAdControl.Width != 728 || w8AdDuplexAdControl.Height != 90 || w8AdDuplexAdControl.Size != "728x90")
                    {
                        w8AdDuplexAdControl.Width = 728;
                        w8AdDuplexAdControl.Height = 90;
                        w8AdDuplexAdControl.Size = "728x90";
                    }
                }

                if (w8AdControl != null)
                {
                    w8AdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                if (w8AdControlSmall != null)
                {
                    w8AdControlSmall.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                if (w8AdDuplexAdControl != null)
                {
                    if (ShowAds && DungeonGame.takeScreenShot == false)
                    {
                        if (w8AdDuplexAdControl.Visibility != Windows.UI.Xaml.Visibility.Visible)
                        {
                            w8AdDuplexAdControl.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        }
                    }
                    else
                    {
                        if (w8AdDuplexAdControl.Visibility != Windows.UI.Xaml.Visibility.Collapsed)
                        {
                            w8AdDuplexAdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        }
                    }
                }

                if (DungeonGame.ScreenSize.Width < 1300 && DungeonGame.ScreenSize.Width >= 1024)
                {
                    if (w8AdDuplexAdControl.HorizontalAlignment != Windows.UI.Xaml.HorizontalAlignment.Right)
                    {
                        w8AdDuplexAdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Right;
                    }
                }
                else
                {
                    if (w8AdDuplexAdControl.HorizontalAlignment != Windows.UI.Xaml.HorizontalAlignment.Center)
                    {
                        w8AdDuplexAdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                    }
                }
            }
#endif

            HandleInput();
        }

        public bool ShowAds
        {
            set { showAds = value; }
            get 
            { 
                bool showAdsLocal = showAds;
                if (DateTime.Now - startTime < delayAdsSeconds)
                {
                    showAdsLocal = false;
                }
                return showAdsLocal; 
            }
        }


        public void Draw(GameTime gameTime)
        {

            if (ShowAds)
            {

                // draw in a default add for when ads are not getting displayed by any network
                SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;

                spriteBatch.Begin();
                Texture2D currentDefaultAd = defaultAd;
                String currentAdString = "LegendsOfDescent.com";
                SpriteFont font = Fonts.HeaderFont;
                Vector2 adLocation = new Vector2(DungeonGame.ScreenSize.Width / 2 - currentDefaultAd.Width / 2, 0);
#if WIN8
                currentAdString = "Unlock Ads Free Version";
                if (DungeonGame.ScreenSize.Width >= 728)
                {
                    currentDefaultAd = defaultAd_728_90;
                }
                else
                {
                    currentDefaultAd = defaultAd_292_60;
                    font = Fonts.NameFont;
                }

                if (DungeonGame.ScreenSize.Width < 1300 && DungeonGame.ScreenSize.Width >= 1024)
                {
                    adLocation = new Vector2(DungeonGame.ScreenSize.Width - currentDefaultAd.Width, 0);
                }
                else
                {
                    adLocation = new Vector2(DungeonGame.ScreenSize.Width / 2 - currentDefaultAd.Width / 2, 0);
                }


#endif
                spriteBatch.Draw(currentDefaultAd, adLocation, Color.White);
                Fonts.DrawCenteredText(spriteBatch, font, currentAdString, adLocation + new Vector2(currentDefaultAd.Width / 2, currentDefaultAd.Height / 2), Color.Red);
                adRect = new Rectangle((int)adLocation.X, (int)adLocation.Y, currentDefaultAd.Width, currentDefaultAd.Height);

                spriteBatch.End();


#if WINDOWS_PHONE
                if (adType == AdControlType.Millenial)
                {
                    spriteBatch.Begin();
                    adView.Draw(gameTime);
                    spriteBatch.End();
                }
                else if (adType == AdControlType.AdDuplex)
                {
                    if (DungeonGame.currentlyPortraitMode)
                        adDuplex.Draw(spriteBatch, new Vector2(0, 0));
                    else
                        adDuplex.Draw(spriteBatch, new Vector2(800 / 2 - 480 / 2, 0));
                }
#endif
            }

        }


        public void HandleInput()
        {
            if (!ShowAds)
                return;

            if (InputManager.IsLocationTapped(adRect))
            {
#if WIN8
                if (adType != AdControlType.AdDuplex)
                    DungeonGame.productsManager.PurchaseLicense("Disable Ads");
#endif
            }
        }


#if WIN8

        void w8AdControl_IsEngagedChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Microsoft.Advertising.WinRT.UI.AdControl control = (Microsoft.Advertising.WinRT.UI.AdControl)sender;
            if (control.IsEngaged)
                AudioManager.audioManager.PauseMusic();
            else
                AudioManager.audioManager.ResumeMusic();
        }

        void w8AdControl_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            adInErrorState = true;
        }

        void w8AdControl_AdRefreshed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            anyAdServed = true;
            adInErrorState = false;
        }

        public Grid AdGrid
        {
            set
            {
                adGrid = value;

                w8AdControl = new Microsoft.Advertising.WinRT.UI.AdControl();
                w8AdControl.Width = 728;
                w8AdControl.Height = 90;
                w8AdControl.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                w8AdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                w8AdControl.Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
                w8AdControl.IsAutoRefreshEnabled = false;
                w8AdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                w8AdControl.AdRefreshed += w8AdControl_AdRefreshed;
                w8AdControl.ErrorOccurred += w8AdControl_ErrorOccurred;
                w8AdControl.IsEngagedChanged += w8AdControl_IsEngagedChanged;

                if (testMode)
                {
                    w8AdControl.AdUnitId = w8TestAdUnit;
                    w8AdControl.ApplicationId = w8TestAppId;
                }
                else
                {
                    w8AdControl.AdUnitId = w8AdUnit;
                    w8AdControl.ApplicationId = w8AppID;
                }

                adGrid.Children.Add(w8AdControl);
                w8AdControl.IsEnabled = false;


                w8AdControlSmall = new Microsoft.Advertising.WinRT.UI.AdControl();
                w8AdControlSmall.Width = 292;
                w8AdControlSmall.Height = 60;
                w8AdControlSmall.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                w8AdControlSmall.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                w8AdControlSmall.Margin = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
                w8AdControlSmall.IsAutoRefreshEnabled = false;
                w8AdControlSmall.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                w8AdControlSmall.AdRefreshed += w8AdControl_AdRefreshed;
                w8AdControlSmall.ErrorOccurred += w8AdControl_ErrorOccurred;
                w8AdControlSmall.IsEngagedChanged += w8AdControl_IsEngagedChanged;

                if (testMode)
                {
                    w8AdControlSmall.AdUnitId = w8TestAdUnitSmall;
                    w8AdControlSmall.ApplicationId = w8TestAppId;
                }
                else
                {
                    w8AdControlSmall.AdUnitId = w8AdUnit;
                    w8AdControlSmall.ApplicationId = w8AppID;
                }

                adGrid.Children.Add(w8AdControlSmall);
                w8AdControlSmall.IsEnabled = false;


                w8AdDuplexAdControl = new AdDuplex.Controls.AdControl();
                w8AdDuplexAdControl.Width = 728;
                w8AdDuplexAdControl.Height = 90;
                w8AdDuplexAdControl.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
                w8AdDuplexAdControl.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                w8AdDuplexAdControl.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                w8AdDuplexAdControl.AppId = w8AdDuplexID;
                w8AdDuplexAdControl.IsTest = testMode;
                w8AdDuplexAdControl.Size = "728x90";
 
                adGrid.Children.Add(w8AdDuplexAdControl);
            }
        }
#endif
    }
}
