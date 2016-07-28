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
    class DifficultyScreen : DescriptionScreen
    {
        public GameDifficulty Difficulty { get; set; }

        public DifficultyScreen()
            : base()
        {
            exitOnTouch = false;

            Initialize();
            AddLine("Difficulty", Fonts.DCFont, Color.Red);
            AddSpace(30f);
            AddButton("Easy");
            AddSpace(20f);
            AddButton("Normal");
            AddSpace(20f);
            AddButton("Hard");

            SetFinalize();
        }

        public override void entry_Selected(object sender, EventArgs e)
        {
            base.entry_Selected(sender, e);
            Difficulty = (GameDifficulty)selectedEntry;
            ActionHandled = true;
        }
    }
}
