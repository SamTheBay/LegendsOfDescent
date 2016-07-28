using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendsOfDescent;

namespace LegendsOfDescent
{
    public class TownPortal : EnvItemSprite, ISaveable
    {
        int dungeonLevel = 0;
        Vector2 dungeonLocation = Vector2.Zero;

        public TownPortal(Vector2 position, bool initializeTextures = true)
            : base(position, "TownPortal", 1, 0, new Point(64, 128), initializeTextures)
        {
            centeredReduce = 0;
        }


        public void SetTPProperties(int dungeonLevel, Vector2 dungeonLocation)
        {
            this.dungeonLevel = dungeonLevel;
            this.dungeonLocation = dungeonLocation;
        }

        public override bool ActivateItem(PlayerSprite player)
        {
            SaveGameManager.PersistDungeon(GameplayScreen.Dungeon);

            int newlevel = 0;
            Vector2 newPosition;
            if (SaveGameManager.CurrentPlayer.DungeonLevel == 0)
            {
                newlevel = dungeonLevel;
                newPosition = dungeonLocation;
                Deactivate();
            }
            else
            {
                newlevel = 0;
                newPosition = Position = GameplayScreen.Dungeon.GetTile(15, 16).Position;
            }


            DungeonLevel newDungeon = new DungeonLevel();
            player.DungeonLevel = newlevel;
            SaveGameManager.LoadDungeon(ref newDungeon);
            newDungeon.GoingDown = false;
            player.Position = newPosition;
            DungeonGame.ScreenManager.AddScreen(new InitialLoadScreen(newDungeon));
            DungeonGame.ScreenManager.RemoveScreen(GameplayScreen.Instance);


            player.StopMoving();

            return false;
        }


        public override bool CanItemActivate()
        {
            return true;
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (isActive)
            {
                spriteBatch.Draw(Texture, Position.Trunc() - GameplayScreen.viewportCorner.Trunc(), sourceRectangle, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, GetLayerDepth(3));
            }
        }


        public void SetPosition()
        {
            if (SaveGameManager.CurrentPlayer.DungeonLevel == 0)
            {
                if (Position != GameplayScreen.Dungeon.GetTile(15, 15).Position)
                    Position = GameplayScreen.Dungeon.GetTile(15, 15).Position;
            }
            else if (SaveGameManager.CurrentPlayer.DungeonLevel == dungeonLevel)
            {
                if (Position != dungeonLocation)
                    Position = dungeonLocation;
            }
            else
            {
                if (Position.X != -1000 && Position.Y != -1000)
                    Position = new Vector2(-1000, -1000);
            }
        }

        public override void Activate()
        {
            base.Activate();
            AudioManager.audioManager.PlaySFX("TownPort");

            SetPosition();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            SetPosition();

            if (isActive)
            {
                ParticleSystem.AddParticles(CenteredPosition, ParticleType.ExplosionWhite, color: Color.LightBlue, sizeScale: 1f, numParticlesScale: .02f);
            }
        }

        public virtual void Persist(BinaryWriter writer)
        {
            writer.Write((Int32)dungeonLevel);
            writer.Write(dungeonLocation);
            writer.Write(isActive);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            dungeonLevel = reader.ReadInt32();
            dungeonLocation = reader.ReadVector2();
            isActive = reader.ReadBoolean();

            return true;
        }

    }
}
