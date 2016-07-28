using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{

    public class SearchData
    {
        public bool[,] occupiedMap;
    }


    public class TileSprite : GameSprite, IPathNode<SearchData>, IEnvItem
    {
        public Point OccupiedSlot { get; set; }
        TileType[] tileTypes = new TileType[3]; // 3 layers of tiles can be piled up
        int[] tileOffset = new int[3];
        bool forceNotWalkable = false;
        bool forceFlyable = false;
        static Point nFrameOrigin = new Point(10, 10);
        static Vector2 nSourceOffset = new Vector2(0, 0);
        Rectangle[] sourceRectangle = new Rectangle[3];
        public static Point tileDimension = new Point(64, 64);
        public static Point drawingFrame = new Point(128, 64);
        Ability abilityEffect = null;
        bool isDoorOpen = false;
        bool isDoor = false;
        int loopIndex = 0;
        TileSet tileSet;

        public TileSprite(TileType tileType, Vector2 nPosition, TileSet tileSet)
            : this(tileType, TileType.Empty, TileType.Empty, nPosition, tileSet)
        {
        }

        public TileSprite(TileType tileType, TileType tileType2, Vector2 nPosition, TileSet tileSet)
            : this(tileType, tileType2, TileType.Empty, nPosition, tileSet)
        {
        }

        public TileSprite(TileType tileType, TileType tileType2, TileType tileType3, Vector2 nPosition, TileSet tileSet)
            : base(nPosition, tileSet.SetName, new Point(tileSet.SpriteDimension, tileSet.SpriteDimension), nFrameOrigin, tileSet.FramesPerRow, nSourceOffset, false)
        {
            this.tileSet = tileSet;
            this.tileTypes[0] = tileType;
            this.tileTypes[1] = tileType2;
            this.tileTypes[2] = tileType3;

            CrunchTileTypes();

            HasBeenSeen = false;
            TileLocation = new Point((int)Position.X / tileDimension.X, (int)Position.Y / tileDimension.Y);
            Activate();

            centeredReduce = 50;
        }


        public void CrunchTileTypes()
        {
            for (int i = 0; i < tileTypes.Length; i++)
            {
                if (tileTypes[i] != TileType.Empty)
                {
                    tileOffset[i] = tileSet.GetTileOffset(tileTypes[i]);
                    int column = (tileOffset[i]) / FramesPerRow;
                    sourceRectangle[i] = new Rectangle(
                        (tileOffset[i] - (column * FramesPerRow)) * FrameDimensions.X,
                        column * FrameDimensions.Y,
                        FrameDimensions.X, FrameDimensions.Y);
                }
                if (tileTypes[i] >= TileType.WoodenDoorVirtical && tileTypes[i] <= TileType.MetalDoorHorizontal)
                {
                    isDoor = true;
                }
            }
        }


        Vector2 newPosition = new Vector2();
        public override void Draw(SpriteBatch spriteBatch)
        {
            newPosition.X = Position.X - tileDimension.X - (int)GameplayScreen.viewportCorner.X;
            newPosition.Y = Position.Y - tileDimension.Y - (int)GameplayScreen.viewportCorner.Y;

            for (int tileIndex = 0; tileIndex < tileTypes.Length; tileIndex++)
            {
                float layerDepth;
                if (tileTypes[tileIndex] >= TileType.WallVertical && tileTypes[tileIndex] <= TileType.Column ||
                    tileTypes[tileIndex] >= TileType.InnerWallVertical && tileTypes[tileIndex] <= TileType.InnerWallRichCornerBottomLeft ||
                    tileTypes[tileIndex] >= TileType.Torch && tileTypes[tileIndex] <= TileType.MetalDoorHorizontal)
                {
                    layerDepth = GetLayerDepth(3);
                }
                else
                {
                    layerDepth = GetLayerDepth(0);
                }

                Color tint = Color.White;
                if (abilityEffect != null)
                {
                    tint = abilityEffect.GetAbilityTint();
                }

                // Special handling for doors
                if (tileTypes[tileIndex] >= TileType.WoodenDoorVirtical && tileTypes[tileIndex] <= TileType.MetalDoorHorizontal)
                {
                    bool isVert = false;
                    Rectangle source = new Rectangle(0 ,0, 64, 64);
                    if (tileTypes[tileIndex] == TileType.WoodenDoorVirtical)
                    {
                        source.Y = 64;
                        isVert = true;
                    }
                    else if (tileTypes[tileIndex] == TileType.MetalDoorHorizontal)
                    {
                        source.Y = 128;
                    }
                    else if (tileTypes[tileIndex] == TileType.MetalDoorVirtical)
                    {
                        source.Y = 192;
                        isVert = true;
                    }

                    if (isDoorOpen)
                    {
                        source.X = 128;
                        spriteBatch.Draw(InternalContentManager.GetTexture("Doors"), newPosition, source, tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                        source.X = 64;
                        if (isVert)
                            layerDepth = GetLayerDepth(3, 0, 63);
                        else
                            layerDepth = GetLayerDepth(3, 63, 0);
                        spriteBatch.Draw(InternalContentManager.GetTexture("Doors"), newPosition, source, tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                    }
                    else
                    {
                        spriteBatch.Draw(InternalContentManager.GetTexture("Doors"), newPosition, source, tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                    }
                }
                else if (tileTypes[tileIndex] >= TileType.Open && tileTypes[tileIndex] <= TileType.Open3 ||
                    tileTypes[tileIndex] >= TileType.Road && tileTypes[tileIndex] <= TileType.InnerInside4)
                {
                    Rectangle source = sourceRectangle[tileIndex];
                    source.X += 32;
                    source.Y += 32;
                    source.Width -= 32;
                    source.Height -= 32;
                    layerDepth -= .000001f * (float)tileIndex;
                    spriteBatch.Draw(Texture, newPosition + new Vector2(64, 64), source, tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                }
                else if (tileTypes[tileIndex] != TileType.Empty)
                {
                    layerDepth -= .000001f * (float)tileIndex;
                    spriteBatch.Draw(Texture, newPosition, sourceRectangle[tileIndex], tint, 0f, Vector2.Zero, 2f, SpriteEffects.None, layerDepth);
                }
            }

        }

        
        public bool ActivateItem(PlayerSprite player)
        {
            isDoorOpen = !isDoorOpen;

            if (isDoorOpen)
            {
                AudioManager.audioManager.PlaySFX("DoorOpen" + Util.Random.Next(1, 4).ToString());
            }
            else
            {
                AudioManager.audioManager.PlaySFX("DoorClose" + Util.Random.Next(1, 5).ToString());
            }

            return false;
        }


        public bool CanItemActivate()
        {
            return isDoor;
        }

        public override Rectangle GetBoundingRectangle(ref Rectangle rect)
        {
            rect.X = (int)Position.X;
            rect.Y = (int)Position.Y;
            rect.Width = FrameDimensions.X;
            rect.Height = FrameDimensions.Y;
            return rect;
        }

        public override Vector2 CenteredPosition
        {
            get
            {
                return Position + (tileDimension.ToVector2() / 2);
            }
        }

        public TileType[] TileTypes
        {
            get { return tileTypes; }
        }

        public Boolean IsEmpty()
        {
            for (int i = 0; i < tileTypes.Length; i++)
            {
                if (tileTypes[i] != TileType.Empty)
                    return false;
            }
            return true;
        }

        public bool ForceNotWalkable
        {
            set { forceNotWalkable = value; }
        }

        public bool IsFlyable
        {
            get { return forceFlyable || IsWalkable(null); }
            set { forceFlyable = value; }
        }

        public bool NotAWall
        {
            get { return forceNotWalkable || IsWalkable(null); }
        }

        public Boolean IsWalkable(SearchData searchData = null)
        {
            if (forceNotWalkable == true)
            {
                return false;
            }

            if (searchData != null)
            {
                if ((int)SaveGameManager.CurrentPlayer.CenteredPosition.X / tileDimension.X == (int)Position.X / tileDimension.X &&
                    (int)SaveGameManager.CurrentPlayer.CenteredPosition.Y / tileDimension.Y == (int)Position.Y / tileDimension.Y)
                {
                    return true;
                }

                if (searchData.occupiedMap != null)
                {
                    if (searchData.occupiedMap[(int)(Position.X / tileDimension.X), (int)(Position.Y / tileDimension.Y)])
                    {
                        return false;
                    }
                        
                }
            }
 
            if (IsEmpty())
                return false;

            for (int i = 0; i < tileTypes.Length; i++)
            {
                var t = tileTypes[i];
                if (t != TileType.Empty && t != TileType.Open && t != TileType.Open2 && t != TileType.Open3 &&
                    t != TileType.Road && t != TileType.InnerInside && t != TileType.InnerInside2 &&
                    t != TileType.InnerInside3 && t != TileType.InnerInside4 && t != TileType.Vent && t != TileType.Dirt &&
                    t != TileType.InnerInsideRich && t != TileType.InnerInsideRich2 && t != TileType.InnerInsideRich3 && t != TileType.InnerInsideRich4 &&
                    t != TileType.InnerInsideRich5 && t != TileType.InnerInsideRich6 &&
                    (!IsDoor || !isDoorOpen))
                {
                    return false;
                }
            }

            return true;
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            loopIndex++;

            for (int i = 0; i < tileTypes.Length; i++)
            {
                TileType t = tileTypes[i];
                if (t == TileType.Torch)
                {
                    if (loopIndex % 2 == 0)
                    {
                        ParticleSystem.AddParticles(Position, ParticleType.ExplosionPilliar, sizeScale: .4f, numParticlesScale: .01f, lifetimeScale: .6f, velocityScale: .5f);
                        ParticleSystem.AddParticles(Position, ParticleType.SmokePlume, sizeScale: .15f, numParticlesScale: .01f, lifetimeScale: .2f, velocityScale: .5f);
                    }
                }
            }
        }


        public void WakingOnTile(CharacterSprite sprite)
        {
            // when characters walk on this tile they will call into this function
            if (abilityEffect != null)
            {
                abilityEffect.TileAbilityEfffect(sprite);
            }
        }

        public void ForceNotColumn()
        {
            for (int i = 0; i < tileTypes.Length; i++)
            {
                if (tileTypes[i] == TileType.Column)
                {
                    tileTypes[i] = TileType.Empty;
                    CrunchTileTypes();
                }
            }
        }
        

        public Point TileLocation { get; set; }

        public bool HasBeenSeen { get; set; }

        public Ability AbilityEffect
        {
            get { return abilityEffect; }
            set { abilityEffect = value; }
        }

        public bool IsDoor
        {
            get { return isDoor; }
        }

        public bool IsDoorOpen
        {
            get { return isDoorOpen; }
            set { isDoorOpen = value; }
        }
    }
}
