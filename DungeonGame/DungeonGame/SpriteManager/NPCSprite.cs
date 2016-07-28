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
    public enum PathFindType
    {
        Melee,
        Ranged,
        RunAway
    }



    public class NPCSprite : CharacterSprite
    {
        static protected Color friendlyNPCColor = Color.LightGreen;

        // Path finding variables
        protected LinkedList<Vector2> path;
        protected GameSprite target;
        protected Point lastTileLoc = new Point();
        protected TileSprite lastTargetTile;
        protected PathFindType pathFindType = PathFindType.Melee;
        bool isTargetFollowing = false;
        protected PlayerSprite player;

        // timers
        int displayLifeTime = 1000;
        int displayLifeElapsed = 1000;
        protected int attackDelayTime = 400;
        protected int attackDelayElapsed = 400;

        // life bar
        Texture2D BlankBarHorz;
        Texture2D FullBarHorz;
        Vector2 lifeBarCorner;
        Rectangle lifeBarFill;

        // base attack and movement variables
        protected float movementRange = 300;
        protected float attackRange = 130;
        protected int attackTime = 2000;
        protected int attackElapsed = 0;
        protected List<float> attackDamage = new List<float>();
        protected List<DamageType> attackDamageTypes = new List<DamageType>();

        // debugging stuff
        Texture2D pathDrawTex;


        public NPCSprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset, PlayerSprite player)
            : base(nPosition, nFrameOrigin, nSourceOffset)
        {
            this.player = player;

            BlankBarHorz = InternalContentManager.GetTexture("EmptyBarHoriz");
            FullBarHorz = InternalContentManager.GetTexture("FullBarHoriz");
            lifeBarCorner = new Vector2(0, 0);
            lifeBarFill = new Rectangle(0, 0, 0, FullBarHorz.Height);

            for (int i = 0; i < attackDamage.Count; i++)
            {
                attackDamage[i] = 0f;
            }

            pathDrawTex = InternalContentManager.GetSolidColorTexture(Util.Random.Color());
            SetPath(null);
        }


        public void SetNPCPosition(Vector2 position, DungeonLevel dungeon = null)
        {
            SetCenteredPosition(position.X, position.Y);
            currentDestination = CenteredPosition;
            if (dungeon != null)
                lastTileLoc = dungeon.OccupyPosition(CenteredPosition.ToPoint());
            else
                lastTileLoc = GameplayScreen.Dungeon.OccupyPosition(CenteredPosition.ToPoint());
        }


        public override void FearComplete()
        {
            base.FearComplete();
            if (!isDead)
                InvalidatePath();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (isDead && !currentAnimation.Name.Contains("Die"))
            {
                PlayAnimation("Die", lastDirection);
            }
            
            if (!isActive || isDead)
                return;

            // update timers
            if (displayLifeElapsed < displayLifeTime)
            {
                displayLifeElapsed += gameTime.ElapsedGameTime.Milliseconds;
            }

            if (attackDelayElapsed < attackDelayTime)
            {
                attackDelayElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (attackDelayElapsed >= attackDelayTime)
                {
                    AttackFinish();
                }
            }

            if (IsStunned)
            {
                return;
            }
            if (InKnockback)
            {
                return;
            }


            if (!atDestination && (baseAnimationString != "Attack" || IsPlaybackComplete))
            {
                // move the character towards their current destination
                // determine how to move
                Vector2 movement = new Vector2();
                movement.X = currentDestination.X - CenteredPosition.X;
                movement.Y = currentDestination.Y - CenteredPosition.Y;

                // normalize vector
                float distance = (float)Math.Sqrt((float)MathExt.Square(movement.X) + (float)MathExt.Square(movement.Y));
                if (distance > MovementSpeed)
                {
                    movement.X /= distance;
                    movement.Y /= distance;

                    movement.X *= MovementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;
                    movement.Y *= MovementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;

                    // update position
                    MovePosition(movement.X, movement.Y);

                    Direction direction = GetDirectionFromVector(movement);
                    PlayAnimation("Walk", direction);
                    lastDirection = direction;

                }
                else
                {
                    // we have arrived at the current destination
                    SetCenteredPosition(currentDestination.X, currentDestination.Y);

                    if (path == null || path.Count == 0)
                    {
                        atDestination = true;
                        PlayAnimation("Idle", lastDirection);
                    }
                    else
                    {
                        currentDestination = path.First();
                        path.RemoveFirst();

                        // check if this is a valid location
                        if (GameplayScreen.Dungeon.IsPositionOccupied(currentDestination.ToPoint()))
                        {
                            InvalidatePath();
                        }
                        else
                        {
                            GameplayScreen.Dungeon.AttemptToOccupyTile(ref lastTileLoc, currentDestination.ToPoint());
                        }
                    }
                }
            }

            // attempt to attack
            if (this is EnemySprite && target is PlayerSprite)
            {
                CharacterSprite sprite = (CharacterSprite)target;

                if (!sprite.IsInvisible && !IsFeared)
                {
                    if (attackElapsed >= attackTime)
                    {
                        if (Vector2.Distance(CenteredPosition, target.CenteredPosition) < attackRange)
                        {
                            Attack();
                            attackElapsed = 0;
                        }
                    }
                    else
                    {
                        attackElapsed += gameTime.ElapsedGameTime.Milliseconds;
                    }
                }
            }

        }



        public void InvalidatePath()
        {
            SetPath(null);
            atDestination = true;
            lastTargetTile = null;
        }


        public override void KnockBackComplete()
        {
            if (!isDead)
                InvalidatePath();

            base.KnockBackComplete();
        }


        SearchData searchData = new SearchData();
        LinkedList<Vector2> tempPath;
        public void UpdatePathfind(List<NPCSprite> enemies, PlayerSprite player)
        {
            if (isDead == true || isActive == false)
                return;

            // check if we are currently in range
            if (Vector2.Distance(CenteredPosition, target.CenteredPosition) < movementRange)
            {
                SetPath(null);
                return;
            }
            else if (atDestination)
            {
                // we think we are at our destination but we aren't close enough
                lastTargetTile = null;
            }

            if (GameplayScreen.Dungeon.GetTileAtPosition((int)target.CenteredPosition.X, (int)target.CenteredPosition.Y).Equals(lastTargetTile))
            {
                return;
            }
            else
            {
                lastTargetTile = GameplayScreen.Dungeon.GetTileAtPosition((int)target.CenteredPosition.X, (int)target.CenteredPosition.Y);
            }

            // update our path to the target
            if (isTargetFollowing && isActive)
            {
                searchData.occupiedMap = GameplayScreen.Dungeon.OccupiedMap;
                Vector2 destination = target.CenteredPosition;
                if (IsFeared)
                {
                    // we need to run in the opposite direction of the target
                    destination = new Vector2(destination.X - CenteredPosition.X, destination.Y - CenteredPosition.Y);
                    int dungeonWidth = GameplayScreen.Dungeon.TileDimension * GameplayScreen.Dungeon.Dimension.X - 1;
                    if (destination.X < 0)
                        destination.X = dungeonWidth;
                    else
                        destination.X = 0;
                    if (destination.Y < 0)
                        destination.Y = dungeonWidth;
                    else
                        destination.Y = 0;

                }
                tempPath = GameplayScreen.Dungeon.GetPath(CenteredPosition, destination, searchData);
            }

            CharacterSprite sprite = target as CharacterSprite;
            if (!(sprite != null && sprite.IsInvisible))
            {
                SetPath(tempPath);
            }
            else if (path == null)
            {
                SetPath(null);
            }
        }


        public virtual void Attack()
        {
            if (baseAnimationString != "Attack" || IsPlaybackComplete)
            {
                lastDirection = GetDirectionFromVector(target.CenteredPosition - CenteredPosition);
                PlayAnimation("Attack", lastDirection);
                ResetAnimation();
                attackDelayElapsed = 0;
            }
        }


        public virtual void AttackFinish()
        {
            // implemented by inherited types for them to do whatever action should occur at the middle of the
            // attack animation (when it looks correct to shoot something)

            if (pathFindType != PathFindType.Ranged)
            {
                // check for kickback damage from the player
                List<float> damage = new List<float>();
                List<DamageType> damageTypes = new List<DamageType>();
                bool anyDamage = player.GetKickbackDamage(damage, damageTypes, this);

                if (anyDamage)
                {
                    Damage(damage, damageTypes, player);
                }
            }
        }


        public void SetPath(LinkedList<Vector2> path)
        {
            if (path != null && path.Count > 0)
            {
                // check if this is a valid location
                if (GameplayScreen.Dungeon.IsPositionOccupied(path.First().ToPoint(), currentDestination.ToPoint()))
                {
                    InvalidatePath();
                }
                else
                {
                    this.path = path;
                    atDestination = false;
                    currentDestination = path.First();
                    path.RemoveFirst();
                    GameplayScreen.Dungeon.AttemptToOccupyTile(ref lastTileLoc, currentDestination.ToPoint());
                }
            }
            else
            {
                atDestination = true;
                if (baseAnimationString != "Attack" || IsPlaybackComplete)
                {
                    PlayAnimation("Idle", lastDirection);
                }
            }
            
        }


        public void SetTarget(GameSprite target)
        {
            this.target = target;
            isTargetFollowing = true;
            atDestination = false;
        }


        public override void Die(CharacterSprite killer)
        {
            base.Die(killer);
            GameplayScreen.Dungeon.ReleaseOccupiedPosition(lastTileLoc);
            isCollisionable = false;
            PlayAnimation("Die", lastDirection);
        }


        public virtual void Refresh()
        {
            // implemented by inherited classes if needed.
            // Is called every time that the npc's are added into a game screen.
        }




        public void Resurect()
        {
            health = MaxHealth;
            isDead = false;
            isCollisionable = true;
            Activate();
        }


        public virtual WeaponSprite[] GetCollisionableWeaponSet()
        {
            // If characters have projectiles that need to be added into the gameplay they
            // should return them form this function
            return null;
        }

        public override void Damage(List<float> damage, List<DamageType> damageType, CharacterSprite attacker, float extraAdjust = 1f, bool autoCrit = false, bool allowReflect = true)
        {
            base.Damage(damage, damageType, attacker, extraAdjust, autoCrit, allowReflect);

            displayLifeElapsed = 0;
        }

        public override void Fear(int duration, CharacterSprite owner)
        {
            base.Fear(duration, owner);
            if (!isDead)
                InvalidatePath();
        }


        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            if (!isDead && !IsInvisible)
            {
                base.Draw(spriteBatch, position, 3, spriteEffect);
            }
            else if (isDead)
            {
                base.Draw(spriteBatch, position, 1, spriteEffect);
            }

            if (!isActive)
                return;

            if (displayLifeElapsed < displayLifeTime && !isDead && !IsInvisible)
            {
                // draw life bar
                Color lifeColor = Color.LightGreen;
                if ((float)Health / (float)MaxHealth < .3f)
                {
                    lifeColor = Color.OrangeRed;
                }
                else if ((float)Health / (float)MaxHealth < .6f)
                {
                    lifeColor = Color.Yellow;
                }

                lifeBarCorner.X = Position.X - GameplayScreen.viewportCorner.X + FrameDimensions.X / 2 - FullBarHorz.Width / 2;
                lifeBarCorner.Y = Position.Y - BlankBarHorz.Height - GameplayScreen.viewportCorner.Y + 6;

                spriteBatch.Draw(BlankBarHorz, lifeBarCorner, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .002f);
                lifeBarFill.Width = (int)((float)FullBarHorz.Width * (float)Health / (float)MaxHealth);
                spriteBatch.Draw(FullBarHorz,
                    lifeBarCorner,
                    lifeBarFill,
                    lifeColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, .001f);
            }

            if (DungeonGame.TestMode)
            {
                // Uncomment to debug path finding code
                //DrawCurrentPath(spriteBatch);

                // Uncomment to debug collision detection
                DrawBoundingBox(spriteBatch);
            }
        }


        public void DrawCurrentPath(SpriteBatch spriteBatch)
        {
            if (path != null) 
            {
                foreach (Vector2 i in path)
                {
                    spriteBatch.Draw(pathDrawTex, 
                                     new Rectangle((int)i.X - 5 - (int)GameplayScreen.viewportCorner.X, 
                                                   (int)i.Y - 5 - (int)GameplayScreen.viewportCorner.Y, 
                                                   10, 
                                                   10), 
                                     Color.White);
                }
            }
        }


    }
}
