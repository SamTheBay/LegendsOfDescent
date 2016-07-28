using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class PopUpScreen : MenuScreen
    {
        protected Rectangle dimension = new Rectangle();

        bool closeOnTapOutOfDimension = false;

        public Vector2 WindowCorner
        {
            get { return dimension.Location.ToVector2(); }
            set { dimension.Location = value.ToPoint(); }
        }

        public void MoveWindowCorner(int x, int y)
        {
            dimension.X += x;
            dimension.Y += y;
        }


        public void SetWindowCorner(int x, int y)
        {
            dimension.X = x;
            dimension.Y = y;
        }

        public Rectangle Dimension
        {
            get { return dimension; }
            set { dimension = value; }
        }

        public PopUpScreen() : base()
        {
            IsPopup = true;
        }

        public bool CloseOnTapOutOfDimension
        {
            get { return closeOnTapOutOfDimension; }
            set { closeOnTapOutOfDimension = value; }
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (closeOnTapOutOfDimension)
            {
                Vector2 tapPoint = new Vector2();
                bool haveTap = InputManager.GetTapPoint(ref tapPoint);
                if (haveTap && !dimension.Contains(tapPoint.ToPoint()))
                {
                    ExitScreen();
                }
            }
        }

    }
}