using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class MultiShotAbility : Ability
    {
        Arrow[] arrows = new Arrow[50];
        float baseDelay = 300f;
        int arrowNum = 1;

        public MultiShotAbility(PlayerSprite player)
            : base("Multi Shot", player, 15)
        {
            manaCost[0] = 6;
            manaCost[1] = 10;
            manaCost[2] = 14;
            manaCost[3] = 20;
            manaCost[4] = 25;
            manaCost[5] = 30;
            duration = 500;
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i] = new Arrow(player);
            }
            abilityType = AbilityType.MultiShot;

            abilityPointCost[0] = 6;
            abilityPointCost[1] = 20;
            abilityPointCost[2] = 45;
            abilityPointCost[3] = 110;
            abilityPointCost[4] = 350;
            abilityPointCost[5] = 1100;
        }


        public override WeaponSprite[] GetCollisionableSet()
        {
            return arrows;
        }


        public override bool Activate(Vector2 direction)
        {
            if (IsReady && player.IsRangedEquipped)
            {
                player.SetAttackAnimationInterval(player.AttackSpeedAdjust);

                if (player.Mana >= manaCost[level - 1])
                {
                    arrowNum = level + 1;
                    player.UseMana(manaCost[level - 1]);
                }
                else
                {
                    arrowNum = 1;
                }

                // reset timer
                activateDelayTime = (int)(baseDelay / (player.AttackSpeedAdjust + 1f));
                base.Activate(direction);
                activateDelayElapsed = 0;
                elapsed = 0;
                return true;
            }
            return false;
        }


       

        public override void ActivateFinish(Vector2 direction)
        {
            // shoot
            for (int j = 0; j < arrowNum; j++)
            {
                float angleAdjust = Util.GetAngleAdjust(arrowNum, j);

                for (int i = 0; i < arrows.Length; i++)
                {
                    if (!arrows[i].IsActive)
                    {
                        player.Attack(ref arrows[i].damage, ref arrows[i].damageTypes);
                        for (int x = 0; x < arrows[i].damage.Count; x++)
                        {
                            arrows[i].damage[x] *= (.7f);
                        }
                        float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                        angle += angleAdjust;
                        arrows[i].Fire(angle, new Vector2(player.CenteredPosition.X - arrows[i].FrameDimensions.X / 2, player.CenteredPosition.Y - arrows[i].FrameDimensions.Y / 2));
                        arrows[i].AutoCrit = autoCrit;
                        break;
                    }
                }
            }

            AudioManager.audioManager.PlaySFX("BowShoot");
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
                        return "Shoot 2 arrows at once that deal 70% of damage each. You must have a ranged weapon equipped to use this.";
                    case 1:
                        return "Shoot 2 arrows at once that deal 70% of damage each. You must have a ranged weapon equipped to use this.";
                    case 2:
                        return "Shoot 3 arrows at once that deal 70% of damage each. You must have a ranged weapon equipped to use this.";
                    case 3:
                        return "Shoot 4 arrows at once that deal 70% of damage each. You must have a ranged weapon equipped to use this.";
                }
                return "";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increase the number of arrows that you shoot using multi shot";
            }
        }


        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);
        }
    }
}
