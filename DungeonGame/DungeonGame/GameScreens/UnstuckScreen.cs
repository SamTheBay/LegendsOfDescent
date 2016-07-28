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
    class UnstuckScreen : DescriptionScreen
    {
        PlayerSprite player;
        PopUpScreen pause;
        DungeonLevel dungeon;
        bool unstuckAllowed;

        public UnstuckScreen(PlayerSprite player, PopUpScreen pause, DungeonLevel dungeon)
            : base()
        {
            this.player = player;
            this.pause = pause;
            this.dungeon = dungeon;

            exitOnTouch = false;
            Initialize();

            if (DateTime.Now - TimeSpan.FromHours(24) > player.LastUnstuckTime)
            {
                unstuckAllowed = true;

                AddLine("The unstuck feature is here protect you against unknown bugs in the dungeon generation that may cause your character to get stuck. You can use it once a day to get a free warp. Would you like to use it now?", Fonts.DescriptionFont, Color.Red);
                AddSpace(30f);
                AddButton("Yes");
                AddSpace(20f);
                AddButton("No");
            }
            else
            {
                unstuckAllowed = false;

                AddLine("You have already used the unstuck feature in the last 24 hours. You will have to wait to use it again.", Fonts.DescriptionFont, Color.Red);
                AddSpace(30f);
                AddButton("Close");
            }

            SetFinalize();
        }

        public override void entry_Selected(object sender, EventArgs e)
        {
            base.entry_Selected(sender, e);

            if (unstuckAllowed)
            {
                if (selectedEntry == 0)
                {
                    // regenerate the dungeon level
                    DungeonGame.ScreenManager.AddScreen(new WarpScreen(GameplayScreen.Dungeon, SaveGameManager.CurrentPlayer, false));
                    SaveGameManager.CurrentPlayer.LastUnstuckTime = DateTime.Now;
                    pause.ExitScreen();
                }
            }
        }
    }
}
