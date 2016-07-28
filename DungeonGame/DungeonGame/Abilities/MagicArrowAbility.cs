using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class MagicArrowAbility : Ability
    {
        Arrow[] arrows = new Arrow[15];
        float[] damageAdjust = { .4f, .6f, .8f, 1f, 1.2f, 1.4f };
        bool manaPaid = false;
        float baseDelay = 300f;

        public MagicArrowAbility(PlayerSprite player)
            : base("Magic Arrow", player, 9)
        {
            manaCost[0] = 4;
            manaCost[1] = 7;
            manaCost[2] = 10;
            manaCost[3] = 14;
            manaCost[4] = 18;
            manaCost[5] = 22;

            duration = 500;
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i] = new Arrow(player);
            }
            abilityType = AbilityType.MagicArrow;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 90;
            abilityPointCost[4] = 300;
            abilityPointCost[5] = 900;
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

                // reset timer
                if (player.Mana >= manaCost[level - 1])
                {
                    player.UseMana(manaCost[level - 1]);
                    manaPaid = true;
                }
                else
                {
                    manaPaid = false;
                }
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
            for (int i = 0; i < arrows.Length; i++)
            {
                if (!arrows[i].IsActive)
                {
                    player.Attack(ref arrows[i].damage, ref arrows[i].damageTypes);
                    if (manaPaid)
                    {
                        for (int j = 0; j < arrows[i].damage.Count; j++)
                        {
                            arrows[i].damage[j] *= (damageAdjust[level - 1] + 1f);
                        }
                        arrows[i].SetParticle(ParticleType.ExplosionWhite, Color.Violet);
                    }
                    else
                    {
                        arrows[i].ResetParticle();
                    }
                    float angle = (float)Math.Atan2(direction.Y - player.CenteredPosition.Y, direction.X - player.CenteredPosition.X);
                    arrows[i].Fire(angle, new Vector2(player.CenteredPosition.X - arrows[i].FrameDimensions.X / 2, player.CenteredPosition.Y - arrows[i].FrameDimensions.Y / 2));
                    arrows[i].AutoCrit = autoCrit;
                    break;
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
                float damage;
                if (level == 0)
                    damage = damageAdjust[0] * 100;
                else 
                    damage = damageAdjust[level - 1] * 100;
                return "Shoot a magic arrow that deals " + damage.ToString("F0") + "% extra damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                float damage = damageAdjust[level] * 100;
                return "Increase magic damage to " + damage.ToString("F0") + "%";
            }
        }


        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);
        }
    }
}
