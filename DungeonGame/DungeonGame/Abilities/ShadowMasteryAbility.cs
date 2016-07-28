using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    class ShadowMasteryAbility : Ability
    {
        float[] stayInvisChance = { .2f, .3f, .4f, .5f, .6f, .7f };

        public ShadowMasteryAbility(PlayerSprite player)
            : base("Shadow Mastery", player, 30)
        {
            isPassive = true;
            abilityType = AbilityType.PoisonMastery;

            abilityPointCost[0] = 3;
            abilityPointCost[1] = 8;
            abilityPointCost[2] = 20;
            abilityPointCost[3] = 50;
            abilityPointCost[4] = 170;
            abilityPointCost[5] = 600;
        }


        public override string Description
        {
            get
            {
                int offset = 0;
                if (level > 0)
                    offset = level - 1;

                return "There is a " + (stayInvisChance[offset] * 100).ToString() + "% chance that your invisibility will not be interrupted by attacking.";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Chance of staying invisible while attacking increases to " + (stayInvisChance[level] * 100).ToString() + "%";
            }
        }



        public float StayInvisChance
        {
            get
            {
                if (level == 0)
                    return 0;
                else
                    return stayInvisChance[level - 1];
            }
        }


    }
}
