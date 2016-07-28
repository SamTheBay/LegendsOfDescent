using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{

    class BowAbility : Ability
    {
        Arrow[] arrows = new Arrow[30];
        float baseDelay = 300f;

        public BowAbility(PlayerSprite player)
            : base("Bow Mastery", player, 9)
        {
            isPassive = true;
            duration = 600;
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i] = new Arrow(player);
            }
            abilityType = AbilityType.Bow;

            abilityPointCost[0] = 2;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
        }


        public override WeaponSprite[] GetCollisionableSet()
        {
            return arrows;
        }


        public override bool Activate(Vector2 direction)
        {
            // set the delay time based on the weapon that is equipped
            activateDelayTime = (int)(baseDelay / (player.AttackSpeedAdjust + 1f));

            if (IsReady)
            {
                // reset timer
                base.Activate(direction);
                player.SetAttackAnimationInterval(player.AttackSpeedAdjust);
                activateDelayElapsed = 0;
                elapsed = 0;
                return true;
            }
            return false;
        }


        public override void ActivateFinish(Vector2 direction)
        {
            // shoot
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].IsActive)
                {
                    player.Attack(ref arrows[i].damage, ref arrows[i].damageTypes);
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    arrows[i].Fire(angle, new Vector2(player.CenteredPosition.X - arrows[i].FrameDimensions.X / 2, player.CenteredPosition.Y - arrows[i].FrameDimensions.Y / 2));
                    arrows[i].AutoCrit = autoCrit;
                    AudioManager.audioManager.PlaySFX("BowShoot");
                    break;
                }
            }

            base.ActivateFinish(direction);
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].Draw(spriteBatch, 3);
            }
        }


        public override string Description
        {
            get
            {
                switch (level)
                {
                    case 0:
                        return "Become proficient with bows";
                    case 1:
                        return "Become an expert with bows";
                    case 2:
                        return "Become a master with bows";
                    case 3:
                        return "You are a master with bows";
                }
                return "";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increased damage with bows";
            }
        }


        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);
        }

    }
}
