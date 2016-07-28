using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    class CleaveAbility : Ability
    {
        float range = 100;
        float rangeWidth = 100;
        float baseDelay = 300;
        List<float> damage = new List<float>();
        List<DamageType> damageTypes = new List<DamageType>();
        float[] damageAdjust = { -.3f, -.2f, -.1f, 0f, .1f, .2f };
        bool manaPaid = false;
        int[] maxEnemyHits = { 2, 3, 4, 4, 4, 4 };

        public CleaveAbility(PlayerSprite player)
            : base("Cleave", player, 25)
        {
            manaCost[0] = 5;
            manaCost[1] = 8;
            manaCost[2] = 12;
            manaCost[3] = 15;
            manaCost[4] = 20;
            manaCost[5] = 25;
            duration = 600;
            isPassive = false;
            abilityType = AbilityType.Cleave;
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
                int offset = 0;
                if (level > 0)
                    offset = level - 1;

                float damage = 100 + (damageAdjust[offset] * 100);

                return "Make a powerful wide swing that hits up to " + maxEnemyHits[offset].ToString() + " enemies in front of you at once dealing " + damage.ToString("F0") + "% damage";
            }
        }

        public override string NextDescription
        {
            get
            {
                float damage = 100 + (damageAdjust[level] * 100);
                return "Damage up to " + maxEnemyHits[level].ToString() + " enemies for " + damage.ToString("F0") + "%";
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

            if (manaPaid)
            {
                // determine the bounding box for enemies to hit
                Vector2 v1 = player.CenteredPosition, v2, v3, v4 = player.CenteredPosition;

                float xinc = (direction.X - player.CenteredPosition.X) / Vector2.Distance(direction, player.CenteredPosition);
                float yinc = (direction.Y - player.CenteredPosition.Y) / Vector2.Distance(direction, player.CenteredPosition);

                v1 += (rangeWidth) * new Vector2(yinc * -1, xinc);
                v2 = v1 + (range) * new Vector2(xinc, yinc);
                v4 += (rangeWidth) * new Vector2(yinc, xinc * -1);
                v3 = v4 + (range) * new Vector2(xinc, yinc);

                // damage enemies in the area
                int enemiesHit = 0;
                foreach (var enemy in GameplayScreen.Instance.Enemies)
                {
                    if (enemy != null && enemy.IsActive && !enemy.IsDead)
                    {
                        if (HitDetect.HitDetectConvexQuadrialteral(v1, v2, v3, v4, enemy.CenteredPosition))
                        {
                            player.Attack(ref damage, ref damageTypes);

                            List<ITimedEffect> effects = player.GetAttackEffects(enemy);
                            for (int i = 0; i < effects.Count; i++)
                            {
                                enemy.AddTimedEffect(effects[i]);
                            }

                            enemy.Damage(damage, damageTypes, player);
                            anyEnemyHit = true;
                            enemiesHit++;

                            if (enemiesHit == maxEnemyHits[level-1])
                                break;
                        }
                    }
                }

             }
            else
            {
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
            }

            if (anyEnemyHit)
            {
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
