
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{

    public enum AnimationType
    {
        Bounce,
        Slide
    }


    public class MenuEntry
    {
        // general appearance settings
        string text;
        SpriteFont spriteFont;
        float fontScale = 1f;
        Vector2 position;
        private Rectangle location = new Rectangle();
        private PopUpScreen owningPopup = null;
        private SpriteEffects spriteEffect = SpriteEffects.None;
        private Color fontColor = Color.White;
        private Color selectedFontColor = Color.White;
        private bool highlightLastSelected = true;

        // settings for generic button texturing
        private bool genericButton = false;
        private Color backgroundColor = Color.Red;
        private Color boarderColor = Color.White;
        private int boarderWidth = 4;
        private bool isActive = true;
        private int textYAdjust = 0;

        // settings for textured button
        private Texture2D texture;
        private Texture2D pressedTexture;

        public MenuEntry(string text)
        {
            this.text = text;
            this.Color = Color.White;
        }

        public MenuEntry(string text, Rectangle location, Color backgroundColor, Color boarderColor, int boarderWidth, SpriteFont font)
        {
            SetGenericButton(backgroundColor, boarderColor, boarderWidth);
            this.text = text;
            this.Color = Color.White;
            this.Location = location;
            this.Font = font;
        }

        public MenuEntry(string text, Rectangle location)
        {
            SetGenericButton(Color.Red, Color.White, 4);
            this.text = text;
            this.Color = Color.White;
            this.Location = location;
            this.Font = Fonts.ButtonFont;
            this.textYAdjust = 5;
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public SpriteFont Font
        {
            get { return spriteFont; }
            set { spriteFont = value; }
        }

        public float FontScale
        {
            get { return fontScale; }
            set { fontScale = value; }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                location.X = (int)position.X;
                location.Y = (int)position.Y;
            }
        }

        public PopUpScreen OwningPopup
        {
            set { owningPopup = value; }
        }

        public Texture2D Texture
        {
            get { return texture; }
            set
            {
                texture = value;
                location.Width = texture.Width;
                location.Height = texture.Height;
            }
        }

        public bool IsActive
        {
            get { return isActive; }
            set { isActive = value; }
        }

        public Color FontColor
        {
            get { return fontColor; }
            set { fontColor = value; }
        }

        public Color SelectedFontColor
        {
            get { return selectedFontColor; }
            set { selectedFontColor = value; }
        }

        public Texture2D PressedTexture
        {
            get { return pressedTexture; }
            set { pressedTexture = value; }
        }

        public SpriteEffects SpriteEffect
        {
            get { return spriteEffect; }
            set { spriteEffect = value; }
        }

        public Rectangle Location
        {
            get
            {
                if (owningPopup != null)
                {
                    location.X = (int)(position.X + owningPopup.WindowCorner.X);
                    location.Y = (int)(position.Y + owningPopup.WindowCorner.Y);
                }
                return location;
            }

            set
            {
                location = value;
                position.X = location.X;
                position.Y = location.Y;
            }
        }

        public Color Color { get; set; }


        public void SetGenericButton(Color backgroundColor, Color boarderColor, int boarderWidth)
        {
            genericButton = true;
            this.backgroundColor = backgroundColor;
            this.boarderColor = boarderColor;
            this.boarderWidth = boarderWidth;
        }



        public event EventHandler<EventArgs> Selected;


        protected internal virtual void OnSelectEntry()
        {
            if (Selected != null)
                Selected(this, EventArgs.Empty);
        }



        public virtual void Draw(MenuScreen screen, bool isSelected, GameTime gameTime)
        {
            Vector2 drawPosition = new Vector2();
            Rectangle drawLocation = location;
            Vector2 textPosition = new Vector2();
            Color color = isSelected ? selectedFontColor : fontColor;
            color = isActive ? color : Color.Gray;
            ScreenManager screenManager = screen.ScreenManager;
            SpriteBatch spriteBatch = screenManager.SpriteBatch;
            Color bgColor = Color.Red;
            Color boColor = isActive ? boarderColor : Color.Gray;

            int boarderWidth = 4;
            if (location.Height < 50)
            {
                boarderWidth = 2;
            }
            else if (location.Height <= 60)
            {
                boarderWidth = 3;
            }

            // determine if we are drawing relative to a popup window corner
            if (owningPopup != null)
            {
                drawPosition = position + owningPopup.WindowCorner;
                drawLocation.X = (int)drawPosition.X;
                drawLocation.Y = (int)drawPosition.Y;
            }
            else
                drawPosition = position;


            // measure string if we have one
            Vector2 textSize = Vector2.Zero;
            if (spriteFont != null)
            {
                textSize = spriteFont.MeasureString(text);
                textSize.X *= fontScale;
                textSize.Y *= fontScale;
            }

            if (genericButton)
            {
                if (InputManager.IsLocationPressed(drawLocation) && IsActive && !screen.OtherScreenHasFocus)
                {
                    Texture2D side = InternalContentManager.GetTexture("CelticSide" + location.Height.ToString() + "Select");
                    spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(boColor), new Rectangle(location.X + side.Width, location.Y, location.Width - side.Width * 2, boarderWidth), null, Color.White);
                    spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(boColor), new Rectangle(location.X + side.Width, location.Y + location.Height - boarderWidth, location.Width - side.Width * 2, boarderWidth), null, Color.White);
                    spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(bgColor), new Rectangle(location.X + side.Width, location.Y + boarderWidth, location.Width - side.Width * 2, location.Height - boarderWidth * 2), null, Color.White);

                    spriteBatch.Draw(side, new Vector2(location.X, location.Y), null, boColor);
                    spriteBatch.Draw(side, new Vector2(location.X + location.Width - side.Width, location.Y), null, boColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                }
                else
                {
                    Texture2D side = InternalContentManager.GetTexture("CelticSide" + location.Height.ToString());
                    spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(boColor), new Rectangle(location.X + side.Width, location.Y, location.Width - side.Width * 2, boarderWidth), null, Color.White);
                    spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(boColor), new Rectangle(location.X + side.Width, location.Y + location.Height - boarderWidth, location.Width - side.Width * 2, boarderWidth), null, Color.White);

                    spriteBatch.Draw(side, new Vector2(location.X, location.Y), null, boColor);
                    spriteBatch.Draw(side, new Vector2(location.X + location.Width - side.Width, location.Y), null, boColor, 0f, Vector2.Zero, 1f, SpriteEffects.FlipHorizontally, 0f);
                }

                textPosition.X = drawPosition.X + (float)Math.Floor((location.Width - textSize.X) / 2);
                textPosition.Y = drawPosition.Y + (float)Math.Floor((location.Height - textSize.Y) / 2) + textYAdjust;
                spriteBatch.DrawString(spriteFont, text, textPosition, color, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0);
            }
            else if (texture != null)
            {
                if (pressedTexture != null && isActive && (InputManager.IsLocationPressed(drawLocation) || (isSelected && highlightLastSelected)))
                    spriteBatch.Draw(pressedTexture, drawPosition, null, color, 0f, Vector2.Zero, 1f, spriteEffect, 0f);
                else
                    spriteBatch.Draw(texture, drawPosition, null, color, 0f, Vector2.Zero, 1f, spriteEffect, 0f);

                if ((spriteFont != null) && !String.IsNullOrEmpty(text))
                {
                    textPosition.X = drawPosition.X + (float)Math.Floor((texture.Width - textSize.X) / 2);
                    textPosition.Y = drawPosition.Y + (float)Math.Floor((texture.Height - textSize.Y) / 2);
                    spriteBatch.DrawString(spriteFont, text, textPosition, color, 0f, Vector2.Zero, fontScale, SpriteEffects.None, 0);
                }
            }
            else if ((spriteFont != null) && !String.IsNullOrEmpty(text))
            {
                spriteBatch.DrawString(spriteFont, text, drawPosition, color);
            }
        }

        public virtual int GetHeight(MenuScreen screen)
        {
            return Font.LineSpacing;
        }


        public bool HighlightLastSelected
        {
            get { return highlightLastSelected; }
            set { highlightLastSelected = value; }
        }


        // Penner bounce
        public static float Bounce(float pos)
        {
            if (pos < (1f / 2.75f))
            {
                return (7.5625f * pos * pos);
            }
            else if (pos < (2f / 2.75f))
            {
                return (7.5625f * (pos -= (1.5f / 2.75f)) * pos + .75f);
            }
            else if (pos < (2.5f / 2.75f))
            {
                return (7.5625f * (pos -= (2.25f / 2.75f)) * pos + .9375f);
            }
            else
            {
                return (7.5625f * (pos -= (2.625f / 2.75f)) * pos + .984375f);
            }
        }
    }
}
