using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{

    // * Balance Theory
    // The goal of balancing is to be able to determine how difficult the game should be
    // when the player is at level 1 (easy) and how difficult it should be at level 15 (lightly difficult)
    // and draw a linear line between the two for all levels. This line can then be extrapolated out
    // forever creating a game that continuously increase in difficulty at a steady rate. The factors involved
    // are how much damage a player can do, how much damage a player can take, how much damage an enemy can do
    // and how much damage and enemy can take. Ultimately, this should boil down to how many hits you expect
    // the player to sustain at a given level and how many hits an enemy can. We might want to consider giving 
    // a light exponential curve so that increases in items don't become insignificant in the end game.
    // The important thing is that the hits taken / hits given ration changes linearly in a more difficult direction.

    // * Experience and leveling
    // A similar concept applies to experience and leveling. We want each level to take more experience and higher
    // level enemies to give more experience in an exponential fashion. However, this really boils down to the number
    // of kills per level, which we want to increase in a linear fashion. The goal will be to make a curve that 
    // has the player reach level 15 after clearing level 10 of the dungeon. This curve can then be extrapolated forever.

    // * Other factors
    // - We must consider that players will be operating at above the curve since they will have magic items giving them
    //   bonuses. This is fine though because the effect of the bonuses is based on the curve, so they will also help less
    //   as the game progresses. Nonetheless, this will probably require some tuning.
    // - All base items must have an approximately neutral effect. So, if an item allows you to attack faster, it must also
    //   do less damage or have some other effect. Otherwise, it will throw off the curve.
    // - The initial calculations of how much experience an enemy should give is dependant on how many enemies we put in a 
    //   level. This will determine the total experience value of the level. We should probably use a static number of enemies
    //   for a dungeon level to keep this simple.
    // - We must consider other bonuses that could give experience to the player such as completing quests or clearing a level
    //   of the dungeon. We should account for these in our curve
    // - Spells are a bit tricky since their damage doesn't scale by level the same way items do. We will have to put some
    //   thought into how to deal with this. Should spells damage increase by level to keep in them inline with the curve?

    class BalanceManager
    {
        // statistical factors (these are mostly cosmetic and can be changed without
        // affecting the balance of the game.
        const float minimumDamage = 5f;
        const float damageIncreaseFactor = 2f;
        const float minimumArmor = 15;
        const float armorIncreaseFactor = 4f;
        const int minimumHealth = 100;
        const int healthIncreaseFactor = 15;
        const int minimumMana = 50;
        const int manaIncreaseFactor = 10;
        const float armorEffectFactor = 10; // decrease to increase armor effect
        const float manaRegenFactor = .07f;
        const float healthRegenFactor = .07f;

        // difficulty controls (major balancing knobs)
        
        // This is how many hits an enemy should take to kill at the
        // start of the game. Increase to increase initial difficulty
        static float[] initialEnemyHitDifficulty = { 1.5f, 2f, 3f };

        // This is the rate at which enemies will increase the hits
        // they can take on average. Increase to increase difficulty
        // as the game progresses
        static float[] enemyHitDifficultyFactor = { 5f, 6f, 8f };

        // This is the number of hits the player can take from enemies
        // on average at the start of the game. Decrease it to increase
        // the initial game difficulty.
        static float[] initialPlayerHitDifficulty = { 85f, 75f, 60f };

        // This is the rate at which the player will be able to sustain
        // less hits from enemies. Increase it to increase the difficulty
        // of the game as it progresses.
        static float[] playerHitDifficultyFactor = { 2.0f, 2.5f, 3.5f };

        // increase to make leveling take longer
        const float experienceDifficultyFactor = 10f;

        // decrease to make initial leveling easier
        const float initialExpirienceDifficulty = 50f;


        public static int GetBaseDamage(int level)
        {
            return (int)(minimumDamage + level * damageIncreaseFactor);
        }


        public static int GetBaseArmor(int level, EquipSlot slot)
        {
            int armor = (int)(minimumArmor + level * armorIncreaseFactor);
            if (slot == EquipSlot.Chest)
            {
                armor = (int)(armor * .5f);
            }
            else if (slot == EquipSlot.Legs)
            {
                armor = (int)(armor * .3f);
            }
            else if (slot == EquipSlot.Head)
            {
                armor = (int)(armor * .2f);
            }
            else if (slot == EquipSlot.OffHand)
            {
                armor = (int)(armor * .4f);
            }

            return armor;
        }


        public static int GetBaseHealth(int level)
        {
            return minimumHealth + (int)(level * healthIncreaseFactor);
        }


        public static int GetBaseHealthRegen(int level)
        {
            return (int)(GetBaseHealth(level) * healthRegenFactor);
        }

        public static int GetBaseMana(int level)
        {
            return minimumMana + (int)(level * manaIncreaseFactor);
        }


        public static int GetBaseManaRegen(int level)
        {
            return (int)(GetBaseMana(level) * manaRegenFactor);
        }

        public static int GetBaseSpellDamage(int level)
        {
            return (int)(GetBaseDamage(level) * 1.5f);
        }


        public static int GetBaseEnemyLife(int level)
        {
            return (int)(GetBaseDamage(level) * initialEnemyHitDifficulty[(int)SaveGameManager.CurrentPlayer.GameDifficulty] + (level - 1) * enemyHitDifficultyFactor[(int)SaveGameManager.CurrentPlayer.GameDifficulty]);
        }


        public static int GetBaseEnemeyDamage(int level)
        {
            return (int)(GetBaseHealth(level) / initialPlayerHitDifficulty[(int)SaveGameManager.CurrentPlayer.GameDifficulty] + level * playerHitDifficultyFactor[(int)SaveGameManager.CurrentPlayer.GameDifficulty]);
        }


        public static float GetArmorEffect(int armor)
        {
            return armor / armorEffectFactor;
        }


        public static int GetCumulativeExpNeeded(int startingLevel, int endingLevel)
        {
            int expNeeded = 0;
            for (int level = startingLevel + 1; level <= endingLevel; level++)
            {
                expNeeded += BalanceManager.GetExperienceToLevel(level);
            }
            return expNeeded;
        }

        public static int GetExperienceToLevel(int level)
        {
            return (int)(Math.Pow(level, 1.7f) * 200);
        }


        public static int GetExperienceForEnemyKill(int level)
        {
            return (int)(GetExperienceToLevel(level) / (initialExpirienceDifficulty + (level * experienceDifficultyFactor)));
        }


        public static int DungeonLevelToMonsterLevel(int dungeonLevel)
        {
            return dungeonLevel;
        }


        public static int GetBaseGoldDrop(int level)
        {
            return 10 + 10 * level;
        }

        public static int GetBaseItemValue(int level)
        {
            return (int)(6f * (float)BalanceManager.GetBaseGoldDrop(level));
        }

    }
}
