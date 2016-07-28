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


    class ContinueQuestion : DescriptionScreen
    {
        bool result = false;
        bool isFinished = false;
        bool cancelled = false;

        public bool Result
        {
            get { return result; }
        }

        public bool IsFinished
        {
            get { return isFinished; }
        }


        public ContinueQuestion(string text = "Are you sure?", string button1 = "Yes", string button2 = "No")
        {
            exitOnTouch = false;

            Initialize();
            AddLine(text, Fonts.HeaderFont, Color.White);
            AddSpace(15);
            AddButton(button1);
            AddSpace(15);
            AddButton(button2);
            SetFinalize();
        }


        public void Reset()
        {
            result = false;
            isFinished = false;
            cancelled = false;
        }


        override public void entry_Selected(object sender, EventArgs e)
        {
            if (sender.Equals(MenuEntries[0]))
            {
                // Yes
                result = true;
            }
            else if (sender.Equals(MenuEntries[1]))
            {
                // No
                result = false;
            }
            isFinished = true;

            base.entry_Selected(sender, e);
        }


        protected override void OnCancel()
        {
            cancelled = true;
            isFinished = true;
            result = false;
            base.OnCancel();
        }

        public bool Cancelled
        {
            get { return cancelled; }
        }
    }
}