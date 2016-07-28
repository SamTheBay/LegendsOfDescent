using System;
using System.IO;

namespace LegendsOfDescent.Quests
{
    public class EnemyKillsQuest : Quest
    {
        private EnemyType enemyType;
        private int killCount = 0;
        private int killsNeeded = 0;

        public EnemyKillsQuest()
        {
        }

        public EnemyKillsQuest(QuestParams p)
        {
            this.enemyType = (EnemyType)Enum.Parse(typeof(EnemyType), p.Get("enemyType"), true);
            int enemyCount = p.GetInt("enemyCount");
            this.DungeonLevel = p.GetInt("dungeonLevel");
            this.killsNeeded = enemyCount;
            this.Name = "Slay " + this.killsNeeded + " " + this.enemyType.ToDescription() + "s";
            this.Description = "Rid the dungeon of the " + this.enemyType.ToDescription().ToLower() + " hoards on dungeon level " + this.DungeonLevel.ToString() + ".";
            this.InProgressText = "Keep killing those " + this.enemyType.ToDescription().ToLower() + "s. They don't have a chance against you!";
            this.AskText = "A horde of " + this.enemyType.ToDescription().ToLower() + "s has invaded the depths below the city on dungeon level " + this.DungeonLevel.ToString() + ". Can you exterminate them for us?";
            Reward.Gold = BalanceManager.GetBaseGoldDrop(this.DungeonLevel) * enemyCount / 5;
            Reward.Experience = BalanceManager.GetExperienceForEnemyKill(this.DungeonLevel) * enemyCount / 5;

            if (p.ContainsKey("returnToNpcName"))
            {
                this.ReturnToNpcName = p["returnToNpcName"];
            }
            else
            {
                this.ReturnToNpcName = QuesterSprite.MainQuesterName;
            }
        }

        public static EnemyKillsQuest Generate(PlayerSprite player, string returnToNpcName)
        {
            QuestParams p = new QuestParams();
            LevelConfig config = LevelOrchestrator.GetConfig(player.DungeonLevelsGenerated + 1);
            int enemyOffset = Util.Random.Next(0, config.Enemies.Count);
            int enemyCount = Util.Random.Between(Math.Min(50, config.Enemies[enemyOffset].Count), config.Enemies[enemyOffset].Count);
            p["enemyType"] = config.Enemies[enemyOffset].Type.ToString();
            p["enemyCount"] = enemyCount.ToString();
            p["dungeonLevel"] = (player.DungeonLevelsGenerated + 1).ToString();
            p["returnToNpcName"] = returnToNpcName;

            return new EnemyKillsQuest(p);
        }

        public override void EnemyDied(EnemySprite enemy, bool killedByPlayer)
        {
            if (isComplete) return;

            if (dungeon.Level != this.DungeonLevel) return;

            if (enemy.GetEnemyType() == this.enemyType)
            {
                this.killCount++;
            }

            if (this.killCount == this.killsNeeded)
            {
                player.QuestLog.SetQuestUpdateNotification(this);
                this.Description = "You have exterminated of the hoard of " + this.enemyType.ToDescription().ToLower() + ". return to " + ReturnToNpcName + " for your reward.";
            }
        }

        public override string Status
        {
            get { return this.killCount + " of " + this.killsNeeded; }
        }

        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);
            writer.Write((int)this.enemyType);
            writer.Write(this.killCount);
            writer.Write(this.killsNeeded);
        }

        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);
            this.enemyType = (EnemyType)reader.ReadInt32();
            this.killCount = reader.ReadInt32();
            this.killsNeeded = reader.ReadInt32();
            return true;
        }

        public override bool TalkedToNpc(DialogueNPCSprite npc)
        {
            if (!isComplete && this.killCount >= this.killsNeeded && npc.Name == this.ReturnToNpcName)
            {
                this.isComplete = true;
                return false;
            }
            return true;
        }


        public override bool ReadyForReturn()
        {
            if (this.killCount >= this.killsNeeded && !isComplete)
            {
                return true;
            }

            return false;
        }
    }
}
