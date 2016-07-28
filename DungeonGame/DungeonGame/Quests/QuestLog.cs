using System;
using System.Collections.Generic;
using System.IO;
using LegendsOfDescent;
using System.Linq;
using System.Xml.Linq;

namespace LegendsOfDescent.Quests
{
    public class QuestLog : ISaveable
    {
        private DungeonLevel dungeon;
        private PlayerSprite player;

        private List<Quest> quests = new List<Quest>();
        public List<Quest> Quests { get { return quests; } }

        public IEnumerable<Quest> Active { get { return quests.Where(q => !q.IsComplete); } }

        public IEnumerable<Quest> Complete { get { return quests.Where(q => q.IsComplete); } }

        public static List<Quest> FixedQuests = new List<Quest>();

        Quest questUpdatedNotification = null;

        public void Initialize(PlayerSprite player)
        {
            this.player = player;

            quests.ForEach(q => q.Initialize(player));
        }

        public void SetDungeon(DungeonLevel dungeon)
        {
            this.dungeon = dungeon;

            quests.ForEach(q => q.SetDungeon(dungeon));
        }

        public void PlayerUpdated(PlayerSprite player)
        {
            Active.ForEach(q => q.PlayerUpdated(player));
            CheckComplete();
        }

        public void EnemyDied(EnemySprite enemy, bool killedByPlayer)
        {
            Active.ForEach(q => q.EnemyDied(enemy, killedByPlayer));
            CheckComplete();
        }

        public void ItemSold(ItemSprite selectedItem)
        {
            Active.ForEach(q => q.ItemSold(selectedItem));
            CheckComplete();
        }

        public void ItemPickedUp(ItemSprite pickedUpItem)
        {
            Active.ForEach(q => q.ItemPickedUp(pickedUpItem));
            CheckComplete();
        }

        public bool TalkedToNpc(DialogueNPCSprite npc)
        {
            var shouldActivateNpc = Active.Aggregate(true, (val, quest) => val && quest.TalkedToNpc(npc));
            CheckComplete();
            return shouldActivateNpc;
        }

        private void CheckComplete()
        {
            foreach (var quest in Quests)
            {
                if (!quest.WasRewarded && quest.IsComplete)
                {
                    quest.Reward.GiveTo(player);
                    quest.SetRewarded();
                    DungeonGame.ScreenManager.AddScreen(new QuestCompleteScreen(quest));
                    InputManager.ClearInputForPeriod(500);
                }
            }
        }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(quests.Count);
            foreach (var quest in quests)
            {
                writer.Write(quest.GetType().Name);
                writer.Write(quest);
            }
        }

        public static string QuestNamespace = typeof(QuestLog).Namespace;
        public bool Load(BinaryReader reader, int dataVersion)
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string name = reader.ReadString();
                Quest quest = Create(name);
                quest.Load(reader, dataVersion);
                quests.Add(quest);
            }

            return true;
        }

        private static Quest Create(string name, QuestParams queryParams = null)
        {
            var type = Type.GetType(QuestNamespace + "." + name);
            if (queryParams != null)
            {
                return (Quest)Activator.CreateInstance(type, queryParams);
            }
            else
            {
                return (Quest)Activator.CreateInstance(type);
            }
        }

        public static void LoadFixedQuests()
        {
            FixedQuests.Clear();
            QuestParams questParams = new QuestParams();
#if WIN8
            XDocument root = XmlHelper.Load("Assets/Data/Quests.xml");
#else
            XDocument root = XmlHelper.Load("Content/Data/Quests.xml");
#endif
            foreach (var questNode in root.Descendants("Quest"))
            {
                questParams.Clear();

                var baseNode = questNode.Descendants("BaseQuest").Single();
                var typeName = baseNode.Attribute("Name").Value + "Quest";

                foreach (var p in baseNode.Descendants("Param"))
                {
                    questParams.Add(
                        p.Attribute("Name").Value,
                        p.Attribute("Value").Value);
                }

                var quest = Create(typeName, questParams);
                quest.Name = questNode.Attribute("Name").Value;
                quest.AskText = questNode.Attribute("AskText").Value;
                quest.InProgressText = questNode.Attribute("InProgressText").Value;
                quest.Description = questNode.Attribute("Description").Value;
                quest.AvailableAtLevel = questNode.Attribute("AvailableAtLevel").IntValue();
                quest.BlocksEnteringLevel = questNode.Attribute("BlocksEnteringLevel").IntValue();
                quest.Prerequisites = questNode.Attribute("Prerequisites").IfNotNull(a => a.Value.Split(',', ';'));
                quest.IsAbandonable = false;

                var rewardNode = questNode.Descendants("Reward").Single();
                if (rewardNode != null)
                {
                    quest.Reward.Gold = rewardNode.Attribute("Gold").IntValue();
                    quest.Reward.Experience = rewardNode.Attribute("Experience").IntValue();
                }
                FixedQuests.Add(quest);
            }
        }

        public bool CanAdvanceToLevel(int level)
        {
            return !FixedQuests.Any(q => level >= q.BlocksEnteringLevel && !Quests.Any(a => a.Name == q.Name && a.IsComplete));
        }

        public Quest GetQuestFromGiver(string questerName)
        {
            return this.quests.FirstOrDefault(q => !q.IsComplete && q.QuestGiverName == questerName);
        }

        public Quest GetQuestToReturnToNPC(string questerName)
        {
            return this.quests.FirstOrDefault(q => !q.IsComplete && q.ReturnToNpcName == questerName);
        }

        public void SetQuestUpdateNotification(Quest quest)
        {
            questUpdatedNotification = quest;
        }

        public void ResetQuestUpdateNotification()
        {
            questUpdatedNotification = null;
        }

        public bool HasQuestUpdateNotification
        {
            get { return questUpdatedNotification != null; }
        }

        public Quest QuestForUpdateNotification
        {
            get { return questUpdatedNotification; }
        }

        public static void GiveUnassignedQuestsForLevel(int level, PlayerSprite player)
        {
            IEnumerable<Quest> quests = FixedQuests.Where(q => q.DungeonLevel == level);
            foreach (Quest quest in quests)
            {
                if (!player.QuestLog.quests.Any(q => q.Name == quest.Name))
                {
                    quest.Initialize(player);
                    player.QuestLog.Quests.Add(quest);
                    quest.QuestGiverName = QuesterSprite.MainQuesterName;
                }
            }
        }
    }
}
