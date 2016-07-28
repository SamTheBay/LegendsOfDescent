using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    class MeleeAbility : Ability
    {
        int range = 100;
        float baseDelay = 300;
        List<float> damage = new List<float>();
        List<DamageType> damageTypes = new List<DamageType>();

        public MeleeAbility(PlayerSprite player)
            : base("Melee Mastery", player, 1)
        {
            duration = 600;
            isPassive = true;
            abilityType = AbilityType.Melee;
            abilityPointCost[0] = 2;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
        }


        public override string Description
        {
            get
            {
                switch (level)
                {
                    case 0:
                        return "Become proficient with melee weapons";
                    case 1:
                        return "Become an expert with melee weapons";
                    case 2:
                        return "Become a master with melee weapons";
                    case 3:
                        return "You are a master with melee weapons";
                }
                return "";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increased damage with melee weapons";
            }
        }

        Rectangle boundingBox = new Rectangle();
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
            EnemySprite enemyHit = GetFirstCloseEnemy(direction);
            
            // if enemy is found close enough, damage him
            if (enemyHit != null)
            {
                player.Attack(ref damage, ref damageTypes);

                List<ITimedEffect> effects = player.GetAttackEffects(enemyHit);
                for (int i = 0; i < effects.Count; i++)
                {
                    enemyHit.AddTimedEffect(effects[i]);
                }

                enemyHit.Damage(damage, damageTypes, player, autoCrit: this.autoCrit);
                AudioManager.audioManager.PlaySFX("Sword" + Util.Random.Next(1,7).ToString());
            }

            base.ActivateFinish(direction);
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
