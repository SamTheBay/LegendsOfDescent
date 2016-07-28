using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    class PrecisionAbility : Ability
    {
        float[] bleedChance = { .2f, .3f, .4f, .5f, .6f, .7f };

        public PrecisionAbility(PlayerSprite player)
            : base("Precision", player, 26)
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

                return "Every attack has a " + (bleedChance[offset] * 100).ToString() + "% to cause an additional 25% bleeding damage every second over 5 seconds";
            }
        }

        public override string NextDescription
        {
            get
            {
                return "Increase chance of causing bleeding in enemies to " + (bleedChance[level] * 100).ToString() + "%";
            }
        }



        public float BleedChance
        {
            get
            {
                if (level == 0)
                    return 0;
                else
                    return bleedChance[level - 1];
            }
        }


    }
}
