using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#if WINDOWS_PHONE
using Microsoft.Phone.Tasks;
#endif

#if WINDOWS_PHONE || WIN8
using Microsoft.Xna.Framework.Input.Touch;
#endif

namespace LegendsOfDescent
{
    public class ReviewScreen : DescriptionScreen
    {

        public ReviewScreen()
        {
            Initialize();
            AddLine("Review LoD", Fonts.DCFont, Color.Red);
            AddSpace(10f);
            AddLine("Are you enjoying your time with Legends of Descent? If so, give a little back and post a review for the game. It only takes a second and you can hit back to resume playing without even having to reload. Thanks for all your support!", Fonts.DescriptionFont, Color.White);
            AddSpace(20f);
            AddButton("Sure!");
            AddSpace(10f);
            AddButton("Nope");
            SetFinalize();

            exitOnTouch = false;

            EnabledGestures = GestureType.Tap;
        }


        public override void entry_Selected(object sender, EventArgs e)
        {
            base.entry_Selected(sender, e);

            if (selectedEntry == 0)
            {
#if WINDOWS_PHONE
                MarketplaceReviewTask task = new MarketplaceReviewTask();
                task.Show();
#endif
#if WIN8
                Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=9610AwesomerGames.LegendsofDescent_z81gxng9d6ydy"));
#endif
            }
        }

    }
}
