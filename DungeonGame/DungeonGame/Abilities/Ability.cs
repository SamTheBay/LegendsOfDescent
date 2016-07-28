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
    public enum AbilityType
    {
        Melee,
        Bow,
        Defence,
        FireBolt,
        IceBolt,
        Lightning,
        Heal,
        FireBall,
        ChainLightning,
        Nova,
        Invisibility,
        SlowTime,
        Fear,
        MultiShot,
        PoisonCloud,
        FrostArmor,
        Sprint,
        Frenzy,
        StoneSkin,
        MagicArrow,
        Cleave,
        FrozenGround,
        MoltenEarth,
        ExplosionTrap,
        PoisonTrap,
        Earthquake,
        Blizzard,
        PoisonMastery,
        ShadowMastery,
        Precision,
        EpicStrike
    }


    public class Ability : ISelectable, IActiveIcon
    {
        public const int MaxLevel = 6;

        protected String name;
        protected PlayerSprite player;
        protected int duration = 0;
        protected int elapsed = 0;
        protected int level = 0; // up to MaxLevel
        protected bool isPassive = false;
        protected int activateDelayTime = 250;
        protected int activateDelayElapsed = 250;
        protected Vector2 activateDirection;
        protected bool isMagic = false;
        protected AbilityType abilityType;
        protected int requiredLevel = 0;
        protected int[] abilityPointCost = new int[MaxLevel];
        protected AbilityType requiredAbilityType;
        protected int[] manaCost = new int[MaxLevel];
        protected bool autoCrit = false;
        protected bool speedAdjustable = true;

        // icon info for creating a macro button
        protected Texture2D iconTexture;
        protected Rectangle iconSource;
        protected String iconTextureName;

        public Ability(String name, PlayerSprite player, int iconOffset)
        {
            this.name = name;
            this.player = player;
            this.iconSource = new Rectangle((iconOffset % 10) * 48, (iconOffset / 10) * 48, 48, 48);
            this.iconTextureName = "AbilityIcons";
            this.iconTexture = InternalContentManager.GetTexture(iconTextureName);

            abilityPointCost[0] = 1;
            abilityPointCost[1] = 4;
            abilityPointCost[2] = 8;
            abilityPointCost[3] = 50;
            abilityPointCost[4] = 150;
            abilityPointCost[5] = 600;
        }


        public virtual bool Activate(Vector2 direction)
        {
            // Implemented by inherited classes
            activateDirection = direction;
            if (player.IsInvisible)
            {
                autoCrit = true;
            }
            player.EndInvisible();
            return false;
        }

        public virtual void ActivateFinish(Vector2 direction)
        {
            // Implemented by inherited classes
            autoCrit = false;
        }


        public virtual void Update(GameTime gameTime)
        {
            if (Duration > elapsed)
            {
                elapsed += gameTime.ElapsedGameTime.Milliseconds;
            }
            int delay = Math.Max(1, activateDelayTime);
            if (delay > activateDelayElapsed)
            {
                activateDelayElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (activateDelayElapsed >= delay)
                {
                    ActivateFinish(activateDirection);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            // implemented by inherited classes
        }


        public virtual bool CanLevelUp()
        {
            if (level < MaxLevel && (requiredLevel == 0 || player.Abilities[(int)requiredAbilityType].Level >= requiredLevel) && player.AbilityPoints >= abilityPointCost[level])
            {
                return true;
            }
            return false;
        }

        public virtual void LevelUp()
        {
            if (CanLevelUp())
            {
                if (player.TryUseAbilityPoints(abilityPointCost[level]))
                {
                    level++;
                }
            }
        }


        public bool IsReady
        {
            get { return elapsed >= Duration; }
        }

        public String Name
        {
            get { return name; }
        }

        public int Duration
        {
            get
            {
                if (isMagic && speedAdjustable)
                {
                    return Math.Max(1, (int)((float)duration / (player.CastSpeedAdjust + 1f)));
                }
                else if (speedAdjustable)
                {
                    return Math.Max(1, (int)((float)duration / (player.AttackSpeedAdjust + 1f)));
                }
                else
                {
                    return duration;
                }
            }
        }


        public int Elapsed
        {
            get { return elapsed; }
            set { elapsed = value; }
        }


        public int DurationRemaining
        {
            get { return (Duration - elapsed) / 1000; }
        }


        public Texture2D GetActiveIconTexture()
        {
            return iconTexture;
        }


        public Rectangle GetActiveIconSource()
        {
            return iconSource;
        }

        public bool IsDebuff()
        {
            return false;
        }


        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            if (other.GetActiveIconTime() > GetActiveIconTime())
            {
                return other;
            }

            return this;
        }

        public virtual int GetActiveIconTime()
        {
            return 0;
        }

        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            if (other is Ability)
            {
                Ability ability = (Ability)other;
                if (ability.abilityType == this.abilityType)
                {
                    return true;
                }
            }
            return false;
        }



        public static Ability[] GetAllAbilities(PlayerSprite player)
        {
            Ability[] abilities = new Ability[31];
            abilities[(int)AbilityType.Melee] = new MeleeAbility(player);
            abilities[(int)AbilityType.Bow] = new BowAbility(player);
            abilities[(int)AbilityType.Defence] = new DefenceAbility(player);
            abilities[(int)AbilityType.FireBolt] = new FireBoltAbility(player);
            abilities[(int)AbilityType.Lightning] = new LightningAbility(player);
            abilities[(int)AbilityType.Heal] = new HealAbility(player);
            abilities[(int)AbilityType.FireBall] = new FireBallAbility(player);
            abilities[(int)AbilityType.IceBolt] = new IceBoltAbility(player);
            abilities[(int)AbilityType.ChainLightning] = new ChainLightningAbility(player);
            abilities[(int)AbilityType.Nova] = new NovaAbility(player);
            abilities[(int)AbilityType.Invisibility] = new InvisibilityAbility(player);
            abilities[(int)AbilityType.SlowTime] = new SlowTimeAbility(player);
            abilities[(int)AbilityType.Fear] = new FearAbility(player);
            abilities[(int)AbilityType.MultiShot] = new MultiShotAbility(player);
            abilities[(int)AbilityType.PoisonCloud] = new PoisonCloudAbility(player);
            abilities[(int)AbilityType.FrostArmor] = new FrostArmorAbility(player);
            abilities[(int)AbilityType.Sprint] = new SprintAbility(player);
            abilities[(int)AbilityType.Frenzy] = new FrenzyAbility(player);
            abilities[(int)AbilityType.StoneSkin] = new StoneSkinAbility(player);
            abilities[(int)AbilityType.MagicArrow] = new MagicArrowAbility(player);
            abilities[(int)AbilityType.Cleave] = new CleaveAbility(player);
            abilities[(int)AbilityType.FrozenGround] = new FrozenGroundAbility(player);
            abilities[(int)AbilityType.MoltenEarth] = new MoltenEarthAbility(player);
            abilities[(int)AbilityType.ExplosionTrap] = new ExplosionTrapAbility(player);
            abilities[(int)AbilityType.PoisonTrap] = new PoisonTrapAbility(player);
            abilities[(int)AbilityType.Earthquake] = new EarthquakeAbility(player);
            abilities[(int)AbilityType.Blizzard] = new BlizzardAbility(player);
            abilities[(int)AbilityType.PoisonMastery] = new PoisonMasteryAbility(player);
            abilities[(int)AbilityType.ShadowMastery] = new ShadowMasteryAbility(player);
            abilities[(int)AbilityType.Precision] = new PrecisionAbility(player);
            abilities[(int)AbilityType.EpicStrike] = new EpicStrikeAbility(player);
            return abilities;
        }


        public virtual WeaponSprite[] GetCollisionableSet()
        {
            return null;
        }


        public virtual bool IsSelectable()
        {
            return !isPassive && level != 0;
        }

        public virtual void Select()
        {
            if (GetActiveIconTime() > 0)
            {
                GameplayScreen.ui.AddActiveIcon(this);
            }
        }

        public MacroButton GetMacroButton(PlayerSprite player = null)
        {
            MacroButton button = new MacroButton(type: MacroButtonType.Ability, abilityType: abilityType, player:player);
            button.SetIcon(iconTextureName, iconSource);
            return button;
        }


        public int RefundAbilityPoints()
        {
            int points = 0;
            while (level > 0)
            {
                level--;
                points += abilityPointCost[level];
            }
            return points;
        }

        public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position, Vector2 frame)
        {
            position.X += (frame.X - iconSource.Width) / 2;
            position.Y += (frame.Y - iconSource.Width) / 2;
            spriteBatch.Draw(iconTexture, position, iconSource, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        }


        public virtual void ResetOnDeath()
        {
            elapsed = duration;
        }


        public int Level
        {
            get { return level; }
            set { level = value;  }
        }



        public virtual void AddDescriptionDetails(DescriptionScreen screen)
        {
            screen.AddTexture(iconTexture, iconSource);
            screen.AddLine(name, Fonts.HeaderFont, Color.White);

            if (isPassive)
            {
                screen.AddLine("Passive", Fonts.DescriptionFont, Color.Tan);
            }
            else
            {
                float cooldown = (float)duration / 1000f;
                screen.AddLine("Cool Down: " + cooldown.ToString() + " seconds", Fonts.DescriptionFont, Color.Tan);
            }

            if (manaCost[level == 0 ? 0 : level - 1] > 0)
            {
                screen.AddLine("Mana Cost: " + manaCost[level == 0 ? 0 : level - 1].ToString(), Fonts.DescriptionFont, Color.SkyBlue);
            }

            if (!String.IsNullOrEmpty(Description))
            {
                screen.AddLine(Description, Fonts.DescriptionFont, Color.White);
            }

            if (level != MaxLevel)
            {
                Color upgradeColor = Color.Green;
                if (player.AbilityPoints < abilityPointCost[level])
                {
                    upgradeColor = Color.Red;
                }
                if (level != 0)
                {
                    screen.AddLine("Upgrade: " + abilityPointCost[level].ToString() + " AP", Fonts.DescriptionFont, upgradeColor);                }
                else
                {
                    screen.AddLine("Learn: " + abilityPointCost[level].ToString() + " AP", Fonts.DescriptionFont, upgradeColor);
                }

                upgradeColor = Color.Green;
                if (requiredLevel != 0 && player.Abilities[(int)requiredAbilityType].Level < requiredLevel)
                {
                    upgradeColor = Color.Red;
                }

                if (requiredLevel != 0)
                {
                    screen.AddLine("Requires " + player.Abilities[(int)requiredAbilityType].Name + " level " + requiredLevel.ToString(), Fonts.DescriptionFont, upgradeColor);
                }

                if (manaCost[level] > 0 && level != 0)
                {
                    screen.AddLine("Upgraded Mana Cost: " + manaCost[level].ToString(), Fonts.DescriptionFont, Color.SkyBlue);
                }

                if (level != 0)
                {
                    screen.AddLine("Next Level: " + NextDescription, Fonts.DescriptionFont, Color.White);
                }
            }

        }


        virtual public Color GetAbilityTint()
        {
            // return a tint for tiles if this ability acts on tiles
            return Color.White;
        }

        virtual public void TileAbilityEfffect(CharacterSprite sprite)
        {
            // an enemy is on a tile that this ability effects
        }



        public virtual string Description { get { return String.Empty; } }
        public virtual string NextDescription { get { return String.Empty; } }
        public AbilityType AbilityType { get { return abilityType; } }
    }
}
