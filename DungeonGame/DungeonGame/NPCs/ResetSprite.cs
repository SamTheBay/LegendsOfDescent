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
    class ResetSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {
        int resetCost = 1000;
        ContinueQuestion contQuestion = new ContinueQuestion("Would you like to unlock this vender which can reset your characters ability points in exchange for gold?", "Purchase");
        bool contQuestionAdded = false;

        public ResetSprite(Vector2 nPosition, PlayerSprite player)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = "Reset";

            text = "I can give you a second chance. For " + resetCost + " gold I can reset your ability points and allow you choose once again. Would you like that?";
            buttonText[0] = "Do it";
            buttonText[1] = "No";
            hasDialogueAction = true;
            centeredReduce = 60;

            AddTexture("NPCMageRed2", new Point(128, 128), 1);
            AddAnimationSet("Idle", 1, 0, 100, false);

            InitializeTextures();
            PlayAnimation("IdleRight");

            Activate();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lastDirection = GetDirectionFromVector(SaveGameManager.CurrentPlayer.CenteredPosition - CenteredPosition);
            PlayAnimation("Idle", lastDirection);

            if (contQuestionAdded)
            {
                if (contQuestion.IsFinished)
                {
                    contQuestionAdded = false;
                    if (contQuestion.Result == true)
                    {
#if WIN8
                        DungeonGame.productsManager.PurchaseLicense("Reset Character");
#endif
                    }
                }
            }
        }



        public override void DialogueAction(int buttonSelected)
        {
            if (buttonSelected == 0 && player.Gold >= resetCost)
            {
                player.AddGold(resetCost * -1);
                player.ResetAbilities();
            }
        }


        public override bool ActivateItem(PlayerSprite player)
        {
            bool isValid = true;
            resetCost = BalanceManager.GetBaseGoldDrop(player.Level) * 100;

#if WIN8
            isValid = DungeonGame.productsManager.IsLiscenseValid("Reset Character");
            resetCost = BalanceManager.GetBaseGoldDrop(player.Level) * 50;
#endif

            bool playerHasGold = player.Gold >= resetCost;

            if (isValid && playerHasGold)
            {
                text = "I can give you a second chance. For " + resetCost + " gold I can reset your ability points and allow you choose once again. Would you like that?";
                hasDialogueAction = true;
                return base.ActivateItem(player);
            }
            else if (isValid && !playerHasGold)
            {
                text = "Your destiny can be changed for a price, but you will need " + resetCost + " gold. Come back when you have it.";
                hasDialogueAction = false;
                return base.ActivateItem(player);
            }
            else
            {
                contQuestion.Reset();
                DungeonGame.ScreenManager.AddScreen(contQuestion);
                contQuestionAdded = true;
            }
            return false;
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write(Position);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            Position = reader.ReadVector2();

            return true;
        }



        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            // draw name
            nameSprite.Color = friendlyNPCColor;
            nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, 0) + GameplayScreen.viewportCorner;
            nameSprite.Draw(spriteBatch);

        }



    }
}
