using LegendsOfDescent.Quests;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    class QuestCompleteScreen : DescriptionScreen
    {

        public QuestCompleteScreen(Quest quest)
            : base()
        {
            exitOnTouch = false;
            GameplayScreen.Instance.Pause();

            AddLine("Quest Complete!", Fonts.ButtonFont, Color.Red);
            AddLine(quest.Name, Fonts.DescriptionFont, Color.White);
            AddSpace(20f);
            AddLine("Rewards:", Fonts.ButtonFont, Color.White);
            if (quest.Reward.Experience > 0)
            {
                AddLine(quest.Reward.Experience.ToString() + " exp", Fonts.DescriptionFont, Color.MediumPurple);
            }
            if (quest.Reward.Gold > 0)
            {
                AddLine(quest.Reward.Gold.ToString() + " gold", Fonts.DescriptionFont, Color.Goldenrod);
            }
            if (quest.Reward.Gold == 0 && quest.Reward.Experience == 0)
            {
                AddLine("none", Fonts.DescriptionFont, Color.LightGray);
            }
            AddSpace(20f);
            AddButton("Close");

            SetFinalize();
        }

       
        public override void OnRemoval()
        {
            base.OnRemoval();

            GameplayScreen.Instance.UnPause();
        }


    }
}
