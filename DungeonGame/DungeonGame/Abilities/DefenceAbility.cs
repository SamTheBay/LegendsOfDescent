using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace LegendsOfDescent
{
    class DefenceAbility : Ability
    {

        public DefenceAbility(PlayerSprite player)
            : base("Armor Mastery", player, 7)
        {
            isPassive = true;
            abilityType = AbilityType.Defence;
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
                        return "Become proficient with armor";
                    case 1:
                        return "Become an expert with armor";
                    case 2:
                        return "Become a master with armor";
                    case 3:
                        return "You are a master with armor";
                }
                return "";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increased armor";
            }
        }



    }
}
