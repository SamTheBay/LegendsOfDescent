using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{

    public class DescriptionLine
    {
        public string text;
        public SpriteFont font;
        public Vector2 dimensions;
        public Color color;
        public Texture2D texture;
        public Rectangle textureSource;
        public string buttonText;
        public int buttonIndex;
    }


    public class DescriptionScreen : PopUpScreen
    {
        List<DescriptionLine> lines = new List<DescriptionLine>();
        protected float height;
        protected float width;
        protected int buffer = 20;
        protected int border = 12;
        protected int maxWidth = 400;
        List<bool> buttonPressed = new List<bool>();
        bool actionHandled = true;
        bool isClosed = true;
        protected bool exitOnTouch = true;
        protected bool drawBorder = true;
        protected Point BorderAdjust = new Point(0, 0);
        protected bool showSS = false;
        protected Vector2 SSLoc = new Vector2(20, 20);
        protected Texture2D SSTex = InternalContentManager.GetTexture("Screenshot");
        protected Texture2D SSTexSelect = InternalContentManager.GetTexture("ScreenshotSelect");

        public DescriptionScreen()
            : base()
        {
        }


        public void Initialize(ItemSprite item)
        {
            lines.Clear();
            buttonPressed.Clear();
            MenuEntries.Clear();
            width = 0;
            height = 0;
            item.AddDescriptionDetails(this);
            actionHandled = false;
            isClosed = false;
        }


        public void Initialize(Ability ability)
        {
            lines.Clear();
            buttonPressed.Clear();
            MenuEntries.Clear();
            width = 0;
            height = 0;
            ability.AddDescriptionDetails(this);
            actionHandled = false;
            isClosed = false;
        }

        public void Initialize()
        {
            lines.Clear();
            buttonPressed.Clear();
            MenuEntries.Clear();
            width = 0;
            height = 0;
            actionHandled = false;
            isClosed = false;
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (showSS && InputManager.IsLocationTapped(new Rectangle((int)(SSLoc.X + WindowCorner.X), (int)(SSLoc.Y + WindowCorner.Y), 48, 48)))
            {
                // take a screenshot
                DungeonGame.takeScreenShot = true;
            }
            else if ((InputManager.IsBackTriggered() || (exitOnTouch && InputManager.IsLocationTapped(DungeonGame.ScreenSize))))
            {
                ExitScreen();
            }

        }

        public override void ExitScreen()
        {
            isClosed = true;
            base.ExitScreen();
        }

        public void AddTexture(Texture2D texture, Rectangle textureSource)
        {
            DescriptionLine line = new DescriptionLine();
            line.texture = texture;
            line.textureSource = textureSource;
            line.dimensions = new Vector2(textureSource.Width, textureSource.Height);
            lines.Add(line);

            // adjust bounds
            if (line.dimensions.X > width)
            {
                width = line.dimensions.X;
            }
            height += line.dimensions.Y;
        }


        public void AddButton(string text)
        {
            DescriptionLine line = new DescriptionLine();
            line.buttonText = text;
            line.buttonIndex = buttonPressed.Count;
            buttonPressed.Add(false);
            line.dimensions = new Vector2(300, 50);
            lines.Add(line);

            // adjust bounds
            if (line.dimensions.X > width)
            {
                width = line.dimensions.X;
            }
            height += line.dimensions.Y;
        }



        public void AddLine(String text, SpriteFont font, Color color)
        {
            // check if we need to split the next line
            List<String> textLines = Fonts.BreakTextIntoList(text, font, maxWidth);

            for (int i = 0; i < textLines.Count; i++)
            {
                DescriptionLine line = new DescriptionLine();
                line.text = textLines[i];
                line.font = font;
                line.color = color;
                line.dimensions = font.MeasureString(textLines[i]);

                // adjust bounds
                if (line.dimensions.X > width)
                {
                    width = line.dimensions.X;
                }
                height += line.dimensions.Y;

                lines.Add(line);
            }
        }


        public void AddSpace(float space)
        {
            DescriptionLine line = new DescriptionLine();
            line.text = null;
            line.texture = null;
            line.dimensions = new Vector2(0, space);

            // adjust bounds
            if (line.dimensions.X > width)
            {
                width = line.dimensions.X;
            }
            height += line.dimensions.Y;

            lines.Add(line);
        }



        public void SetFinalize()
        {
            // set starting location
            Vector2 offset = new Vector2(0, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y - height / 2);

            // loop through lines drawing them
            for (int i = 0; i < lines.Count; i++)
            {
                offset.X = DungeonGame.ScreenSize.Width / 2 - lines[i].dimensions.X / 2;

                if (lines[i].texture != null)
                {
                    // skip
                }
                else if (lines[i].buttonText != null)
                {
                    Rectangle location = new Rectangle((int)offset.X, (int)offset.Y, 300, 50);
                    if (!DungeonGame.currentlyPortraitMode && !DungeonGame.pcMode && !DungeonGame.paid)
                    {
                        location.Y += 40;
                    }
                    MenuEntry entry = new MenuEntry(lines[i].buttonText, location);
                    entry.Selected += new EventHandler<EventArgs>(entry_Selected);
                    MenuEntries.Add(entry);
                }
                else if (!string.IsNullOrEmpty(lines[i].text))
                {
                    // skip
                }

                offset.Y += lines[i].dimensions.Y;
            }
        }

        public virtual void entry_Selected(object sender, EventArgs e)
        {
            buttonPressed[selectedEntry] = true;
            ExitScreen();
        }


        public override void Draw(GameTime gameTime)
        {
            SetWindowCorner(
                DungeonGame.ScreenSize.Width / 2 - (int)width / 2 - buffer - border - BorderAdjust.X / 2,
                DungeonGame.ScreenSize.Height / 2 - (int)height / 2 - border - buffer + DungeonGame.ScreenSize.Y - BorderAdjust.Y / 2);

            if (!DungeonGame.portraitMode && !DungeonGame.pcMode && !DungeonGame.paid)
            {
                MoveWindowCorner(40, 0);
            }

            // draw the info about this item for the user
            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            // draw the boarder
            if (drawBorder)
            {
                GameScreen.DrawBorder(new Rectangle((int)WindowCorner.X,
                    (int)WindowCorner.Y,
                    (int)width + buffer * 2 + border * 2 + BorderAdjust.X,
                    (int)height + buffer * 2 + border * 2 + BorderAdjust.Y),
                    InternalContentManager.GetTexture("Blank"), Color.Red, 6, Color.Black, spriteBatch);
            }

            // set starting location
            Vector2 offset = new Vector2(0, DungeonGame.ScreenSize.Height / 2 + DungeonGame.ScreenSize.Y - height / 2);
            if (!DungeonGame.portraitMode && !DungeonGame.pcMode && !DungeonGame.paid)
            {
                offset.Y += 40;
            }
            
            // loop through lines drawing them
            for (int i = 0; i < lines.Count; i++)
            {
                offset.X = DungeonGame.ScreenSize.Width / 2 - lines[i].dimensions.X / 2;

                if (lines[i].texture != null)
                {
                    spriteBatch.Draw(lines[i].texture, offset, lines[i].textureSource, Color.White);
                }
                else if (lines[i].buttonText != null)
                {
                    // skip
                }
                else if (!string.IsNullOrEmpty(lines[i].text))
                {
                    spriteBatch.DrawString(lines[i].font, lines[i].text, offset, lines[i].color);
                }

                offset.Y += lines[i].dimensions.Y;
            }

            if (showSS)
            {
                if (!InputManager.IsLocationPressed(new Rectangle((int)(SSLoc.X + WindowCorner.X), (int)(SSLoc.Y + WindowCorner.Y), 48, 48)))
                {
                    spriteBatch.Draw(SSTex, SSLoc + WindowCorner, Color.White);
                }
                else
                {
                    spriteBatch.Draw(SSTexSelect, SSLoc + WindowCorner, Color.White);
                }
            }

            spriteBatch.End();


            base.Draw(gameTime);
        }


        public void Clear()
        {
            lines.Clear();
            buttonPressed.Clear();
            MenuEntries.Clear();
            width = 0;
            height = 0;
            actionHandled = false;
            isClosed = false;
        }



        public bool ButtonPressed(int index)
        {
            if (index < buttonPressed.Count)
            {
                return buttonPressed[index];
            }
            return false;
        }


        public bool ActionHandled
        {
            get { return actionHandled; }
            set { actionHandled = value; }
        }

        public bool IsClosed
        {
            get { return isClosed; }
        }


        public bool ShowSS
        {
            get { return showSS; }
            set { showSS = value; }
        }
    }
}
