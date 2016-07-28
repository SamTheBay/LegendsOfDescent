using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class Stairs : EnvItemSprite
    {
        bool stairsUp;

        public Stairs(bool stairsUp, TileSet tileSet, Vector2 position, bool initializeTextures = true)
            : base(position, tileSet.SetName, tileSet.FramesPerRow, tileSet.GetTileOffset(StairsTileType(stairsUp)), new Point(tileSet.TileDimension, tileSet.TileDimension), initializeTextures)
        {
            this.stairsUp = stairsUp;
        }

        static public TileType StairsTileType(bool stairsUp)
        {
            if (stairsUp)
            {
                return TileType.StairsUp;
            }
            else
            {
                return TileType.StairsDown;
            }
        }


        public override bool ActivateItem(PlayerSprite player)
        {
            if (!stairsUp && !player.QuestLog.CanAdvanceToLevel(GameplayScreen.Dungeon.Level + 1))
            {
                DungeonGame.ScreenManager.Message(
                    "Quest Incomplete",
                    "The town needs your help! Complete your quests before venturing deeper! Visit " + QuesterSprite.MainQuesterName + " in the east of the city to learn more.");
                return false;
            }
            //else if (DungeonGame.pcMode && !DungeonGame.paid && GameplayScreen.Dungeon.Level >= 5)
            //{
            //    DungeonGame.ScreenManager.Message(
            //        "End of Demo!",
            //        "If you enjoyed playing through the Demo then go to the Alpha Feedback page and let us know that we need to get off our butts and get the full version published so that you can enjoy it! Also, while you are there you can let us know what features you want added.");
            //    return false;
            //}
            else if (!DungeonGame.pcMode && DungeonGame.paid && DungeonGame.IsTrialModeCached && GameplayScreen.Dungeon.Level >= 5)
            {
                DungeonGame.ScreenManager.Message(
                    "End of Trial!",
                    "If you enjoyed playing through the trial then purchase the game in order to unlock it. Or, you can download the free ad supported version in the marketplace.");
                return false;
            }
            DungeonGame.ScreenManager.AddScreen(new StairsScreen(stairsUp, player, GameplayScreen.Dungeon));
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
                spriteBatch.Draw(Texture, Position.Trunc() - GameplayScreen.viewportCorner.Trunc(), sourceRectangle, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, GetLayerDepth(3));
            }
        }


        public override Rectangle GetBoundingRectangle(ref Rectangle rect)
        {
            if (stairsUp)
            {
                rect.X = (int)Position.X + centeredReduce / 2;
                rect.Y = (int)Position.Y + centeredReduce / 2;
                rect.Width = (int)(FrameDimensions.X * 2) - centeredReduce;
                rect.Height = (int)(FrameDimensions.Y * 2) - centeredReduce;
            }
            else
            {
                // stairs down only actually fill 1/4 of the space
                rect.X = (int)Position.X + centeredReduce / 2 + FrameDimensions.X;
                rect.Y = (int)Position.Y + centeredReduce / 2 + FrameDimensions.Y;
                rect.Width = (int)(FrameDimensions.X) - centeredReduce;
                rect.Height = (int)(FrameDimensions.Y) - centeredReduce;
            }
            return rect;
        }


        public override Vector2 CenteredPosition
        {
            get
            {
                // because we are doubling the frame size we need to add an extra frame size in order to get the center
                return base.CenteredPosition + (frameDimensions[0].ToVector2() / 2);
            }
        }

    }
}
