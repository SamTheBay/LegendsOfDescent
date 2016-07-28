using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegendsOfDescent
{
    class TextInputScreen : MenuScreen
    {
#if WIN8
        public string Input { get { return DungeonGame.Instance.TextInputBox.Text; } set { DungeonGame.Instance.TextInputBox.Text = value; } }
#else
        public string Input { get; set; }
#endif
        public bool IsFinished { get; set; }
        public bool IsSuccessful { get; set; }
        string title;

        public TextInputScreen(string initialInput = "", string title = "")
            : base()
        {
#if WIN8
            DungeonGame.Instance.TextInputBox.Text = initialInput;
#else
            Input = initialInput;
#endif
            IsFinished = false;
            IsSuccessful = false;
            this.title = title;

            int buttonStart = DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 70 * 2 - 270;
            int buttonOffset = 70;
            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, buttonStart, 450, 60);

            MenuEntry entry = new MenuEntry("Done", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonOffset;

            entry = new MenuEntry("Cancel", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            MenuEntries.Add(entry);
            location.Y += buttonOffset;

#if WIN8
            DungeonGame.Instance.TextInputBox.Visibility  = Windows.UI.Xaml.Visibility.Visible;
#endif
        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                IsSuccessful = true;
                ExitScreen();
            }
            else
            {
                ExitScreen();
            }
        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            int currentX = DungeonGame.ScreenSize.Width / 2;
            int currentY = 200;
            foreach (string descriptionLine in Fonts.BreakTextIntoList(title, Fonts.HeaderFont, DungeonGame.ScreenSize.Width - 40))
            {
                Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, descriptionLine, new Vector2(currentX, currentY), Color.White);
                currentY += Fonts.HeaderFont.LineSpacing;
            }

            //Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, Input, new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Height / 2), Color.Red);

            spriteBatch.End();
        }


        public override void OnRemoval()
        {
            IsFinished = true;
#if WIN8
            DungeonGame.Instance.TextInputBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
#endif
            base.OnRemoval();
        }


        public override void HandleInput()
        {
            base.HandleInput();
        //    string newInput = "";

        //    // capture screen input
        //    if (InputManager.IsKeyTriggered(Keys.Back))
        //    {
        //        if (Input.Length > 0)
        //            Input = Input.Remove(Input.Length - 1);
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.Enter))
        //    {
        //        IsSuccessful = true;
        //        ExitScreen();
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.Space))
        //    {
        //        newInput = " ";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.A))
        //    {
        //        newInput = "a";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.B))
        //    {
        //        newInput = "b";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.C))
        //    {
        //        newInput = "c";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.D))
        //    {
        //        newInput = "d";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.E))
        //    {
        //        newInput = "e";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.F))
        //    {
        //        newInput = "f";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.G))
        //    {
        //        newInput = "g";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.H))
        //    {
        //        newInput = "h";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.I))
        //    {
        //        newInput = "i";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.J))
        //    {
        //        newInput = "j";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.K))
        //    {
        //        newInput = "k";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.L))
        //    {
        //        newInput = "l";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.M))
        //    {
        //        newInput = "m";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.N))
        //    {
        //        newInput = "n";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.O))
        //    {
        //        newInput = "o";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.P))
        //    {
        //        newInput = "p";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.Q))
        //    {
        //        newInput = "q";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.R))
        //    {
        //        newInput = "r";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.S))
        //    {
        //        newInput = "s";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.T))
        //    {
        //        newInput = "t";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.U))
        //    {
        //        newInput = "u";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.V))
        //    {
        //        newInput = "v";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.W))
        //    {
        //        newInput = "w";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.X))
        //    {
        //        newInput = "x";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.Y))
        //    {
        //        newInput = "y";
        //    }
        //    if (InputManager.IsKeyTriggered(Keys.Z))
        //    {
        //        newInput = "z";
        //    }


        //    if (InputManager.IsKeyPressed(Keys.LeftShift) || InputManager.IsKeyPressed(Keys.RightShift))
        //    {
        //        newInput = newInput.ToUpper();
        //    }

        //    if (Input.Length < 12)
        //        Input = Input + newInput;
        }


    }
}
