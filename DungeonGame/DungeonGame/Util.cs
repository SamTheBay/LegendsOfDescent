using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;

namespace LegendsOfDescent
{
    public static class Util
    {
        public static int Seed { get { return seed; } }
        public static Random Random { get { return random; } }

        private static int seed = Environment.TickCount;
        private static Random random = new Random(seed);

        public static void Reseed(int newSeed)
        {
            seed = newSeed;
            random = new Random(seed);
        }

        public static double DistanceTo(this Point point, Point other)
        {
            int a = point.X - other.X;
            int b = point.Y - other.Y;
            return Math.Sqrt(a * a + b * b);
        }

        public static Point Offset(this Point point, int offsetX, int offsetY)
        {
            return new Point(point.X + offsetX, point.Y + offsetY);
        }

        public static Vector2 Offset(this Vector2 vector, int offsetX, int offsetY)
        {
            return new Vector2(vector.X + offsetX, vector.Y + offsetY);
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }
        
        public static String Wrap(this String text, SpriteFont font, int width)
        {
            String line = String.Empty;
            String returnString = String.Empty;
            String[] wordArray = text.Split(' ');

            foreach (String word in wordArray)
            {
                if (font.MeasureString(line + word).Length() > width)
                {
                    returnString = returnString + line + '\n';
                    line = String.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }

        public static string ToDescription(this Enum value)
        {
            // Get the Description attribute value for the enum value
#if WIN8
            FieldInfo fi = value.GetType().GetRuntimeField(value.ToString());
#else
            FieldInfo fi = value.GetType().GetField(value.ToString());
#endif
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }


        // returns how to adjust the angle to get a nice spread for many projectiles
        public static float GetAngleAdjust(int boltNum, int j)
        {
            float angleAdjust = 0f;

            if (boltNum == 2 && j == 0)
            {
                angleAdjust = .3f;
            }
            else if (boltNum == 2)
            {
                angleAdjust = -.3f;
            }


            else if (boltNum == 3 && j == 0)
            {
                angleAdjust = -.45f;
            }
            else if (boltNum == 3 && j == 2)
            {
                angleAdjust = .45f;
            }


            else if (boltNum == 4 && j == 0)
            {
                angleAdjust = -.5f;
            }
            else if (boltNum == 4 && j == 1)
            {
                angleAdjust = -.2f;
            }
            else if (boltNum == 4 && j == 2)
            {
                angleAdjust = .2f;
            }
            else if (boltNum == 4 && j == 3)
            {
                angleAdjust = .5f;
            }

            else if (boltNum == 5 && j == 0)
            {
                angleAdjust = -.5f;
            }
            else if (boltNum == 5 && j == 1)
            {
                angleAdjust = -.2f;
            }
            else if (boltNum == 5 && j == 3)
            {
                angleAdjust = .2f;
            }
            else if (boltNum == 5 && j == 4)
            {
                angleAdjust = .5f;
            }

            else if (boltNum == 6 && j == 0)
            {
                angleAdjust = -.5f;
            }
            else if (boltNum == 6 && j == 1)
            {
                angleAdjust = -.3f;
            }
            else if (boltNum == 6 && j == 2)
            {
                angleAdjust = -.1f;
            }
            else if (boltNum == 6 && j == 3)
            {
                angleAdjust = .1f;
            }
            else if (boltNum == 6 && j == 4)
            {
                angleAdjust = .3f;
            }
            else if (boltNum == 6 && j == 5)
            {
                angleAdjust = .5f;
            }



            else if (boltNum == 7 && j == 0)
            {
                angleAdjust = -.5f;
            }
            else if (boltNum == 7 && j == 1)
            {
                angleAdjust = -.3f;
            }
            else if (boltNum == 7 && j == 2)
            {
                angleAdjust = -.15f;
            }
            else if (boltNum == 7 && j == 4)
            {
                angleAdjust = .15f;
            }
            else if (boltNum == 7 && j == 5)
            {
                angleAdjust = .3f;
            }
            else if (boltNum == 7 && j == 6)
            {
                angleAdjust = .5f;
            }

            return angleAdjust;
        }


    }
    
    public struct FloatRectangle
    {
        public float X, Y, Width, Height;

        public FloatRectangle(float X, float Y, float Width, float Height)
        {
            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;
        }

        public bool Intersects(FloatRectangle otherRect)
        {
            if ((X + Width > otherRect.X && X < otherRect.X + otherRect.Width) && 
                (Y + Height > otherRect.Y && Y < otherRect.Y + otherRect.Height))
            {
                return true;
            }
            
            return false;
        }

        public bool Contains(Vector2 point)
        {
            return point.X > X && point.X < X + Width && point.Y > Y && point.Y < Y + Height;
        }

        public Vector2 Corner
        {
            get
            {
                return new Vector2(X, Y);
            }
        }
    }

}
