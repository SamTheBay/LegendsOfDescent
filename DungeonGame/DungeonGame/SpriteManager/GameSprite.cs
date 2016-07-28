using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LegendsOfDescent
{
    public class GameSprite
    {
        protected bool isActive = false;
        protected bool isCollisionable = false;
        private Vector2 position = Vector2.Zero;
        private Vector2 centeredPosition = Vector2.Zero;
        protected bool isVirticleLooping = true;

        protected List<string> textureName = new List<string>();
        protected List<Texture2D> texture = new List<Texture2D>();
        protected List<int> framesPerRow = new List<int>();
        protected Vector2 sourceOffset;

        protected Rectangle boundingBox1 = new Rectangle();
        protected Rectangle boundingBox2 = new Rectangle();

        // adjustments
        protected int reduceTop = 0;
        protected int reduceSides = 0;
        protected int centeredReduce = 0;

        // Constructor used for when more than one texture will be added
        public GameSprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset)
        {
            Position = nPosition;
            frameOrigin = nFrameOrigin;
            sourceOffset = nSourceOffset;
        }


        // constructor used for only 1 texture. No need to call InitializeTextures() separately in that case.
        public GameSprite(Vector2 nPosition, string nTextureName, Point nFrameDimensions, Point nFrameOrigin, int nFramesPerRow, Vector2 nSourceOffset, bool initializeTextures = true)
        {
            textureName.Add(nTextureName);
            framesPerRow.Add(nFramesPerRow);
            frameDimensions.Add(nFrameDimensions);
            Position = nPosition;
            frameOrigin = nFrameOrigin;
            sourceOffset = nSourceOffset;
            if (initializeTextures)
                InitializeTextures();
        }


        // Should be called after all textures being used have been added and their assets have
        // been loaded already.
        public virtual void InitializeTextures()
        {
            for (int i = 0; i < textureName.Count; i++)
            {
                if (texture.Count <= i)
                {
                    texture.Add(InternalContentManager.GetTexture(textureName[i]));
                }
                else
                {
                    texture[i] = InternalContentManager.GetTexture(textureName[i]);
                }
            }
            Position = position;
        }


        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, 0);
        }

        public virtual void Draw(SpriteBatch spriteBatch, int segment)
        {
            // implemented by inherited classes
        }

        public virtual void Update(GameTime gameTime)
        {
            // implemented by inherited classes
        }



        public virtual bool CollisionDetect(GameSprite otherSprite)
        {
            // get the useable bounding rectangles
            Rectangle myRect = GetBoundingRectangle(ref boundingBox1);
            Rectangle theirRect = otherSprite.GetBoundingRectangle(ref boundingBox2);

            if (theirRect.Intersects(myRect))
            {
                return true;
            }

            // No intersection found
            return false;
        }


        public virtual void CollisionAction(GameSprite otherSprite)
        {
            // implemented by high level classes
        }


        public virtual bool IsActive
        {
            get { return isActive; }
        }

        public bool IsCollisionable
        {
            get { return isCollisionable && isActive; }
        }


        public Vector2 Position
        {
            get { return position; }
            set 
            { 
                position = value;
                centeredPosition.X = position.X + frameOrigin.X;
                centeredPosition.Y = position.Y + frameOrigin.Y; 
            }
        }

        public void SetPosition(float x, float y)
        {
            position.X = x;
            position.Y = y;
            centeredPosition.X = position.X + frameOrigin.X;
            centeredPosition.Y = position.Y + frameOrigin.Y; 
        }


        public void SetCenteredPosition(float x, float y)
        {
            position.X = x - frameOrigin.X;
            position.Y = y - frameOrigin.Y;
            centeredPosition.X = x;
            centeredPosition.Y = y; 
        }


        public void MovePosition(float x, float y)
        {
            position.X += x;
            position.Y += y;
            centeredPosition.X = position.X + frameOrigin.X;
            centeredPosition.Y = position.Y + frameOrigin.Y; 
        }


        static float longestLength = (float)(MathExt.Square(1200) + MathExt.Square(880)) * 10;
        public virtual float GetLayerDepth(int segment, float adjustX = 0, float adjustY = 0)
        {
            float layerDepth = (float)(MathExt.Square((double)(Position.X + frameDimensions[0].X + 200 + adjustX - GameplayScreen.viewportCorner.X - centeredReduce / 2)) +
                MathExt.Square((double)(Position.Y + frameDimensions[0].Y + 200 + adjustY - GameplayScreen.viewportCorner.Y - centeredReduce / 2)));
            layerDepth /= longestLength * 10;
            layerDepth *= segment + 1;
            layerDepth = 1 - layerDepth;
            return layerDepth;
        }


        public virtual void DrawBoundingBox(SpriteBatch spriteBatch)
        {
            // Add in to debug collision detection

            //Rectangle rect = new Rectangle();
            //GetBoundingRectangle(ref rect);
            //rect.X -= (int)GameplayScreen.viewportCorner.X;
            //rect.Y -= (int)GameplayScreen.viewportCorner.Y;
            //int thickness = 1;
            //spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(0, 255, 0)), new Rectangle(rect.X, rect.Y, rect.Width, thickness), Color.White, 0f);
            //spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(0, 255, 0)), new Rectangle(rect.X, rect.Y + rect.Height, rect.Width, thickness), Color.White, 0f);
            //spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(0, 255, 0)), new Rectangle(rect.X, rect.Y, thickness, rect.Height), Color.White, 0f);
            //spriteBatch.Draw(InternalContentManager.GetSolidColorTexture(new Color(0, 255, 0)), new Rectangle(rect.X + rect.Width, rect.Y, thickness, rect.Height), Color.White, 0f);
        }


        public virtual Vector2 CenteredPosition
        {
            get { return centeredPosition; }
        }

        public virtual void Activate()
        {
            isActive = true;
        }

        public virtual void Deactivate()
        {
            isActive = false;
        }

        public virtual void ShowName()
        {
            // default implementation is to do nothing
        }

        /// <summary>
        /// The dimensions of a single frame of animation.
        /// </summary>
        protected List<Point> frameDimensions = new List<Point>();

        /// <summary>
        /// The width of a single frame of animation.
        /// </summary>
        public Point FrameDimensions
        {
            get { return frameDimensions[0]; }
            set
            {
                frameDimensions[0] = value;
                frameOrigin.X = frameDimensions[0].X / 2;
                frameOrigin.Y = frameDimensions[0].Y / 2;
            }
        }


        public string TextureName
        {
            get { return textureName[0]; }
            set { textureName[0] = value; }
        }


        public Texture2D Texture
        {
            get { return texture[0]; }
            set { texture[0] = value; }
        }


        public void ResetTextures()
        {
            textureName.Clear();
            frameDimensions.Clear();
            framesPerRow.Clear();
            texture.Clear();
        }

        public int AddTexture(string textureName, Point frameDimensions, int framesPerRow)
        {
            this.textureName.Add(textureName);
            this.frameDimensions.Add(frameDimensions);
            this.framesPerRow.Add(framesPerRow);
            return this.textureName.Count - 1;
        }


        public int FramesPerRow
        {
            get { return framesPerRow[0]; }
            set { framesPerRow[0] = value; }
        }

        public Vector2 SourceOffset
        {
            get { return sourceOffset; }
            set { sourceOffset = value; }
        }


        /// <summary>
        /// The origin of the sprite, within a frame.
        /// </summary>
        protected Point frameOrigin;



        public virtual Rectangle GetBoundingRectangle(ref Rectangle rect)
        {
            rect.X = (int)Position.X + centeredReduce / 2;
            rect.Y = (int)Position.Y + centeredReduce / 2;
            rect.Width = FrameDimensions.X - centeredReduce;
            rect.Height = FrameDimensions.Y - centeredReduce;
            return rect;
        }

    }
}