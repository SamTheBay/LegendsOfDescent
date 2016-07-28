using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

#if WIN8
using Windows.ApplicationModel.Store;
using System.Threading.Tasks;
#endif

namespace LegendsOfDescent
{
    class IntroScreen : MenuScreen
    {
        String version = "Alpha v0.5.3";
        String[] buttonNames = { "New Game", "Load Game", "Update Notes", "Alpha Feedback", "Credits"};
        bool reviewScreenShown = false;
        int titleHeight;
        Texture2D title = InternalContentManager.GetTexture("LoDTitle");
        GameScreen reviewScreen = new ReviewScreen();
        DateTime startTime;
        TimeSpan delayAdsSeconds = TimeSpan.FromSeconds(1);

        public IntroScreen()
            : base()
        {
            startTime = DateTime.Now;
            if (!DungeonGame.pcMode && DungeonGame.paid && DungeonGame.IsTrialModeCached)
            {
                version += " Trial";
            }
            else if (!DungeonGame.pcMode && DungeonGame.paid && !DungeonGame.IsTrialModeCached)
            {
                version += "+";
            }


            int centeredOffset = (DungeonGame.ScreenSize.Height - 600) / 2 + DungeonGame.ScreenSize.Y;
            int buttonStartHeight = centeredOffset + 270;
            int buttonSpacing = 57;
            int smallButtonSpacing = 47;
            titleHeight = centeredOffset + 10;
            int smallButtonHeight = 40;
            int buttonHeight = 50;

            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStartHeight, 450, buttonHeight);

            // add in the MenuEntries
            MenuEntry entry;
            for (int i = 0; i < buttonNames.Length; i++)
            {
                if (i >= 2)
                {
                    location.Height = smallButtonHeight;
                    entry = new MenuEntry(buttonNames[i], location, Color.Black, Color.White, 2, Fonts.DescriptionFont);
                    location.Y += smallButtonSpacing;
                }
                else
                {
                    location.Height = buttonHeight;
                    entry = new MenuEntry(buttonNames[i], location, Color.Black, Color.White, 3, Fonts.ButtonFont);
                    location.Y += buttonSpacing;
                    if (i == 1) location.Y += 7;
                }
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
            }

            //if (DungeonGame.pcMode)
            //{
            //    entry = new MenuEntry("Screen Resolution", location, Color.Black, Color.White, 2, Fonts.DescriptionFont);
            //    entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            //    MenuEntries.Add(entry);
            //    location.Y += smallButtonSpacing;
            //}

            if (!DungeonGame.paid)
            {
                entry = new MenuEntry("Go Ads Free", location, Color.Black, Color.White, 2, Fonts.DescriptionFont);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
            }

            if (DungeonGame.paid && !DungeonGame.pcMode && DungeonGame.IsTrialModeCached)
            {
                entry = new MenuEntry("Full Free Version", location, Color.Black, Color.White, 2, Fonts.DescriptionFont);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
            }

#if DEBUG
            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 50);
            entry.Texture = InternalContentManager.GetTexture("Clear");
            MenuEntries.Add(entry);
#endif
            
            AudioManager.audioManager.PlaySFX("MainMusic");

            // increment the number of plays for the user
            SaveGameManager.TotalPlays = SaveGameManager.TotalPlays + 1;
            SaveGameManager.PersistSettings();
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectedEntry == 0)
            {
                // new game
                AddNextScreen(new NewGameScreen());
            }
            else if (selectedEntry == 1)
            {
                // load game
                AddNextScreen(new LoadGameScreen());

            }
            else if (selectedEntry == 2)
            {
                // Update Notes
                AddNextScreen(new UpdateNotesScreen());
            }
            else if (selectedEntry == 3)
            {
                // Beta Feedback
                AddNextScreen(new FeedBackScreen());
            }
            else if (selectedEntry == 4)
            {
                // credits
                AddNextScreen(new CreditsScreen());
            }
            else if (selectedEntry == 5 && !DungeonGame.paid && !DungeonGame.pcMode)
            {
                // Jump to paid version on marketplace
#if WINDOWS_PHONE
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.ContentIdentifier = "735e9868-a32b-4ec5-90e0-7da24db338f3";
                task.ContentType = MarketplaceContentType.Applications;
                task.Show();
#endif
            }
            //else if (selectedEntry == 5 && DungeonGame.pcMode)
            //{
            //    AddNextScreen(new ResolutionSelectScreen());
            //}
            else if (selectedEntry == 5 && ((DungeonGame.paid && !DungeonGame.pcMode && DungeonGame.IsTrialModeCached) || (!DungeonGame.paid && DungeonGame.pcMode)))
            {
#if WINDOWS_PHONE
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.ContentIdentifier = "9e0d80d9-6f7b-4c37-808e-68e270dde4f0";
                task.ContentType = MarketplaceContentType.Applications;
                task.Show();
#else
#if WIN8
                DungeonGame.productsManager.PurchaseLicense("Disable Ads");
#endif
#endif
            }

            else if (selectedEntry == 6 && !DungeonGame.paid ||
                     selectedEntry == 6 && DungeonGame.pcMode ||
                     selectedEntry == 6 && !DungeonGame.pcMode && DungeonGame.paid && DungeonGame.IsTrialModeCached ||
                     selectedEntry == 6 && DungeonGame.paid && !DungeonGame.pcMode && !DungeonGame.IsTrialModeCached)
            {
                DungeonGame.TestMode = !DungeonGame.TestMode;
            }
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

#if WINDOWS_PHONE || WIN8
            if (SaveGameManager.TotalPlays == 5 && !reviewScreenShown)
            {
                ScreenManager.AddScreen(reviewScreen);
                reviewScreenShown = true;
            }
#endif

            // delay showing ads until initial loading is done. Was causing a crash in ad duplex ads for some reason
            if (DateTime.Now - startTime > delayAdsSeconds)
            {
                if (!DungeonGame.paid)
                {
                    DungeonGame.adControlManager.ShowAds = true;
                }
                else
                {
                    DungeonGame.adControlManager.ShowAds = false;
                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            ResetDimensions();
            DungeonGame.initialLoadComplete = true;

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            if (DungeonGame.TestMode)
            {
                spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(Color.Blue), DungeonGame.ScreenSize, null, Color.White);   
            }

            // draw the title
            spriteBatch.Draw(title, new Vector2((DungeonGame.ScreenSize.Width - title.Width) / 2, titleHeight), Color.White);
 
            // draw version
            spriteBatch.DrawString(Fonts.DescriptionFont, version, new Vector2(10, DungeonGame.ScreenSize.Y + DungeonGame.ScreenSize.Height - 25), Color.White);

            if (!string.IsNullOrEmpty(SaveGameManager.Username))
            {
                spriteBatch.DrawAlignedText(HorizontalAlignment.Right, VericalAlignment.Top, Fonts.DescriptionFont, "Logged in as " + SaveGameManager.Username + ".",
                    new Vector2(DungeonGame.ScreenSize.X + DungeonGame.ScreenSize.Width - 10, DungeonGame.ScreenSize.Y + DungeonGame.ScreenSize.Height - 25), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


        public override void ExitScreen()
        {
            base.ExitScreen();

            DungeonGame.Instance.Exit();
        }

        protected override void OnCancel()
        {
            base.OnCancel();

            DungeonGame.Instance.Exit();
        }


        public override void TopFullScreenAcquired()
        {
            base.TopFullScreenAcquired();
            DungeonGame.Instance.SetPortraitMode();

            AudioManager.audioManager.PlaySFX("MainMusic");
        }


        public void ResetDimensions()
        {
            int centeredOffset = (DungeonGame.ScreenSize.Height - 600) / 2 + DungeonGame.ScreenSize.Y;
            int buttonStartHeight = centeredOffset + 270;
            int buttonSpacing = 57;
            int smallButtonSpacing = 47;
            titleHeight = centeredOffset + 10;
            Vector2 position = new Vector2(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStartHeight);

            // add in the MenuEntries
            for (int i = 0; i < MenuEntries.Count; i++)
            {
                if (i >= 2)
                {
                    MenuEntries[i].Position = position;
                    position.Y += smallButtonSpacing;
                }
                else
                {
                    MenuEntries[i].Position = position;
                    position.Y += buttonSpacing;
                    if (i == 1) position.Y += 7;
                }
            }

            // check if the ads have been removed
            if (DungeonGame.paid && MenuEntries.Count >= 6 && MenuEntries[5].Text == "Go Ads Free")
            {
                MenuEntries.RemoveAt(5);
            }
        }

    }
}