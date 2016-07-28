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
    class EpicStrikeAbility : Ability
    {
        float range = 100;
        float baseDelay = 300;
        List<float> damage = new List<float>();
        List<DamageType> damageTypes = new List<DamageType>();
        float[] damageAdjust = { .1f, .2f, .3f, .4f, .5f, .6f };
        bool manaPaid = false;
        int knockback = 100;

        public EpicStrikeAbility(PlayerSprite player)
            : base("Epic Strike", player, 32)
        {
            manaCost[0] = 5;
            manaCost[1] = 8;
            manaCost[2] = 12;
            manaCost[3] = 15;
            manaCost[4] = 20;
            manaCost[5] = 25;
            duration = 600;
            isPassive = false;
            abilityType = AbilityType.EpicStrike;
            abilityPointCost[0] = 3;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 90;
            abilityPointCost[4] = 230;
            abilityPointCost[5] = 700;
        }


        public override string Description
        {
            get
            {
                float damage;
                if (level > 0)
                {
                    damage = 100 + (damageAdjust[level - 1] * 100);
                }
                else
                {
                    damage = 100 + (damageAdjust[0] * 100);
                }

                return "Make a full powered attack that knocks back your enemy dealing " + damage.ToString("F0") + "% damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                float damage = 100 + (damageAdjust[level] * 100);
                return "Increase damage to " + damage.ToString("F0") + "%";
            }
        }

        Rectangle boundingBox = new Rectangle();
        public override bool Activate(Vector2 direction)
        {
            // set the delay time based on the weapon that is equipped
            activateDelayTime = (int)(baseDelay / (player.AttackSpeedAdjust + 1f));

            if (IsReady)
            {
                if (player.Mana >= manaCost[level - 1])
                {
                    player.UseMana(manaCost[level - 1]);
                    manaPaid = true;
                }
                else
                {
                    manaPaid = false;
                }

                // reset timer
                base.Activate(direction);
                player.SetAttackAnimationInterval(player.AttackSpeedAdjust);
                activateDelayElapsed = 0;
                elapsed = 0;
                AudioManager.audioManager.PlaySFX("Swing" + Util.Random.Next(1, 3).ToString());
                return true;
            }
            return false;
        }

        public EnemySprite GetFirstCloseEnemy(Vector2 direction)
        {
            // check for an enemy in the given direction
            // TODO: only consider enemies that reside on screen or in a rectangle of possible choices
            float x = player.CenteredPosition.X;
            float y = player.CenteredPosition.Y;

            float xinc = (direction.X - x) / Vector2.Distance(direction, player.CenteredPosition);
            float yinc = (direction.Y - y) / Vector2.Distance(direction, player.CenteredPosition);

            for (int i = 0; i < range; i++)
            {
                foreach (var enemy in GameplayScreen.Instance.Enemies)
                {
                    if (enemy != null && enemy.IsActive && !enemy.IsDead)
                    {
                        enemy.GetBoundingRectangle(ref boundingBox);
                        if (boundingBox.Contains((int)x, (int)y))
                        {
                            return enemy;
                        }
                    }
                }

                x += xinc;
                y += yinc;
            }
            return null;
        }

        public override void ActivateFinish(Vector2 direction)
        {
            base.ActivateFinish(direction);
            bool anyEnemyHit = false;


            EnemySprite enemyHit = GetFirstCloseEnemy(direction);

            // if enemy is found close enough, damage him
            if (enemyHit != null)
            {
                List<ITimedEffect> effects = player.GetAttackEffects(enemyHit);
                for (int i = 0; i < effects.Count; i++)
                {
                    enemyHit.AddTimedEffect(effects[i]);
                }

                player.Attack(ref damage, ref damageTypes);
                enemyHit.Damage(damage, damageTypes, player);
                anyEnemyHit = true;
            }

            if (anyEnemyHit)
            {
                if (manaPaid)
                {
                    // knockback the enemy
                    float x = player.CenteredPosition.X;
                    float y = player.CenteredPosition.Y;

                    float xinc = (direction.X - x) / Vector2.Distance(direction, player.CenteredPosition);
                    float yinc = (direction.Y - y) / Vector2.Distance(direction, player.CenteredPosition);

                    enemyHit.KnockBack(xinc, yinc, knockback);
                }

                AudioManager.audioManager.PlaySFX("Sword" + Util.Random.Next(1, 7).ToString());
            }
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }



        public override void Select()
        {
            // set the players active ability to this one
            player.SetActiveAbility(this);
        }
    }
}
