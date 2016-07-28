using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    public class PlayerClass
    {
        public String name = "";
        public int tier = 0;
        public PlayerClassType type;
        public List<PlayerClass> nextClasses = new List<PlayerClass>();
        public List<PropertyModifier> properties = new List<PropertyModifier>();
        public String texturePrefix = "";
        public string description = "";
        public PlayerClass parentClass;
        public List<AbilityType> abilities = new List<AbilityType>();

        public PlayerClass(String name, int tier, PlayerClassType type)
        {
            this.name = name;
            this.tier = tier;
            this.type = type;
        }


        public float GetPropertyValue(Property property, float baseValue)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].property == property)
                {
                    if (baseValue != 0)
                    {
                        return (properties[i].amount / 100f) * baseValue;
                    }
                    else
                    {
                        return properties[i].amount;
                    }
                }
            }

            return 0;
        }



        public bool HasAbility(AbilityType ability)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                if (ability == abilities[i])
                {
                    return true;
                }
            }

            if (parentClass != null)
            {
                return parentClass.HasAbility(ability);
            }


            return false;
        }


        public void GetAbilityList(PlayerSprite player, List<Ability> abilities)
        {
            if (parentClass != null)
            {
                parentClass.GetAbilityList(player, abilities);
            }

            foreach (AbilityType type in this.abilities)
            {
                abilities.Add(player.Abilities[(int)type]);
            }
        }

    }
}
