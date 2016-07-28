using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using LegendsOfDescent;


namespace LegendsOfDescent
{


    public class EquipableItem : ItemSprite
    {
        EquipSlot equipSlot;
        bool isRanged = false;


        public EquipableItem(string name, string texture, int framesPerRow, int offset, int level, float valueAdjust, string description, int value)
            : base(name, texture, framesPerRow, offset, level, valueAdjust, description, value)
        {
            isEquipable = true;
        }


        public override void Select()
        {
            EquipableItem equippedItem = (EquipableItem)owner.GetItem(new Point(-1, (int)EquipSlot.Hand));
            if (equippedItem != null)
            {
                if (equippedItem.IsRanged)
                    owner.SetActiveAbility(AbilityType.Bow);
                else
                    owner.SetActiveAbility(AbilityType.Melee);
            }
        }


        public EquipSlot EquipSlot
        {
            set 
            { 
                equipSlot = value;
                if (equipSlot == EquipSlot.Hand || equipSlot == EquipSlot.TwoHand || equipSlot == EquipSlot.MainHand)
                    isSelectable = true;
            }
            get { return equipSlot; }
        }


        public EquipSlot BasicEquipSlot
        {
            get
            {
                if (equipSlot == LegendsOfDescent.EquipSlot.TwoHand ||
                    equipSlot == LegendsOfDescent.EquipSlot.MainHand)
                {
                    return LegendsOfDescent.EquipSlot.Hand;
                }
                else
                {
                    return equipSlot;
                }
            }
        }

        public override ItemSprite CopyItem()
        {
            EquipableItem item = new EquipableItem(name, TextureName, FramesPerRow, offset, level, valueAdjust, description, value);
            item.SetIcon(iconTextureName, iconFramesPerRow, iconOffset);
            item.EquipSlot = equipSlot;
            for (int i = 0; i < modifiers.Count; i++)
            {
                item.modifiers.Add(new PropertyModifier(modifiers[i]));
            }
            item.IsRanged = isRanged;
            if (showParticles)
            {
                item.SetParticleGlow(particleType, particleColor);
            }
            return item;
        }


        public float GetSwingSpeed()
        {
            float speed = .6f;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].property == Property.BaseAttackSpeed ||
                    modifiers[i].property == Property.AttackSpeed)
                {
                    speed = speed - (speed * (modifiers[i].amount / 100f));
                }
            }
            return speed;
        }

        public float GetCombinedDamage()
        {
            int damage = 0;
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i].property == Property.Damage ||
                    modifiers[i].property == Property.FireDamage ||
                    modifiers[i].property == Property.ColdDamage ||
                    modifiers[i].property == Property.LightningDamage)
                {
                    damage += modifiers[i].amount;
                }
            }
            return damage;
        }


        public override void AddDescriptionDetails(DescriptionScreen screen)
        {
            screen.AddTexture(iconTexture, iconSource);
            screen.AddLine(name + " (" + level.ToString() + ")", Fonts.HeaderFont, ClassColor());
            screen.AddLine("Slot: " + equipSlot.ToDescription(), Fonts.DescriptionFont, Color.White);
            int damage = GetPropertyValue(Property.Damage);

            if ((equipSlot == EquipSlot.Hand || equipSlot == EquipSlot.MainHand || equipSlot == EquipSlot.TwoHand) && GetCombinedDamage() > 0)
            {
                float dps = (float)GetCombinedDamage() / GetSwingSpeed();

                if (isRanged)
                {
                    screen.AddLine("Fire Speed: " + GetSwingSpeed().ToString("F2") + " Sec/Fire", Fonts.DescriptionFont, Color.White);
                }
                else
                {
                    screen.AddLine("Swing Speed: " + GetSwingSpeed().ToString("F2") + " Sec/Swing", Fonts.DescriptionFont, Color.White);
                }

                screen.AddLine("DPS: " + dps.ToString("F1"), Fonts.DescriptionFont, Color.White);
            }

            if (slot.Y == (int)LegendsOfDescent.EquipSlot.OffHand && slot.X == -1 && damage > 0)
            {
                screen.AddLine("(-50% damage in offhand)", Fonts.DescriptionFont, Color.Red);
            }

            for (int i = 0; i < modifiers.Count; i++)
            {
                if (!modifiers[i].property.ToString().Contains("Base"))
                {
                    screen.AddLine(ModifierToString(modifiers[i]), Fonts.DescriptionFont, Color.White);
                }
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

        public override int GetPropertyValue(Property property)
        {
            if (slot.Y == (int)LegendsOfDescent.EquipSlot.OffHand && slot.X == -1)
            {
                if (property == Property.Damage ||
                    property == Property.FireDamage ||
                    property == Property.ColdDamage ||
                    property == Property.LightningDamage ||
                    property == Property.BleedDamage ||
                    property == Property.BurnDamage ||
                    property == Property.PoisonDamage)
                {
                    int origonalValue = base.GetPropertyValue(property);
                    return (origonalValue + 1) / 2;
                }
            }
            return base.GetPropertyValue(property);
        }


        public ItemProfile GetItemProfile()
        {
            ItemProfile profile = new ItemProfile();

            profile.Name = name;
            profile.Level = level;
            profile.IconFrame = iconOffset;
            profile.ItemClass = (int)itemClass;
            profile.Description = description;
            profile.Properties.Add("Slot", equipSlot.ToString());
            

            if (equipSlot == EquipSlot.Hand)
            {
                int damage = GetPropertyValue(Property.Damage);
                float dps = (float)damage / GetSwingSpeed();

                profile.Properties.Add("IsRanged", isRanged.ToString());
                profile.Properties.Add("Speed", GetSwingSpeed().ToString("F2"));
                profile.Properties.Add("DPS", dps.ToString("F1"));
            }

            foreach (var modifier in modifiers)
            {
                profile.Properties.Add(ItemManager.PropertyToString(modifier.property), modifier.amount.ToString());
            }

            return profile;
        }



        public override void Persist(BinaryWriter writer)
        {
            base.Persist(writer);

            // write equip-able specific info
            writer.Write((Int32)equipSlot);
            writer.Write(isRanged);
        }


        public override bool Load(BinaryReader reader, int dataVersion)
        {
            base.Load(reader, dataVersion);

            // load equip-able specific info
            EquipSlot = (EquipSlot)reader.ReadInt32();
            isRanged = reader.ReadBoolean();

            return true;
        }



        public override void PlayDroppedSoundEffect()
        {
            if (EquipSlot == EquipSlot.Augment)
            {
                AudioManager.audioManager.PlaySFX("MetalDrop");
            }
            else if (name.Contains("Chain Mail"))
            {
                AudioManager.audioManager.PlaySFX("Chain" + Util.Random.Next(1, 4).ToString());
            }
            else if (name.Contains("Plate"))
            {
                AudioManager.audioManager.PlaySFX("HeavyDrop" + Util.Random.Next(1, 4).ToString());
            }
            else
            {
                base.PlayDroppedSoundEffect();
            }
        }


        public bool IsRanged
        {
            get { return isRanged; }
            set { isRanged = value; }
        }


        public override MacroButton GetMacroButton(PlayerSprite player = null)
        {
            MacroButton button = new MacroButton(type: MacroButtonType.Weapon, player:player);
            button.SetIcon(iconTextureName, iconSource);
            return button;
        }

    }
}
