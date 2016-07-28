using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class ResolutionSelectScreen : DescriptionScreen
    {
        List<Vector2> resolutions = new List<Vector2>();

        public ResolutionSelectScreen()
        {
            resolutions.Add(new Vector2(1366, 768));
            resolutions.Add(new Vector2(1360, 768));
            resolutions.Add(new Vector2(1280, 768));
            resolutions.Add(new Vector2(1280, 720));
            resolutions.Add(new Vector2(1024, 768));
            resolutions.Add(new Vector2(1024, 600));
            resolutions.Add(new Vector2(800, 600));

            exitOnTouch = false;

            Initialize();

            if (DungeonGame.fullScreen)
                AddButton("FullScreen");
            else
                AddButton("Windowed");
            AddSpace(20f);

            for (int i = 0; i < resolutions.Count; i++)
            {
                AddButton(resolutions[i].X.ToString() + " x " + resolutions[i].Y.ToString());
                if (i != resolutions.Count - 1)
                    AddSpace(20f);
            }

            SetFinalize();

        }


        protected override void OnSelectEntry(int entryIndex)
        {
            if (selectedEntry == 0)
            {
                DungeonGame.fullScreen = !DungeonGame.fullScreen;
                if (DungeonGame.fullScreen)
                    MenuEntries[0].Text = "FullScreen";
                else
                    MenuEntries[0].Text = "Windowed";
            }
            else
            {
                DungeonGame.Instance.SetResolution(resolutions[selectedEntry - 1]);
                ExitScreen();
            }

            SaveGameManager.PersistSettings();
        }

    }
}
