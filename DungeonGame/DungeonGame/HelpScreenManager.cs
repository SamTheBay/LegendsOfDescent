using System;
using System.Collections.Generic;
using System.IO;
using LegendsOfDescent;

namespace LegendsOfDescent
{
    public enum HelpScreens
    {
        Abilities = 0,
        Inventory = 1,
        Statistics = 2,
        HowToAttack = 3,
        LevelUp = 4,
        Merchants = 5,
        Potions = 6,
        Stairs = 7,
        WanderingMerchants = 8,
        Warping = 9,
        Quests = 10,
        Blacksmith = 11,
        Stash = 12,
        Class = 13,
        Teleport = 14,
        HowToMove = 15,
        Dragnor = 16,
        HowToMoveJoystick = 17,
        HowToAttackJoystick = 18,
        Help = 19,
        QuestComplete = 20,
        HowToAttackTouch = 21
    }

    public class HelpScreenManager : ISaveable
    {
        public static HelpScreenManager Instance;

        List<HelpScreen> helpScreens = new List<HelpScreen>();
        bool[] hasBeenShownBefore;

        public HelpScreenManager()
        {
            Instance = this;

            // put together all of the help screens

            // 0
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Abilities", "", "Click or tap an ability's title to display info about the ability and to see the option to upgrade it. Drag the ability's icon to a red hot key in order to be able to use it during game play."));
            else
                helpScreens.Add(new HelpScreen("Abilities", "", "Tap an ability's title to display info about the ability and to see the option to upgrade it. Drag the ability's icon to a red hot key in order to be able to use it during game play."));
            
            // 1
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Inventory", "", "Click or tap items to display information about them. Drag items to move them or equip them to your character. You can drag weapons and usable items to the red circle hot keys in order to use them in game. Drag items over the X slot to destroy them or the arrow slot to drop them. Click or tap the circle arrows to swap to your secondary weapon. Click or tap the house icon to use a town portal. You can drag the circle arrows or house to a hot key in order to use it during game play."));
            else
                helpScreens.Add(new HelpScreen("Inventory", "", "Tap items to display information about them. Drag items to move them or equip them to your character. You can drag weapons and usable items to the red circle hot keys in order to use them in game. Drag items over the X slot to destroy them or the arrow slot to drop them. Tap the circle arrows to swap to your secondary weapon. Tap the house icon to use a town portal. You can drag the circle arrows or house to a hot key in order to use it during game play."));
            
            // 2
            helpScreens.Add(new HelpScreen("Statistics", "", "This screen displays general statistics about your character. You can also upgrade you characters class from here. Your first class upgrade is at level 5"));
            
            // 3
            helpScreens.Add(new HelpScreen("How to Attack", "", "Right click a location on the screen in order to attack in that direction. Click the red hot keys on the bottom of the screen in order to use the item/ability that has been placed there (or use their hotkey number on your keyboard). Also, touch screen mode can be enabled in the settings if your computer supports it."));        

            // 4
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Level Up!", "Ability", "Your character has just leveled up! He has automatically become stronger. Also, you have gained ability points. Press esc and select the \"Abilities\" tab (or press the A hotkey) in order to use them on new abilities. You can also tap the icon in the top left corner to open the menu."));
            else
                helpScreens.Add(new HelpScreen("Level Up!", "Ability", "Your character has just leveled up! He has automatically become stronger. Also, you have gained ability points. Press back and select the \"Abilities\" tab in order to use them on new abilities."));
            
            // 5
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Merchants", "", "Drag items from your inventory to the merchant's inventory to sell them. Drag items from the Merchant's inventory to yours to buy them. Click or tap items to view their properties."));
            else
                helpScreens.Add(new HelpScreen("Merchants", "", "Drag items from your inventory to the merchant's inventory to sell them. Drag items from the Merchant's inventory to yours to buy them. Tap items to view their properties."));

            // 6
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Using Potions", "", "Your life or mana is below half. If you click or tap your life bar or mana bar at the top of the screen it will use the most appropriate potion that you have in order to restore you."));
            else
                helpScreens.Add(new HelpScreen("Using Potions", "", "Your life or mana is below half. If you tap your life bar or mana bar at the top of the screen it will use the most appropriate potion that you have in order to restore you."));

            // 7
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Using the Stairs", "", "You have found the stairs down to the next dungeon level. Stand next to them and click or tap them in order to go down. As you descend down the creatures will become more powerful and the loot will be more valuable."));
            else
                helpScreens.Add(new HelpScreen("Using the Stairs", "", "You have found the stairs down to the next dungeon level. Stand next to them and tap them in order to go down. As you descend down the creatures will become more powerful and the loot will be more valuable."));

            // 8
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Merchants", "", "Your are near a merchant NPC. Click or tap him in order to initiate trading items."));
            else
                helpScreens.Add(new HelpScreen("Merchants", "", "Your are near a merchant NPC. Tap him in order to initiate trading items."));

            // 9
            helpScreens.Add(new HelpScreen("Warping", "", "Warping allows you to travel instantly anywhere in a dungeon. Simply click or tap the location on the map where you would like to go and then hit the warp button. The green dot indicates where you have currently selected to warp to. You must have discovered a location before you can warp to it."));

            // 10
            helpScreens.Add(new HelpScreen("Quests", "", "Complete quests to earn gold, experience, and more! There are new quests for every level of the dungeon, so be sure to check the NPC's in town regularly."));

            // 11
            helpScreens.Add(new HelpScreen("Blacksmith", "", "The blacksmith can enhance items that you have by increasing the effectiveness of weapons and armor. Drag an item into the upgrade box to see details on what upgrades it can receive and how much gold it will cost."));

            // 12
            helpScreens.Add(new HelpScreen("Stash", "", "You can store items that you are not currently using in your Stash for safe keeping. There are five sections that can be unlocked in your stash for maximum storage."));
            
            // 13
            helpScreens.Add(new HelpScreen("Class Upgrades", "", "You have just reached level 5! You can now pick your tier 1 class of Warrior, Rogue or Mage. Go to the stats screen in order to upgrade."));

            // 14
            helpScreens.Add(new HelpScreen("Teleporting", "", "You can teleport to the entry point to any dungeon level that you have previously been to. Simply select the dungeon that you would like to be teleported to."));
            
            // 15
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("How to Move", "", "Left click a location or tap on the screen to move your character. Left click or tap on a character to talk with them. Press ESC to open up the menu or tap the menu button in the upper left corner of th screen."));
            else
                helpScreens.Add(new HelpScreen("How to Move", "", "Tap a location on the screen to move your character. Tap on a character to talk with them. Press the back key to open up the menu."));

            // 16
            helpScreens.Add(new HelpScreen("Dragnor", "", "Dragnor has the power to summon skeletons to do his bidding. Dispose of these skeletons quickly or else he will absorb them back into himself to gain health."));

            // 17
            helpScreens.Add(new HelpScreen("How to Move", "JoystickMoveHelp", "Use the joystick in the bottm right corner to move. Tap on a character to talk with them or items and doors to use them. Tap the icon in the top left corner in order to open the menu. There are alternate control schemes in the menu if you don't want to use the joystick."));

            // 18
            helpScreens.Add(new HelpScreen("How to Attack", "JoystickAttackHelp", "Press the sword button in the bottom right of the screen to use your active attack. Tap the red hot keys on the bottom center of the screen in order to use the item/ability that has been placed there."));

            // 19
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("Help", "help", "Tap the \"?\" icon in order to get hlep information about how to use any particular game screen. Also, you can access the menu, which includes your inventory and abilities by pressing esc or tapping the icon in the top left corner."));
            else
                helpScreens.Add(new HelpScreen("Help", "help", "Tap the \"?\" icon in order to get hlep information about how to use any particular game screen. Also, you can access the menu, which includes your inventory and abilities by pressing the back button."));

            // 20
            helpScreens.Add(new HelpScreen("Quest Completed", "", "You have completed a quest. Select the \"!\" icon in the top right corner to open up the quest menu to see more details. To complete this quest, return to Quentin and speak with him."));

            // 21
            if (DungeonGame.pcMode)
                helpScreens.Add(new HelpScreen("How to Attack", "AttackHelp", "Press and hold the bottom left corner while tapping the screen in order to attack. Or, with a mouse you can right click a location on the screen in order to attack. Click or tap the red hot keys on the bottom of the screen in order to use the item/ability that has been placed there."));
            else
                helpScreens.Add(new HelpScreen("How to Attack", "AttackHelp", "Press and hold the bottom left corner while tapping the screen in order to attack. Tap the red hot keys on the bottom of the screen in order to use the item/ability that has been placed there."));
     

            // mark them as not having been seen before
            hasBeenShownBefore = new bool[helpScreens.Count];
            for (int i = 0; i < hasBeenShownBefore.Length; i++)
            {
                hasBeenShownBefore[i] = false;
            }
        }

        public void Persist(BinaryWriter writer)
        {
            writer.Write((Int32)hasBeenShownBefore.Length);
            for (int i = 0; i < hasBeenShownBefore.Length; i++)
            {
                writer.Write((Boolean)hasBeenShownBefore[i]);
            }
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            Int32 num = reader.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                hasBeenShownBefore[i] = reader.ReadBoolean();
            }

            return true;
        }


        public void ShowHelpScreen(HelpScreens index)
        {
            GameplayScreen.Instance.Pause();
            MarkAsShown(index);
            DungeonGame.ScreenManager.AddScreen(helpScreens[(int)index]);
            InputManager.ClearInputForPeriod(500);
        }


        public bool AutoShowHelpScreen(HelpScreens index)
        {
            if (!hasBeenShownBefore[(int)index])
            {
                ShowHelpScreen(index);
                return true;
            }
            return false;
        }

        public bool HasBeenShownBefore(HelpScreens index)
        {
            return hasBeenShownBefore[(int)index];
        }

        public void MarkAsShown(HelpScreens index)
        {
            hasBeenShownBefore[(int)index] = true;
        }
    }
}
