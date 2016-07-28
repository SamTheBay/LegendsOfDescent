using System.IO;
using Microsoft.Xna.Framework;
using System.Linq;
using LegendsOfDescent;
using System;

namespace LegendsOfDescent.Quests
{
    public class KillInvadersQuest : Quest
    {
        enum State
        {
            Created,
            Placed,
            Killed
        }

        private EnemyType enemyType;
        private int invaderCount;
        private int remainingCount;
        private State state = State.Created;

        public KillInvadersQuest()
        {
        }

        public KillInvadersQuest(QuestParams p)
        {
            this.DungeonLevel = p.GetInt("dungeonLevel");
            this.invaderCount = p.GetInt("invaderCount");
            this.enemyType = (EnemyType)Enum.Parse(typeof(EnemyType), p.Get("enemyType"), true);
            Reward.Gold = BalanceManager.GetBaseGoldDrop(this.DungeonLevel) * Math.Min(1, invaderCount / 5);
            Reward.Experience = BalanceManager.GetExperienceForEnemyKill(this.DungeonLevel) * Math.Min(1, invaderCount / 5);

            this.Name = "Slay the Invaders";
            this.Description = "Several " + this.enemyType.ToDescription().ToLower() + "s have invaded. Finish them off.";
            this.InProgressText = "We need your help protecting us, please defend us.";
            this.AskText = this.enemyType.ToDescription().ToLower() + "s have invaded the town, can you fend them off?";

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

            if (state == State.Created && dungeon.Level == DungeonLevel && SaveGameManager.CurrentPlayer.DungeonLevelsGenerated == DungeonLevel)
            {
                if (DungeonGame.touchEnabled)
                {
                    if (DungeonGame.joystickEnabled)
                    {
                        HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.HowToAttackJoystick);
                    }
                    else
                    {
                        HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.HowToAttackTouch);
                    }
                }
                else
                {
                    HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.HowToAttack);
                }
                EnemySprite.PlaceEnemies(this.enemyType, this.invaderCount, this.dungeon, BalanceManager.DungeonLevelToMonsterLevel(DungeonLevel), this.player);
                this.dungeon.InitializeEnemyTextures();
                this.remainingCount = this.invaderCount;
                state = State.Placed;
            }
        }

        public override void EnemyDied(EnemySprite enemy, bool killedByPlayer)
        {
            if (isComplete) return;

            if (enemy.GetEnemyType() == this.enemyType)
            {
                if (remainingCount > 0)
                    this.remainingCount -= 1;

                if (this.remainingCount == 0)
                {
                    this.Description = "You have stopped the invasion! Return to " + ReturnToNpcName + " for your reward.";
                    state = State.Killed;
                    player.QuestLog.SetQuestUpdateNotification(this);
                    HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.QuestComplete);
                }
            }
        }
        
        public override string Status
        {
            get { return this.remainingCount + " of " + this.invaderCount + " invaders still live!"; }
        }

        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);
            writer.Write(this.DungeonLevel);
            writer.Write((int)this.enemyType);
            writer.Write(this.invaderCount);
            writer.Write(this.remainingCount);
            writer.Write((Int32)state);
        }

        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);
            this.DungeonLevel = reader.ReadInt32();
            this.enemyType = (EnemyType)reader.ReadInt32();
            this.invaderCount = reader.ReadInt32();
            this.remainingCount = reader.ReadInt32();
            this.state = (State)reader.ReadInt32();

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
