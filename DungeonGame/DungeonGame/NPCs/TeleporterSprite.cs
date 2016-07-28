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
    class TeleporterSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {
        public TeleporterSprite(Vector2 nPosition, PlayerSprite player)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = "Teleporter";
            text = "I have mastered the arts of teleportation. Would you like me to send you somewhere?";
            buttonText[0] = "Yes";
            buttonText[1] = "Not now";
            hasDialogueAction = true;
            centeredReduce = 60;

            AddTexture("NPCMageGreen", new Point(128, 128), 1);
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
                GameplayScreen.Instance.ScreenManager.AddScreen(new TeleportScreen(player));
                InputManager.ClearInputForPeriod(500);
            }
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
