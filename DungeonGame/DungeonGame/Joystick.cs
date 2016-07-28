using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class Joystick
    {
        Rectangle location;
        Vector2 center;
        Vector2 direction;
        Vector2 pressLoc;
        Texture2D Border;
        Texture2D Dot;
        Color tint;
        bool hasDirection = false;


        public Joystick(Rectangle location, Color tint)
        {
            this.location = location;
            this.center = this.pressLoc = location.Center.ToVector2();
            direction = Vector2.Zero;
            Border = InternalContentManager.GetTexture("JoystickCircle");
            Dot = InternalContentManager.GetTexture("JoystickDot");
            this.tint = tint;
        }


        public bool UpdateDirection()
        {
            hasDirection = InputManager.IsLocationPressed(location, ref pressLoc);

            if (hasDirection)
            {
                // calculate the direction
                direction.X = pressLoc.X - center.X;
                direction.Y = pressLoc.Y - center.Y;

                // normalize vector
                float distance = (float)Math.Sqrt((float)MathExt.Square(direction.X) + (float)MathExt.Square(direction.Y));
                direction.X /= distance;
                direction.Y /= distance;
            }

            return hasDirection;
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Border, new Vector2(location.X + ((location.Width - Border.Width) / 2), location.Y + ((location.Height - Border.Height) / 2)), tint);

            Vector2 dotLocation = pressLoc + new Vector2(Dot.Width / 2, Dot.Height / 2);
            if (Vector2.Distance(dotLocation, center) > 45)
            {
                Vector2 direction = (dotLocation - center);
                direction /= Vector2.Distance(dotLocation, center);
                dotLocation = center + (direction * 45);
            }

            spriteBatch.Draw(Dot, dotLocation - new Vector2(Dot.Width / 2, Dot.Height / 2), tint);
        }


        public Vector2 Direction
        {
            get { return direction; }
        }


        public Color Tint
        {
            get { return tint; }
            set { tint = value; }
        }


        public bool HasDirection
        {
            get { return hasDirection; }
        }

        public Rectangle Location
        {
            get { return location; }
            set
            {
                if (location != value)
                {
                    location = value;
                    this.center = this.pressLoc = location.Center.ToVector2();
                    hasDirection = false;
                }
            }
        }
    }
}
