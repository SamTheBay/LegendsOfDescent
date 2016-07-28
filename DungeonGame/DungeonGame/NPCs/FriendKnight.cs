using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class FriendKnight : NPCSprite
    {
        public FriendKnight(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            int walkFrameSpeed = 70;

            // add in other textures
            AddTexture("KnightWalking", new Point(128, 128), 12);
            AddTexture("KnightAttack", new Point(128, 128), 12);

            // TODO: placeholder animation
            AddAnimation(new Animation("IdleRight", 1, 1, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleUp", 13, 13, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleRightUp", 25, 25, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleLeftUp", 37, 37, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleDown", 49, 49, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleRightDown", 61, 61, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleLeftDown", 73, 73, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("IdleLeft", 85, 85, 100, true, SpriteEffects.None, 1));
            AddAnimation(new Animation("WalkRight", 1, 12, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkUp", 13, 24, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkRightUp", 25, 36, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkLeftUp", 37, 48, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkDown", 49, 60, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkRightDown", 61, 72, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkLeftDown", 73, 84, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            AddAnimation(new Animation("WalkLeft", 85, 96, walkFrameSpeed, true, SpriteEffects.None, Color.White));
            PlayAnimation("IdleRight");
            lastDirection = Direction.Right;
            Activate();
            baseMovementSpeed = 9f;
            movementRange = 90;
            maxHealthBase = BalanceManager.GetBaseEnemyLife(level);
            health = BalanceManager.GetBaseEnemyLife(level);
            this.level = level;
            isCollisionable = false;
            SetTarget(player);
            InitializeTextures();
        }
    }
}
