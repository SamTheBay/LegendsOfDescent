using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


#if WINDOWS_PHONE
    using Microsoft.Phone.Tasks;
#endif

namespace LegendsOfDescent
{
    class UpdateNotesScreen : MenuScreen
    {
        List<String> updatePoints = new List<String>();
        List<String> updatePointsSplit = new List<String>();
        Texture2D arrowUp;
        float textStartHeight = DungeonGame.ScreenSize.Y + 120;
        int textLinesToShow = (DungeonGame.ScreenSize.Height - 120) / 26;
        int currentTextStartLine = 0;


        public UpdateNotesScreen()
            : base()
        {
            if (DungeonGame.pcMode)
            {
                //textLinesToShow = 18;
                ActivateBackButton(new Vector2(20, 140));
            }

            // add the update notes
            updatePoints.Add("Alpha Version 0.5.3");
            updatePoints.Add("- Fix bug in review game link");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.5.2");
            updatePoints.Add("- Add support for Windows 8 RT");
            updatePoints.Add("- User interface improvements for tablets and PC's");
            updatePoints.Add("- Show items names when they are dropped");
            updatePoints.Add("- Minor bug fixes");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.5.1");
            updatePoints.Add("- Add paid ads free version for WP7");
            updatePoints.Add("- Add a low memory mode for cheaper devices");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.5");
            updatePoints.Add("- Added an offhand equipment slot");
            updatePoints.Add("- Adjusted items to include two hand and off hand weapons. Also adjusted swing speeds for two hand weapons.");
            updatePoints.Add("- Changed warp to require a scroll and allow you to go anywhere that you have discovered");
            updatePoints.Add("- Added Legendary weapons which are extremely rare and powerful with unique names and abilities");
            updatePoints.Add("- Added new Life Steal, Mana Steal and Reflect Damage item properties");
            updatePoints.Add("- You can now take screenshots in game and they will be saved to your device");
            updatePoints.Add("- Several UI tweaks");
            updatePoints.Add("- Several adjustments to how quests work");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.4.2");
            updatePoints.Add("- Fix a backward compat bug causing some players to restart back to dungeon level 1. Also, attempt to recover their old dungeon depth.");
            updatePoints.Add("- Poison, Bleed and Burn no longer stack per hit. Only one of each can be active at a time on a character or player.");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.4.1");
            updatePoints.Add("- Critical bug fixes");
            updatePoints.Add("- Death penalty reduced to 5% gold");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.4");
            updatePoints.Add("- New boss encounters");
            updatePoints.Add("- New drake enemy type");
            updatePoints.Add("- New town starting location with merchants and quests");
            updatePoints.Add("- Many new magic properties for weapons including poison, bleeding, burning, gold drop, and magic drop");
            updatePoints.Add("- Town Portal is now available in the options menu");
            updatePoints.Add("- Added a variety of merchants that sell class specific gear");
            updatePoints.Add("- Added a teleporter NPC that can send you to any dungeon level you have been to before");
            updatePoints.Add("- Added quests that are given to you by NPC's and a main quest line (old characters may miss most of the main quest line, just roll a new one)");
            updatePoints.Add("- Lots of new magic properties for items");
            updatePoints.Add("- NPC's now have dialogue");
            updatePoints.Add("- Added support for doors which can be opened and closed");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.3.6");
            updatePoints.Add("- Critical bug fixes");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.3.5");
            updatePoints.Add("- Abilities can now be upgraded to level 6");
            updatePoints.Add("- New abilities Blizzard, Frozen Ground, Molten Earth, Cleave, Earthquake, Epic Strike, Precision, Poison Mastery, Shadow Mastery, Poison Trap, and Explosion Trap");
            updatePoints.Add("- Some new high level items added in");
            updatePoints.Add("- Add poison elementals and poison skeletons");
            updatePoints.Add("- Reduced the brightness of some of the tile sets");
            updatePoints.Add("- Fixed math issues in attack/cast speed and capped them at 200%");
            updatePoints.Add("- Other tweaks and bug fixes based on user feedback");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.3.2");
            updatePoints.Add("- Added an \"I'm stuck\" button that will regenerate a dungeon level in case people encounter bugs that make them stuck.");
            updatePoints.Add("- Other minor bug fixes");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.3.1");
            updatePoints.Add("- Fix a backward compat bug that allowed people to use abilities they had not learned which caused a crash.");
            updatePoints.Add("- Added new difficulty levels of Easy, Normal and Hard");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.3");
            updatePoints.Add("- Added player classes. You can pick your tier 1 class at level 5.");
            updatePoints.Add("- Add Frenzy, Stone Skin, Magic Arrow and Sprint abilities");
            updatePoints.Add("- Added icons on the left of the screen that display active effects such as potions");
            updatePoints.Add("- Added several new potion types");
            updatePoints.Add("- Refactored stats screen to give much more information");
            updatePoints.Add("- Added magic resistance property");
            updatePoints.Add("- New Blacksmith NPC that can enhance your items");
            updatePoints.Add("- New Enchanter NPC that can add magic properties to your items");
            updatePoints.Add("- Tons of new items that are more powerful and show up progressively as you get deeper in the dungeon");
            updatePoints.Add("- A stash where you can purchase space to store items");
            updatePoints.Add("- Enhanced landscape support");
            updatePoints.Add("- Lots of minor updates, bug fixes and balance tweaks");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.2.3");
            updatePoints.Add("- Fix but that causes enemies to deal double damage after being loaded");
            updatePoints.Add("- Fix bug that causes corpses to stand up when they die feared");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.2.2");
            updatePoints.Add("- Tweak center of player for landscape mode");
            updatePoints.Add("- Balance adjustments for game difficulty to make it a bit easier at the start");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.2.1");
            updatePoints.Add("- Upgrade to Mango in order to take advantage of fast app switching");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.2");
            updatePoints.Add("- Add champion creatures that are extra difficult but drop extra powerful loot and more XP");
            updatePoints.Add("- Brand new procedurally generated quest engine");
            updatePoints.Add("- Add Invisibility, Slow Time, Multi-shot, Poison Cloud, Frost Armor and Fear abilities");
            updatePoints.Add("- Improve load times and reduce memory footprint");
            updatePoints.Add("- Added option for landscape mode");
            updatePoints.Add("- New Goblin and Goblin Leader enemies");
            updatePoints.Add("- Ghost enemies now are invisible until they attack or are damaged");
            updatePoints.Add("- Make the merchant have better loot to sell");
            updatePoints.Add("- Tons of UI improvements");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.1.3");
            updatePoints.Add("- Fix a bug that causes levels past dungeon 10 to crash when loading");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.1.2");
            updatePoints.Add("- Fix string parsing issue that causes a crash at load time in non-English markets");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.1.1");
            updatePoints.Add("- Minor bug fixes");
            updatePoints.Add("-----------------------------------");
            updatePoints.Add("Alpha Version 0.1");
            updatePoints.Add("- The first look at a brand new action RPG that will rock the windows phone platform!");
            updatePoints.Add("- Dynamic dungeon generation");
            updatePoints.Add("- Diverse set of abilities to learn and specialize your character with");
            updatePoints.Add("- Dynamic item generation based on level including weapons, armor and consumables");
            updatePoints.Add("- Dynamic character generation based on level");
            updatePoints.Add("- Merchant system to buy and sell items");
            updatePoints.Add("- Automatic saving in the background");


            // add the scroll buttons
            arrowUp = DungeonGame.Instance.Content.Load<Texture2D>("Textures/UI/ArrowUp");
            MenuEntry entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Texture = arrowUp;
            entry.Position = new Vector2(DungeonGame.ScreenSize.Width / 2 + 180, textStartHeight);
            MenuEntries.Add(entry);

            entry = new MenuEntry("");
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            entry.Texture = arrowUp;
            entry.SpriteEffect = SpriteEffects.FlipVertically;
            entry.Position = new Vector2(DungeonGame.ScreenSize.Width / 2 + 180, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 80);
            MenuEntries.Add(entry);

            UpdatePositions();
        }


        public void UpdatePositions()
        {
            textStartHeight = DungeonGame.ScreenSize.Y + 120;
            textLinesToShow = (DungeonGame.ScreenSize.Height - 120) / 26;

            if (DungeonGame.pcMode)
            {
                textStartHeight += 90;
                textLinesToShow -= 3;
            }

            // expand the update notes into proper lines
            List<String> newList = new List<String>();
            for (int i = 0; i < updatePoints.Count; i++)
            {
                List<String> tempList = Fonts.BreakTextIntoList(updatePoints[i], Fonts.DescriptionFont, DungeonGame.ScreenSize.Width - 200);
                for (int j = 0; j < tempList.Count; j++)
                {
                    newList.Add(tempList[j]);
                }
                newList.Add("");
            }
            updatePointsSplit = newList;

            // add the scroll buttons
            MenuEntries[0].Position = new Vector2(DungeonGame.ScreenSize.Width - 80, textStartHeight);
            MenuEntries[1].Position = new Vector2(DungeonGame.ScreenSize.Width - 80, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 80);

        }


        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            if (currentTextStartLine < 0)
            {
                currentTextStartLine = 0;
            }
            else if (currentTextStartLine > updatePointsSplit.Count - textLinesToShow)
            {
                currentTextStartLine = updatePointsSplit.Count - textLinesToShow;
            }

            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Vector2 titlePosition = new Vector2(DungeonGame.ScreenSize.Width / 2, DungeonGame.ScreenSize.Y + 70);
            if (DungeonGame.pcMode)
            {
                titlePosition.Y += 90;
            }
            Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, "Update Notes", titlePosition, Color.Red);

            Vector2 position = new Vector2(100, textStartHeight);
            float lineHeight = Fonts.DescriptionFont.MeasureString(updatePointsSplit[0]).Y;

            for (int i = currentTextStartLine; i < updatePointsSplit.Count && i < currentTextStartLine + textLinesToShow; i++)
            {
                spriteBatch.DrawString(Fonts.DescriptionFont, updatePointsSplit[i], position, Color.White);
                position.Y += lineHeight;
            }

            spriteBatch.End();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            UpdatePositions();
        }


        public override void HandleInput()
        {
            base.HandleInput();

            if (InputManager.GetKeyboardAction(keyboardActionSet.Down))
            {
                if (currentTextStartLine < updatePoints.Count - textLinesToShow)
                {
                    currentTextStartLine++;
                }
            }
            else if (InputManager.GetKeyboardAction(keyboardActionSet.Up))
            {
                if (currentTextStartLine > 0)
                {
                    currentTextStartLine--;
                }
            }
        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (selectedEntry == 0)
            {
                // scroll up
                if (currentTextStartLine > 0)
                {
                    currentTextStartLine--;
                }
            }
            else
            {
                // scroll down
                if (currentTextStartLine < updatePointsSplit.Count - textLinesToShow)
                {
                    currentTextStartLine++;
                }
            }
        }
    }
}
