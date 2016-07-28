using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using LegendsOfDescent.Quests;

namespace LegendsOfDescent
{
    class QuestScreen : PopUpScreen
    {
        PopUpScreen owner;
        PlayerSprite player;
        Texture2D arrowRight;
        HelpButton helpButton;
        int currentQuestIndex = 0;
        ContinueQuestion abandonQuestion = null;
        bool showCompleted = false;

        public IEnumerable<Quest> Quests
        {
            get
            {
                if (showCompleted)
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Complete;
                }
                else
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Active;
                }
            }
        }

        public int QuestCount
        {
            get
            {
                if (showCompleted)
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Complete.Count();
                }
                else
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Active.Count();
                }
            }
        }


        public Quest QuestAtCurrentIndex
        {
            get
            {
                if (showCompleted)
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Complete.ElementAtOrDefault(currentQuestIndex);
                }
                else
                {
                    return SaveGameManager.CurrentPlayer.QuestLog.Active.ElementAtOrDefault(currentQuestIndex);
                }
            }
        }

        public QuestScreen(PopUpScreen owner, PlayerSprite player, Rectangle dimension)
            : base()
        {
            this.dimension = dimension;
            this.IsPopup = true;
            this.owner = owner;
            this.player = player;
            arrowRight = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/ArrowRight");

            MenuEntry entry;

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(owner.WindowCorner.X + 10 - 12, dimension.Height / 2 - arrowRight.Height / 2);
            entry.Texture = arrowRight;
            entry.OwningPopup = this;
            entry.SpriteEffect = SpriteEffects.FlipHorizontally;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(480 - 60, dimension.Height / 2 - arrowRight.Height / 2);
            entry.Texture = arrowRight;
            entry.OwningPopup = this;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Texture = InternalContentManager.GetTexture("Delete");
            entry.PressedTexture = InternalContentManager.GetTexture("DeleteSelect");
            entry.Position = new Vector2(480 - 60, dimension.Height - 70);
            entry.OwningPopup = this;
            entry.HighlightLastSelected = false;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Texture = InternalContentManager.GetTexture("Check");
            entry.PressedTexture = InternalContentManager.GetTexture("CheckSelected");
            entry.Position = new Vector2(480 - 60, dimension.Height - 70);
            entry.OwningPopup = this;
            entry.HighlightLastSelected = false;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Texture = InternalContentManager.GetTexture("QuestSelected");
            entry.PressedTexture = InternalContentManager.GetTexture("QuestSelected");
            entry.Position = new Vector2(480 - 60, dimension.Height - 70);
            entry.OwningPopup = this;
            entry.HighlightLastSelected = false;
            MenuEntries.Add(entry);

            helpButton = new HelpButton(HelpScreens.Quests, Vector2.Zero);
        }

        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                if (currentQuestIndex > 0)
                {
                    currentQuestIndex--;
                }
                else if (currentQuestIndex == 0)
                {
                    currentQuestIndex = QuestCount - 1;
                }
            }
            else if (selectorIndex == 1)
            {
                if (currentQuestIndex < QuestCount - 1)
                {
                    currentQuestIndex++;
                }
                else if (currentQuestIndex == QuestCount - 1)
                {
                    currentQuestIndex = 0;
                }
            }
            else if (selectedEntry == 2)
            {
                Quest quest = Quests.ElementAtOrDefault(currentQuestIndex);
                if (quest != null && !quest.IsComplete && quest.IsAbandonable)
                {
                    abandonQuestion = new ContinueQuestion("Are you sure you want to abandon this quest?");
                    AddNextScreen(abandonQuestion);
                }
            }
            else if (selectedEntry == 3)
            {
                showCompleted = true;
                currentQuestIndex = 0;
                MenuEntries[3].Texture = InternalContentManager.GetTexture("CheckSelected");
                MenuEntries[3].PressedTexture = InternalContentManager.GetTexture("CheckSelected");
                MenuEntries[4].Texture = InternalContentManager.GetTexture("Quest");
                MenuEntries[4].PressedTexture = InternalContentManager.GetTexture("QuestSelected");
            }
            else if (selectedEntry == 4)
            {
                showCompleted = false;
                currentQuestIndex = 0;
                MenuEntries[3].Texture = InternalContentManager.GetTexture("Check");
                MenuEntries[3].PressedTexture = InternalContentManager.GetTexture("CheckSelected");
                MenuEntries[4].Texture = InternalContentManager.GetTexture("QuestSelected");
                MenuEntries[4].PressedTexture = InternalContentManager.GetTexture("QuestSelected");
            }
        }


        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (currentQuestIndex >= QuestCount && currentQuestIndex != 0)
            {
                currentQuestIndex = QuestCount - 1;
            }

            if (SaveGameManager.CurrentPlayer.QuestLog.HasQuestUpdateNotification)
            {
                showCompleted = false;
                for (int i = 0; i < QuestCount; i++)
                {
                    if (SaveGameManager.CurrentPlayer.QuestLog.QuestForUpdateNotification == Quests.ElementAtOrDefault(i))
                    {
                        currentQuestIndex = i;
                        break;
                    }
                }
                SaveGameManager.CurrentPlayer.QuestLog.ResetQuestUpdateNotification();
            }

            Quest quest = QuestAtCurrentIndex;
            if (quest == null || quest.IsComplete || !quest.IsAbandonable)
            {
                MenuEntries[2].IsActive = false;
            }
            else
            {
                MenuEntries[2].IsActive = true;
            }

            if (abandonQuestion != null && abandonQuestion.IsFinished == true)
            {
                if (abandonQuestion.Result == true)
                {
                    // abandon this quest
                    if (!quest.IsComplete && quest.IsAbandonable)
                    {
                        player.QuestLog.Quests.Remove(quest);
                        player.QuestLog.Quests.TrimExcess();
                    }

                    while (currentQuestIndex > player.QuestLog.Quests.Count && currentQuestIndex > 0)
                    {
                        currentQuestIndex--;
                    }
                }

                abandonQuestion = null;
            }
        }


        public override void HandleInput()
        {
            base.HandleInput();
            helpButton.HandleInput();
        }


        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2 - 30);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(0, 50);

                MenuEntries[0].Position = new Vector2(0, 280);
                MenuEntries[1].Position = new Vector2(dimension.Width - arrowRight.Width, 280);
                MenuEntries[2].Position = new Vector2(dimension.Width - 160, dimension.Height - 75);
                MenuEntries[3].Position = new Vector2(dimension.Width - 230, dimension.Height - 75);
                MenuEntries[4].Position = new Vector2(dimension.Width - 300, dimension.Height - 75);

                helpButton.Location = new Vector2(WindowCorner.X + dimension.Width - 90, WindowCorner.Y + dimension.Height - 75);
            }
            else
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, borderWidth, 800 - 80 - borderWidth * 2 - 50, 400 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(60, 0);

                MenuEntries[0].Position = new Vector2(0, dimension.Height / 2 - arrowRight.Height / 2);
                MenuEntries[1].Position = new Vector2(dimension.Width - arrowRight.Width, dimension.Height / 2 - arrowRight.Height / 2);
                MenuEntries[2].Position = new Vector2(dimension.Width - 180, dimension.Height - 65);
                MenuEntries[3].Position = new Vector2(dimension.Width - 270, dimension.Height - 65);
                MenuEntries[4].Position = new Vector2(dimension.Width - 360, dimension.Height - 65);

                helpButton.Location = new Vector2(WindowCorner.X + dimension.Width - 90, WindowCorner.Y + dimension.Height - 65);
            }
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            UpdateOrientation();

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            var fontColor = Color.White;

            string headerText = null;

            if (QuestCount > 0)
            {
                if (showCompleted)
                {
                    headerText = "Completed quest " + (currentQuestIndex + 1).ToString() + " of " + QuestCount;
                }
                else
                {
                    headerText = "Active quest " + (currentQuestIndex + 1).ToString() + " of " + QuestCount;
                }
            }
            else
            {
                if (showCompleted)
                {
                    headerText = "No complete quests";
                }
                else
                {
                    headerText = "No active quests";
                }
            }

            Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, headerText, new Vector2(WindowCorner.X + dimension.Width / 2, WindowCorner.Y + 35), fontColor);

            Rectangle border = new Rectangle((int)WindowCorner.X + 40, (int)WindowCorner.Y + 70, dimension.Width - 80, dimension.Height - 150);
            DrawBorder(border, InternalContentManager.GetTexture("Blank"), Color.Gray, 2, Color.Black, spriteBatch);

            Quest quest = QuestAtCurrentIndex;
            if (quest != null)
            {
                int currentX = (int)WindowCorner.X + 60;
                int currentY = (int)WindowCorner.Y + 80;

                foreach (string nameLine in Fonts.BreakTextIntoList(quest.Name, Fonts.HeaderFont, border.Width - 40))
                {
                    spriteBatch.DrawString(Fonts.HeaderFont, nameLine, new Vector2(currentX, currentY), fontColor);
                    currentY += Fonts.HeaderFont.LineSpacing;
                }

                currentY += 30;

                foreach (string descriptionLine in Fonts.BreakTextIntoList(quest.Description, Fonts.DescriptionFont, border.Width - 40))
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, descriptionLine, new Vector2(currentX, currentY), fontColor);
                    currentY += Fonts.DescriptionFont.LineSpacing;
                }

                currentY += 20;

                if (quest.IsComplete)
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, "Complete", new Vector2(currentX, currentY), Color.Green);
                }
                else
                {
                    spriteBatch.DrawString(Fonts.DescriptionFont, "Reward: " + quest.RewardDescription, new Vector2(currentX, currentY), fontColor);
                    currentY += Fonts.DescriptionFont.LineSpacing + 10;
                    spriteBatch.DrawString(Fonts.DescriptionFont, "Status: " + quest.Status, new Vector2(currentX, currentY), fontColor);
                }
            }

            if (QuestCount > 0)
            {
                Fonts.DrawAlignedText(
                    spriteBatch,
                    HorizontalAlignment.Left,
                    VericalAlignment.Middle,
                    Fonts.HeaderFont,
                    (currentQuestIndex + 1) + "/" + QuestCount,
                    new Vector2(WindowCorner.X + 40, WindowCorner.Y + dimension.Height - 50),
                    fontColor);
            }

            helpButton.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
