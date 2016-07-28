using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    interface IStackable
    {
        void DecrementStack();
        bool CombineStacks(UsableItem otherItem);
        bool IsStackEmpty { get; }
    }


    public class UsableItem : ItemSprite, IStackable, ITimedEffect
    {
        int duration;
        int elapsedDuration = 0;
        int maxStackSize = 20;
        int currentStackCount = 1;
        int maxGroup = 999;


        public UsableItem(string name, string texture, int framesPerRow, int offset, int level, float valueAdjust, string description, int value)
            : base(name, texture, framesPerRow, offset, level, valueAdjust, description, value)
        {
            isSelectable = true;
        }


        public override void Select()
        {
            if (name.Contains("Scroll"))
            {
                AudioManager.audioManager.PlaySFX("Magic" + Util.Random.Next(1, 6).ToString());
            }
            else
            {
                AudioManager.audioManager.PlaySFX("Potion" + Util.Random.Next(1, 4).ToString());
            }
            owner.ActivateUsableItem(this);
        }


        public override void PlayDroppedSoundEffect()
        {
            if (name.Contains("Scroll"))
            {
                AudioManager.audioManager.PlaySFX("Drop");
            }
            else
            {
                AudioManager.audioManager.PlaySFX("Potion" + Util.Random.Next(1, 4).ToString());
            }
        }


        public override ItemSprite CopyItem()
        {
            UsableItem item = new UsableItem(name, TextureName, FramesPerRow, offset, level, valueAdjust, description, value);
            item.SetIcon(iconTextureName, iconFramesPerRow, iconOffset);
            item.duration = duration;
            for (int i = 0; i < modifiers.Count; i++)
            {
                item.modifiers.Add(new PropertyModifier(modifiers[i]));
            }
            if (showParticles)
            {
                item.SetParticleGlow(particleType, particleColor);
            }
            item.maxGroup = maxGroup;
            return item;
        }


        public bool UpdateDuration(GameTime gameTime)
        {
            elapsedDuration += gameTime.ElapsedGameTime.Milliseconds;
            if (elapsedDuration > duration)
                return true;
            return false;
        }

        public bool IsDebuff()
        {
            return false;
        }

        public IActiveIcon CombineActiveIcons(IActiveIcon other)
        {
            UsableItem o = other as UsableItem;
            if (o != null && o.name == name)
            {
                if (other.GetActiveIconTime() > GetActiveIconTime())
                {
                    return other;
                }
            }

            return this;
        }


        public Texture2D GetActiveIconTexture()
        {
            return iconTexture;
        }


        public Rectangle GetActiveIconSource()
        {
            // sorta hacky way to deal with the very small potion icon
            if (level <= 4)
                return new Rectangle(13, 20, 24, 24);
            else if (level <= 8)
                return new Rectangle(60, 16, 24, 24);
            else
                return iconSource;
        }

        public bool IsEqualActiveIcon(IActiveIcon other)
        {
            if (other is UsableItem)
            {
                UsableItem otherUsable = (UsableItem)other;
                if (otherUsable.name == this.name)
                {
                    return true;
                }
            }
            return false;
        }


        public int GetActiveIconTime()
        {
            return Math.Max(duration - elapsedDuration, 0);
        }




        public override void AddDescriptionDetails(DescriptionScreen screen)
        {
            screen.AddTexture(iconTexture, iconSource);
            screen.AddLine(name, Fonts.HeaderFont, ClassColor());

            if (!IsInstantUse)
            {
                screen.AddLine("Duration: " + (duration / 1000).ToString(), Fonts.DescriptionFont, Color.White);
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                screen.AddLine(modifiers[i].ToString(), Fonts.DescriptionFont, Color.White);
            }

            if (owner is PlayerSprite)
            {
                screen.AddLine("Sell For: " + Value.ToString(), Fonts.DescriptionFont, Color.Gold);
            }
            else
            {
                screen.AddLine("Buy For: " + Value.ToString(), Fonts.DescriptionFont, Color.Gold);
            }

            screen.AddSpace(15);
            screen.AddLine(description, Fonts.DescriptionItFont, Color.SandyBrown);

            screen.ShowSS = true;
        }


        public override void DrawIcon(SpriteBatch spriteBatch, Vector2 position, Vector2 frame)
        {
            base.DrawIcon(spriteBatch, position, frame);

            // draw the stack count
            String text = currentStackCount.ToString();
            Vector2 textPos = new Vector2(position.X + frame.X / 2 + iconSource.Width / 2 - Fonts.DescriptionFont.MeasureString(text).X + 10, position.Y + frame.Y / 2 + iconSource.Height / 2 - Fonts.DescriptionFont.MeasureString(text).Y + 15);
            Fonts.DrawCenteredText(spriteBatch, Fonts.DescriptionFont, text, textPos, Color.White);
        }


        public void ApplyImmediateEffect()
        {
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].property == Property.Health)
                {
                    owner.Heal(modifiers[i].amount);
                    ParticleSystem.AddParticles(owner.CenteredPosition, ParticleType.Fountain, color: Color.FromNonPremultiplied(255, 0, 0, 100));
                }
                else if (modifiers[i].property == Property.Mana)
                {
                    owner.AddMana(modifiers[i].amount);
                    ParticleSystem.AddParticles(owner.CenteredPosition, ParticleType.Fountain, color: Color.FromNonPremultiplied(0, 0, 255, 100));
                }
            }

            // special case for a warp scroll
            if (name.Contains("Warp"))
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new WarpScreen(GameplayScreen.Dungeon, SaveGameManager.CurrentPlayer));
            }
        }


        public override MacroButton GetMacroButton(PlayerSprite player = null)
        {
            MacroButton button = new MacroButton(type: MacroButtonType.Usable, itemName:name, player:player);
            button.SetIcon(iconTextureName, iconSource);
            return button;
        }


        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);

            // write equip-able specific info
            writer.Write((Int32)duration);
            writer.Write((Int32)currentStackCount);
        }


        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            // load equip-able specific info
            duration = reader.ReadInt32();
            currentStackCount = reader.ReadInt32();

            return true;
        }


        public int Duration
        {
            set { duration = value; }
        }

        public bool IsInstantUse
        {
            get { return duration == 0; }
        }

        public void ResetElapsedDuration()
        {
            elapsedDuration = 0;
        }


        public bool CombineStacks(UsableItem otherItem)
        {
            if (otherItem.Equals(this))
                return false;

            if (currentStackCount == maxStackSize)
                return false;

            if (name != otherItem.name)
                return false;

            int total = currentStackCount + otherItem.currentStackCount;
            if (total > maxStackSize)
            {
                currentStackCount = maxStackSize;
                otherItem.currentStackCount = total - maxStackSize;
            }
            else
            {
                currentStackCount = total;
                otherItem.currentStackCount = 0;
            }
            return true;
        }

        public void DecrementStack()
        {
            currentStackCount--;
        }

        public bool IsStackEmpty
        {
            get { return currentStackCount == 0; }
        }

        public override int Value
        {
            get
            {
                return base.Value * currentStackCount;
            }
        }


        public int MaxGroup
        {
            get { return maxGroup; }
            set { maxGroup = value; }
        }
    }
}
