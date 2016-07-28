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

    public enum Direction
    {
        // regular directions
        Up,
        Down,
        Left,
        Right,

        // diagonal directions
        RightUp,
        RightDown,
        LeftUp,
        LeftDown
    }

    public class AnimatedSprite : GameSprite
    {

        // Animation description
        protected List<Animation> animations = new List<Animation>();
        protected Animation currentAnimation = null;
        protected int currentFrame;
        protected float elapsedTime;
        protected float currentRotation;
        protected Rectangle sourceRectangle;
        protected float scale = 1f;

        // extra animation pieces
        protected string baseAnimationString;
        protected Direction baseAnimationDirection;
        protected int currentFrameOffset;

        // values for effects
        protected Color permaTint = Color.White;

        // collision variables
        protected bool isFlying = false;


        public AnimatedSprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset)
            : base(nPosition, nFrameOrigin, nSourceOffset)
        {

        }


        public AnimatedSprite(string nTextureName, Point nFrameDimensions, Point nFrameOrigin, int nFramesPerRow, Vector2 nSourceOffset, Vector2 nPosition)
            : base(nPosition, nTextureName, nFrameDimensions, nFrameOrigin, nFramesPerRow, nSourceOffset)
        {

        }


       

        public List<Animation> Animations
        {
            get { return animations; }
            set { animations = value; }
        }

        public Rectangle SourceRectangle
        {
            get { return sourceRectangle; }
        }




        /// <summary>
        /// Enumerate the animations on this animated sprite.
        /// </summary>
        /// <param name="animationName">The name of the animation.</param>
        /// <returns>The animation if found; null otherwise.</returns>
        public Animation this[string animationName]
        {
            get
            {
                if (String.IsNullOrEmpty(animationName))
                {
                    return null;
                }
                foreach (Animation animation in animations)
                {
                    if (String.Compare(animation.Name, animationName) == 0)
                    {
                        return animation;
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// Add the animation to the list, checking for name collisions.
        /// </summary>
        /// <returns>True if the animation was added to the list.</returns>
        public bool AddAnimation(Animation animation)
        {
            if ((animation != null) && (this[animation.Name] == null))
            {
                animations.Add(animation);
                return true;
            }

            return false;
        }


        /// <summary>
        /// Play the given animation on the sprite.
        /// </summary>
        /// <remarks>The given animation may be null, to clear any animation.</remarks>
        public void PlayAnimation(Animation animation)
        {
            // start the new animation, ignoring redundant Plays
            if (animation != currentAnimation)
            {
                currentAnimation = animation;
                ResetAnimation();
            }
        }

        public void PlayAnimation(Animation animation, float rotation)
        {
            // start the new animation, ignoring redundant Plays
            if (animation != currentAnimation || rotation != currentRotation)
            {
                currentAnimation = animation;
                ResetAnimation();
                currentRotation = rotation;
            }
        }


        /// <summary>
        /// Play an animation given by index.
        /// </summary>
        public void PlayAnimation(int index)
        {
            // check the parameter
            if ((index < 0) || (index >= animations.Count))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            PlayAnimation(this.animations[index]);
        }


        /// <summary>
        /// Play an animation given by name.
        /// </summary>
        public virtual void PlayAnimation(string name)
        {
            // check the parameter
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            PlayAnimation(this[name]);
        }


        public void PlayAnimation(string name, float rotation)
        {
            // check the parameter
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            PlayAnimation(this[name], rotation);
        }


        /// <summary>
        /// Play a given animation name, with the given direction suffix.
        /// </summary>
        /// <example>
        /// For example, passing "Walk" and Direction.South will play the animation
        /// named "WalkSouth".
        /// </example>
        public void PlayAnimation(string name, Direction direction)
        {
            // check the parameter
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            baseAnimationString = name;
            baseAnimationDirection = direction;
            PlayAnimation(name + direction.ToString());
        }

        public void PlayAnimation(string name, Direction direction, int StartFrameOffset)
        {
            // check the parameter
            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException("name");
            }

            baseAnimationString = name;
            baseAnimationDirection = direction;
            PlayAnimation(name + direction.ToString());
            currentFrame = currentAnimation.StartingFrame + StartFrameOffset;
            currentFrameOffset = StartFrameOffset;
        }


        /// <summary>
        /// Reset the animation back to its starting position.
        /// </summary>
        public void ResetAnimation()
        {
            currentRotation = 0f;
            elapsedTime = 0f;
            if (currentAnimation != null)
            {
                currentFrame = currentAnimation.StartingFrame;
                currentFrameOffset = 0;
                // calculate the source rectangle by updating the animation
                UpdateAnimation(0f);
            }
        }


        /// <summary>
        /// Advance the current animation to the final sprite.
        /// </summary>
        public void AdvanceToEnd()
        {
            if (currentAnimation != null)
            {
                currentFrame = currentAnimation.EndingFrame;
                // calculate the source rectangle by updating the animation
                UpdateAnimation(0f);
            }
        }


        /// <summary>
        /// Stop any animation playing on the sprite.
        /// </summary>
        public void StopAnimation()
        {
            currentAnimation = null;
        }


        public void FlipAnimationDirection(Direction newDirection)
        {
            if (baseAnimationDirection != newDirection)
            {
                currentAnimation = this[baseAnimationString + newDirection.ToString()];
                baseAnimationDirection = newDirection;
                currentFrame = currentAnimation.StartingFrame + currentFrameOffset;
            }
        }


        /// <summary>
        /// Returns true if playback on the current animation is complete, or if
        /// there is no animation at all.
        /// </summary>
        public bool IsPlaybackComplete
        {
            get
            {
                return ((currentAnimation == null) ||
                    (!currentAnimation.IsLoop &&
                    (currentFrame > currentAnimation.EndingFrame)));
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            UpdateAnimation((float)gameTime.ElapsedGameTime.TotalSeconds);
        }



        /// <summary>
        /// Update the current animation.
        /// </summary>
        public void UpdateAnimation(float elapsedSeconds)
        {
            if (IsPlaybackComplete)
            {
                return;
            }

            // loop the animation if needed
            if (currentAnimation.IsLoop && (currentFrame > currentAnimation.EndingFrame))
            {
                currentFrame = currentAnimation.StartingFrame;
                currentFrameOffset = 0;
            }

            // update the source rectangle
            int column = (currentFrame - 1) / framesPerRow[currentAnimation.TextureIndex];
            sourceRectangle = new Rectangle(
                (currentFrame - 1 - (column * framesPerRow[currentAnimation.TextureIndex])) * frameDimensions[currentAnimation.TextureIndex].X,
                column * frameDimensions[currentAnimation.TextureIndex].Y,
                frameDimensions[currentAnimation.TextureIndex].X, frameDimensions[currentAnimation.TextureIndex].Y);

            // update the elapsed time
            elapsedTime += elapsedSeconds;

            currentRotation += currentAnimation.RotationSpeed;

            // advance to the next frame if ready
            float interval = Math.Max(1, currentAnimation.Interval);
            while (elapsedTime * 1000f > interval)
            {
                currentFrame++;
                currentFrameOffset++;
                elapsedTime -= interval / 1000f;
            }
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if (currentAnimation != null)
                Draw(spriteBatch, Position.Trunc() - GameplayScreen.viewportCorner.Trunc(), 0);
        }


        public override void Draw(SpriteBatch spriteBatch, int segment)
        {
            if (currentAnimation != null)
                Draw(spriteBatch, Position.Trunc() - GameplayScreen.viewportCorner.Trunc(), segment);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, int segment)
        {
            Draw(spriteBatch, position.Trunc(), segment, currentAnimation.SpriteEffect);
        }


        Vector2 textureAdjust = new Vector2();
        virtual public void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            if (isActive == false)
                return;

            // check the parameters
            if (spriteBatch == null)
            {
                throw new ArgumentNullException("spriteBatch");
            }

            if (texture != null)
            {
                textureAdjust.X = (frameDimensions[0].X - frameDimensions[currentAnimation.TextureIndex].X) / 2;
                textureAdjust.Y = (frameDimensions[0].Y - frameDimensions[currentAnimation.TextureIndex].Y) / 2;
                if (permaTint == Color.White)
                {
                    spriteBatch.Draw(texture[currentAnimation.TextureIndex], position + sourceOffset + textureAdjust, sourceRectangle, currentAnimation.Tint, currentRotation,
                        sourceOffset, scale, spriteEffect, GetLayerDepth(segment));
                }
                else
                {
                    spriteBatch.Draw(texture[currentAnimation.TextureIndex], position + sourceOffset + textureAdjust, sourceRectangle, permaTint, currentRotation,
                        sourceOffset, scale, spriteEffect, GetLayerDepth(segment));
                }
            }
        }




        public virtual bool CanMove(ref Vector2 movement, DungeonLevel dungeon, int collisionSize = 84, int rightAllowance = 35, int downAllowance = 35)
        {
            Vector2 adjustedPos = new Vector2();
            bool canWalk = true;


            // check the x direction
            adjustedPos.X = Position.X + movement.X;
            adjustedPos.Y = Position.Y;
            if (movement.X > 0)
            {
                adjustedPos.X += FrameDimensions.X - rightAllowance;
            }
            else
            {
                adjustedPos.X += FrameDimensions.X - collisionSize;
            }
            for (int i = FrameDimensions.Y - collisionSize; i < FrameDimensions.Y + TileSprite.tileDimension.Y - 1 - downAllowance; i += TileSprite.tileDimension.Y)
            {
                if (i > FrameDimensions.Y - 1 - downAllowance)
                    i = FrameDimensions.Y - 1 - downAllowance;
                TileSprite tile = dungeon.GetTileAtPosition((int)adjustedPos.X, (int)adjustedPos.Y + i);
                if (!tile.IsWalkable(null) && !isFlying || !tile.IsFlyable && isFlying)
                {
                    canWalk = false;
                    // allow player to move up to the edge of the tile
                    if (movement.X > 0)
                    {
                        movement.X = Math.Max(0, tile.Position.X - FrameDimensions.X - Position.X + rightAllowance);
                    }
                    else if (movement.X < 0)
                    {
                        movement.X = Math.Min(0, tile.Position.X + TileSprite.tileDimension.X - Position.X - FrameDimensions.X + collisionSize);
                    }
                    if (Math.Abs(movement.X) < 1)
                    {
                        movement.X = 0;
                    }
                    break;
                }

            }



            // check the y direction
            adjustedPos.X = Position.X;
            adjustedPos.Y = Position.Y + movement.Y;
            if (movement.Y > 0)
            {
                adjustedPos.Y += FrameDimensions.Y - downAllowance;
            }
            else
            {
                adjustedPos.Y += FrameDimensions.Y - collisionSize;
            }
            for (int i = FrameDimensions.X - collisionSize; i < FrameDimensions.X + TileSprite.tileDimension.X - 1 - rightAllowance; i += TileSprite.tileDimension.X)
            {
                if (i > FrameDimensions.X - 1 - rightAllowance)
                    i = FrameDimensions.X - 1 - rightAllowance;
                TileSprite tile = dungeon.GetTileAtPosition((int)adjustedPos.X + i, (int)adjustedPos.Y);
                if (tile == null || (!tile.IsWalkable(null) && !isFlying) || (!tile.IsFlyable && isFlying))
                {
                    canWalk = false;
                    // allow player to move up to the edge of the tile
                    if (movement.Y > 0)
                    {
                        movement.Y = Math.Max(0, tile.Position.Y - FrameDimensions.Y - Position.Y + downAllowance);
                    }
                    else if (movement.Y < 0)
                    {
                        movement.Y = Math.Min(0, tile.Position.Y + TileSprite.tileDimension.Y - Position.Y - FrameDimensions.Y + collisionSize);
                    }
                    if (Math.Abs(movement.Y) < 1)
                    {
                        movement.Y = 0;
                    }
                    break;
                }
            }

            return canWalk;
        }




        protected void AddAnimationSet(String animationName, int framesPerDirection, int animationIndex, int speed, bool repeat, int framesToUsePerDirection = -1, String postString = "", bool offsetToDir1 = true)
        {
            // set frames to use (note that we subtract one because the specification is inclusive)
            if (framesToUsePerDirection == -1)
            {
                framesToUsePerDirection = framesPerDirection - 1;
            }
            else
            {
                framesToUsePerDirection--;
            }

            for (int i = 0; i < 8; i++)
            {
                int startFrame = i * framesPerDirection + 1;
                if (offsetToDir1)
                    AddAnimation(new Animation(animationName + OffsetToDirection(i) + postString, startFrame, startFrame + framesToUsePerDirection, speed, repeat, SpriteEffects.None, animationIndex));
                else
                    AddAnimation(new Animation(animationName + OffsetToDirection2(i) + postString, startFrame, startFrame + framesToUsePerDirection, speed, repeat, SpriteEffects.None, animationIndex));

            }
        }

        protected String OffsetToDirection(int offset)
        {
            switch (offset)
            {
                case 0:
                    {
                        return "Right";
                    }
                case 1:
                    {
                        return "Up";
                    }
                case 2:
                    {
                        return "RightUp";
                    }
                case 3:
                    {
                        return "LeftUp";
                    }
                case 4:
                    {
                        return "Down";
                    }
                case 5:
                    {
                        return "RightDown";
                    }
                case 6:
                    {
                        return "LeftDown";
                    }
                case 7:
                    {
                        return "Left";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

        protected String OffsetToDirection2(int offset)
        {
            switch (offset)
            {
                case 0:
                    {
                        return "Down";
                    }
                case 1:
                    {
                        return "LeftDown";
                    }
                case 2:
                    {
                        return "Left";
                    }
                case 3:
                    {
                        return "LeftUp";
                    }
                case 4:
                    {
                        return "Up";
                    }
                case 5:
                    {
                        return "RightUp";
                    }
                case 6:
                    {
                        return "Right";
                    }
                case 7:
                    {
                        return "RightDown";
                    }
                default:
                    {
                        return "";
                    }
            }
        }

    }
}