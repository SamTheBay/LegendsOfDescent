﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.ApplicationSettings;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.GamerServices;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace LegendsOfDescent
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        public ContentManager Content { get; set; }
        public GameServiceContainer Services { get; set; }
        public DungeonGame gamePage;


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.InitializeXNA();
            this.Suspending += OnSuspending;
            this.Resuming += App_Resuming;
        }


        void App_Resuming(object sender, object e)
        {
            gamePage.GamePage_Resume();
        }


        private void InitializeXNA()
        {
            // Create the service provider
            Services = new GameServiceContainer();

            // Add the graphics device  manager.
            Services.AddService(typeof(IGraphicsDeviceService), new SharedGraphicsDeviceManager());

            // Create the ContentManager so the application can load precompiled assets
            Content = new ContentManager(Services, "GameAssets");
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Add privacy policy to settings pane
            SettingsPane.GetForCurrentView().CommandsRequested += GroupedItemsPage_CommandsRequested;

            // Do not repeat app initialization when already running, just ensure that
            // the window is active
            if (gamePage != null)
            {
                gamePage.GamePage_Suspend();
                gamePage.GamePage_Resume();
            }
            else
            {
                //Create a new game page
                gamePage = new DungeonGame();
            }

            // Place the frame in the current Window and ensure that it is active
            Window.Current.Content = gamePage;
            Window.Current.Activate();
        }


        public static void AddSettingsCommands(SettingsPaneCommandsRequestedEventArgs args)
        {
            args.Request.ApplicationCommands.Clear();

            SettingsCommand facebook = new SettingsCommand("facebook", "Facebook", (uiCommand) =>
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.facebook.com/pages/Legends-Of-Descent/110521525719792"));
            });
            args.Request.ApplicationCommands.Add(facebook);

            SettingsCommand forums = new SettingsCommand("forums", "Forums", (uiCommand) =>
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://legendsofdescent.com/board"));
            });
            args.Request.ApplicationCommands.Add(forums);

            SettingsCommand privacyPref = new SettingsCommand("privacyPref", "Privacy Policy", (uiCommand) =>
            {
                Windows.System.Launcher.LaunchUriAsync(new Uri("http://LegendsOfDescent.com/privacy"));
            });
            args.Request.ApplicationCommands.Add(privacyPref);
        }


        void GroupedItemsPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            AddSettingsCommands(args);
        }


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            gamePage.GamePage_Suspend();
        }
    }
}
