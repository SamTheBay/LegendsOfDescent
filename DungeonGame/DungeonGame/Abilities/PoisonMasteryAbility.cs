using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    class PoisonMasteryAbility : Ability
    {
        float[] poisonChance = { .2f, .3f, .4f, .5f, .6f, .7f };

        public PoisonMasteryAbility(PlayerSprite player)
            : base("Poison Mastery", player, 31)
        {
            isPassive = true;
            abilityType = AbilityType.PoisonMastery;

            abilityPointCost[0] = 5;
            abilityPointCost[1] = 10;
            abilityPointCost[2] = 30;
            abilityPointCost[3] = 80;
            abilityPointCost[4] = 250;
            abilityPointCost[5] = 800;
        }


        public override string Description
        {
            get
            {
                int offset = 0;
                if (level > 0)
                    offset = level - 1;

                return "Every attack has a " + (poisonChance[offset] * 100).ToString() + "% to cause an additional 20% poison damage every second over 5 seconds";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increase chance of poisoning enemies to " + (poisonChance[level] * 100).ToString() + "%";
            }
        }



        public float PoisonChance
        {
            get
            {
                if (level == 0)
                    return 0;
                else
                    return poisonChance[level - 1];
            }
        }


    }
}
