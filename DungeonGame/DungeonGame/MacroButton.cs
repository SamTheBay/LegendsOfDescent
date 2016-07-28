using System;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public enum MacroButtonType : byte
    {
        Weapon,
        Usable,
        Ability,
        Swap,
        TownPortal,
        None
    }

    public class MacroButton : ISaveable
    {
        public static MacroButton emptyMacro = new MacroButton();

        MacroButtonType type = MacroButtonType.None;
        AbilityType abilityType = AbilityType.Melee;
        String itemName = "";
        PlayerSprite player = SaveGameManager.CurrentPlayer;

        // members for drawing the icon
        Texture2D texture = InternalContentManager.GetSolidColorTexture(Color.Transparent);
        String textureName = "";
        Rectangle iconLocationInTexture = new Rectangle();

        public MacroButton(MacroButtonType type = MacroButtonType.None, AbilityType abilityType = AbilityType.Melee, String itemName = "", PlayerSprite player = null)
        {
            this.type = type;
            this.abilityType = abilityType;
            this.itemName = itemName;
            if (player != null)
            {
                this.player = player;
            }
        }


        public void SetIcon(String textureName, Rectangle iconLocationInTexture)
        {
            this.textureName = textureName;
            this.iconLocationInTexture = iconLocationInTexture;
            texture = InternalContentManager.GetTexture(textureName);
        }

        public void Select()
        {
            if (type == MacroButtonType.Weapon)
            {
                // select the weapon that the user has equipped
                EquipableItem weapon = player.EquippedItems[(int)EquipSlot.Hand,0];
                if (weapon != null)
                {
                    weapon.Select();
                }
            }
            else if (type == MacroButtonType.Usable)
            {
                // search for this item in the players inventory
                ItemSprite[,] items = player.Inventory;
                for (int x = 0; x < items.GetLength(0); x++)
                {
                    for (int y = 0; y < items.GetLength(1); y++)
                    {
                        if (items[x,y] != null && items[x, y].Name == itemName)
                        {
                            items[x, y].Select();
                            return;
                        }
                    }
                }
            }
            else if (type == MacroButtonType.Ability)
            {
                if (player.Abilities[(int)abilityType].Level > 0)
                {
                    // select the ability that this macro represents
                    player.Abilities[(int)abilityType].Select();
                }
            }
            else if (type == MacroButtonType.Swap)
            {
                player.SwapItem();
            }
            else if (type == MacroButtonType.TownPortal)
            {
                if (SaveGameManager.CurrentPlayer.DungeonLevel != 0)
                {
                    SaveGameManager.CurrentPlayer.ActivateTownPortal();
                }
            }
        }


        public void DrawIcon(SpriteBatch spriteBatch, Vector2 position, Vector2 frame)
        {
            if (type == MacroButtonType.Weapon)
            {
                EquipableItem weapon = player.EquippedItems[(int)EquipSlot.Hand, 0];
                if (weapon != null)
                {
                    weapon.DrawIcon(spriteBatch, position, frame);
                }
                else
                {
                    // draw the empty hand icon
                    spriteBatch.Draw(InternalContentManager.GetTexture("ItemIcons"), position + new Vector2(frame.X / 2 - 24, frame.Y / 2 - 24), new Rectangle(48, 48, 48, 48), Color.White);
                }
            }
            else
            {
                position.X += (frame.X - iconLocationInTexture.Width) / 2;
                position.Y += (frame.Y - iconLocationInTexture.Height) / 2;

                if (type == MacroButtonType.TownPortal && SaveGameManager.CurrentPlayer.DungeonLevel == 0)
                {
                    spriteBatch.Draw(texture, position, iconLocationInTexture, Color.Gray, 0f, Vector2.Zero, 1f, SpriteEffects.None, .0001f);
                }
                else
                {
                    spriteBatch.Draw(texture, position, iconLocationInTexture, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, .0001f);
                }

                if (type == MacroButtonType.Ability)
                {
                    Ability ability = player.Abilities[(int)abilityType];
                    if (!ability.IsReady && ability.Duration > 1000)
                    {
                        position.X += iconLocationInTexture.Width / 2;
                        position.Y += iconLocationInTexture.Height / 2;
                        Fonts.DrawCenteredText(spriteBatch, Fonts.HeaderFont, ability.DurationRemaining.ToString(), position, Color.Red);
                    }
                }
            }
        }


        public void Persist(BinaryWriter writer)
        {
            writer.Write((Byte)type);
            writer.Write((Byte)abilityType);
            writer.Write(itemName);
            writer.Write(textureName);
            writer.Write(iconLocationInTexture);
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            type = (MacroButtonType)reader.ReadByte();

            // backward compat for 0.3
            if (dataVersion == 1 && type == MacroButtonType.Swap)
                type = MacroButtonType.None;

            // backward compat for 0.5
            if ((dataVersion == 7 || dataVersion == 8) && type == MacroButtonType.TownPortal)
                type = MacroButtonType.None;

            abilityType = (AbilityType)reader.ReadByte();
            itemName = reader.ReadString();
            textureName = reader.ReadString();
            if (textureName == "Town")
                textureName = "TownButton";
            iconLocationInTexture = reader.ReadRectangle();

            if (type != MacroButtonType.None)
            {
                texture = InternalContentManager.GetTexture(textureName);
                player = SaveGameManager.CurrentPlayer;
            }

            // check to make sure this macro is still valid
            ValidateMacro();

            return true;
        }


        public void ValidateMacro()
        {
            if (player == null)
                return;

            if (type == MacroButtonType.Ability)
            {
                if (player.Abilities[(int)abilityType].Level == 0)
                {
                    type = MacroButtonType.None;
                    texture = InternalContentManager.GetSolidColorTexture(Color.Transparent);
                }
            }
        }


        public bool IsEqual(MacroButton otherMacro)
        {
            if (type == otherMacro.type)
            {
                if (type == MacroButtonType.Ability && abilityType == otherMacro.abilityType)
                {
                    return true;
                }
                if (type == MacroButtonType.Usable && itemName == otherMacro.itemName)
                {
                    return true;
                }
                if (type == MacroButtonType.Weapon)
                {
                    return true;
                }
                if (type == MacroButtonType.Swap)
                {
                    return true;
                }
                if (type == MacroButtonType.TownPortal)
                {
                    return true;
                }
            }
            return false;
        }


        public bool IsEqual(ISelectable selectable)
        {
            if (MacroButtonType.None == type || IsEmpty || MacroButtonType.Swap == type || MacroButtonType.TownPortal == type)
            {
                return false;
            }
            if (selectable is Ability && MacroButtonType.Usable != type)
            {
                Ability ability = (Ability)selectable;
                if ((ability.AbilityType == AbilityType.Bow || ability.AbilityType == AbilityType.Melee) && type == MacroButtonType.Weapon)
                {
                    // special case for weapons
                    return true;
                }
                if (ability.AbilityType == abilityType)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsEmpty
        {
            get { return type == MacroButtonType.None; }
        }

        public MacroButtonType Type
        {
            get { return type; }
        }
    }
}
