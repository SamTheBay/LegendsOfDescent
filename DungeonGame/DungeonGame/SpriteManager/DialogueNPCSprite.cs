using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class DialogueNPCSprite : NPCSprite, IEnvItem, IDialogueAction
    {
        public Point OccupiedSlot { get; set; }

        protected string text;
        protected bool hasDialogueAction = false;
        protected string[] buttonText = { "Accept", "Reject" };


        public DialogueNPCSprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset, PlayerSprite player)
            : base(nPosition, nFrameOrigin, nSourceOffset, player)
        {

        }

        public virtual bool CanItemActivate()
        {
            return true;
        }


        public virtual bool ActivateItem(PlayerSprite player)
        {
            if (!player.QuestLog.TalkedToNpc(this)) return false;

            DialogueScreen dialogue = new DialogueScreen(null, Rectangle.Empty, Name, text);
            if (hasDialogueAction)
            {
                dialogue.SetActionable(this, buttonText);
            }


            GameplayScreen.Instance.ScreenManager.AddScreen(dialogue);
            InputManager.ClearInputForPeriod(500);

            return false;
        }


        public virtual void DialogueAction(int buttonSelected)
        {
            // implemented by inherited classes if they ask a question in their dialogue
        }
    }
}