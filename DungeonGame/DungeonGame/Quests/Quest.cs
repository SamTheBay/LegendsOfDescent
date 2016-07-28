using System.IO;
using LegendsOfDescent;

namespace LegendsOfDescent.Quests
{
    public abstract class Quest : ISaveable
    {
        public static readonly KeyEqualityComparer<Quest> NameComparer = new KeyEqualityComparer<Quest>(q => q.Name);

        protected PlayerSprite player;
        protected DungeonLevel dungeon;
        protected bool isComplete = false;
        protected bool wasRewarded = false;
        protected Reward reward = new Reward();

        public Quest()
        {
            this.ReturnToNpcName = string.Empty;
            this.QuestGiverName = string.Empty;
            this.DungeonLevel = 0;
            this.IsAbandonable = true;
        }

        public virtual void Initialize(PlayerSprite player)
        {
            this.player = player;
        }

        public virtual void SetDungeon(DungeonLevel dungeon)
        {
            this.dungeon = dungeon;
        }

        public bool WasRewarded 
        {
            get { return wasRewarded; }
            private set { wasRewarded = value; }
        }

        public void SetRewarded()
        {
            this.WasRewarded = true;
        }

        public Reward Reward
        {
            get { return reward; }
        }

        public string AskText { get; set; }

        public string InProgressText { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string[] prerequisites = new string[0];
        public string[] Prerequisites 
        {
            get { return prerequisites; }
            set { prerequisites = value; }
        }

        public abstract string Status { get; }

        public int AvailableAtLevel { get; set; }

        public int BlocksEnteringLevel { get; set; }

        public virtual void PlayerUpdated(PlayerSprite player) { }

        public virtual void EnemyDied(EnemySprite enemy, bool killedByPlayer) { }

        public virtual void ItemSold(ItemSprite selectedItem) { }

        public virtual void ItemPickedUp(ItemSprite pickedUpItem) { }

        /// <summary>
        /// Called when the player activates an NPC
        /// </summary>
        /// <param name="npc"></param>
        /// <returns>True if the activation should continue, false otherwise.</returns>
        public virtual bool TalkedToNpc(DialogueNPCSprite npc) { return true; }

        public virtual void AutoComplete()
        {
            isComplete = true;
            wasRewarded = true;
        }

        public virtual bool IsComplete 
        { 
            get { return this.isComplete; } 
        }

        public virtual string RewardDescription
        {
            get { return reward.ToString(); }
        }

        public virtual void Persist(BinaryWriter writer)
        {
            writer.Write(this.isComplete);
            writer.Write(this.wasRewarded);
            writer.Write(this.reward);
            
            writer.Write(this.AvailableAtLevel);
            writer.Write(this.BlocksEnteringLevel);
            writer.Write(this.Name);
            writer.Write(this.AskText);
            writer.Write(this.InProgressText);
            writer.Write(this.Description);
            writer.Write(this.Prerequisites);
            writer.Write(this.QuestGiverName);
            writer.Write(this.DungeonLevel);
            writer.Write(this.IsAbandonable);

            writer.Write(this.ReturnToNpcName);
        }

        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            this.isComplete = reader.ReadBoolean();
            this.wasRewarded = reader.ReadBoolean();
            this.reward.Load(reader, dataVersion);

            if (dataVersion <= 6) return true;

            this.AvailableAtLevel = reader.ReadInt32();
            this.BlocksEnteringLevel = reader.ReadInt32();
            this.Name = reader.ReadString();
            this.AskText = reader.ReadString();
            this.InProgressText = reader.ReadString();
            this.Description = reader.ReadString();
            this.Prerequisites = reader.ReadArrayOfString();
            this.QuestGiverName = reader.ReadString();
            this.DungeonLevel = reader.ReadInt32();
            this.IsAbandonable = reader.ReadBoolean();

            if (dataVersion <= 8)
            {
                this.ReturnToNpcName = this.QuestGiverName;
                if (string.IsNullOrEmpty(this.ReturnToNpcName))
                {
                    this.ReturnToNpcName = QuesterSprite.MainQuesterName;
                }
                return true;
            }

            this.ReturnToNpcName = reader.ReadString();

            return true;
        }


        public virtual bool ReadyForReturn()
        {
            return false;
        }


        public string QuestGiverName { get; set; }

        public int DungeonLevel { get; set; }

        public bool IsAbandonable { get; set; }

        public string ReturnToNpcName { get; set; }
    }
}
