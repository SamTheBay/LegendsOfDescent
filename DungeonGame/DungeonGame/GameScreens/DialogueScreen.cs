using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{

    public interface IDialogueAction
    {
        void DialogueAction(int buttonSelected);
    }


    public class DialogueScreen : PopUpScreen
    {
        // original data
        Texture2D portrait;
        Rectangle portraitSource;
        String speakerName;
        String text;

        // how to draw
        Rectangle dimension = new Rectangle((DungeonGame.ScreenSize.Width - Math.Min(700, DungeonGame.ScreenSize.Width)) / 2, 
                                            DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 200, 
                                            Math.Min(700, DungeonGame.ScreenSize.Width), 
                                            200);
        int borderWidth = 6;
        int edgeBuffer = 20;
        SpriteFont font = Fonts.DescriptionFont;

        // crunched data
        List<String> lines = new List<String>();
        int currentTopLine = 0;
        int lineheight;
        int linesPerScreen;

        // actionable dialogue
        string[] buttonText = { "Accept", "Reject" };
        IDialogueAction actionCallback = null;

        public DialogueScreen(Texture2D portrait, Rectangle portraitSource, String speakerName, String text)
            : base()
        {
            this.portrait = portrait;
            this.portraitSource = portraitSource;
            this.speakerName = speakerName;
            this.text = text;

            // crunch the lines
            int textWidth = dimension.Width - borderWidth * 2 - edgeBuffer * 2;
            if (portrait != null)
            {
                textWidth -= portraitSource.Width + edgeBuffer;
            }

            lineheight = (int)font.MeasureString(speakerName).Y;
            linesPerScreen = dimension.Height - borderWidth * 2 - edgeBuffer * 2;
            linesPerScreen /= lineheight;
            linesPerScreen--;

            String[] del = { "\\n" };
            String[] splitLines = text.Split(del, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitLines.Length; i++)
            {
                List<String> tempLines = Fonts.BreakTextIntoList(splitLines[i], font, textWidth);
                for (int j = 0; j < tempLines.Count; j++)
                {
                    lines.Add(tempLines[j]);
                }
                while (lines.Count % linesPerScreen != 0)
                {
                    lines.Add("");
                }
            }

            GameplayScreen.Instance.Pause();
        }


        public void SetActionable(IDialogueAction actionCallback, String[] buttonText)
        {
            this.actionCallback = actionCallback;
            this.buttonText = buttonText;
        }

        protected override void OnCancel()
        {
            GameplayScreen.Instance.UnPause();
            base.OnCancel();
            ExitScreen();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (InputManager.IsLocationTapped(dimension) || InputManager.IsBackTriggered())
            {
                if (currentTopLine + linesPerScreen >= lines.Count)
                {
                    if (actionCallback != null)
                    {
                        currentTopLine = lines.Count;
                        if (MenuEntries.Count == 0)
                        {
                            Rectangle location = new Rectangle(dimension.X + dimension.Width / 2 - 175, dimension.Y + dimension.Height / 2 - 35, 350, 50);
                            MenuEntry entry = new MenuEntry(buttonText[0], location);
                            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                            MenuEntries.Add(entry);
                            location.Y += 60;

                            entry = new MenuEntry(buttonText[1], location);
                            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                            MenuEntries.Add(entry);
                        }
                    }
                    else
                    {
                        // we have shown all the text
                        GameplayScreen.Instance.UnPause();
                        ExitScreen();
                    }

                }
                else
                {
                    // next screen
                    currentTopLine += linesPerScreen;
                }
            }
        }


        void entry_Selected(object sender, EventArgs e)
        {
            actionCallback.DialogueAction(selectedEntry);
            GameplayScreen.Instance.UnPause();
            ExitScreen();
        }


        public override void Draw(GameTime gameTime)
        {
            WindowCorner = dimension.Location.ToVector2();

            // draw the info about this item for the user
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the boarder
            GameScreen.DrawBorder(dimension, InternalContentManager.GetTexture("Blank"), Color.Red, borderWidth, Color.Black, spriteBatch);

            // set starting location
            Vector2 offset = new Vector2(dimension.X + borderWidth + edgeBuffer, dimension.Y + borderWidth + edgeBuffer);
            if (portrait != null)
            {
                offset.X += portraitSource.Width + edgeBuffer;
            }

            // write name
            spriteBatch.DrawString(font, speakerName, offset, Color.Red);
            offset.Y += lineheight + 10;

            // loop through lines drawing them
            for (int i = currentTopLine; i < lines.Count && i < currentTopLine + linesPerScreen; i++)
            {
                spriteBatch.DrawString(font, lines[i], offset, Color.White);
                offset.Y += lineheight;
            }

            if (portrait != null)
            {
                offset = new Vector2(dimension.X + borderWidth + edgeBuffer, dimension.Y + borderWidth + edgeBuffer);
                spriteBatch.Draw(portrait, offset, portraitSource, Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }


    }
}
