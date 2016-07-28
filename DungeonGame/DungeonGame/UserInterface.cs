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
    public interface ISelectable
    {
        bool IsSelectable();
        void Select();
        MacroButton GetMacroButton(PlayerSprite player = null);
    }


    public interface IActiveIcon
    {
        Texture2D GetActiveIconTexture();
        Rectangle GetActiveIconSource();
        int GetActiveIconTime();
        bool IsEqualActiveIcon(IActiveIcon other);
        IActiveIcon CombineActiveIcons(IActiveIcon other);
        bool IsDebuff();
    }


    class UserInterface
    {
        public const int maxNumMacroButtons = 10;
#if WINDOWS_PHONE
        public  static int numMacroButtons = 5;
#else
        public static int numMacroButtons = 8;
#endif

        PlayerSprite player;

        Texture2D celticBoarder;
        Texture2D BlankBar;
        Texture2D FullBar;
        Vector2 healthBarCorner;
        Rectangle healthbarFill;
        Texture2D blank;
        Rectangle healthButton;

        Vector2 manaBarCorner;
        Rectangle manaBarFill;
        Rectangle manaButton;

        Texture2D buttonOutline;
        Texture2D buttonOutlineHighlight;
        Vector2 buttonPosition;
        Rectangle buttonRectangle;

        Texture2D cornerButton;
        public bool IsHoldButtonDown = false;
        List<IActiveIcon> activeIcons = new List<IActiveIcon>();
        int maxActiveIconsDrawn = 8;
        Vector2 activeIconStartLocation = new Vector2(10, 130 + DungeonGame.ScreenSize.Y);
        Timer fastFlashTimer = new Timer(200, TimerState.Running, TimerType.Auto);

        Texture2D questUpdateButton = InternalContentManager.GetTexture("Quest");
        Texture2D questUpdateButtonSelected = InternalContentManager.GetTexture("QuestSelected");
        Rectangle questUpdatebuttonLoc = new Rectangle(0, 0, 48, 48);
        Quests.Quest previousQuest = null;

        Texture2D menuButton = InternalContentManager.GetTexture("Menu");
        Texture2D menuButtonSelected = InternalContentManager.GetTexture("MenuSelect");
        Rectangle menubuttonLoc = new Rectangle(0, 0, 48, 48);

        Texture2D namesButton = InternalContentManager.GetTexture("names");
        Texture2D namesButtonSelected = InternalContentManager.GetTexture("namesSelect");
        Rectangle namesbuttonLoc = new Rectangle(0, 0, 48, 48);

        Joystick joystick;
        Point joystickSize = new Point(250, 250);
        Rectangle joystickActionSize = new Rectangle(0, 0, 100, 100);
        Texture2D actionButton;
        Texture2D actionButtonPress;
        bool actionPressed = false;
        bool actionPressedPrev = false;

        Vector2[] macroButtonPositions = new Vector2[maxNumMacroButtons];
        Rectangle[] macroButtonRectangles = new Rectangle[maxNumMacroButtons];

        public UserInterface()
        {
            blank = InternalContentManager.GetTexture("Blank");
            if (DungeonGame.pcMode)
            {
                BlankBar = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\emptybar260");
                FullBar = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\blankfullbar260");
                celticBoarder = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\celticBoarder260");
            }
            else
            {
                BlankBar = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\emptybar");
                FullBar = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\blankfullbar");
                celticBoarder = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\celticBoarder");
            }


            if (DungeonGame.pcMode)
            {
                healthBarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - FullBar.Width - 50, DungeonGame.ScreenSize.Height - FullBar.Height - 70);
                manaBarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - FullBar.Width + 50, DungeonGame.ScreenSize.Height - FullBar.Height - 70);
            }
            else
            {
                healthBarCorner = new Vector2(DungeonGame.ScreenSize.Width - 10 - FullBar.Width, DungeonGame.ScreenSize.Y + 10);
                manaBarCorner = new Vector2(DungeonGame.ScreenSize.Width - 60 - FullBar.Width * 2, DungeonGame.ScreenSize.Y + 10);
            }
            healthbarFill = new Rectangle(0, 0, 0, FullBar.Height);
            manaBarFill = new Rectangle(0, 0, 0, FullBar.Height);

            buttonOutline = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\SelectOutline");
            buttonOutlineHighlight = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\PlayerDisplay\\SelectOutlineHighlight");
            buttonPosition = new Vector2(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 70);
            buttonRectangle = new Rectangle(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 80, 80, 80);

            cornerButton = DungeonGame.Instance.Content.Load<Texture2D>("Textures\\UI\\cornerButton");

            healthButton = new Rectangle((int)healthBarCorner.X, (int)healthBarCorner.Y, FullBar.Width, FullBar.Height + 20);
            manaButton = new Rectangle((int)manaBarCorner.X, (int)manaBarCorner.Y, FullBar.Width, FullBar.Height + 20);

            joystick = new Joystick(new Rectangle(0, DungeonGame.ScreenSize.Height - joystickSize.Y, joystickSize.X, joystickSize.Y), Color.Red);
            joystickActionSize.X = DungeonGame.ScreenSize.Width - joystickActionSize.Width / 2 - 125;
            joystickActionSize.Y = DungeonGame.ScreenSize.Height - joystickActionSize.Height / 2 - 125;
            actionButton = InternalContentManager.GetTexture("ActionButton");
            actionButtonPress = InternalContentManager.GetTexture("ActionButtonPress");

            for (int i = 0; i < maxNumMacroButtons; i++)
            {
                macroButtonPositions[i] = new Vector2();
                macroButtonRectangles[i] = new Rectangle(0, 0, 80, 80);
            }
        }



        public void Initialize(PlayerSprite player)
        {
            this.player = player;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            // update locations for screen size change
            RefreshScreenPositions();

            //spriteBatch.Draw(InternalContentManager.GetTexture("Grayout"),
            //    new Rectangle(DungeonGame.ScreenSize.Width / 2 - numMacroButtons / 2 * 80, DungeonGame.ScreenSize.Height - 120, numMacroButtons * 80, 120), Color.White);

            // draw health bar
            int healthDeplete = (int)((float)FullBar.Width * (1f - ((float)player.Health) / (float)player.MaxHealth));
            spriteBatch.Draw(BlankBar, healthBarCorner, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);
            int healthInc = (int)((float)FullBar.Width * ((float)player.HealAmount) / (float)player.MaxHealth);
            if (healthInc > healthDeplete)
                healthInc = healthDeplete;
            healthbarFill.Width = FullBar.Width - healthDeplete + healthInc;
            spriteBatch.Draw(FullBar,
                healthBarCorner,
                healthbarFill,
                Color.Salmon, 0f, Vector2.Zero, 1f, SpriteEffects.None, .0015f);
            healthbarFill.Width = FullBar.Width - healthDeplete;
            spriteBatch.Draw(FullBar,
                healthBarCorner,
                healthbarFill,
                Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, .001f);

            spriteBatch.Draw(celticBoarder, new Vector2(healthBarCorner.X - 9, healthBarCorner.Y - 2), null, Color.LightSlateGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);


            // draw mana bar
            int manaDeplete = (int)((float)FullBar.Width * (1f - ((float)player.Mana) / (float)player.MaxMana));
            spriteBatch.Draw(BlankBar, manaBarCorner, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);
            int manaInc = (int)((float)FullBar.Width * ((float)player.AddManaAmount) / (float)player.MaxMana);
            if (manaInc > manaDeplete)
                manaInc = manaDeplete;
            manaBarFill.Width = FullBar.Width - manaDeplete + manaInc;
            spriteBatch.Draw(FullBar,
                manaBarCorner,
                manaBarFill,
                Color.LightBlue, 0f, Vector2.Zero, 1f, SpriteEffects.None, .0015f);
            manaBarFill.Width = FullBar.Width - manaDeplete;
            spriteBatch.Draw(FullBar,
                manaBarCorner,
                manaBarFill,
                Color.Blue, 0f, Vector2.Zero, 1f, SpriteEffects.None, .001f);

            spriteBatch.Draw(celticBoarder, new Vector2(manaBarCorner.X - 9, manaBarCorner.Y - 2), null, Color.LightSlateGray, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);

            // Draw hold button
            if (DungeonGame.touchEnabled && DungeonGame.joystickEnabled)
            {
                joystick.Draw(spriteBatch);

                Texture2D actionTex = InputManager.IsLocationPressed(joystickActionSize) ? actionButtonPress : actionButton;
                spriteBatch.Draw(actionTex,
                    joystickActionSize.Location.ToVector2(),
                    null,
                    Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else if (DungeonGame.touchEnabled)
            {
                spriteBatch.Draw(cornerButton,
                    new Vector2(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - cornerButton.Height),
                    IsHoldButtonDown ? new Color(255, 0, 0, 100) : new Color(255, 255, 255, 100));

                if (DungeonGame.pcMode)
                {
                    spriteBatch.Draw(cornerButton,
                        new Vector2(DungeonGame.ScreenSize.Width - cornerButton.Width, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - cornerButton.Height),
                        null,
                        IsHoldButtonDown ? new Color(255, 0, 0, 100) : new Color(255, 255, 255, 100), 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                }
            }

            // Draw adjustable buttons
            for (int i = 0; i < numMacroButtons; i++)
            {
                MacroButton macro = player.GetMacroButton(i);
                if (null != player.ActiveSelectable && macro.IsEqual(player.ActiveSelectable))
                {
                    spriteBatch.Draw(buttonOutlineHighlight, macroButtonPositions[i], null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .001f);
                }
                else
                {
                    spriteBatch.Draw(buttonOutline, macroButtonPositions[i], null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .001f);
                }

                if (DungeonGame.pcMode && !DungeonGame.joystickEnabled)
                {
                    // draw hotkey number
                    spriteBatch.DrawString(Fonts.DescriptionFont, (i + 1).ToString(), macroButtonPositions[i] + new Vector2(50, 45), Color.White);
                }

                macro.DrawIcon(spriteBatch, macroButtonPositions[i], buttonOutline.GetDimensions());
            }

            // Draw xp bar
            spriteBatch.Draw(blank, new Rectangle(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 5, DungeonGame.ScreenSize.Width, 5), null, Color.Gray, 0f, Vector2.Zero, SpriteEffects.None, .002f);
            spriteBatch.Draw(blank, new Rectangle(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 5, (int)(DungeonGame.ScreenSize.Width * player.PercentThroughLevel), 5), null, Color.Purple, 0f, Vector2.Zero, SpriteEffects.None, .001f);

            // Draw active icons
            int iconsDrawn = 0;
            Rectangle destination = new Rectangle((int)activeIconStartLocation.X, (int)activeIconStartLocation.Y, 24, 24);
            for (int i = 0; i < activeIcons.Count && iconsDrawn < maxActiveIconsDrawn; i++)
            {
                if (activeIcons[i] != null)
                {
                    int activeTime = activeIcons[i].GetActiveIconTime();
                    if (activeTime == 0)
                    {
                        activeIcons.RemoveAt(i);
                        continue;
                    }

                    if (activeTime > 5000 || fastFlashTimer.RemainingDuration > 100)
                    {
                        Color backcolor = Color.Black;
                        if (activeIcons[i].IsDebuff())
                            backcolor = Color.DarkRed;

                        GameScreen.DrawBorder(new Rectangle(destination.X - 1, destination.Y - 1, 26, 26), InternalContentManager.GetSolidColorTexture(Color.Gray), 1, backcolor, spriteBatch);
                        spriteBatch.Draw(activeIcons[i].GetActiveIconTexture(), destination, activeIcons[i].GetActiveIconSource(), Color.White, 0f, Vector2.Zero, SpriteEffects.None, .001f);
                    }
                    iconsDrawn++;
                    destination.Y += 27;
                }
            }

            List<ITimedEffect> activeTimedEffects = player.ActiveTimedEffects;
            for (int i = 0; i < activeTimedEffects.Count && iconsDrawn < maxActiveIconsDrawn; i++)
            {
                if (activeTimedEffects[i] != null)
                {
                    int activeTime = activeTimedEffects[i].GetActiveIconTime();
                    Color backcolor = Color.Black;
                    if (activeTimedEffects[i].IsDebuff())
                        backcolor = Color.DarkRed;

                    if (activeTime > 5000 || fastFlashTimer.RemainingDuration > 100)
                    {
                        GameScreen.DrawBorder(new Rectangle(destination.X - 1, destination.Y - 1, 26, 26), InternalContentManager.GetSolidColorTexture(Color.Gray), 1, backcolor, spriteBatch);
                        spriteBatch.Draw(activeTimedEffects[i].GetActiveIconTexture(), destination, activeTimedEffects[i].GetActiveIconSource(), Color.White, 0f, Vector2.Zero, SpriteEffects.None, .001f);
                    }
                    Fonts.DrawAlignedText(spriteBatch, HorizontalAlignment.Left, VericalAlignment.Top, Fonts.DescriptionFont, (activeTime / 1000).ToString(), destination.Location.ToVector2() + new Vector2(30, 0), activeTimedEffects[i].IsDebuff() ? Color.Red : Color.White, borderColor: Color.Black);
                    iconsDrawn++;
                    destination.Y += 27;
                }
            }


            // draw quest notification
            if (player.QuestLog.HasQuestUpdateNotification)
            {
                if (!InputManager.IsLocationPressed(questUpdatebuttonLoc))
                {
                    spriteBatch.Draw(questUpdateButton, questUpdatebuttonLoc, Color.White);
                }
                else
                {
                    spriteBatch.Draw(questUpdateButtonSelected, questUpdatebuttonLoc, Color.White);
                }

                if (previousQuest == null || !previousQuest.Equals(player.QuestLog.QuestForUpdateNotification))
                    ParticleSystem.AddParticles(questUpdatebuttonLoc.Center.ToVector2() + GameplayScreen.viewportCorner, ParticleType.Starburst, color: Color.White, numParticlesScale: 5f, sizeScale: 2f);
            }
            previousQuest = player.QuestLog.QuestForUpdateNotification;

            // draw menu button
            if (DungeonGame.pcMode)
            {
                if (!InputManager.IsLocationPressed(menubuttonLoc))
                {
                    spriteBatch.Draw(menuButton, menubuttonLoc, Color.White);
                }
                else
                {
                    spriteBatch.Draw(menuButtonSelected, menubuttonLoc, Color.White);
                }
            }

            if (DungeonGame.pcMode)
            {
                if (!InputManager.IsLocationPressed(namesbuttonLoc))
                {
                    spriteBatch.Draw(namesButton, namesbuttonLoc, Color.White);
                }
                else
                {
                    spriteBatch.Draw(namesButtonSelected, namesbuttonLoc, Color.White);
                }
            }
        }



        public void Update(GameTime gameTime)
        {
            fastFlashTimer.Update(gameTime);

            if (DungeonGame.pcMode)
            {
                if (DungeonGame.ScreenSize.Width > 1024 || DungeonGame.joystickEnabled)
                {
                    numMacroButtons = 8;
                }
                else
                {
                    numMacroButtons = 7;
                }
            }

            if (DungeonGame.joystickEnabled)
            {
                joystick.UpdateDirection();
                actionPressedPrev = actionPressed;
                actionPressed = InputManager.IsLocationPressed(joystickActionSize);
            }
        }



        public bool ActionTriggered
        {
            get { return actionPressed == true && actionPressedPrev == false; }
        }

        public void ButtonSelected(int index)
        {
            player.GetMacroButton(index).Select();
        }


        public bool CheckButtonPress(Vector2 press)
        {
            RefreshScreenPositions();

            if (DungeonGame.touchEnabled && !DungeonGame.joystickEnabled)
            {
                if (Vector2.Distance(press, new Vector2(0, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y)) <= 100.0f || 
                    (DungeonGame.pcMode && Vector2.Distance(press, new Vector2(DungeonGame.ScreenSize.Width, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y)) <= 100.0f))
                {
                    // account for the location of the hold button
                    return true;
                }
            }
            else if (DungeonGame.touchEnabled && DungeonGame.joystickEnabled)
            {
                if (joystick.Location.Contains(press.ToPoint()) ||
                    joystickActionSize.Contains(press.ToPoint()))
                {
                    // account for the location of joystick and action button
                    return true;
                }
            }

            // check macro buttons
            for (int j = 0; j < numMacroButtons; j++)
            {
                if (macroButtonRectangles[j].Contains((int)press.X, (int)press.Y))
                {
                    ButtonSelected(j);
                    return true;
                }
            }

            // check health bar buttons
            if (healthButton.Contains((int)press.X, (int)press.Y))
            {
                player.UseHealthPotion();
                return true;
            }
            if (manaButton.Contains((int)press.X, (int)press.Y))
            {
                player.UseManaPotion();
                return true;
            }


            // check quest notification button
            if (questUpdatebuttonLoc.Contains((int)press.X, (int)press.Y))
            {
                GameplayScreen.Instance.PauseScreen.SelectedTab = 3;
                GameplayScreen.Instance.PauseScreen.Show();
                player.StopMoving();
                return true;
            }

            // check for menu button
            if (DungeonGame.pcMode)
            {
                if (menubuttonLoc.Contains((int)press.X, (int)press.Y))
                {
                    GameplayScreen.Instance.PauseScreen.Show();
                    player.StopMoving();
                    return true;
                }
            }

            // check for names button
            if (DungeonGame.pcMode)
            {
                if (namesbuttonLoc.Contains((int)press.X, (int)press.Y))
                {
                    GameplayScreen.Instance.ShowAllItemNames();
                    return true;
                }
            }

            return false;
        }


        public bool IsButtonLocation(Vector2 location, ref int button)
        {
            for (int j = 0; j < numMacroButtons; j++)
            {
                if (macroButtonRectangles[j].Contains((int)location.X, (int)location.Y))
                {
                    button = j;
                    return true;
                }
            }
            return false;
        }


        public void AddActiveIcon(IActiveIcon icon)
        {
            for (int i = 0; i < activeIcons.Count; i++)
            {
                if (activeIcons[i].IsEqualActiveIcon(icon))
                {
                    activeIcons.RemoveAt(i);
                }
            }

            activeIcons.Add(icon);
        }


        private void RefreshScreenPositions()
        {
            if (DungeonGame.currentlyPortraitMode)
            {
                healthBarCorner.X = DungeonGame.ScreenSize.Width - 10 - FullBar.Width;
                healthBarCorner.Y = DungeonGame.ScreenSize.Y + 10;
                manaBarCorner.X = DungeonGame.ScreenSize.Width - 60 - FullBar.Width * 2;
                manaBarCorner.Y = DungeonGame.ScreenSize.Y + 10;
                healthButton.X = (int)healthBarCorner.X;
                healthButton.Y = (int)healthBarCorner.Y;
                manaButton.X = (int)manaBarCorner.X;
                manaButton.Y = (int)manaBarCorner.Y;
                buttonPosition.X = 100;
                buttonPosition.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 70;
                buttonRectangle.X = 90;
                buttonRectangle.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 80;
                if (DungeonGame.pcMode)
                {
                    buttonRectangle.X = (DungeonGame.ScreenSize.Width - numMacroButtons * 80) / 2;
                    buttonPosition.X = buttonRectangle.X + 10;
                }
                activeIconStartLocation.Y = 130 + DungeonGame.ScreenSize.Y;
                activeIconStartLocation.X = 10;
                questUpdatebuttonLoc.X = DungeonGame.ScreenSize.Width - 60;
                questUpdatebuttonLoc.Y = DungeonGame.ScreenSize.Y + 60;

                // Set adjustable button positions
                for (int i = 0; i < numMacroButtons; i++)
                {
                    macroButtonPositions[i] = buttonPosition;
                    macroButtonRectangles[i] = buttonRectangle;
                    buttonPosition.X += 80;
                    buttonRectangle.X += 80;
                }
            }
            else
            {
                healthBarCorner.X = DungeonGame.ScreenSize.Width - 10 - FullBar.Width;
                healthBarCorner.Y = DungeonGame.ScreenSize.Y + 10;
                manaBarCorner.X = 10;
                manaBarCorner.Y = DungeonGame.ScreenSize.Y + 10;
                healthButton.X = (int)healthBarCorner.X;
                healthButton.Y = (int)healthBarCorner.Y;
                manaButton.X = (int)manaBarCorner.X;
                manaButton.Y = (int)manaBarCorner.Y;
                buttonPosition.X = DungeonGame.ScreenSize.Width - 70;
                buttonPosition.Y = DungeonGame.ScreenSize.Y + DungeonGame.ScreenSize.Height - 100;
                buttonRectangle.X = DungeonGame.ScreenSize.Width - 80;
                buttonRectangle.Y = DungeonGame.ScreenSize.Y + DungeonGame.ScreenSize.Height - 110;
                activeIconStartLocation.Y = 160 + DungeonGame.ScreenSize.Y;
                activeIconStartLocation.X = 25;
                questUpdatebuttonLoc.X = DungeonGame.ScreenSize.Width - 60;
                questUpdatebuttonLoc.Y = DungeonGame.ScreenSize.Y + 60;
                if (!DungeonGame.pcMode)
                {
                    questUpdatebuttonLoc.X -= 80;
                }

                for (int i = 0; i < numMacroButtons; i++)
                {
                    macroButtonPositions[i] = buttonPosition;
                    macroButtonRectangles[i] = buttonRectangle;
                    buttonPosition.Y -= 80;
                    buttonRectangle.Y -= 80;
                }
            }

            if (DungeonGame.pcMode)
            {
                healthBarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 - FullBar.Width - 20, DungeonGame.ScreenSize.Height - FullBar.Height - 80);
                manaBarCorner = new Vector2(DungeonGame.ScreenSize.Width / 2 + 20, DungeonGame.ScreenSize.Height - FullBar.Height - 80);
                healthButton.X = (int)healthBarCorner.X;
                healthButton.Y = (int)healthBarCorner.Y;
                manaButton.X = (int)manaBarCorner.X;
                manaButton.Y = (int)manaBarCorner.Y;

                if (DungeonGame.joystickEnabled)
                {
                    healthBarCorner.Y += 60;
                    manaBarCorner.Y += 60;
                    healthButton.Y += 60;
                    manaButton.Y += 60;
                }

                menubuttonLoc.X = DungeonGame.ScreenSize.Width - 60;
                menubuttonLoc.Y = DungeonGame.ScreenSize.Y + 10;
                namesbuttonLoc.X = DungeonGame.ScreenSize.Width - 120;
                namesbuttonLoc.Y = DungeonGame.ScreenSize.Y + 10;
                questUpdatebuttonLoc.X = DungeonGame.ScreenSize.Width - 180;
                questUpdatebuttonLoc.Y = DungeonGame.ScreenSize.Y + 10;

                int macroButtonsWidth = (int)(80f * ((float)numMacroButtons / 2f));
                buttonPosition.X = DungeonGame.ScreenSize.Width / 2 - macroButtonsWidth - 10;
                buttonPosition.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 70;
                buttonRectangle.X = DungeonGame.ScreenSize.Width / 2 - macroButtonsWidth;
                buttonRectangle.Y = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 80;

                activeIconStartLocation.Y = 260 + DungeonGame.ScreenSize.Y;
                activeIconStartLocation.X = 10;

                if (DungeonGame.ScreenSize.Width <= 1300)
                {
                    menubuttonLoc.Y += 90;
                    questUpdatebuttonLoc.Y += 90;
                    namesbuttonLoc.Y += 90;
                }

                if (DungeonGame.joystickEnabled)
                {
                    macroButtonPositions[0] = new Vector2(DungeonGame.ScreenSize.Width - 200, DungeonGame.ScreenSize.Height - 80);
                    macroButtonRectangles[0] = new Rectangle((int)macroButtonPositions[0].X, (int)macroButtonPositions[0].Y, 80, 80);
                    macroButtonPositions[1] = new Vector2(DungeonGame.ScreenSize.Width - 250, DungeonGame.ScreenSize.Height - 160);
                    macroButtonRectangles[1] = new Rectangle((int)macroButtonPositions[1].X, (int)macroButtonPositions[1].Y, 80, 80);
                    macroButtonPositions[2] = new Vector2(DungeonGame.ScreenSize.Width - 200, DungeonGame.ScreenSize.Height - 240);
                    macroButtonRectangles[2] = new Rectangle((int)macroButtonPositions[2].X, (int)macroButtonPositions[2].Y, 80, 80);
                    macroButtonPositions[3] = new Vector2(DungeonGame.ScreenSize.Width - 110, DungeonGame.ScreenSize.Height - 240);
                    macroButtonRectangles[3] = new Rectangle((int)macroButtonPositions[3].X, (int)macroButtonPositions[3].Y, 80, 80);

                    buttonPosition = new Vector2(DungeonGame.ScreenSize.Width - 80, DungeonGame.ScreenSize.Height - 340);
                    buttonRectangle = new Rectangle((int)buttonPosition.X, (int)buttonPosition.Y, 80, 80);
                    for (int i = 4; i < numMacroButtons; i++)
                    {
                        macroButtonPositions[i] = buttonPosition;
                        macroButtonRectangles[i] = buttonRectangle;
                        buttonPosition.Y -= 80;
                        buttonRectangle.Y -= 80;
                    }
                }
                else
                {
                    for (int i = 0; i < numMacroButtons; i++)
                    {
                        macroButtonPositions[i] = buttonPosition;
                        macroButtonRectangles[i] = buttonRectangle;
                        buttonPosition.X += 80;
                        buttonRectangle.X += 80;
                    }
                }
            }

            joystick.Location = new Rectangle(0, DungeonGame.ScreenSize.Height - joystickSize.Y, joystickSize.X, joystickSize.Y);
            joystickActionSize.X = DungeonGame.ScreenSize.Width - joystickActionSize.Width / 2 - 125;
            joystickActionSize.Y = DungeonGame.ScreenSize.Height - joystickActionSize.Height / 2 - 125;
        }



        public Joystick Joystick
        {
            get { return joystick; }
        }
    }
}
