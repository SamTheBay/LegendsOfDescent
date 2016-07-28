using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LegendsOfDescent
{
    class EnchanterSprite : DialogueNPCSprite, IEnvItem, ISaveable
    {

        DateTime lastRefreshTime = DateTime.Now;
        int refreshMinInterval = 5;
        List<Property> properties = new List<Property>();
        int activeProperty = 0;
        static Property[] validProperties = {
            Property.Health,
            Property.Mana,
            Property.MoveSpeed,
            Property.AttackSpeed,
            Property.CastSpeed,
            Property.FireDamage,
            Property.ColdDamage,
            Property.LightningDamage,
            Property.PoisonDamage,
            Property.BurnDamage,
            Property.BleedDamage,
            Property.SpellDamage,
            Property.HealthRegen,
            Property.ManaRegen,
            Property.CriticalHitChance,
            Property.MagicResistance,
            Property.GoldDrop,
            Property.MagicDrop};

        public EnchanterSprite(Vector2 nPosition, PlayerSprite player, int level)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2), player)
        {
            Name = "Enchanter";
            text = "Magic is in everything. All you need to do is learn how to manipulate it. Or, I can do it for you... for a price.";
            buttonText[0] = "Let's See";
            buttonText[1] = "Not now";
            hasDialogueAction = true;
            this.level = level;
            centeredReduce = 60;

            AddTexture("NPCMageRed", new Point(128, 128), 1);
            AddAnimationSet("Idle", 1, 0, 100, false);

            InitializeTextures();
            PlayAnimation("IdleRight");

            SelectProperties();

            Activate();
        }


        public void SelectProperties()
        {
            properties.Clear();
            for (int i = 0; i < 3; i++)
            {
                Property pick = validProperties[Util.Random.Next(0, validProperties.Length)];
                bool isUnique = true;
                for (int j = 0; j < properties.Count; j++)
                {
                    if (pick == properties[j])
                    {
                        i--;
                        isUnique = false;
                        break;
                    }
                }

                if (isUnique)
                {
                    properties.Add(pick);
                }
            }
        }


        public override void Refresh()
        {
            base.Refresh();

            TimeSpan difference = DateTime.Now - lastRefreshTime;
            if (Math.Abs(difference.TotalMinutes) > refreshMinInterval)
            {
                SelectProperties();
                lastRefreshTime = DateTime.Now;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            lastDirection = GetDirectionFromVector(SaveGameManager.CurrentPlayer.CenteredPosition - CenteredPosition);
            PlayAnimation("Idle", lastDirection);
        }



        public override void DialogueAction(int buttonSelected)
        {
            if (buttonSelected == 0)
            {
                GameplayScreen.Instance.ScreenManager.AddScreen(new EnchanterScreen(player, this));
                InputManager.ClearInputForPeriod(500);
            }
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write((UInt32)maxHealthBase);
            writer.Write((UInt32)maxManaBase);
            writer.Write((UInt32)level);
            writer.Write(Position);

            // save enchantment types
            writer.Write(properties.Count);
            for (int i = 0; i < properties.Count; i++)
            {
                writer.Write((Int32)properties[i]);
            }

            writer.Write(lastRefreshTime);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            maxHealthBase = reader.ReadInt32();
            maxManaBase = reader.ReadInt32();
            level = reader.ReadInt32();
            Position = reader.ReadVector2();

            // load enchantment types
            properties.Clear();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                properties.Add((Property)reader.ReadInt32());
            }

            if (dataVersion >= 7)
            {
                // TODO: remove try catch before publishing. here to allow backward compat during testing.
                try
                {
                    lastRefreshTime = reader.ReadDateTime().ToLocalTime();
                }
                catch { }
            }

            return true;
        }



        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            base.Draw(spriteBatch, position, segment, spriteEffect);

            // draw the blacksmiths name
            nameSprite.Color = friendlyNPCColor;
            nameSprite.Position = position + new Vector2(FrameDimensions.X / 2, 0) + GameplayScreen.viewportCorner;
            nameSprite.Draw(spriteBatch);

        }


        public void MoveActiveProperty(bool right)
        {
            if (right)
            {
                activeProperty++;
                activeProperty %= properties.Count;
            }
            else
            {
                activeProperty--;
                if (activeProperty < 0)
                {
                    activeProperty = properties.Count - 1;
                }
            }

        }


        public Property ActiveProperty
        {
            get { return properties[activeProperty]; }
        }

    }
}
