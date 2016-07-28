using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class ClassUpgradeScreen : DescriptionScreen
    {
        PlayerSprite player;
        int classIndex = 0;
        Texture2D arrowRight;

        public ClassUpgradeScreen(PlayerSprite player)
            : base()
        {
            this.player = player;
            exitOnTouch = false;
            BorderAdjust.X = 50;
            maxWidth = 350;
            arrowRight = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/ArrowRight");

            RefreshScreen();
        }


        public override void entry_Selected(object sender, EventArgs e)
        {
            if (selectorIndex == 0)
            {
                player.UpgradePlayerClass(player.PlayerClass.nextClasses[classIndex]);
                InternalContentManager.LoadPlayer(SaveGameManager.CurrentPlayer.PlayerClass);
                SaveGameManager.CurrentPlayer.InitializeTextures();
                ExitScreen();
            }
            else if (selectorIndex == 1)
            {
                classIndex -= 1;
                if (classIndex < 0)
                {
                    classIndex = player.PlayerClass.nextClasses.Count - 1;
                }
                RefreshScreen();
            }
            else if (selectorIndex == 2)
            {
                classIndex += 1;
                classIndex %= player.PlayerClass.nextClasses.Count;
                RefreshScreen();
            }
            
        }


        public void RefreshScreen()
        {
            PlayerClass playerClass = player.PlayerClass.nextClasses[classIndex];
            Clear();
            AddLine(playerClass.name, Fonts.ButtonFont, Color.Red);
            AddLine(playerClass.description, Fonts.DescriptionFont, Color.White);
            AddSpace(20);
            AddButton("Upgrade");
            SetFinalize();

            MenuEntry entry;
            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(0, height / 2);
            entry.Texture = arrowRight;
            entry.OwningPopup = this;
            entry.SpriteEffect = SpriteEffects.FlipHorizontally;
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Position = new Vector2(width + BorderAdjust.X + border, height / 2);
            entry.Texture = arrowRight;
            entry.OwningPopup = this;
            MenuEntries.Add(entry);
        }




    }
}
