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
    public class TextSprite : GameSprite
    {
        string text;
        Color color;
        SpriteFont font;
        Vector2 movement;
        Timer timer;
        bool alwaysOn = false;

        public Color? BorderColor { get; set; }

        public TextSprite()
            : base(Vector2.Zero, Point.Zero, Vector2.Zero)
        {
            frameDimensions.Add(Point.Zero);
            timer = new Timer(0, TimerState.Stopped, TimerType.Manual);
        }

        public TextSprite(string text, Vector2 position, Color color, SpriteFont font, Vector2 movement, int duration)
            : base(position, Point.Zero, Vector2.Zero)
        {
            frameDimensions.Add(font.MeasureString(text).ToPoint());
            this.text = text;
            this.color = color;
            this.font = font;
            this.movement = movement;
            timer = new Timer(duration, TimerState.Stopped, TimerType.Manual);
        }


        public void SetDetails(string text, Vector2 position, Color color, SpriteFont font, Vector2 movement, int duration)
        {
            Position = position;
            this.text = text;
            frameDimensions[0] = font.MeasureString(text).ToPoint();
            this.color = color;
            this.font = font;
            this.movement = movement;
            timer = new Timer(duration, TimerState.Stopped, TimerType.Manual);
        }


        public int Duration
        {
            get { return timer.RemainingDuration; }
            set { timer.ResetTimerAndRun(value); }
        }

        public override void Activate()
        {
            base.Activate();

            timer.State = TimerState.Running;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isActive)
            {
                Position += movement * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;

                if (timer.Update(gameTime) && !alwaysOn)
                {
                    Deactivate();
                }
            }



        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            if (isActive)
            {
                Fonts.DrawCenteredText(spriteBatch, font, text, Position - GameplayScreen.viewportCorner, color, 1f, 0f, BorderColor);
            }
        }


        public bool AlwaysOn
        {
            get { return alwaysOn; }
            set { alwaysOn = value; }
        }


        public Color Color
        {
            set { color = value; }
        }

    }
}
