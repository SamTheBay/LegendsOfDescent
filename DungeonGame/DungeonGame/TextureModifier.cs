using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace LegendsOfDescent
{
    struct HSVColor
    {
        public int sat;
        public int hue;
        public int val;

        public HSVColor(int h, int s, int v)
        {
            this.hue = h;
            this.sat = s;
            this.val = v;
        }

        Color ToColor(int alpha)
        {
            int hi = Convert.ToInt32(Math.Floor((double)hue / 60)) % 6;
            double f = hue / 60 - Math.Floor((double)hue / 60);

            int v = Convert.ToInt32(val);
            int p = Convert.ToInt32(val * (1 - ((double)sat / 255)));
            int q = Convert.ToInt32(val * (1 - f * ((double)sat / 255)));
            int t = Convert.ToInt32(val * (1 - (1 - f) * ((double)sat / 255)));


            if (hi == 0)
                return new Color(v, t, p, alpha);
            else if (hi == 1)
                return new Color(q, v, p, alpha);
            else if (hi == 2)
                return new Color(p, v, t, alpha);
            else if (hi == 3)
                return new Color(p, q, v, alpha);
            else if (hi == 4)
                return new Color(t, p, v, alpha);
            else
                return new Color(v, p, q, alpha);
        }
    }


    struct HSLColor
    {
        public int s;
        public int h;
        public int l;

        public HSLColor(int h, int s, int l)
        {
            this.h = h;
            this.s = s;
            this.l = l;
        }

        public Color ToColor(int Alpha)
        {
            double num4 = 0.0;
            double num5 = 0.0;
            double num6 = 0.0;
            double num = ((double)h) % 360.0;
            double num2 = ((double)s) / 100.0;
            double num3 = ((double)l) / 100.0;
            if (num2 == 0.0)
            {
                num4 = num3;
                num5 = num3;
                num6 = num3;
            }
            else
            {
                double d = num / 60.0;
                int num11 = (int)Math.Floor(d);
                double num10 = d - num11;
                double num7 = num3 * (1.0 - num2);
                double num8 = num3 * (1.0 - (num2 * num10));
                double num9 = num3 * (1.0 - (num2 * (1.0 - num10)));
                switch (num11)
                {
                    case 0:
                        num4 = num3;
                        num5 = num9;
                        num6 = num7;
                        break;
                    case 1:
                        num4 = num8;
                        num5 = num3;
                        num6 = num7;
                        break;
                    case 2:
                        num4 = num7;
                        num5 = num3;
                        num6 = num9;
                        break;
                    case 3:
                        num4 = num7;
                        num5 = num8;
                        num6 = num3;
                        break;
                    case 4:
                        num4 = num9;
                        num5 = num7;
                        num6 = num3;
                        break;
                    case 5:
                        num4 = num3;
                        num5 = num7;
                        num6 = num8;
                        break;
                }
            }
            return new Color((int)(num4 * 255.0), (int)(num5 * 255.0), (int)(num6 * 255.0), Alpha);
        }
    }


    class TextureModifier
    {

        static public UInt16 ColorTo565(Color color)
        {
            return (UInt16)((((UInt16)color.R >> 3) << 11) | (((UInt16)color.G >> 2) << 5) | ((UInt16)color.B >> 3));
        }


        static public Color Five65ToColor(UInt16 five65)
        {
            Color color = Color.Black;
            color.R = (byte)((five65 >> 11) << 3);
            color.G = (byte)((five65 & 2016) >> 3);
            color.B = (byte)((five65 & 31) << 3);
            return color;
        }


        static public void AdjustHSLDXT5(Texture2D texture, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            // get the textures data
            byte[] data = new byte[texture.Width * texture.Height];
#if !SILVERLIGHT
            texture.GetData(data);
#endif

            // alter the texture
            for (int blockIndex = 0; blockIndex < data.Length; blockIndex += 16)
            {
                // extract out the colors
                UInt16 color1 = (UInt16)((UInt16)data[blockIndex + 8] + ((UInt16)data[blockIndex + 9] << 8));
                UInt16 color2 = (UInt16)((UInt16)data[blockIndex + 10] + ((UInt16)data[blockIndex + 11] << 8));

                HSLColor hsl = Five65ToColor(color1).ToHSL();
                hsl.h += hueAdjust;
                hsl.h %= 360;
                if (hsl.h < 0)
                    hsl.h += 360;
                hsl.l += (int)((double)hsl.l * ((double)lightAdjust / 100.0f));
                hsl.s += (int)((double)hsl.s * ((double)satAdjust / 100.0f));
                Color color = hsl.ToColor(0);
                color1 = ColorTo565(color);
                data[blockIndex + 8] = (byte)((color1 << 8) >> 8);
                data[blockIndex + 9] = (byte)(color1 >> 8);

                hsl = Five65ToColor(color2).ToHSL();
                hsl.h += hueAdjust;
                hsl.h %= 360;
                if (hsl.h < 0)
                    hsl.h += 360;
                hsl.l += (int)((double)hsl.l * ((double)lightAdjust / 100.0f));
                hsl.s += (int)((double)hsl.s * ((double)satAdjust / 100.0f));
                color = hsl.ToColor(0);
                color2 = ColorTo565(color);
                data[blockIndex + 10] = (byte)((color2 << 8) >> 8);
                data[blockIndex + 11] = (byte)(color2 >> 8);
            }

            // write the texture back
#if !SILVERLIGHT
            texture.SetData(data);
#endif
        }


        static public void AdjustHSL(Texture2D texture, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            if (texture.Format == SurfaceFormat.Color)
            {
                AdjustHSLColor(texture, hueAdjust, satAdjust, lightAdjust);
            }
            else if (texture.Format == SurfaceFormat.Dxt5)
            {
                AdjustHSLDXT5(texture, hueAdjust, satAdjust, lightAdjust);
            }
            else
            {
                throw new Exception("Don't know how to do color transformations on this type");
            }
        }



        static public void AdjustHSLColor(Texture2D texture, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            // get the textures data
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            // alter the texture
            for (int i = 0; i < data.Length; i++)
            {
                HSLColor hsl = data[i].ToHSL();
                hsl.h += hueAdjust;
                hsl.h %= 360;
                if (hsl.h < 0)
                    hsl.h += 360;
                hsl.l += (int)((double)hsl.l * ((double)lightAdjust / 100.0f));
                hsl.s += (int)((double)hsl.s * ((double)satAdjust / 100.0f));
                data[i] = hsl.ToColor(data[i].A);
            }

            // write the texture back
            texture.SetData(data);
        }



        static public void AdjustHSLInBand(Texture2D texture, Color bandCenter, int bandWidth, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            if (texture.Format == SurfaceFormat.Color)
            {
                AdjustHSLInBandColor(texture, bandCenter, bandWidth, hueAdjust, satAdjust, lightAdjust);
            }
            else if (texture.Format == SurfaceFormat.Dxt5)
            {
                AdjustHSLInBandDXT5(texture, bandCenter, bandWidth, hueAdjust, satAdjust, lightAdjust);
            }
            else
            {
                throw new Exception("Don't know how to do color transformations on this type");
            }
        }



        static public void AdjustHSLInBandColor(Texture2D texture, Color bandCenter, int bandWidth, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            // get the textures data
            Color[] data = new Color[texture.Width * texture.Height];
            texture.GetData(data);

            // alter the texture
            for (int i = 0; i < data.Length; i++)
            {
                
                if (data[i].R >= bandCenter.R - bandWidth / 2 && data[i].R <= bandCenter.R + bandWidth / 2 &&
                    data[i].G >= bandCenter.G - bandWidth / 2 && data[i].G <= bandCenter.G + bandWidth / 2 &&
                    data[i].B >= bandCenter.B - bandWidth / 2 && data[i].B <= bandCenter.B + bandWidth / 2)
                {
                    HSLColor hsl = data[i].ToHSL();
                    hsl.h += hueAdjust;
                    hsl.h %= 360;
                    hsl.l += lightAdjust;
                    //hsl.l = Math.Min(100, hsl.l);
                    hsl.s += satAdjust;
                    //hsl.s = Math.Min(100, hsl.s);
                    data[i] = hsl.ToColor(data[i].A);
                }
                
            }


            texture.SetData(data);
        }


        static public void AdjustHSLInBandDXT5(Texture2D texture, Color bandCenter, int bandWidth, int hueAdjust = 0, int satAdjust = 0, int lightAdjust = 0)
        {
            // get the textures data
            byte[] data = new byte[texture.Width * texture.Height];
#if !SILVERLIGHT
            texture.GetData(data);
#endif

            // alter the texture
            for (int blockIndex = 0; blockIndex < data.Length; blockIndex += 16)
            {
                // extract out the colors
                UInt16 color1 = (UInt16)((UInt16)data[blockIndex + 8] + ((UInt16)data[blockIndex + 9] << 8));
                UInt16 color2 = (UInt16)((UInt16)data[blockIndex + 10] + ((UInt16)data[blockIndex + 11] << 8));

                Color color = Five65ToColor(color1);
                if (color.R >= bandCenter.R - bandWidth / 2 && color.R <= bandCenter.R + bandWidth / 2 &&
                    color.G >= bandCenter.G - bandWidth / 2 && color.G <= bandCenter.G + bandWidth / 2 &&
                    color.B >= bandCenter.B - bandWidth / 2 && color.B <= bandCenter.B + bandWidth / 2)
                {
                    HSLColor hsl = Five65ToColor(color1).ToHSL();
                    hsl.h += hueAdjust;
                    hsl.h %= 360;
                    if (hsl.h < 0)
                        hsl.h += 360;
                    hsl.l += (int)((double)hsl.l * ((double)lightAdjust / 100.0f));
                    hsl.s += (int)((double)hsl.s * ((double)satAdjust / 100.0f));
                    color = hsl.ToColor(0);
                    color1 = ColorTo565(color);
                    data[blockIndex + 8] = (byte)((color1 << 8) >> 8);
                    data[blockIndex + 9] = (byte)(color1 >> 8);
                }

                color = Five65ToColor(color2);
                if (color.R >= bandCenter.R - bandWidth / 2 && color.R <= bandCenter.R + bandWidth / 2 &&
                    color.G >= bandCenter.G - bandWidth / 2 && color.G <= bandCenter.G + bandWidth / 2 &&
                    color.B >= bandCenter.B - bandWidth / 2 && color.B <= bandCenter.B + bandWidth / 2)
                {
                    HSLColor hsl = Five65ToColor(color2).ToHSL();
                    hsl.h += hueAdjust;
                    hsl.h %= 360;
                    if (hsl.h < 0)
                        hsl.h += 360;
                    hsl.l += (int)((double)hsl.l * ((double)lightAdjust / 100.0f));
                    hsl.s += (int)((double)hsl.s * ((double)satAdjust / 100.0f));
                    color = hsl.ToColor(0);
                    color2 = ColorTo565(color);
                    data[blockIndex + 10] = (byte)((color2 << 8) >> 8);
                    data[blockIndex + 11] = (byte)(color2 >> 8);
                }
            }

            // write the texture back
#if !SILVERLIGHT
            texture.SetData(data);
#endif
        }

    }
}
