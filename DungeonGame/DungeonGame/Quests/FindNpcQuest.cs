using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LegendsOfDescent.Quests
{
    public class FindNpcQuest : Quest
    {
        private string npcName = "";

        public FindNpcQuest() { }
        
        public FindNpcQuest(QuestParams p)
        {
            this.npcName = p["npcName"];
            this.ReturnToNpcName = npcName;
            this.reward.Experience = 10;
        }

        public override string Status
        {
            get { return "not found"; }
        }

        public override bool TalkedToNpc(DialogueNPCSprite npc)
        {
            if (npc.Name == this.npcName && !isComplete)
            {
                isComplete = true;
                //DungeonGame.ScreenManager.Message(npcName, "Congratulations, you have completed your first quest by finding " + npcName + "! He will help you discover this new world and your part in it.");
            }
            else
            {
                DungeonGame.ScreenManager.Dialogue(npc.Name, "Shouldn't you be looking for " + npcName + "? I believe he is on the east side of town.");
            }
            return false;
        }

        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);
            writer.Write(this.npcName);
        }

        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);
            this.npcName = reader.ReadString();
            return true;
        }

        public override bool ReadyForReturn()
        {
            return !isComplete;
        }
    }
}
