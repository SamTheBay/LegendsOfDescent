using System;
using System.IO;
using Microsoft.Xna.Framework;
using System.Linq;
using LegendsOfDescent;

namespace LegendsOfDescent.Quests
{
    public class KillChampionQuest : Quest
    {
        enum State
        {
            Init,
            Created,
            Killed
        }

        private string championName = "";
        private string itemName = "";
        private State state = State.Init;

        public KillChampionQuest()
        {
        }

        public KillChampionQuest(QuestParams p)
        {
            this.DungeonLevel = p.GetInt("dungeonLevel");
            this.championName = p["championName"];

            int goldModifier = Util.Random.InRange(10, 20);
            int expModifier = 20 - goldModifier;
            Reward.Gold = goldModifier * BalanceManager.GetBaseGoldDrop(this.DungeonLevel);
            Reward.Experience = expModifier * BalanceManager.GetExperienceForEnemyKill(this.DungeonLevel);

            this.Name = "Slay " + this.championName;
            this.Description = this.championName + " is wreaking havoc on dungeon level " + DungeonLevel.ToString() + ". Destroy him.";
            this.InProgressText = championName + " must be stopped quickly. Don't waste any time.";
            this.AskText = "The abomination " + this.championName + " is wreaking havoc on dungeon level " + DungeonLevel.ToString() + ". Can you defeat him to ensure our safety?";

            if (p.ContainsKey("returnToNpcName"))
            {
                this.ReturnToNpcName = p["returnToNpcName"];
            }
            else
            {
                this.ReturnToNpcName = QuesterSprite.MainQuesterName;
            }
        }

        public override void SetDungeon(DungeonLevel dungeon)
        {
            base.SetDungeon(dungeon);

            if (dungeon.Level == DungeonLevel && state == State.Init)
            {
                var champion = EnemySprite.GetEnemy(BalanceManager.DungeonLevelToMonsterLevel(DungeonLevel), Vector2.Zero, player, this.dungeon.EnemyTypes.ToArray().Random());
                champion.SetChampion();
                champion.Name = championName;
                dungeon.AddNpc(champion, 10);

                // Give the champion a special item
                var item = ItemManager.Instance.GetBasicItem(DungeonLevel, MerchantType.Equipable);
                item.Name = itemName = this.championName + "'s " + item.Name;
                item.Value = (int)((100 + (int)champion.ChampionTier * 10 * Util.Random.Between(5f, 10f)) * DungeonLevel);
                champion.AddSpecialLoot(item);
                state = State.Created;
            }
        }

        public override void EnemyDied(EnemySprite enemy, bool killedByPlayer)
        {
            if (isComplete) return;

            if (enemy.Name == this.championName)
            {
                player.QuestLog.SetQuestUpdateNotification(this);
                this.Description = "You have slain " + championName + "! Return to " + ReturnToNpcName + " for your reward";
                state = State.Killed;
            }
        }

        public override string Status
        {
            get 
            {
                if (state == State.Killed)
                {
                    return "the champion is slain!";
                }
                else
                {
                    return "the champion lives!";
                }
            }
        }

        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);
            writer.Write(this.championName);
            writer.Write(this.itemName);
            writer.Write((Int32)state);
        }

        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);
            this.championName = reader.ReadString();
            this.itemName = reader.ReadString();

            if (dataVersion >= 7)
            {
                this.state = (State)reader.ReadInt32();
            }

            return true;
        }


        public override bool TalkedToNpc(DialogueNPCSprite npc)
        {
            if (!isComplete && state == State.Killed && npc.Name == this.ReturnToNpcName)
            {
                this.isComplete = true;
                return false;
            }
            return true;
        }

        public override bool ReadyForReturn()
        {
            return (!isComplete && state == State.Killed);
        }
    }
}
