using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace LegendsOfDescent
{

    public interface IEnvItem
    {
        bool ActivateItem(PlayerSprite player);

        bool CanItemActivate();

        Vector2 CenteredPosition { get; }

        Vector2 Position { get; set; }

        Point OccupiedSlot { get; set; }

        void Update(GameTime gameTime);

        bool IsActive { get; }

        Rectangle GetBoundingRectangle(ref Rectangle rect);

        void Activate();

        void Draw(SpriteBatch spriteBatch);

        void ShowName();
    }


    public class EnvItemSprite : GameSprite, IEnvItem
    {
        protected int offset;
        protected Rectangle sourceRectangle;


        public Point OccupiedSlot { get; set; }

        public EnvItemSprite(Vector2 position, string texture, int framesPerRow, int offset, Point frameDimensions, bool initializeTextures = true)
            : base(position, texture, frameDimensions, new Point(frameDimensions.X / 2, frameDimensions.Y / 2), framesPerRow, Vector2.Zero, initializeTextures)
        {
            this.TextureName = texture;
            this.offset = offset;

            int column = (offset) / framesPerRow;
            sourceRectangle = new Rectangle(
                (offset - (column * framesPerRow)) * FrameDimensions.X,
                column * FrameDimensions.Y,
                FrameDimensions.X, FrameDimensions.Y);
        }


        public void SetTextureOffset(int offset)
        {
            this.offset = offset;

            int column = (offset) / FramesPerRow;
            sourceRectangle = new Rectangle(
                (offset - (column * FramesPerRow)) * FrameDimensions.X,
                column * FrameDimensions.Y,
                FrameDimensions.X, FrameDimensions.Y);
        }


        public override void Deactivate()
        {
            GameplayScreen.Dungeon.ReleaseItemPosition(OccupiedSlot);
            base.Deactivate();
        }


        // return true if the item should be removed from the dungeon
        public virtual bool ActivateItem(PlayerSprite player)
        {
            return false;
        }

        public virtual bool CanItemActivate()
        {
            return false;
        }


    }
}
