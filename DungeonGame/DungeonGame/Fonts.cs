using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Text;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#if SILVERLIGHT
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;
#endif

namespace LegendsOfDescent
{
    /// <summary>
    /// Static storage of SpriteFont objects and colors for use throughout the game.
    /// </summary>
    static class Fonts
    {
        #region Fonts


        private static SpriteFont headerFont;
        public static SpriteFont HeaderFont
        {
            get { return headerFont; }
        }

        private static SpriteFont buttonFont;
        public static SpriteFont ButtonFont
        {
            get { return buttonFont; }
        }

        private static SpriteFont descriptionFont;
        public static SpriteFont DescriptionFont
        {
            get { return descriptionFont; }
        }


        private static SpriteFont descriptionItFont;
        public static SpriteFont DescriptionItFont
        {
            get { return descriptionItFont; }
        }

        private static SpriteFont dcFont;
        public static SpriteFont DCFont
        {
            get { return dcFont; }
        }

        private static SpriteFont dcFontLarge;
        public static SpriteFont DCFontLarge
        {
            get { return dcFontLarge; }
        }

        private static SpriteFont nameFont;
        public static SpriteFont NameFont
        {
            get { return nameFont; }
        }

        private static SpriteFont critFont;
        public static SpriteFont CritFont
        {
            get { return critFont; }
        }

        #endregion


        #region Initialization


        /// <summary>
        /// Load the fonts from the content pipeline.
        /// </summary>
        public static void LoadContent(ContentManager contentManager)
        {
            // check the parameters
            if (contentManager == null)
            {
                throw new ArgumentNullException("contentManager");
            }

#if SILVERLIGHT
            FontSource fontSource = new FontSource(Application.GetResourceStream(
                new Uri("/LoDSilverlight;component/Content/Fonts/ARIAL.TTF", UriKind.Relative)).Stream);
            FontFamily fontFamily = new FontFamily("Arial");

            SilverlightFontTranslations.Add("Fonts/HeaderFont", new SpriteFontTTF(fontSource, fontFamily, 24));
            SilverlightFontTranslations.Add("Fonts/DescriptionFont", new SpriteFontTTF(fontSource, fontFamily, 14));
            SilverlightFontTranslations.Add("Fonts/NameFont", new SpriteFontTTF(fontSource, fontFamily, 18));

            fontSource = new FontSource(Application.GetResourceStream(
                new Uri("/LoDSilverlight;component/Content/Fonts/ARIALI.TTF", UriKind.Relative)).Stream);
            fontFamily = new FontFamily("Arial");
            SilverlightFontTranslations.Add("Fonts/DescriptionItFont", new SpriteFontTTF(fontSource, fontFamily, 14));

            fontSource = new FontSource(Application.GetResourceStream(
                new Uri("/LoDSilverlight;component/Content/Fonts/DUM1.TTF", UriKind.Relative)).Stream);
            fontFamily = new FontFamily("Dumbledor 1");
            SilverlightFontTranslations.Add("Fonts/ButtonFont", new SpriteFontTTF(fontSource, fontFamily, 28));

            fontSource = new FontSource(Application.GetResourceStream(
                new Uri("/LoDSilverlight;component/Content/Fonts/DC_S_3.TTF", UriKind.Relative)).Stream);
            fontFamily = new FontFamily("Regular");
            SilverlightFontTranslations.Add("Fonts/DCFont", new SpriteFontTTF(fontSource, fontFamily, 36));
            SilverlightFontTranslations.Add("Fonts/DCFontLarge", new SpriteFontTTF(fontSource, fontFamily, 48));
#endif

            // load each font from the content pipeline
            headerFont = contentManager.Load<SpriteFont>("Fonts/HeaderFont");
            buttonFont = contentManager.Load<SpriteFont>("Fonts/ButtonFont");
            descriptionFont = contentManager.Load<SpriteFont>("Fonts/DescriptionFont");
            descriptionItFont = contentManager.Load<SpriteFont>("Fonts/DescriptionItFont");
            dcFont = contentManager.Load<SpriteFont>("Fonts/DCFont");
            dcFontLarge = contentManager.Load<SpriteFont>("Fonts/DCFontLarge");
            nameFont = contentManager.Load<SpriteFont>("Fonts/NameFont");
            critFont = contentManager.Load<SpriteFont>("Fonts/CritFont");
        }


        /// <summary>
        /// Release all references to the fonts.
        /// </summary>
        public static void UnloadContent()
        {
            headerFont = null;
            buttonFont = null;
            descriptionFont = null;
            descriptionItFont = null;
            dcFont = null;
            dcFontLarge = null;
            nameFont = null;
            critFont = null;
        }


        #endregion


        #region Text Helper Methods


        /// <summary>
        /// Adds newline characters to a string so that it fits within a certain size.
        /// </summary>
        /// <param name="text">The text to be modified.</param>
        /// <param name="maximumCharactersPerLine">
        /// The maximum length of a single line of text.
        /// </param>
        /// <param name="maximumLines">The maximum number of lines to draw.</param>
        /// <returns>The new string, with newline characters if needed.</returns>
        public static string BreakTextIntoLines(string text,
            int maximumCharactersPerLine, int maximumLines)
        {
            if (maximumLines <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumLines");
            }
            if (maximumCharactersPerLine <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumCharactersPerLine");
            }

            // if the string is trivial, then this is really easy
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            // if the text is short enough to fit on one line, then this is still easy
            if (text.Length < maximumCharactersPerLine)
            {
                return text;
            }

            // construct a new string with carriage returns
            StringBuilder stringBuilder = new StringBuilder(text);
            int currentLine = 0;
            int newLineIndex = 0;
            while (((text.Length - newLineIndex) > maximumCharactersPerLine) &&
                (currentLine < maximumLines))
            {
                text.IndexOf(' ', 0);
                int nextIndex = newLineIndex;
                while ((nextIndex >= 0) && (nextIndex < maximumCharactersPerLine))
                {
                    newLineIndex = nextIndex;
                    nextIndex = text.IndexOf(' ', newLineIndex + 1);
                }
                stringBuilder.Replace(' ', '\n', newLineIndex, 1);
                currentLine++;
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// Adds new-line characters to a string to make it fit.
        /// </summary>
        /// <param name="text">The text to be drawn.</param>
        /// <param name="maximumCharactersPerLine">
        /// The maximum length of a single line of text.
        /// </param>
        public static string BreakTextIntoLines(string text,
            int maximumCharactersPerLine)
        {
            // check the parameters
            if (maximumCharactersPerLine <= 0)
            {
                throw new ArgumentOutOfRangeException("maximumCharactersPerLine");
            }

            // if the string is trivial, then this is really easy
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            // if the text is short enough to fit on one line, then this is still easy
            if (text.Length < maximumCharactersPerLine)
            {
                return text;
            }

            // construct a new string with carriage returns
            StringBuilder stringBuilder = new StringBuilder(text);
            int currentLine = 0;
            int newLineIndex = 0;
            while (((text.Length - newLineIndex) > maximumCharactersPerLine))
            {
                text.IndexOf(' ', 0);
                int nextIndex = newLineIndex;
                while ((nextIndex >= 0) && (nextIndex < maximumCharactersPerLine))
                {
                    newLineIndex = nextIndex;
                    nextIndex = text.IndexOf(' ', newLineIndex + 1);
                }
                stringBuilder.Replace(' ', '\n', newLineIndex, 1);
                currentLine++;
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// Break text up into separate lines to make it fit.
        /// </summary>
        /// <param name="text">The text to be broken up.</param>
        /// <param name="font">The font used ot measure the width of the text.</param>
        /// <param name="rowWidth">The maximum width of each line, in pixels.</param>
        public static List<string> BreakTextIntoList(string text, SpriteFont font,
            int rowWidth)
        {
            // check parameters
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }
            if (rowWidth <= 0)
            {
                throw new ArgumentOutOfRangeException("rowWidth");
            }

            // create the list
            List<string> lines = new List<string>();

            // check for trivial text
            if (String.IsNullOrEmpty("text"))
            {
                lines.Add(String.Empty);
                return lines;
            }

            // check for text that fits on a single line
            if (font.MeasureString(text).X <= rowWidth)
            {
                lines.Add(text);
                return lines;
            }

            // break the text up into words
            string[] words = text.Split(' ');

            // add words until they go over the length
            int currentWord = 0;
            while (currentWord < words.Length)
            {
                int wordsThisLine = 0;
                string line = String.Empty;
                while (currentWord < words.Length)
                {
                    string testLine = line;
                    if (testLine.Length < 1)
                    {
                        testLine += words[currentWord];
                    }
                    else if ((testLine[testLine.Length - 1] == '.') ||
                        (testLine[testLine.Length - 1] == '?') ||
                        (testLine[testLine.Length - 1] == '!'))
                    {
                        testLine += "  " + words[currentWord];
                    }
                    else
                    {
                        testLine += " " + words[currentWord];
                    }
                    if ((wordsThisLine > 0) &&
                        (font.MeasureString(testLine).X > rowWidth))
                    {
                        break;
                    }
                    line = testLine;
                    wordsThisLine++;
                    currentWord++;
                }
                lines.Add(line);
            }
            return lines;
        }

        #endregion


        #region Drawing Helper Methods



        public static void DrawCenteredText(this SpriteBatch spriteBatch, SpriteFont font,
            string text, Vector2 position, Microsoft.Xna.Framework.Color color, float scale = 1f, float layerDepth = 0f, Color? borderColor = null)
        {
            DrawAlignedText(spriteBatch, HorizontalAlignment.Center, VericalAlignment.Middle, font, text, position, color, scale, layerDepth, borderColor);
        }

        public static float DrawAlignedText(this SpriteBatch spriteBatch, HorizontalAlignment horiz, VericalAlignment vert, SpriteFont font,
            string text, Vector2 position, Microsoft.Xna.Framework.Color color, float scale = 1f, float layerDepth = 0f, Color? borderColor = null)
        {
            // check the parameters
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }
            if (font == null)
            {
                throw new ArgumentNullException("font");
            }

            // check for trivial text
            if (String.IsNullOrEmpty(text))
            {
                return 0;
            }

            // calculate the centered position
            Vector2 textSize = font.MeasureString(text);
            textSize.X *= scale;
            textSize.Y *= scale;
            
            switch (horiz)
            {
                case HorizontalAlignment.Center:
                    position.X -= (int)textSize.X / 2;
                    break;
                case HorizontalAlignment.Right:
                    position.X -= (int)textSize.X;
                    break;
            }

            switch (vert)
            {
                case VericalAlignment.Middle:
                    position.Y -= (int)textSize.Y / 2;
                    break;
                case VericalAlignment.Bottom:
                    position.Y -= (int)textSize.Y;
                    break;
            }

            if (borderColor != null)
            {
                var borderPosition = position;
                int borderWidth = 1;

                for (int xo = -borderWidth; xo <= borderWidth; xo++)
                {
                    for (int yo = -borderWidth; yo <= borderWidth; yo++)
                    {
                        if (!(xo == 0 && yo == 0))
                        {
                            borderPosition.X = position.X + xo;
                            borderPosition.Y = position.Y + yo;
                            spriteBatch.DrawString(font, text, borderPosition, borderColor.Value, 0f,
                                Vector2.Zero, scale, SpriteEffects.None, layerDepth + 0.00001f);
                        }
                    }
                }
            }

            // draw the string
            spriteBatch.DrawString(font, text, position, color, 0f,
                Vector2.Zero, scale, SpriteEffects.None, layerDepth);

            return textSize.Y;
        }

        #endregion
    }

    public enum HorizontalAlignment
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public enum VericalAlignment
    {
        Top = 0,
        Middle = 1,
        Bottom = 2
    }
}
