using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace LegendsOfDescent
{
    class StatsScreen : PopUpScreen
    {
        PopUpScreen owner;
        PlayerSprite player;
        HelpButton helpButton;

        public StatsScreen(PopUpScreen owner, PlayerSprite player, Rectangle dimension)
            : base()
        {
            this.dimension = dimension;
            IsPopup = true;
            this.owner = owner;
            this.player = player;
            helpButton = new HelpButton(HelpScreens.Statistics, Vector2.Zero);

            Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 170, 450, 50);
            MenuEntry entry = new MenuEntry("Upgrade Class", location);
            entry.Selected += new EventHandler<EventArgs>(entry_Selected);
            if (player.Level < PlayerClassManager.Instance.GetTierUpgradelevel(player.PlayerClass.tier + 1))
            {
                entry.IsActive = false;
            }
            MenuEntries.Add(entry);

            UpdateOrientation();
        }


        void entry_Selected(object sender, EventArgs e)
        {
            if (player.Level >= PlayerClassManager.Instance.GetTierUpgradelevel(player.PlayerClass.tier + 1) || DungeonGame.TestMode)
            {
                ScreenManager.AddScreen(new ClassUpgradeScreen(player));
            }
        }

        public void UpdateOrientation()
        {
            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, 80 + borderWidth, 480 - borderWidth * 2, 720 - 80 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(0, 65);

                Rectangle location = new Rectangle(DungeonGame.ScreenSize.Width / 2 - 450 / 2, DungeonGame.ScreenSize.Height + DungeonGame.ScreenSize.Y - 145, 450, 50);
                MenuEntries[0].Location = location;

                helpButton.Location = new Vector2(WindowCorner.X + 400, WindowCorner.Y + 5);
            }
            else
            {
                int borderWidth = 6;
                dimension = new Rectangle(borderWidth, borderWidth, 800 - 80 - borderWidth * 2 - 50, 400 - borderWidth * 2);
                WindowCorner = owner.WindowCorner;
                MoveWindowCorner(60, 0);

                Rectangle location = new Rectangle(dimension.Width / 2 - 450 / 2 + (int)WindowCorner.X, (int)WindowCorner.Y + dimension.Height - 90, 450, 75);
                MenuEntries[0].Location = location;

                helpButton.Location = new Vector2(WindowCorner.X + dimension.Width - 60, WindowCorner.Y + 20);
            }
        }



        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            UpdateOrientation();

            if (player.Level >= PlayerClassManager.Instance.GetTierUpgradelevel(player.PlayerClass.tier + 1))
            {
                MenuEntries[0].IsActive = true;
            }
            else
            {
                MenuEntries[0].IsActive = false;
            }

            SpriteBatch spriteBatch = DungeonGame.ScreenManager.SpriteBatch;
            spriteBatch.Begin();

            Color fontColor = Color.White;
            Vector2 offset = new Vector2(10, 35) + WindowCorner;
            SpriteFont font = Fonts.DescriptionFont;
            int textHeight = (int)font.MeasureString("X").Y;

            if (DungeonGame.portraitMode && !DungeonGame.pcMode)
            {
                int leftSideOffset = 30 + (int)WindowCorner.X;
                int centerOffset = 270 + (int)WindowCorner.X;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, player.Name, new Vector2(dimension.Width / 2, offset.Y), Color.Red);
                offset.Y += 30;
                spriteBatch.DrawString(font, "Level: " + player.Level.ToString(), new Vector2(leftSideOffset, offset.Y), fontColor);
                spriteBatch.DrawString(font, "Dungeon: " + player.DungeonLevel.ToString(), new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Class: " + player.PlayerClass.name, new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "XP: " + player.Experience.ToString() + "/" + player.NextLevelExp.ToString(), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight * 2;
                spriteBatch.DrawString(font, "Damage: " + player.TotalAttack.ToString("F1"), new Vector2(leftSideOffset, offset.Y), fontColor);
                spriteBatch.DrawString(font, "Armor: " + player.Armor.ToString(), new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Spell Damage: +" + player.SpellDamage, new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Critical Hit Chance: " + (player.CriticalHitChance * 100).ToString() + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Magic Resistance: " + (player.MagicResistance * 100).ToString() + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Movement Speed: " + (player.MoveSpeedAdjust * 100).ToString() + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Attack Speed: " + ((player.NonBaseAttackSpeedAdjust * 100) + 100).ToString() + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Cast Speed: " + ((player.CastSpeedAdjust * 100) + 100).ToString() + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana Cost: " + ((player.GetBaseManaCostAdjust() * 100)).ToString("F0") + "%", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight * 2;

                spriteBatch.DrawString(font, "Health: " + player.Health.ToString("F0") + "/" + player.MaxHealth.ToString("F0"), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Health Regen: " + player.HealthRegen.ToString("F0") + " / 5 seconds", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana: " + player.Mana.ToString("F0") + "/" + player.MaxMana.ToString("F0"), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana Regen: " + player.ManaRegen.ToString("F0") + " / 5 seconds", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
            }
            else
            {
                int leftSideOffset = 30 + (int)WindowCorner.X;
                int centerOffset = 330 + (int)WindowCorner.X;

                Fonts.DrawCenteredText(spriteBatch, Fonts.DCFont, player.Name, new Vector2(dimension.Width / 2 + WindowCorner.X, offset.Y), Color.Red);
                offset.Y += 40;
                spriteBatch.DrawString(font, "Level: " + player.Level.ToString(), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Dungeon: " + player.DungeonLevel.ToString(), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Class: " + player.PlayerClass.name, new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "XP: " + player.Experience.ToString() + "/" + player.NextLevelExp.ToString(), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Health: " + player.Health.ToString("F0") + "/" + player.MaxHealth.ToString("F0"), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Health Regen: " + player.HealthRegen.ToString("F0") + " / 5 sec", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana: " + player.Mana.ToString("F0") + "/" + player.MaxMana.ToString("F0"), new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana Regen: " + player.ManaRegen.ToString("F0") + " / 5 sec", new Vector2(leftSideOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                offset = new Vector2(10, 65) + WindowCorner;

                spriteBatch.DrawString(font, "Damage: " + player.TotalAttack.ToString("F1"), new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Armor: " + player.Armor.ToString(), new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Spell Damage: +" + player.SpellDamage, new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Magic Resistance: " + (player.MagicResistance * 100).ToString() + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Critical Hit Chance: " + (player.CriticalHitChance * 100).ToString() + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Movement Speed: " + (player.MoveSpeedAdjust * 100).ToString() + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Attack Speed: " + ((player.NonBaseAttackSpeedAdjust * 100) + 100).ToString() + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Cast Speed: " + ((player.CastSpeedAdjust * 100) + 100).ToString() + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;
                spriteBatch.DrawString(font, "Mana Cost: " + ((player.GetBaseManaCostAdjust() * 100)).ToString("F0") + "%", new Vector2(centerOffset, offset.Y), fontColor);
                offset.Y += textHeight;


            }
            

            helpButton.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }


        public override void HandleInput()
        {
            base.HandleInput();

            helpButton.HandleInput();
        }
    }
}
