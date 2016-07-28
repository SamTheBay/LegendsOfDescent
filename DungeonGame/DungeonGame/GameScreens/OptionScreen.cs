using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegendsOfDescent
{
    class OptionScreen : PopUpScreen
    {
        PopUpScreen owner;
        PlayerSprite player;
        SpriteFont font = Fonts.ButtonFont;

        public OptionScreen(PopUpScreen owner, PlayerSprite player, Rectangle dimension)
            : base()
        {
            this.dimension = dimension;
            IsPopup = true;
            this.owner = owner;
            this.player = player;

            int buttonStartHeight = 90 + DungeonGame.ScreenSize.Y;
            int buttonSpacing = 85;
            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStartHeight, 450, 75);


            // add in the MenuEntries
            MenuEntry entry = new MenuEntry("Save + Exit", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonSpacing;

            string buttonText = "SFX ";
            if (AudioManager.sfxOn)
                buttonText += "On";
            else
                buttonText += "Off";

            entry = new MenuEntry(buttonText, location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonSpacing;

            buttonText = "Music ";
            if (AudioManager.musicOn)
                buttonText += "On";
            else
                buttonText += "Off";

            entry = new MenuEntry(buttonText, location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonSpacing;

            entry = new MenuEntry("I'm Stuck!", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonSpacing;

            entry = new MenuEntry("Screenshot", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonSpacing;

            if (!DungeonGame.pcMode)
            {
                buttonText = "";
                if (DungeonGame.portraitMode)
                    buttonText = "Portrait";
                else
                    buttonText = "Landscape";

                entry = new MenuEntry(buttonText, location);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
                location.Y += buttonSpacing;
            }
            else
            {
                buttonText = "";
                if (DungeonGame.touchEnabled)
                    buttonText = "Touch On";
                else
                    buttonText = "Touch Off";

                entry = new MenuEntry(buttonText, location);
                entry.IsActive = DungeonGame.touchSupported;
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                MenuEntries.Add(entry);
                location.Y += buttonSpacing;
            }

            if (DungeonGame.pcMode)
            {
                if (!DungeonGame.touchEnabled)
                {
                    DungeonGame.joystickEnabled = false;
                }

                buttonText = "";
                if (DungeonGame.joystickEnabled)
                    buttonText = "Joystick On";
                else
                    buttonText = "Joystick Off";

                entry = new MenuEntry(buttonText, location);
                entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                entry.IsActive = DungeonGame.touchSupported;
                MenuEntries.Add(entry);
                location.Y += buttonSpacing;
            }

            UpdateOrientation();
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectedEntry == 0)
            {
                // save and exit game
                SaveGameManager.PersistPlayer();
                SaveGameManager.PersistDungeon(GameplayScreen.Dungeon);
                owner.ExitScreen();
                GameplayScreen.Instance.ExitScreen();
            }
            else if (selectedEntry == 1)
            {
                AudioManager.sfxOn = !AudioManager.sfxOn;
                string buttonText = "SFX ";
                if (AudioManager.sfxOn)
                    buttonText += "On";
                else
                    buttonText += "Off";
                MenuEntries[1].Text = buttonText;
                SaveGameManager.PersistSettings();
            }
            else if (selectedEntry == 2)
            {
                AudioManager.musicOn = !AudioManager.musicOn;
                string buttonText = "Music ";
                if (AudioManager.musicOn)
                {
                    buttonText += "On";
                    if (GameplayScreen.Dungeon.MapType == "Town")
                    {
                        AudioManager.audioManager.PlaySFX("MainMusic");
                    }
                    else if (GameplayScreen.Dungeon.MapType == "Boss")
                    {
                        AudioManager.audioManager.PlaySFX("MainMusic3");
                    }
                    else
                    {
                        AudioManager.audioManager.PlaySFX("MainMusic2");
                    }
                }
                else
                {
                    buttonText += "Off";
                    AudioManager.audioManager.StopAllSFX("MainMusic");
                    AudioManager.audioManager.StopAllSFX("MainMusic2");
                    AudioManager.audioManager.StopAllSFX("MainMusic3");
                }
                MenuEntries[2].Text = buttonText;
                SaveGameManager.PersistSettings();
            }
            else if (selectedEntry == 3)
            {
                AddNextScreen(new UnstuckScreen(player, owner, GameplayScreen.Dungeon));

                // Use to skip levels for debugging
                //AddNextScreenAndExit(new StairsScreen(false, player, GameplayScreen.Dungeon));
            }
            else if (selectedEntry == 4)
            {
                GameplayScreen.takeScreenShot = true;
                owner.ExitScreen();
            }
            else if (selectedEntry == 5 && MenuEntries[5].IsActive)
            {
                if (DungeonGame.pcMode)
                {
                    DungeonGame.touchEnabled = !DungeonGame.touchEnabled;

                    String buttonText = "";
                    if (DungeonGame.touchEnabled)
                        buttonText = "Touch On";
                    else
                        buttonText = "Touch Off";
                    MenuEntries[5].Text = buttonText;

                    if (!DungeonGame.touchEnabled)
                    {
                        DungeonGame.joystickEnabled = false;
                        MenuEntries[6].Text = "Joystick Off";
                        MenuEntries[6].IsActive = false;
                    }
                    else if (DungeonGame.touchSupported)
                    {
                        MenuEntries[6].IsActive = true;
                    }
                }
                else
                {
                    DungeonGame.portraitMode = !DungeonGame.portraitMode;
                    String buttonText = "";
                    if (DungeonGame.portraitMode)
                        buttonText = "Portrait";
                    else
                        buttonText = "Landscape";
                    MenuEntries[5].Text = buttonText;

                    if (DungeonGame.portraitMode)
                        DungeonGame.Instance.SetPortraitMode();
                    else
                        DungeonGame.Instance.SetLandscapeMode();
                }

                SaveGameManager.PersistSettings();
            }
            else if (selectedEntry == 6 && MenuEntries[6].IsActive)
            {
                if (DungeonGame.pcMode)
                {
                    DungeonGame.joystickEnabled = !DungeonGame.joystickEnabled;

                    String buttonText = "";
                    if (DungeonGame.joystickEnabled)
                        buttonText = "Joystick On";
                    else
                        buttonText = "Joystick Off";
                    MenuEntries[6].Text = buttonText;
                }

                SaveGameManager.PersistSettings();
            }
        }



        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(0, 55);

                int buttonStartHeight = (int)WindowCorner.Y + 55;
                int buttonSpacing = 80;
                Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStartHeight, 450, 70);
                for (int i = 0; i < MenuEntries.Count; i++)
                {
                    MenuEntry entry = MenuEntries[i];
                    entry.Location = location;
                    location.Y += buttonSpacing;
                }

            }
            else
            {
                int borderWidth = 6;
                int buttonSize = 310;
                dimension = new Rectangle(borderWidth, borderWidth, 800 - 80 - borderWidth * 2 - 50, 400 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(60, 0);

                int buttonStartHeight = 30 + DungeonGame.ScreenSize.Y + (int)WindowCorner.Y;
                int buttonSpacing = 85;
                Rectangle location = new Rectangle(dimension.Width / 2 - buttonSize - 5 + (int)WindowCorner.X, buttonStartHeight, buttonSize, 75);
                for (int i = 0; i < MenuEntries.Count; i++)
                {
                    MenuEntry entry = MenuEntries[i];
                    entry.Location = location;
                    location.Y += buttonSpacing;

                    if (i == 3)
                    {
                        location.Y = buttonStartHeight;
                        location.X = dimension.Width / 2 + 5 + (int)WindowCorner.X;
                    }
                }
            }
        }



        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            UpdateOrientation();

            base.Draw(gameTime);
        }

    }
}
