using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    static class XNAExtensions
    {

        // Vector2 Extensions
        public static Vector2 GetDimensions(this Texture2D t)
        {
            return new Vector2(t.Width, t.Height);
        }


        public static Vector2 Trunc(this Vector2 t)
        {
            return new Vector2((int)t.X, (int)t.Y);
        }


        //Point extensions
        public static Vector2 ToVector2(this Point t)
        {
            return new Vector2(t.X, t.Y);
        }



        // SpriteBatch
        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, Rectangle destination, Color tint, float layerDepth)
        {
            spriteBatch.Draw(texture, destination, null, tint, 0f, Vector2.Zero, SpriteEffects.None, layerDepth); 
        }


        // Color
        public static HSVColor ToHSV(this Color color)
        {
            HSVColor hsv = new HSVColor();

            // compute hue
            int max, min;
            max = Math.Max(color.R, color.B);
            max = Math.Max(max, color.G);
            min = Math.Min(color.R, color.B);
            min = Math.Min(min, color.G);

            // compute value
            hsv.val = max;
            if (hsv.val == 0)
            {
                hsv.hue = hsv.sat = 0;
                return hsv;
            }

            // compute saturation
            hsv.sat = 255 * (max - min) / hsv.val;
            if (hsv.sat == 0)
            {
                hsv.hue = 0;
                return hsv;
            }

            // compute hue
            if (max == color.R)
            {
                hsv.hue = 0 + 43 * (color.G - color.B) / (max - min);
            }
            else if (max == color.G)
            {
                hsv.hue = 85 + 43 * (color.B - color.R) / (max - min);
            }
            else /* max == color.B */
            {
                hsv.hue = 171 + 43 * (color.R - color.G) / (max - min);
            }

            return hsv;

        }


        public static HSLColor ToHSL(this Color color)
        {
            double num7;
            double num8;
            double num4 = ((double)color.R) / 255.0;
            double num5 = ((double)color.G) / 255.0;
            double num6 = ((double)color.B) / 255.0;
            double num = Math.Min(Math.Min(num4, num5), num6);
            double num2 = Math.Max(Math.Max(num4, num5), num6);
            double num9 = num2;
            double num3 = num2 - num;
            if ((num2 == 0.0) || (num3 == 0.0))
            {
                num8 = 0.0;
                num7 = 0.0;
            }
            else
            {
                num8 = num3 / num2;
                if (num4 == num2)
                {
                    num7 = (num5 - num6) / num3;
                }
                else if (num5 == num2)
                {
                    num7 = 2.0 + ((num6 - num4) / num3);
                }
                else
                {
                    num7 = 4.0 + ((num4 - num5) / num3);
                }
            }
            num7 *= 60.0;
            if (num7 < 0.0)
            {
                num7 += 360.0;
            }
            return new HSLColor((int)num7, (int)(num8 * 100.0), (int)(num9 * 100.0));
        }


        public static void Persist(this Color color, BinaryWriter writer)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);

            
        }


        public static Color ReadColor(this BinaryReader reader)
        {
            return new Color((int)reader.ReadByte(), (int)reader.ReadByte(), (int)reader.ReadByte(), (int)reader.ReadByte());
        }

        
    }
}
