using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using System.Collections.Generic;

namespace LegendsOfDescent
{


    public class CharacterSprite : AnimatedSprite
    {
        const float baseDamageVariability = .25f;

        protected Direction lastDirection = Direction.Right;
        private string name;
        protected TextSprite nameSprite;

        // movement variables
        protected Vector2 currentDestination;
        protected bool atDestination = true;

        // character stats
        protected float baseMovementSpeed = 5f;
        protected float health = 100;
        protected float maxHealthBase = 100;
        protected float mana = 50;
        protected float maxManaBase = 50;
        protected int level = 1;
        protected float criticalHitChance = .1f;

        // state
        protected bool isDead = false;
        protected int deadDuration = 1000;
        protected int deadElapsed = 0;
        protected bool canMove = true;
        protected bool canShoot = true;

        // data declarations
        protected Rectangle characterFrame = new Rectangle(0, 0, 60, 60);

        // resistance
        protected float[] resistances = {0f, 0f, 0f, 0f, 0f};

        // Timed effects
        protected List<ITimedEffect> activeTimedEffects = new List<ITimedEffect>();

        // knockback
        int knockbackLeft = 0;
        int knockbackSpeed = 20;
        float knockbackxinc = 0f;
        float knockbackyinc = 0f;

        // timers
        Timer invisibleDuration = new Timer(20000, TimerState.Stopped, TimerType.Manual);

        public CharacterSprite(Vector2 nPosition, Point nFrameOrigin, Vector2 nSourceOffset)
            : base(nPosition, nFrameOrigin, nSourceOffset)
        {
            isCollisionable = true;
        }


        public CharacterSprite(string nTextureName, Point nFrameDimensions, Point nFrameOrigin, int nFramesPerRow, Vector2 nSourceOffset, Vector2 nPosition)
            : base(nTextureName, nFrameDimensions, nFrameOrigin, nFramesPerRow, nSourceOffset, nPosition)
        {
            isCollisionable = true;
        }


        
        protected Direction GetDirectionFromVector(Vector2 movement)
        {
            float angle = (float)Math.Atan2(movement.X, -movement.Y);

            if (angle > 1 * Math.PI / 8 && angle < 3 * Math.PI / 8)
            {
                return Direction.RightUp;
            }
            if (angle > 3 * Math.PI / 8 && angle < 5 * Math.PI / 8)
            {
                return Direction.Right;
            }
            if (angle > 5 * Math.PI / 8 && angle < 7 * Math.PI / 8)
            {
                return Direction.RightDown;
            }

            else if (angle < -1 * Math.PI / 8 && angle > -3 * Math.PI / 8)
            {
                return Direction.LeftUp;
            }
            else if (angle < -3 * Math.PI / 8 && angle > -5 * Math.PI / 8)
            {
                return Direction.Left;
            }
            else if (angle < -5 * Math.PI / 8 && angle > -7 * Math.PI / 8)
            {
                return Direction.LeftDown;
            }

            else if (angle > -Math.PI / 8 && angle < Math.PI / 8)
            {
                return Direction.Up;
            }
            else
            {
                return Direction.Down;
            }
        }


        public override void Update(GameTime gameTime)
        {
            // update timers
            invisibleDuration.Update(gameTime);


            // update usable items
            lock (activeTimedEffects)
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] != null)
                    {
                        bool isExpired = activeTimedEffects[i].UpdateDuration(gameTime);
                        if (isExpired && activeTimedEffects.Count > i)  // if the effect kills the player then all effects will be removed
                        {
                            try
                            {
                                activeTimedEffects.RemoveAt(i);
                            }
                            catch
                            { }
                        }
                    }
                }
            }

            // handle knockback
            if (knockbackLeft > 0)
            {
                Vector2 newPosition = Position + new Vector2(knockbackxinc * Math.Min(knockbackSpeed, knockbackLeft), knockbackyinc * Math.Min(knockbackSpeed, knockbackLeft));
                knockbackLeft -= knockbackSpeed;

                if (GameplayScreen.Dungeon.GetTileAtPosition(newPosition).IsWalkable())
                {
                    Position = newPosition;
                }

                if (knockbackLeft <= 0)
                {
                    KnockBackComplete();
                }
            }


            // handle tinting
            if (isDead)
            {
                permaTint = Color.White;
            }
            else if (IsFeared)
            {
                permaTint = Color.Red;
            }
            else if (IsSlowed)
            {
                permaTint = Color.LightBlue;
            }
            else if (IsPoisoned)
            {
                permaTint = Color.Green;
            }
            else if (IsStunned)
            {
                permaTint = Color.LightGray;
            }
            else
            {
                permaTint = Color.White;
            }


            // trigger any action for the tile that we are walking on
            TileSprite tile = GameplayScreen.Dungeon.GetTileAtPosition(CenteredPosition);
            tile.WakingOnTile(this);


            base.Update(gameTime);
        }


        public virtual void FearComplete()
        {
            // implemented by inherited classes
        }


        private List<float> damageArray = new List<float>();
        private List<DamageType> typeArray = new List<DamageType>();
        public virtual void Damage(int damage, DamageType damageType, CharacterSprite attacker, float extraAdjust = 1f, bool autoCrit = false, bool allowReflect = true)
        {
            damageArray.Clear();
            damageArray.Add(damage);
            typeArray.Clear();
            typeArray.Add(damageType);
            Damage(damageArray, typeArray, attacker, extraAdjust, autoCrit, allowReflect);
        }



        public virtual void Damage(List<float> damage, List<DamageType> damageType, CharacterSprite attacker, float extraAdjust = 1f, bool autoCrit = false, bool allowReflect = true)
        {
            SpriteFont damageFont = Fonts.DescriptionFont;
            Color damageColor = Color.Red;

            DamageType maxDamageType = DamageType.Physical;
            float maxDamageTypeAmount = float.MaxValue * -1;
            float totalDamage = 0;
            for (int i = 0; i < damage.Count; i++)
            {
                if (isActive && !isDead)
                {
                    // apply variability
                    float adjustedDamage = damage[i] * Util.Random.Between(1f - baseDamageVariability, 1f + baseDamageVariability);

                    // apply resistance and armor effects
                    adjustedDamage = (adjustedDamage * (1f - resistances[(int)damageType[i]]));
                    if (damageType[i] != DamageType.Physical)
                    {
                        adjustedDamage -= adjustedDamage * MagicResistance;
                        adjustedDamage -= (BalanceManager.GetArmorEffect(Armor) *.4f);  // armor is 40% effective against elemental damage
                    }
                    else
                    {
                        adjustedDamage -= BalanceManager.GetArmorEffect(Armor);
                    }


                    // debug output
                    if (damage[i] > 0)
                    {
                        Debug.WriteLine(Name + " hit for " + adjustedDamage.ToString() + " with armor effect of " + BalanceManager.GetArmorEffect(Armor) + " for total damage of " + adjustedDamage.ToString() + " from level " + attacker.Level.ToString());
                    }

                    // total it up
                    if (adjustedDamage > 0)
                    {
                        totalDamage += adjustedDamage;
                    }

                    if (adjustedDamage > maxDamageTypeAmount)
                    {
                        maxDamageTypeAmount = adjustedDamage;
                        damageColor = GetColorFromDamageType(damageType[i]);
                        maxDamageType = damageType[i];
                    }
                }
            }

            // check for critical hit
            float roll = Util.Random.Between(0f, 1f);
            if (roll < attacker.CriticalHitChance || autoCrit)
            {
                if (attacker is PlayerSprite)
                {
                    totalDamage *= 2f;
                }
                else
                {
                    totalDamage *= 2f;
                }
                damageFont = Fonts.CritFont;
            }

            totalDamage *= extraAdjust;

            if (this is PlayerSprite)
            {
                PlayerSprite player = (PlayerSprite)this;
                FrenzyAbility frenzy = (FrenzyAbility)player.Abilities[(int)AbilityType.Frenzy];
                totalDamage *= frenzy.GetDamageTakenAdjust();
            }

            if (totalDamage < 1f && !isDead)
                totalDamage = 1f;

            health -= totalDamage;
            if (health <= 0)
            {
                health = 0;
                if (!isDead)
                {
                    Die(attacker);
                }
            }

            float lifeSteal = totalDamage * attacker.LifeStealPercent;
            attacker.Heal(lifeSteal, true);
            float manaSteal = totalDamage * attacker.ManaStealPercent;
            attacker.AddMana(manaSteal);

            if (allowReflect)
            {
                float reflectDamage = totalDamage * ReflectDamagePercent;
                if (reflectDamage > 0)
                {
                    attacker.Damage((int)reflectDamage, DamageType.Physical, this, allowReflect: false);
                }
            }

            // count this damage
            if (this is PlayerSprite)
            {
                SaveGameManager.CurrentPlayer.StatsManager.AddDamage(maxDamageType, (ulong)totalDamage, false);
            }
            else if (this is EnemySprite)
            {
                SaveGameManager.CurrentPlayer.StatsManager.AddDamage(maxDamageType, (ulong)totalDamage, true);
            }

            if (totalDamage >= 1f)
                GameplayScreen.Instance.AddTextSpriteFromPool(totalDamage.ToString("F0"), CenteredPosition, damageColor, damageFont, new Vector2(0, -4), 1000);

        }


        public void AddTimedEffect(ITimedEffect effect)
        {
            if (!isDead)
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i].IsEqualActiveIcon(effect))
                    {
                        ITimedEffect other = activeTimedEffects[i];
                        effect = (ITimedEffect)(effect.CombineActiveIcons((IActiveIcon)other));
                        activeTimedEffects.RemoveAt(i);
                    }
                }

                activeTimedEffects.Add(effect);
            }
        }

        public virtual void DropItem(ItemSprite item)
        {
            item.Position = new Vector2(CenteredPosition.X - item.FrameDimensions.X, CenteredPosition.Y - item.FrameDimensions.Y);
            GameplayScreen.Instance.AddEnvItem(item);
            item.Toss();
            item.Owner = null;
        }


        public Color GetColorFromDamageType(DamageType type)
        {
            if (type == DamageType.Poison)
            {
                return Color.Green;
            }
            else if (type == DamageType.Ice)
            {
                return Color.DeepSkyBlue;
            }
            else if (type == DamageType.Fire)
            {
                return Color.OrangeRed;
            }
            else if (type == DamageType.Lightning)
            {
                return Color.Yellow;
            }
            return Color.Red;
        }



        public virtual void Fear(int duration, CharacterSprite owner)
        {
            AddTimedEffect(new FearEffect(owner, this, duration));
        }


        public bool IsFeared
        {
            get
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is FearEffect)
                        return true;
                }
                return false;
            }
        }


        public virtual void Die(CharacterSprite killer)
        {
            lock (activeTimedEffects)
            {
                activeTimedEffects.Clear();
            }
            isDead = true;
            health = 0;
        }


        public virtual void SlowCharacter(int duration, float amount, CharacterSprite owner)
        {
            AddTimedEffect(new SlowEffect(owner, this, duration, amount));
        }


        public virtual void BecomeInvisible(int duration)
        {
            invisibleDuration.ResetTimerAndRun(duration);
        }


        public virtual void EndInvisible()
        {
            invisibleDuration.State = TimerState.Stopped;
        }



        public virtual void Heal(float amount, bool instant = false)
        {
            health += amount;
            if (health > MaxHealth)
                health = MaxHealth;
        }


        public virtual void AddMana(float amount)
        {
            mana += amount;
            if (mana > MaxMana)
                mana = MaxMana;
        }


        public virtual void UseMana(float amount)
        {
            mana -= amount;
            if (mana < 0)
                mana = 0;
        }

        public virtual float MovementSpeed
        {
            get
            {
                if (IsSlowed)
                    return baseMovementSpeed * SlowAmount;
                else
                    return baseMovementSpeed;
            }
        }

        public bool IsSlowed
        {
            get 
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is SlowEffect)
                        return true;
                }
                return false;
            }
        }


        public float SlowAmount
        {
            get
            {
                float slowAmount = 1f;
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is SlowEffect)
                    {
                        SlowEffect slowEffect = (SlowEffect)activeTimedEffects[i];
                        slowAmount = Math.Min(slowAmount, slowEffect.SlowAmount);
                    }
                }
                return slowAmount;
            }
        }


        // TODO: add generic function to extract out timed effects of a particular type


        public List<ITimedEffect> ActiveTimedEffects
        {
            get { return activeTimedEffects; }
        }



        public virtual float Health
        {
            get { return health; }
        }

        public virtual float MaxHealth
        {
            get { return maxHealthBase; }
        }


        public virtual float Mana
        {
            get { return mana; }
        }

        public virtual float MaxMana
        {
            get { return maxManaBase; }
        }

        public virtual int Armor
        {
            get { return 0; }
        }

        public virtual float MagicResistance
        {
            get { return 0; }
        }


        public int Level
        {
            get { return level; }
        }

        public string Name
        {
            get { return name; }
            set 
            { 
                name = value;
                nameSprite = new TextSprite(name, Vector2.Zero, Color.White, Fonts.NameFont, Vector2.Zero, 0);
                nameSprite.AlwaysOn = true;
                nameSprite.Activate();
                nameSprite.BorderColor = Color.Black;
            }
        }


        public virtual void Poison(float damage, int time, CharacterSprite poisoner)
        {
            AddTimedEffect(new PoisonEffect(poisoner, this, time, (int)damage));
        }

        public void Bleed(float damage, int time, CharacterSprite attacker)
        {
            AddTimedEffect(new BleedEffect(attacker, this, time, (int)damage));
        }

        public void KnockBack(float xinc, float yinc, int amount)
        {
            knockbackLeft = amount;
            knockbackxinc = xinc;
            knockbackyinc = yinc;
        }

        public virtual void KnockBackComplete()
        {

        }

        public bool InKnockback
        {
            get { return knockbackLeft > 0; }
        }


        public bool IsBleeding
        {
            get
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is BleedEffect)
                        return true;
                }
                return false;
            }
        }

        public bool IsPoisoned
        {
            get 
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is PoisonEffect)
                        return true;
                }
                return false;
            }
        }

        public void Stun(int duration, CharacterSprite owner)
        {
            AddTimedEffect(new StunEffect(owner, this, duration));
        }


        public bool IsDead
        {
            get { return isDead; }
        }


        public virtual float CriticalHitChance
        {
            get { return criticalHitChance; }
        }


        public bool IsInvisible
        {
            get { return invisibleDuration.State == TimerState.Running; }
        }

        public int InvisibleRemaining
        {
            get { return invisibleDuration.RemainingDuration; }
        }

        public bool IsStunned
        {
            get
            {
                for (int i = 0; i < activeTimedEffects.Count; i++)
                {
                    if (activeTimedEffects[i] is StunEffect)
                        return true;
                }
                return false;
            }
        }


        public virtual float LifeStealPercent
        {
            get { return 0; }
        }

        public virtual float ManaStealPercent
        {
            get { return 0; }
        }


        public virtual float ReflectDamagePercent
        {
            get { return 0; }
        }

    }
}
