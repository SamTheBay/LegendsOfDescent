using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LegendsOfDescent
{
    public enum PlayerClassType
    {
        Commoner,
        Warrior,
        Rogue,
        Mage
    }

    public class PlayerClassManager
    {
        public static PlayerClassManager Instance = new PlayerClassManager();
        Dictionary<PlayerClassType, PlayerClass> playerClasses = new Dictionary<PlayerClassType,PlayerClass>();

        private PlayerClassManager()
        {
            // generate the class layout
            PlayerClass commoner = new PlayerClass("Commoner", 0, PlayerClassType.Commoner);
            commoner.texturePrefix = "commoner";
            commoner.abilities.Add(AbilityType.Heal);
            commoner.abilities.Add(AbilityType.Sprint);
            commoner.abilities.Add(AbilityType.Cleave);
            commoner.abilities.Add(AbilityType.MagicArrow);
            commoner.abilities.Add(AbilityType.FireBolt);
            commoner.abilities.Add(AbilityType.IceBolt);
            commoner.abilities.Add(AbilityType.Lightning);
            commoner.description = "You are a simple commoner. There is nothing about you that sets you apart from everyone else.";
            playerClasses.Add(PlayerClassType.Commoner, commoner);


            // Teir 1 classes
            PlayerClass warrior = new PlayerClass("Warrior", 1, PlayerClassType.Warrior);
            warrior.texturePrefix = "warrior";
            commoner.nextClasses.Add(warrior);
            warrior.parentClass = commoner;
            warrior.properties.Add(new PropertyModifier(Property.Health, 20));
            warrior.properties.Add(new PropertyModifier(Property.HealthRegen, 30));
            warrior.properties.Add(new PropertyModifier(Property.MeleeDamage, 20));
            warrior.properties.Add(new PropertyModifier(Property.Armor, 30));
            warrior.abilities.Add(AbilityType.FrostArmor);
            warrior.abilities.Add(AbilityType.Frenzy);
            warrior.abilities.Add(AbilityType.StoneSkin);
            warrior.abilities.Add(AbilityType.Earthquake);
            warrior.abilities.Add(AbilityType.Precision);
            warrior.abilities.Add(AbilityType.EpicStrike);
            warrior.abilities.Add(AbilityType.Fear);
            warrior.abilities.Add(AbilityType.SlowTime);
            warrior.description = "Warriors are the strongest and fiercest fighters. The warrior has increased health, health regeneration, armor and damage with melee weapons.";
            playerClasses.Add(PlayerClassType.Warrior, warrior);

            PlayerClass rogue = new PlayerClass("Rogue", 1, PlayerClassType.Rogue);
            rogue.texturePrefix = "rogue";
            commoner.nextClasses.Add(rogue);
            rogue.parentClass = commoner;
            rogue.properties.Add(new PropertyModifier(Property.MoveSpeed, 15));
            rogue.properties.Add(new PropertyModifier(Property.AttackSpeed, 15));
            rogue.properties.Add(new PropertyModifier(Property.RangedDamage, 15));
            rogue.properties.Add(new PropertyModifier(Property.Health, 10));
            rogue.properties.Add(new PropertyModifier(Property.Mana, 10));
            rogue.properties.Add(new PropertyModifier(Property.ManaRegen, 15));
            rogue.properties.Add(new PropertyModifier(Property.HealthRegen, 15));
            rogue.abilities.Add(AbilityType.Invisibility);
            rogue.abilities.Add(AbilityType.MultiShot);
            rogue.abilities.Add(AbilityType.PoisonTrap);
            rogue.abilities.Add(AbilityType.ExplosionTrap);
            rogue.abilities.Add(AbilityType.PoisonMastery);
            rogue.abilities.Add(AbilityType.ShadowMastery);
            rogue.abilities.Add(AbilityType.Fear);
            rogue.abilities.Add(AbilityType.SlowTime);
            rogue.description = "Rogues use their speed and agility to overcome all odds. The Rogue has increased movement speed, increased attack speed and increased damage with ranged weapons as well as modest increases to health and mana";
            playerClasses.Add(PlayerClassType.Rogue, rogue);

            PlayerClass mage = new PlayerClass("Mage", 1, PlayerClassType.Mage);
            mage.texturePrefix = "mage";
            commoner.nextClasses.Add(mage);
            mage.parentClass = commoner;
            mage.properties.Add(new PropertyModifier(Property.Mana, 20));
            mage.properties.Add(new PropertyModifier(Property.ManaRegen, 30));
            mage.properties.Add(new PropertyModifier(Property.SpellDamage, 20));
            mage.properties.Add(new PropertyModifier(Property.CastSpeed, 15));
            mage.abilities.Add(AbilityType.FireBall);
            mage.abilities.Add(AbilityType.ChainLightning);
            mage.abilities.Add(AbilityType.Nova);
            mage.abilities.Add(AbilityType.FrozenGround);
            mage.abilities.Add(AbilityType.MoltenEarth);
            mage.abilities.Add(AbilityType.Blizzard);
            mage.abilities.Add(AbilityType.Fear);
            mage.abilities.Add(AbilityType.SlowTime);
            mage.description = "Mages are masters in the use of magical arts. They have increased mana, mana regeneration, spell damage and cast speed";
            playerClasses.Add(PlayerClassType.Mage, mage);

            // Tier 2 classes
            // Barbarian
            // Paladin
            // Assassin
            // Marksman
            // Elementalist
            // Priest
        }



        public PlayerClass GetClass(PlayerClassType type)
        {
            return playerClasses[type];
        }


        public int GetTierUpgradelevel(int tier)
        {
            switch (tier)
            {
                case 0:
                    {
                        return 1;
                    }
                case 1:
                    {
                        return 5;
                    }
                case 2:
                    {
                        return 999;
                    }
                default:
                    {
                        return 999;
                    }
            }
        }

    }
}
