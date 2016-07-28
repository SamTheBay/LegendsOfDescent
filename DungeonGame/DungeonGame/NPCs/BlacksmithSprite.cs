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
    class BlacksmithSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {
        public BlacksmithSprite(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = "Blacksmith";
            text = "It's a dangerous world out there. I can fix up your gear to give you a better chance of surviving if you want.";
            buttonText[0] = "Show me";
            buttonText[1] = "Not interested";
            hasDialogueAction = true;
            this.level = level;
            centeredReduce = 60;

            AddTexture("NPCCommonGray", new Point(128, 128), 1);
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
        }



        public override void DialogueAction(int buttonSelected)
        {
            if (buttonSelected == 0)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new BlacksmithScreen(player, this));
                InputManager.ClearInputForPeriod(500);
            }
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write((UInt32)maxHealthBase);
            writer.Write((UInt32)maxManaBase);
            writer.Write((UInt32)level);
            writer.Write(Position);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            maxHealthBase = reader.ReadInt32();
            maxManaBase = reader.ReadInt32();
            level = reader.ReadInt32();
            Position = reader.ReadVector2();

            return true;
        }



        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            // draw the blacksmiths name
            nameSprite.Color = friendlyNPCColor;
            nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, 0) + GameplayScreen.viewportCorner;
            nameSprite.Draw(spriteBatch);

        }



    }
}
