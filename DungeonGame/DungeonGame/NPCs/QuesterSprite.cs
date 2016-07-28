using System.IO;
using LegendsOfDescent;
using LegendsOfDescent.Quests;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;

namespace LegendsOfDescent
{
    public class QuesterSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {
        public const string MainQuesterName = "Quentin";

        Quest nextQuest;
        bool mainQuester;
        Texture2D questTex = InternalContentManager.GetTexture("QuestAvail");

        public QuesterSprite(Vector2 nPosition, PlayerSprite player, bool mainQuester = false)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = MainQuesterName;
            text = "I need your help with an urgent task!";
            buttonText[0] = "I accept";
            buttonText[1] = "I can't help you";
            hasDialogueAction = true;
            centeredReduce = 60;
            this.mainQuester = mainQuester;

            AddTexture("NPCCommonYellow", new Point(128, 128), 1);
            AddAnimationSet("Idle", 1, 0, 100, false);

            InitializeTextures();
            PlayAnimation("IdleRight");

            Activate();
        }

        public void GiveQuest()
        {
            if (nextQuest != null)
            {
                this.player.QuestLog.Quests.Add(nextQuest);
                nextQuest.Initialize(this.player);
                nextQuest.SetDungeon(GameplayScreen.Dungeon);
            }
        }

        public Quest RandomQuest()
        {
            // there are not random quests on boss levels
            LevelConfig level = LevelOrchestrator.GetConfig(player.DungeonLevelsGenerated + 1);
            if (level.type == "Boss")
                return null;

            QuestParams p = new QuestParams();
            p["returnToNpcName"] = Name;
            var percent = Util.Random.Next(100);
            switch (Util.Random.From(1, 1, 1, 2, 2, 2, 3, 3, 3))
            {
                case 1:
                    p["championName"] = NameGenerator.GetName(NameGenerator.Style.OrcTolkien);
                    p["dungeonLevel"] = (player.DungeonLevelsGenerated + 1).ToString();
                    return new KillChampionQuest(p);
                case 2:
                    return EnemyKillsQuest.Generate(player, Name);
                case 3:
                    p["dungeonLevel"] = p["itemLevel"] = (player.DungeonLevelsGenerated + 1).ToString();
                    return new FindItemQuest(p);
                default:
                    return null;
            }
        }

        public void SelectNextQuest()
        {
            // check if we already have a quest from this guy
            Quest currentQuest = player.QuestLog.GetQuestFromGiver(Name);
            if (currentQuest != null)
            {
                hasDialogueAction = false;
                text = currentQuest.InProgressText;
                return;
            }

            // then check if we already have a valid quest to give
            if (nextQuest == null ||
                nextQuest.IsComplete ||
                nextQuest.DungeonLevel != player.DungeonLevelsGenerated + 1)
            {
                nextQuest = null;

                if (mainQuester)
                {
                    var unassignedQuests = QuestLog.FixedQuests.Except(player.QuestLog.Quests, Quest.NameComparer);
                    var questCandidates =
                        unassignedQuests
                        .Where(q => player.DungeonLevelsGenerated >= q.AvailableAtLevel)
                        .Where(q => player.QuestLog.Complete.Count(o => q.Prerequisites.Contains(o.Name)) == q.Prerequisites.Length);

                    if (questCandidates.Any())
                    {
                        nextQuest = questCandidates.ToArray().Random();
                    }
                    else
                    {
                        nextQuest = RandomQuest();
                    }
                }
                else
                {
                    nextQuest = RandomQuest();
                }
            }

            if (nextQuest != null)
            {
                nextQuest.QuestGiverName = Name;
                text = nextQuest.AskText;
                hasDialogueAction = true;
            }
            else
            {
                text = "I have no quests to give you at this time.";
                hasDialogueAction = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lastDirection = GetDirectionFromVector(player.CenteredPosition - CenteredPosition);
            PlayAnimation("Idle", lastDirection);
        }

        public override bool ActivateItem(PlayerSprite player)
        {
            if (!player.QuestLog.TalkedToNpc(this)) return false;

            SelectNextQuest();

            DialogueScreen dialogue = new DialogueScreen(null, Rectangle.Empty, Name, text);
            if (hasDialogueAction)
            {
                dialogue.SetActionable(this, buttonText);
            }


            GameplayScreen.Instance.ScreenManager.AddScreen(dialogue);
            InputManager.ClearInputForPeriod(500);

            return false;
        }

        public override void DialogueAction(int buttonSelected)
        {
            if (buttonSelected == 0)
            {
                GiveQuest();
                InputManager.ClearInputForPeriod(500);
            }
        }

        
        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write(Position);
            writer.Write(mainQuester);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            Position = reader.ReadVector2();
            mainQuester = reader.ReadBoolean();

            return true;
        }


        public bool QuestAvailable()
        {
            Quest currentQuest = player.QuestLog.GetQuestFromGiver(Name);
            if (currentQuest != null)
                return false;

            if (nextQuest == null)
                SelectNextQuest();

            if (nextQuest == null)
                return false;

            return true;
        }

        public bool QuestToComplete()
        {
            Quest currentQuest = player.QuestLog.GetQuestToReturnToNPC(Name);
            if (currentQuest != null && currentQuest.ReadyForReturn())
                return true;

            return false;
        }


        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            // draw the merchants name
            nameSprite.Color = friendlyNPCColor;
            nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, 10) + GameplayScreen.viewportCorner;
            nameSprite.Draw(spriteBatch);

            // draw if there is a new quest available
            if (QuestToComplete())
            {
                spriteBatch.Draw(questTex, position + new Vector2(FrameDimensions.X / 2 - questTex.Width / 2, -1 * questTex.Height), Color.Green);
            }
            else if (QuestAvailable())
            {
                spriteBatch.Draw(questTex, position + new Vector2(FrameDimensions.X / 2 - questTex.Width / 2, -1 * questTex.Height), Color.Yellow);
            }
        }
    }
}
