using System;
using System.Collections.Generic;
using System.IO;
using LegendsOfDescent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using LegendsOfDescent.Quests;

namespace LegendsOfDescent
{
    public class PlayerSprite : CharacterSprite, ISaveable
    {
        public static int loadedInstance = 0;
        public int SaveIndex { get; set; }

        const float baseDamagePercent = .1f;

        Guid id;

        // Weapon sprites for use
        Ability[] abilities;
        Ability activeAbility;
        PlayerClass playerClass;

        GameDifficulty difficulty = GameDifficulty.normal;
        int exp = 0;
        int nextLevelExp = BalanceManager.GetExperienceToLevel(1);
        int prevLevelExp = 0;
        int gold = 0;
        int abilityPoints = 1;
        int dungeonLevel = 0;
        int dungeonLevelsGenerated = -1;
        float baseAttackSpeed = 30;
        float healAmount = 0;
        float healSpeed = 0;
        float addManaAmount = 0;
        float addManaSpeed = 0;
        DateTime lastUnstuckTime = DateTime.Now - TimeSpan.FromHours(24);

        // inventory
        ItemSprite[,] items = new ItemSprite[6, 4];
        EquipableItem[,] equippedItems = new EquipableItem[ItemSprite.EquipSlotNum, 1];
        ItemSprite[][,] stash = new ItemSprite[5][,];
        int stashesUnlocked = 1;
        int[] stashUnlcokCost = { 0, 1000, 5000, 10000, 50000, 100000 };
        EquipableItem swapItem = null;
        EquipableItem swapOffhandItem = null;
        MacroButton[] macroButtons = new MacroButton[UserInterface.maxNumMacroButtons];

        // regeneration
        int regenDuration = 5000;
        int regenElapsed = 0;

        // town portal variables
        TownPortal townPortal = new TownPortal(Vector2.Zero);

        public QuestLog QuestLog { get { return questLog; } }
        private QuestLog questLog;

        StatsManager statsManager = new StatsManager();

        public PlayerSprite(Vector2 nPosition, bool resetHelpManager = true)
            : base(nPosition, new Point(128 / 2, 128 / 2), new Vector2(128 / 2, 128 / 2))
        {
            // add in other textures
            AddTexture("DwarfWalking", new Point(128, 128), 12);
            AddTexture("DwarfAttack", new Point(128, 128), 12);
            AddTexture("DwarfDie", new Point(128, 128), 12);
            AddTexture("DwarfWalkingSword", new Point(128, 128), 12);
            AddTexture("DwarfAttackSword", new Point(128, 128), 12);
            AddTexture("DwarfWalkingBow", new Point(128, 128), 12);
            AddTexture("DwarfAttackBow", new Point(128, 128), 12);

            int dieFrameSpeed = 70;


            AddAnimationSet("Idle", 12, 1, 100, false, 1);
            AddAnimationSet("Walk", 12, 0, 60, true);
            AddAnimationSet("Attack", 12, 1, 50, false);
            AddAnimationSet("Die", 12, 2, dieFrameSpeed, false);
            AddAnimationSet("Idle", 12, 4, 100, false, 1, postString: "Sword");
            AddAnimationSet("Walk", 12, 3, 60, true, postString: "Sword");
            AddAnimationSet("Attack", 12, 4, 40, false, postString: "Sword");
            AddAnimationSet("Idle", 12, 6, 100, false, 1, postString: "Bow");
            AddAnimationSet("Walk", 12, 5, 60, true, postString: "Bow");
            AddAnimationSet("Attack", 12, 6, 40, false, postString: "Bow");


            PlayAnimation("IdleRight");
            Activate();
            centeredReduce = 60;
            Name = "Default";
            id = Guid.NewGuid();

            abilities = Ability.GetAllAbilities(this);
            activeAbility = abilities[0];
            playerClass = PlayerClassManager.Instance.GetClass(PlayerClassType.Commoner);
            health = MaxHealth;
            mana = MaxMana;

            for (int i = 0; i < macroButtons.Length; i++)
            {
                macroButtons[i] = MacroButton.emptyMacro;
            }
            MacroButton macro = new MacroButton(type: MacroButtonType.TownPortal, player: this);
            macro.SetIcon("TownButton", new Rectangle(0, 0, 48, 48));
            SetMacroButton(macro, UserInterface.numMacroButtons - 1);
            macro = new MacroButton(type: MacroButtonType.Swap, player: this);
            macro.SetIcon("Swap", new Rectangle(0, 0, 48, 48));
            SetMacroButton(macro, UserInterface.numMacroButtons - 2);

            // initialize stash
            for (int i = 0; i < stash.Length; i++)
            {
                stash[i] = new ItemSprite[5, 3];
            }

            if (DungeonGame.TestMode == true)
            {
                AddTestData();
            }

            // reset the help screen manager in case an old one is there
            if (resetHelpManager)
                HelpScreenManager.Instance = new HelpScreenManager();

            // add starting items
            ItemSprite item = ItemManager.Instance.GetItem("Sword", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Short Bow", 1);
            AddItem(item);
            macroButtons[0] = item.GetMacroButton(this);
            item = ItemManager.Instance.GetItem("Wand", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Leather Armor", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Leather Pants", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Leather Hat", 1);
            AddItem(item);
            for (int i = 0; i < 5; i++)
            {
                item = ItemManager.Instance.GetItem("Health Potion", 1);
                AddItem(item);
            }
            for (int i = 0; i < 5; i++)
            {
                item = ItemManager.Instance.GetItem("Mana Potion", 1);
                AddItem(item);
            }
            for (int i = 0; i < 5; i++)
            {
                item = ItemManager.Instance.GetItem("Warp Scroll", 1);
                AddItem(item);
            }

            baseMovementSpeed = 9f;

            questLog = new QuestLog();
        }


        public void AddTestData()
        {
            abilityPoints = 100000;
            gold = 10000000;

            ItemSprite item = ItemManager.Instance.GetItem("War Hammer", 100);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Crossbow", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Tome", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Small Shield", 1);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 5);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 9);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 13);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 17);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 21);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 25);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 29);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Mana Potion", 33);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Health Potion", 6);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Growth Potion", 11);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Swift Potion", 100);
            AddItem(item);
            item = ItemManager.Instance.GetItem("Stone Skin Potion", 21);
            AddItem(item);


            // put one of every item in the stash to play with
            List<ItemSprite> items = ItemManager.Instance.GenerateAllItems(1);
            int index = 30;
            for (int i = 0; i < stash.Length && index < items.Count; i++)
            {
                for (int j = 0; j < stash[i].GetLength(0) && index < items.Count; j++)
                {
                    for (int k = 0; k < stash[i].GetLength(1) && index < items.Count; k++)
                    {
                        stash[i][j, k] = items[index];
                        items[index].Slot = new Point(j, k);
                        index++;
                    }
                }
            }
        }



        private string GetWeaponAnimString()
        {
            if (equippedItems[(int)EquipSlot.Hand, 0] == null)
            {
                return "";
            }
            else if (equippedItems[(int)EquipSlot.Hand, 0].IsRanged == false)
            {
                return "Sword";
            }
            else
            {
                return "Bow";
            }
        }



        Vector2 movement = new Vector2();
        Vector2[] oldPositions = new Vector2[2];
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // update abilities
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].Update(gameTime);
            }


            if (healAmount > 0)
            {
                float currentAmount = healSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (currentAmount > healAmount)
                    currentAmount = healAmount;
                health += currentAmount;
                if (health > MaxHealth)
                    health = MaxHealth;
                healAmount -= currentAmount;
            }

            if (addManaAmount > 0)
            {
                float currentAmount = addManaSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (currentAmount > addManaAmount)
                    currentAmount = addManaAmount;
                mana += currentAmount;
                if (mana > MaxMana)
                    mana = MaxMana;
                addManaAmount -= currentAmount;
            }

            if (health > MaxHealth)
                health = MaxHealth;
            if (mana > MaxMana)
                mana = MaxMana;

            if (!isDead && !GameplayScreen.Instance.IsPaused)
            {
                regenElapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (regenElapsed >= regenDuration)
                {
                    regenElapsed = 0;
                    base.Heal(HealthRegen);
                    base.AddMana(ManaRegen);
                }
            }

            if (!atDestination && (!currentAnimation.Name.Contains("Attack") || IsPlaybackComplete))
            {
                // move the character towards their current destination
                // determine how to move
                movement.X = currentDestination.X - Position.X;
                movement.Y = currentDestination.Y - Position.Y;

                // normalize vector
                float distance = (float)Math.Sqrt((float)MathExt.Square(movement.X) + (float)MathExt.Square(movement.Y));
                if (distance > MovementSpeed)
                {
                    movement.X /= distance;
                    movement.Y /= distance;

                    movement.X *= MovementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;
                    movement.Y *= MovementSpeed * (float)gameTime.ElapsedGameTime.Milliseconds / 33.33f;
                }
                else
                {
                    // we have arrived at the current destination
                    atDestination = true;
                    GameplayScreen.Instance.PlayerAtDestination();
                }

                // get the angle to determine the direction
                Direction direction = GetDirectionFromVector(movement);

                // check for collisions with the environment
                bool canMove = CanMove(ref movement, GameplayScreen.Dungeon);

                // update position
                if (movement.Length() > 1f)
                {
                    MovePosition(movement.X, movement.Y);
                    if (!atDestination)
                    {
                        PlayAnimation("Walk", direction);
                        lastDirection = direction;
                    }
                    else
                    {
                        PlayAnimation("Idle", direction);
                        lastDirection = direction;
                    }

                    // check old positions to make sure we aren't jittering
                    if (Vector2.Distance(oldPositions[1], CenteredPosition) < 1)
                    {
                        PlayAnimation("Idle", direction);
                        lastDirection = direction;
                        atDestination = true;
                        GameplayScreen.Instance.PlayerAtDestination();
                    }
                    oldPositions[1] = oldPositions[0];
                    oldPositions[0] = CenteredPosition;

                }
                else
                {
                    PlayAnimation("Idle", direction);
                    lastDirection = direction;
                    atDestination = true;
                    GameplayScreen.Instance.PlayerAtDestination();
                }

            }

        }


        public override void Die(CharacterSprite killer)
        {
            base.Die(killer);
            StopMoving();

            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].ResetOnDeath();
            }

            EndInvisible();
            GameplayScreen.Instance.EndSlowTime();

            PlayAnimation("Die", lastDirection);

            statsManager.Add(StatType.TotalDeaths, 1);
        }


        public void Resurrect(Vector2 location)
        {
            isDead = false;
            Position = location;
            health = MaxHealth;
            mana = MaxMana;
            PlayAnimation("Idle", lastDirection);

            // pay death penalty
            gold -= gold / 20;
            activeTimedEffects.Clear();
        }

        public void SetDestination(Vector2 destination, bool centerPosition = true)
        {
            if (destination != currentDestination)
            {
                if (centerPosition)
                {
                    destination.X -= FrameDimensions.X / 2;
                    destination.Y -= FrameDimensions.Y / 2;
                }
                currentDestination = destination;
                atDestination = false;
            }
        }


        public void StopMoving()
        {
            if (!atDestination)
            {
                atDestination = true;
                if (!IsAttacking)
                {
                    PlayAnimation("Idle", lastDirection);
                }
            }
        }


        public void UseActiveAbility(Vector2 direction)
        {
            if (activeAbility.Activate(direction) && (!currentAnimation.Name.Contains("Attack") || IsPlaybackComplete))
            {
                lastDirection = GetDirectionFromVector(direction - CenteredPosition);
                PlayAnimation("Attack", lastDirection);
                ResetAnimation();
            }
        }



        public void SetAttackAnimationInterval(float speedAdjust)
        {
            for (int i = 0; i < animations.Count; i++)
            {
                if (animations[i].Name.Contains("Attack"))
                {
                    animations[i].Interval = (int)(baseAttackSpeed / (speedAdjust + 1.1f));
                }
            }
        }



        public override void Draw(SpriteBatch spriteBatch, Vector2 position, int segment, SpriteEffects spriteEffect)
        {
            // set tinting based on abilities
            StoneSkinAbility stoneSkin = (StoneSkinAbility)abilities[(int)AbilityType.StoneSkin];
            if (IsInvisible)
                permaTint = Color.Black;
            else if (stoneSkin.IsActive())
                permaTint = Color.Gray;
            else if (IsPoisoned)
                permaTint = Color.Green;
            else if (IsFeared)
                permaTint = Color.Red;
            else if (IsSlowed)
                permaTint = Color.LightBlue;
            else
                permaTint = Color.White;

            base.Draw(spriteBatch, position, 3, spriteEffect);

            if (DungeonGame.TestMode)
            {
                DrawBoundingBox(spriteBatch);
            }

            // TODO: add these abilities into the map so that they don't have to be drawn here
            for (int i = 0; i < abilities.Length; i++)
            {
                abilities[i].Draw(spriteBatch);
            }
        }


        public bool AddItem(ItemSprite item, bool forceNoAutoEquip = false)
        {
            // if it is gold add it to our stash
            if (item is GoldItem)
            {
                GoldItem goldItem = (GoldItem)item;
                AddGold(goldItem.Gold);
                return true;
            }

            // if it is equippable and we have nothing equipped, then put it on
            if (!forceNoAutoEquip)
            {
                EquipableItem eq = item as EquipableItem;
                if (eq != null && equippedItems[(int)(eq.BasicEquipSlot), 0] == null)
                {
                    if ((eq.EquipSlot != EquipSlot.TwoHand || equippedItems[(int)EquipSlot.OffHand, 0] == null) &&
                        (eq.EquipSlot != EquipSlot.OffHand || equippedItems[(int)EquipSlot.Hand, 0] == null || equippedItems[(int)EquipSlot.Hand, 0].EquipSlot != EquipSlot.TwoHand))
                    {
                        SetItem(eq, new Point(-1, (int)eq.BasicEquipSlot));
                        return true;
                    }
                }
            }

            // try and add it to a stack
            if (item is UsableItem)
            {
                UsableItem usable = item as UsableItem;
                for (int x = 0; x < items.GetLength(0); x++)
                {
                    for (int y = 0; y < items.GetLength(1); y++)
                    {
                        if (items[x, y] is UsableItem)
                        {
                            bool combined = ((UsableItem)items[x, y]).CombineStacks(usable);
                            if (combined && usable.IsStackEmpty)
                                return true;
                        }
                    }
                }
            }


            for (int x = 0; x < items.GetLength(0); x++)
            {
                for (int y = 0; y < items.GetLength(1); y++)
                {
                    if (items[x, y] == null)
                    {
                        item.SetSlot(x, y);
                        items[x, y] = item;
                        item.Owner = this;
                        return true;
                    }
                }
            }

            return false;
        }


        public ItemSprite GetItem(Point slot)
        {
            if (slot.X == -1)
            {
                return equippedItems[slot.Y, 0];
            }
            else
            {
                return items[slot.X, slot.Y];
            }
        }


        public EquipableItem GetEquippedItem(EquipSlot slot)
        {
            return equippedItems[(int)slot, 0];
        }


        public ItemSprite SetItem(ItemSprite item, Point slot)
        {
            ItemSprite displacedItem = null;

            if (!IsValidSlot(item, slot))
                throw new Exception();

            if (item != null)
            {
                item.Slot = slot;
                item.Owner = this;
            }

            if (slot.X == -1)
            {
                EquipableItem equipableItem = (EquipableItem)item;
                equippedItems[slot.Y, 0] = equipableItem;

                if (equipableItem != null && equipableItem.IsSelectable())
                {
                    equipableItem.Select();
                }


                if (slot.Y == (int)EquipSlot.Hand)
                {
                    RefreshEquippedAnimation(equipableItem);
                }
                if (equipableItem != null && equipableItem.EquipSlot == EquipSlot.TwoHand && equippedItems[(int)EquipSlot.OffHand, 0] != null)
                {
                    // need to un-equip the offhand
                    displacedItem = equippedItems[(int)EquipSlot.OffHand, 0];
                    equippedItems[(int)EquipSlot.OffHand, 0] = null;
                }
                if (equipableItem != null && slot.Y == (int)EquipSlot.OffHand && equippedItems[(int)EquipSlot.Hand, 0] != null && equippedItems[(int)EquipSlot.Hand, 0].EquipSlot == EquipSlot.TwoHand)
                {
                    // need to un-equip the two hand in our main
                    displacedItem = equippedItems[(int)EquipSlot.Hand, 0];
                    equippedItems[(int)EquipSlot.Hand, 0] = null;

                    RefreshEquippedAnimation(equippedItems[(int)EquipSlot.Hand, 0]);
                }
            }
            else if (slot.X == -2)
            {
                swapItem = (EquipableItem)item;
            }
            else if (slot.X == -3)
            {
                swapOffhandItem = (EquipableItem)item;
            }
            else
            {
                items[slot.X, slot.Y] = item;
            }

            if (health > MaxHealth)
                health = MaxHealth;
            if (mana > MaxMana)
                mana = MaxMana;

            return displacedItem;
        }


        protected void RefreshEquippedAnimation(EquipableItem equipableItem)
        {
            if (currentAnimation.Name.Contains("Idle"))
            {
                PlayAnimation("Idle" + lastDirection.ToString());
            }
            else if (currentAnimation.Name.Contains("Walk"))
            {
                PlayAnimation("Walk" + lastDirection.ToString());
            }

            if (equipableItem == null)
            {
                // unarmed needs to count as a melee weapon
                SetActiveAbility(AbilityType.Melee);
            }
        }


        public void SwapItem()
        {
            EquipableItem item = equippedItems[(int)EquipSlot.Hand, 0];
            EquipableItem item2 = equippedItems[(int)EquipSlot.OffHand, 0];
            SetItem( swapItem, new Point(-1, (int)EquipSlot.Hand));
            SetItem(swapOffhandItem, new Point(-1, (int)EquipSlot.OffHand));
            SetItem(item, new Point(-2, 0));
            SetItem(item2, new Point(-3, 0));
        }


        public override void Damage(List<float> damage, List<DamageType> damageType, CharacterSprite attacker, float extraAdjust = 1, bool autoCrit = false, bool allowReflect = true)
        {
            base.Damage(damage, damageType, attacker, extraAdjust, autoCrit, allowReflect);

            if (health < MaxHealth / 2 || mana < MaxMana / 2)
            {
                HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.Potions);
            }

            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Starburst, color: Color.Red, numParticlesScale: 0.1f);
        }


        public void ResetAbilities()
        {
            for (int i = 0; i < abilities.Length; i++)
            {
                abilityPoints += abilities[i].RefundAbilityPoints();
            }
            for (int i = 0; i < macroButtons.Length; i++)
            {
                macroButtons[i].ValidateMacro();
            }
        }


        public bool IsValidSlot(ItemSprite item, Point slot)
        {
            if (item != null && slot.X == -1)
            {
                if (!(item is EquipableItem))
                {
                    return false;
                }
                else
                {
                    EquipableItem equipable = (EquipableItem)item;
                    if (equipable.EquipSlot == EquipSlot.Hand &&
                        (slot.Y == (int)EquipSlot.Hand || slot.Y == (int)EquipSlot.OffHand))
                    {
                        // hand items can go in hand slot or offhand slot
                        return true;
                    }
                    else if (equipable.EquipSlot == EquipSlot.MainHand && slot.Y == (int)EquipSlot.Hand)
                    {
                        // main hand items must go in hand slot
                        return true;
                    }
                    else if (equipable.EquipSlot == EquipSlot.TwoHand && slot.Y == (int)EquipSlot.Hand)
                    {
                        // two hand items can go in hand slot
                        return true;
                    }
                    else if (slot.Y != (int)equipable.EquipSlot)
                    {
                        return false;
                    }
                }
            }

            return true;
        }


        public virtual void Persist(BinaryWriter writer)
        {
            // write basic info
            writer.Write(Name);
            writer.Write((UInt32)gold);
            writer.Write((UInt32)exp);
            writer.Write((UInt32)nextLevelExp);
            writer.Write((UInt32)prevLevelExp);
            writer.Write((UInt32)maxHealthBase);
            writer.Write((UInt32)maxManaBase);
            writer.Write((UInt32)level);
            writer.Write(abilityPoints);
            writer.Write((Int32)dungeonLevel);
            writer.Write((Int32)dungeonLevelsGenerated);
            writer.Write(Position);

            // write ability info
            writer.Write((Int32)abilities.Length);
            for (int i = 0; i < abilities.Length; i++)
            {
                writer.Write((Byte)abilities[i].Level);
            }

            // count the number of items we have
            int itemNum = 0;
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i, j] != null)
                    {
                        itemNum++;
                    }
                }
            }
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i, 0] != null)
                {
                    itemNum++;
                }
            }
            if (swapItem != null)
                itemNum++;
            if (swapOffhandItem != null)
                itemNum++;

            // write item info
            writer.Write((Int32)itemNum);
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i, j] != null)
                    {
                        items[i, j].Persist(writer);
                    }
                }
            }
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i, 0] != null)
                {
                    equippedItems[i, 0].Persist(writer);
                }
            }
            if (swapItem != null)
                swapItem.Persist(writer);
            if (swapOffhandItem != null)
                swapOffhandItem.Persist(writer);

            // write stashes
            for (int x = 0; x < stash.Length; x++)
            {
                itemNum = 0;

                for (int i = 0; i < stash[x].GetLength(0); i++)
                {
                    for (int j = 0; j < stash[x].GetLength(1); j++)
                    {
                        if (stash[x][i, j] != null)
                        {
                            itemNum++;
                        }
                    }
                }
                writer.Write((Int32)itemNum);
                for (int i = 0; i < stash[x].GetLength(0); i++)
                {
                    for (int j = 0; j < stash[x].GetLength(1); j++)
                    {
                        if (stash[x][i, j] != null)
                        {
                            stash[x][i, j].Persist(writer);
                        }
                    }
                }
            }
            writer.Write((Int32)stashesUnlocked);

            // write the macro buttons info
            for (int i = 0; i < macroButtons.Length; i++)
            {
                macroButtons[i].Persist(writer);
            }

            writer.Write((Int32)health);
            writer.Write((Int32)mana);

            // write ability charge info
            writer.Write((Int32)abilities.Length);
            for (int i = 0; i < abilities.Length; i++)
            {
                writer.Write((Int32)abilities[i].Elapsed);
            }

            // save your class
            writer.Write((Int32)playerClass.type);

            // save game difficulty
            writer.Write((Int16)difficulty);

            writer.Write(questLog);

            townPortal.Persist(writer);

            writer.Write(id);

            writer.Write((Int32)activeAbility.AbilityType);

            writer.Write(LastUnstuckTime);

            statsManager.Persist(writer);
        }


        public virtual bool Load(BinaryReader reader, int dataVersion)
        {
            // read basic info
            Name = reader.ReadString();
            gold = reader.ReadInt32();
            exp = reader.ReadInt32();
            nextLevelExp = reader.ReadInt32();
            prevLevelExp = reader.ReadInt32();
            maxHealthBase = reader.ReadInt32();
            maxManaBase = reader.ReadInt32();
            level = reader.ReadInt32();
            abilityPoints = reader.ReadInt32();
            dungeonLevel = reader.ReadInt32();
            dungeonLevelsGenerated = reader.ReadInt32();
            Position = reader.ReadVector2();

            // read ability info
            int abilityNum = reader.ReadInt32();
            for (int i = 0; i < abilityNum; i++)
            {
                abilities[i].Level = reader.ReadByte();
            }

            // clear out old items on this player
            items = new ItemSprite[6, 4];
            equippedItems = new EquipableItem[ItemSprite.EquipSlotNum, 1];

            // read items
            int itemNum = reader.ReadInt32();
            for (int i = 0; i < itemNum; i++)
            {
                ItemSprite item = ItemSprite.LoadItem(reader, dataVersion);
                SetItem(item, item.Slot);
            }

            // read out stash
            if (dataVersion >= 3)
            {
                for (int x = 0; x < stash.Length; x++)
                {
                    int stashItemNum = reader.ReadInt32();
                    for (int i = 0; i < stashItemNum; i++)
                    {
                        ItemSprite item = ItemSprite.LoadItem(reader, dataVersion);
                        stash[x][item.Slot.X, item.Slot.Y] = item;
                        item.Owner = this;
                    }
                }
                stashesUnlocked = reader.ReadInt32();
            }

            // read the macro buttons info
            for (int i = 0; i < macroButtons.Length; i++)
            {
                MacroButton button = new MacroButton();
                button.Load(reader, dataVersion);
                macroButtons[i] = button;
            }

            if (dataVersion >= 3)
            {
                health = reader.ReadInt32();
                if (health > MaxHealth)
                    health = MaxHealth;
                mana = reader.ReadInt32();
                if (mana > MaxMana)
                    mana = MaxMana;

                // read ability charge time
                abilityNum = reader.ReadInt32();
                for (int i = 0; i < abilityNum; i++)
                {
                    abilities[i].Elapsed = reader.ReadInt32();
                }

                PlayerClassType type = (PlayerClassType)reader.ReadInt32();
                playerClass = PlayerClassManager.Instance.GetClass(type);
            }
            if (dataVersion < 5)
            {
                // refund ability points 
                ResetAbilities();
            }

            if (dataVersion >= 4)
            {
                difficulty = (GameDifficulty)reader.ReadInt16();
            }

            if (dataVersion >= 7)
            {
                questLog = reader.Read<QuestLog>(dataVersion);

                townPortal.Load(reader, dataVersion);

                id = reader.ReadGuid();

                activeAbility = abilities[reader.ReadInt32()];
            }
            else
            {
                // we need to auto complete any quests that the player has already passed in the story line
                foreach (Quest quest in QuestLog.FixedQuests)
                {
                    if (quest.DungeonLevel <= dungeonLevelsGenerated)
                    {
                        quest.AutoComplete();
                        questLog.Quests.Add(quest);
                    }
                }

            }

            
            if (dataVersion >= 9)
            {
                LastUnstuckTime = reader.ReadDateTime();
            }

            if (dataVersion >= 10)
            {
                statsManager.Load(reader, dataVersion);
            }

            return true;
        }


        public ItemSprite[,] Inventory
        {
            get { return items; }
        }

        public EquipableItem[,] EquippedItems
        {
            get { return equippedItems; }
        }


        public ItemSprite[,] GetStash(int stashIndex)
        {
            return stash[stashIndex];
        }


        public int StashesUnlocked
        {
            get { return stashesUnlocked; }
            set { stashesUnlocked = value; }
        }


        public int NextStashCost
        {
            get { return stashUnlcokCost[stashesUnlocked]; }
        }

        public void UnlockStash()
        {
            if (gold >= NextStashCost)
            {
                gold -= NextStashCost;
                stashesUnlocked++;
            }
        }


        public override void UseMana(float amount)
        {
            // adjust for armor penalties
            if (amount > 0)
                amount *= GetBaseManaCostAdjust();

            base.UseMana(amount);
        }


        public float GetBaseManaCostAdjust()
        {
            return (GetEquippedPropertyValue(Property.BaseManaCost) / 100f) + 1f;
        }



        public void Attack(ref List<float> damage, ref List<DamageType> damageTypes)
        {
            damage.Clear();
            damageTypes.Clear();
            for (int i = 0; i < (int)DamageType.None; i++)
            {
                damage.Add(GetAttack((DamageType)i));
                damageTypes.Add((DamageType)i);
            }
        }


        public float TotalAttack
        {
            get
            {
                float total = 0;
                for (int i = 0; i < (int)DamageType.None; i++)
                {
                    total += GetAttack((DamageType)i);
                }
                return total;
            }
        }


        public bool IsAttacking
        {
            get { return (currentAnimation.Name.Contains("Attack") && !IsPlaybackComplete); }
        }


        public bool GetKickbackDamage(List<float> damages, List<DamageType> damageTypes, NPCSprite enemy)
        {
            // for now only frost armor does this, but we should add a magic attribute that does kickback damage
            FrostArmorAbility frostArmor = (FrostArmorAbility)abilities[(int)AbilityType.FrostArmor];
            if (frostArmor.IsFrostArmorActive)
            {
                damages.Add(frostArmor.FrostDamage);
                damageTypes.Add(DamageType.Ice);
                enemy.SlowCharacter(2000, .5f, this);
                return true;
            }

            return false;
        }


        public float GetAttack(DamageType damageType)
        {
            if (damageType == DamageType.Physical)
                return CalculatePhysicalDamage();
            else if (damageType == DamageType.Fire)
                return GetEquippedPropertyValue(Property.FireDamage);
            else if (damageType == DamageType.Ice)
                return GetEquippedPropertyValue(Property.ColdDamage);
            else if (damageType == DamageType.Lightning)
                return GetEquippedPropertyValue(Property.LightningDamage);
            return 0;
        }


        public override void EndInvisible()
        {
            float stayInvisChance = ((ShadowMasteryAbility)Abilities[(int)AbilityType.ShadowMastery]).StayInvisChance;
            int roll = Util.Random.Next(0, 100);
            if (roll > stayInvisChance * 100)
            {
                base.EndInvisible();
            }
        }


        public void UseHealthPotion()
        {
            if (health == MaxHealth)
                return;

            float healthToFill = MaxHealth - health;
            float currentFill = 0;

            // find the best health potion to use
            UsableItem item = null;
            for (int x = 0; x < items.GetLength(0); x++)
            {
                for (int y = 0; y < items.GetLength(1); y++)
                {
                    if (items[x, y] is UsableItem && items[x, y].Name.Contains("Health Potion"))
                    {
                        UsableItem temp = (UsableItem)items[x, y];
                        int tempFill = temp.Modifiers[0].amount;

                        // this is a health potion
                        if (item == null)
                            item = temp;
                        else
                        {
                            if (currentFill > healthToFill && tempFill < currentFill)
                            {
                                item = temp;
                            }
                            else if (currentFill < healthToFill && tempFill < healthToFill && tempFill > currentFill)
                            {
                                item = temp;
                            }
                        }
                    }
                }
            }

            if (item != null)
            {
                item.Select();
            }
        }


        public void UseManaPotion()
        {
            if (mana == MaxMana)
                return;

            float manaToFill = MaxHealth - health;
            float currentFill = 0;

            // find the best health potion to use
            UsableItem item = null;
            for (int x = 0; x < items.GetLength(0); x++)
            {
                for (int y = 0; y < items.GetLength(1); y++)
                {
                    if (items[x, y] is UsableItem && items[x, y].Name.Contains("Mana Potion"))
                    {
                        UsableItem temp = (UsableItem)items[x, y];
                        int tempFill = temp.Modifiers[0].amount;

                        // this is a health potion
                        if (item == null)
                            item = temp;
                        else
                        {
                            if (currentFill > manaToFill && tempFill < currentFill)
                            {
                                item = temp;
                            }
                            else if (currentFill < manaToFill && tempFill < manaToFill && tempFill > currentFill)
                            {
                                item = temp;
                            }
                        }
                    }
                }
            }

            if (item != null)
            {
                item.Select();
            }
        }


        public void ActivateTownPortal()
        {
            townPortal.SetTPProperties(dungeonLevel, GameplayScreen.Dungeon.GetClosestOpenTile(CenteredPosition + new Vector2(64, 0)).Position);
            townPortal.Activate();
        }


        public float CalculatePhysicalDamage()
        {
            float damage = GetEquippedPropertyValue(Property.Damage) + GetActiveUsablesValue(Property.Damage);
            damage += BalanceManager.GetBaseDamage(level) * baseDamagePercent;
            damage += playerClass.GetPropertyValue(Property.Damage, damage);
            if (equippedItems[(int)EquipSlot.Hand, 0] != null && equippedItems[(int)EquipSlot.Hand, 0].IsRanged)
            {
                damage += damage * abilities[(int)AbilityType.Bow].Level * 0.15f;
                damage += playerClass.GetPropertyValue(Property.RangedDamage, damage);
            }
            else if (equippedItems[(int)EquipSlot.Hand, 0] != null)
            {
                damage += damage * abilities[(int)AbilityType.Melee].Level * 0.15f;
                damage += playerClass.GetPropertyValue(Property.MeleeDamage, damage);
                FrenzyAbility frenzy = (FrenzyAbility)abilities[(int)AbilityType.Frenzy];
                damage *= frenzy.GetDamageAdjust();
            }
            return damage;
        }

        public override int Armor
        {
            get
            {
                StoneSkinAbility stoneSkin = (StoneSkinAbility)abilities[(int)AbilityType.StoneSkin];
                int armor = (int)((float)(GetEquippedPropertyValue(Property.Armor) + GetActiveUsablesValue(Property.Armor)));
                armor += (int)((float)armor * ((float)abilities[(int)AbilityType.Defence].Level * .2f));
                armor += (int)(playerClass.GetPropertyValue(Property.Armor, armor));
                armor = (int)((float)armor * stoneSkin.GetArmorAdjust());
                return armor;
            }
        }

        public override float MaxHealth
        {
            get
            {
                if (DungeonGame.TestMode == false)
                {
                    return BalanceManager.GetBaseHealth(level) +
                        GetEquippedPropertyValue(Property.Health) +
                        GetActiveUsablesValue(Property.Health) +
                        playerClass.GetPropertyValue(Property.Health, BalanceManager.GetBaseHealth(level));
                }
                else
                {
                    return BalanceManager.GetBaseHealth(level) +
                        GetEquippedPropertyValue(Property.Health) +
                        GetActiveUsablesValue(Property.Health) + 5000 +
                        playerClass.GetPropertyValue(Property.Health, BalanceManager.GetBaseHealth(level));
                }
            }
        }

        public override float MaxMana
        {
            get
            {
                if (DungeonGame.TestMode == false)
                {
                    return BalanceManager.GetBaseMana(level) +
                        GetEquippedPropertyValue(Property.Mana) +
                        GetActiveUsablesValue(Property.Mana) +
                        playerClass.GetPropertyValue(Property.Mana, BalanceManager.GetBaseMana(level));
                }
                else
                {
                    return BalanceManager.GetBaseMana(level) +
                        GetEquippedPropertyValue(Property.Mana) +
                        GetActiveUsablesValue(Property.Mana) + 5000 +
                        playerClass.GetPropertyValue(Property.Mana, BalanceManager.GetBaseMana(level));

                }
            }
        }


        public float CastSpeedAdjust
        {
            get
            {
                float adjust = (GetEquippedPropertyValue(Property.CastSpeed) + playerClass.GetPropertyValue(Property.CastSpeed, 0)) / 100f;
                adjust = Math.Min(1f, adjust);
                adjust += (GetActiveUsablesValue(Property.CastSpeed)) / 100f;
                adjust = Math.Min(2f, adjust);
                adjust += (GetEquippedPropertyValue(Property.BaseCastSpeed)) / 100f;
                return adjust;
            }
        }

        public float AttackSpeedAdjust
        {
            get
            {
                FrenzyAbility frenzy = (FrenzyAbility)abilities[(int)AbilityType.Frenzy];
                float adjust = (GetEquippedPropertyValue(Property.AttackSpeed) + playerClass.GetPropertyValue(Property.AttackSpeed, 0)) / 100f;
                adjust = Math.Min(1f, adjust);
                adjust += (+GetActiveUsablesValue(Property.AttackSpeed) + frenzy.GetAttackSpeedAdjust()) / 100f;
                adjust = Math.Min(2f, adjust);
                adjust += GetEquippedPropertyValue(Property.BaseAttackSpeed) / 100f;

                if (equippedItems[(int)EquipSlot.Hand, 0] != null && equippedItems[(int)EquipSlot.Hand, 0].IsRanged)
                {
                    adjust += GetEquippedPropertyValue(Property.BaseRangedAttackSpeed) / 100f;
                }

                return adjust;
            }
        }



        public virtual void ValidatePosition()
        {
            // move the location until we are sure that it is valid
            DungeonLevel dungeon = GameplayScreen.Dungeon;
            Point newPosition = new Point((int)Position.X / dungeon.TileDimension, (int)Position.Y / dungeon.TileDimension);
            Point center = new Point((int)newPosition.X, (int)newPosition.Y);
            int distance = 1;
            bool found = false;
            while (dungeon.GetTile(newPosition) == null || !dungeon.GetTile(newPosition).IsWalkable())
            {
                // find the closest open space
                for (int x = Math.Max(center.X - distance, 0); x <= center.X + distance && x < dungeon.Dimension.X; x++)
                {
                    for (int y = Math.Max(center.Y - distance, 0); y <= center.Y + distance && y < dungeon.Dimension.Y; y++)
                    {
                        if (dungeon.GetTile(x, y).IsWalkable())
                        {
                            newPosition.X = x;
                            newPosition.Y = y;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                distance++;
            }

            SetPosition(newPosition.X * dungeon.TileDimension + dungeon.TileDimension / 2 - FrameDimensions.X / 2,
                    newPosition.Y * dungeon.TileDimension + dungeon.TileDimension / 2 - FrameDimensions.Y / 2);
        }


        public float NonBaseAttackSpeedAdjust
        {
            get
            {
                FrenzyAbility frenzy = (FrenzyAbility)abilities[(int)AbilityType.Frenzy];
                float adjust = (GetEquippedPropertyValue(Property.AttackSpeed) + playerClass.GetPropertyValue(Property.AttackSpeed, 0)) / 100f;
                adjust = Math.Min(1f, adjust);
                adjust += (GetActiveUsablesValue(Property.AttackSpeed) + frenzy.GetAttackSpeedAdjust()) / 100f;
                adjust = Math.Min(2f, adjust);

                if (equippedItems[(int)EquipSlot.Hand, 0] != null && equippedItems[(int)EquipSlot.Hand, 0].IsRanged)
                {
                    adjust += GetEquippedPropertyValue(Property.BaseRangedAttackSpeed) / 100f;
                }

                return adjust;
            }
        }


        public List<ITimedEffect> GetAttackEffects(CharacterSprite target)
        {
            List<ITimedEffect> effects = new List<ITimedEffect>();

            // check for poison effect and add it
            float poisonDamage = GetEquippedPropertyValue(Property.PoisonDamage);
            float poisonChance = ((PoisonMasteryAbility)Abilities[(int)AbilityType.PoisonMastery]).PoisonChance;
            int roll = Util.Random.Next(0, 100);
            if (roll < poisonChance * 100)
            {
                poisonDamage += GetAttack(DamageType.Physical) * .2f;
            }
            if (poisonDamage >= 1f)
            {
                effects.Add(new PoisonEffect(this, target, 5000, (int)poisonDamage));
            }


            // check for bleed effect and add it
            float bleedDamage = GetEquippedPropertyValue(Property.BleedDamage);
            float bleedChance = ((PrecisionAbility)Abilities[(int)AbilityType.Precision]).BleedChance;
            roll = Util.Random.Next(0, 100);
            if (roll < bleedChance * 100)
            {
                bleedDamage += GetAttack(DamageType.Physical) * .25f;
            }
            if (bleedDamage >= 1f)
            {
                effects.Add(new BleedEffect(this, target, 5000, (int)bleedDamage));
            }


            // add burn damage
            float burnDamage = GetEquippedPropertyValue(Property.BurnDamage);
            if (burnDamage >= 1f)
            {
                effects.Add(new BurnEffect(this, target, 5000, (int)burnDamage));
            }

            // roll for slow
            int slowChance = GetEquippedPropertyValue(Property.SlowChance);
            roll = Util.Random.Next(0, 100);
            if (roll < slowChance)
            {
                effects.Add(new SlowEffect(this, target, 4000, .5f));
            }

            // roll for stun
            int stunChance = GetEquippedPropertyValue(Property.StunChance);
            roll = Util.Random.Next(0, 100);
            if (roll < stunChance)
            {
                effects.Add(new StunEffect(this, target, 2000));
            }

            // roll for fear
            int fearChance = GetEquippedPropertyValue(Property.FearChance);
            roll = Util.Random.Next(0, 100);
            if (roll < fearChance)
            {
                effects.Add(new FearEffect(this, target, 4000));
            }

            return effects;
        }


        public int SpellDamage
        {
            // TODO: spell damage percent increase from class might need to be re-thought a bit here
            get { return GetEquippedPropertyValue(Property.SpellDamage) + GetActiveUsablesValue(Property.SpellDamage) + (int)playerClass.GetPropertyValue(Property.SpellDamage, BalanceManager.GetBaseDamage(level)); }
        }

        public override float MagicResistance
        {
            get
            {
                return (GetEquippedPropertyValue(Property.MagicResistance) + GetActiveUsablesValue(Property.MagicResistance) + (int)playerClass.GetPropertyValue(Property.MagicResistance, 0)) / 100f;
            }
        }

        public int Experience
        {
            get { return exp; }
        }

        public override float CriticalHitChance
        {
            get
            {
                float critChance = base.CriticalHitChance + ((float)(GetEquippedPropertyValue(Property.CriticalHitChance) + GetActiveUsablesValue(Property.CriticalHitChance)) / 100f);
                return Math.Min(.7f, critChance);
            }
        }

        public int DungeonLevel
        {
            get { return dungeonLevel; }
            set { dungeonLevel = value; }
        }

        public int DungeonLevelsGenerated
        {
            get { return dungeonLevelsGenerated; }
            set { dungeonLevelsGenerated = value; }
        }

        public float PercentThroughLevel
        {
            get { return (float)(exp - prevLevelExp) / (float)(nextLevelExp - prevLevelExp); }
        }

        public int NextLevelExp
        {
            get { return nextLevelExp; }
        }


        public float MoveSpeedAdjust
        {
            get
            {
                SprintAbility sprint = (SprintAbility)Abilities[(int)AbilityType.Sprint];
                FrenzyAbility frenzy = (FrenzyAbility)abilities[(int)AbilityType.Frenzy];
                float moveSpeedAdjust = (GetEquippedPropertyValue(Property.MoveSpeed) +
                    GetEquippedPropertyValue(Property.BaseMoveSpeed) +
                    playerClass.GetPropertyValue(Property.MoveSpeed, 0) +
                    sprint.GetSpeedAdjust() +
                    frenzy.GetMoveSpeedAdjust() +
                    GetActiveUsablesValue(Property.MoveSpeed) + 100f) / 100f;
                return Math.Min(250, moveSpeedAdjust);
            }
        }


        public override float MovementSpeed
        {
            get
            {
                float speedModifier = MoveSpeedAdjust;
                if (IsSlowed)
                    speedModifier -= SlowAmount;
                return Math.Max(1, (int)(baseMovementSpeed * speedModifier));
            }
        }

        public int GetEquippedPropertyValue(Property property)
        {
            int value = 0;
            for (int i = 0; i < equippedItems.Length; i++)
            {
                if (equippedItems[i, 0] != null)
                {
                    for (int j = 0; j < equippedItems[i, 0].Modifiers.Count; j++)
                    {
                        if (equippedItems[i, 0].Modifiers[j].property == property)
                        {
                            value += equippedItems[i, 0].Modifiers[j].amount;
                        }
                    }
                }
            }
            return value;
        }


        public int GetActiveUsablesValue(Property property)
        {
            int value = 0;
            for (int i = 0; i < activeTimedEffects.Count; i++)
            {
                if (activeTimedEffects[i] != null && activeTimedEffects[i] is UsableItem)
                {
                    UsableItem activeUsable = (UsableItem)activeTimedEffects[i];
                    for (int j = 0; j < activeUsable.Modifiers.Count; j++)
                    {
                        if (activeUsable.Modifiers[j].property == property)
                        {
                            value += activeUsable.Modifiers[j].amount;
                        }
                    }
                }
            }
            return value;
        }


        public void ActivateUsableItem(UsableItem item)
        {
            // search through inventory for an item of this type
            for (int i = 0; i < items.GetLength(0); i++)
            {
                for (int j = 0; j < items.GetLength(1); j++)
                {
                    if (items[i, j] != null)
                    {
                        if (items[i, j].Name == item.Name)
                        {
                            UsableItem usable = (UsableItem)items[i, j];
                            usable.DecrementStack();
                            if (usable.IsStackEmpty)
                            {
                                items[i, j] = null;
                            }

                            if (item.IsInstantUse)
                            {
                                usable.ApplyImmediateEffect();
                            }
                            if (!item.IsInstantUse)
                            {
                                usable.ResetElapsedDuration();

                                // dedupe to make sure we don't stack effects of potions
                                for (int x = 0; x < activeTimedEffects.Count; x++)
                                {
                                    if (activeTimedEffects[x].IsEqualActiveIcon(usable))
                                    {
                                        ITimedEffect other = activeTimedEffects[x];
                                        usable = (UsableItem)(usable.CombineActiveIcons((IActiveIcon)other));
                                        activeTimedEffects.RemoveAt(x);
                                    }
                                }

                                activeTimedEffects.Add(usable);
                            }

                            return;
                        }
                    }
                }
            }
        }


        public override void Heal(float amount, bool instant = false)
        {
            if (instant)
            {
                health += amount;
                if (health > MaxHealth)
                    health = MaxHealth;
            }
            else
            {
                healAmount += amount;
                healSpeed = healAmount / 3;
            }
        }

        public override void AddMana(float amount)
        {
            addManaAmount += amount;
            addManaSpeed = addManaAmount / 3;
        }


        public float HealthRegen
        {
            get
            {
                return BalanceManager.GetBaseHealthRegen(level) + GetActiveUsablesValue(Property.HealthRegen) + GetEquippedPropertyValue(Property.HealthRegen) + playerClass.GetPropertyValue(Property.HealthRegen, BalanceManager.GetBaseHealthRegen(level));
            }
        }



        public float ManaRegen
        {
            get
            {
                return BalanceManager.GetBaseManaRegen(level) + GetActiveUsablesValue(Property.ManaRegen) + GetEquippedPropertyValue(Property.ManaRegen) + playerClass.GetPropertyValue(Property.ManaRegen, BalanceManager.GetBaseManaRegen(level));
            }
        }

        public void LevelUp()
        {
            HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.LevelUp);
            level++;
            prevLevelExp = nextLevelExp;
            nextLevelExp = nextLevelExp + BalanceManager.GetExperienceToLevel(level);
            abilityPoints += level;
            health = MaxHealth;
            mana = MaxMana;
            if (level == 5)
            {
                HelpScreenManager.Instance.AutoShowHelpScreen(HelpScreens.Class);
            }
        }


        public void AddExperience(int amount)
        {
            exp += amount;
            if (exp > nextLevelExp)
            {
                AudioManager.audioManager.PlaySFX("Trumpet");
                // TODO: play a nice sound and maybe use a particle generator to make the character light up
            }

            while (exp > nextLevelExp)
            {
                LevelUp();
            }

            statsManager.Add(StatType.TotalExperience, (ulong)amount);
        }


        public void SetActiveAbility(Ability ability)
        {
            activeAbility = ability;
        }

        public void SetActiveAbility(AbilityType at)
        {
            activeAbility = abilities[(int)at];
        }


        public void SetMacroButton(MacroButton button, int index)
        {
            macroButtons[index] = button;

            // dedupe
            for (int i = 0; i < macroButtons.Length; i++)
            {
                if (i != index && macroButtons[i].IsEqual(macroButtons[index]))
                {
                    macroButtons[i] = MacroButton.emptyMacro;
                }
            }
        }

        public MacroButton GetMacroButton(int index)
        {
            return macroButtons[index];
        }

        public void AddGold(int amount)
        {
            ParticleSystem.AddParticles(this.CenteredPosition, ParticleType.Starburst, color: Color.Goldenrod, numParticlesScale: .2f);
            gold += amount;
            if (amount >= 0)
                statsManager.Add(StatType.TotalGoldCollected, (ulong)amount);
            else
                statsManager.Add(StatType.TotalGoldSpent, (ulong)(amount * -1));
        }

        public int Gold
        {
            get { return gold; }
        }

        public int AbilityPoints
        {
            get { return abilityPoints; }
        }

        public PlayerClass PlayerClass
        {
            get { return playerClass; }
        }

        public void UpgradePlayerClass(PlayerClass playerClass)
        {
            this.playerClass = playerClass;
        }

        public bool TryUseAbilityPoints(int count)
        {
            if (abilityPoints >= count)
            {
                abilityPoints -= count;
                return true;
            }
            return false;
        }

        public ISelectable ActiveSelectable
        {
            get
            {
                return activeAbility;
            }
        }


        public void SwapItems(Point slot1, Point slot2)
        {
            ItemSprite tempItem = null;
            ItemSprite tempItem2 = null;
            ItemSprite displacedItem = null;
            ItemSprite displacedItem2 = null;
            bool result = true;

            if (IsValidSlot(GetItem(slot1), slot2) &&
                !IsValidSlot(GetItem(slot2), slot1))
            {
                // if slot 1->2 is valid but not 2->1 then un-equip 2
                tempItem2 = GetItem(slot2);
                SetItem(null, slot2);
                tempItem = GetItem(slot1);
                SetItem(null, slot1);
                displacedItem = SetItem(tempItem, slot2);
                if (displacedItem != null)
                {
                    result = AddItem(displacedItem, true);
                    if (!result)
                    {
                        DropItem(displacedItem);
                    }
                }
                result = AddItem(tempItem2, true);
                if (!result)
                {
                    DropItem(tempItem2);
                }
                return;
            }

            if (!IsValidSlot(GetItem(slot1), slot2) ||
                !IsValidSlot(GetItem(slot2), slot1))
            {
                // invalid swap
                return;
            }

            // check for stack combining
            ItemSprite item1 = GetItem(slot1);
            ItemSprite item2 = GetItem(slot2);
            if (item1 is UsableItem && item2 is UsableItem)
            {
                UsableItem usable1 = (UsableItem)item1;
                UsableItem usable2 = (UsableItem)item2;
                bool combined = usable2.CombineStacks(usable1);
                if (combined)
                {
                    if (usable1.IsStackEmpty)
                    {
                        SetItem(null, slot1);
                    }
                    return;
                }
            }

            // swap slots
            tempItem = GetItem(slot2);
            tempItem2 = GetItem(slot1);
            SetItem(null, slot2);
            SetItem(null, (slot1));
            displacedItem = SetItem(tempItem2, slot2);
            displacedItem2 = SetItem(tempItem, (slot1));
            if (displacedItem != null)
            {
                result = AddItem(displacedItem, true);
                if (!result)
                {
                    DropItem(displacedItem);
                }
            }
            if (displacedItem2 != null)
            {
                result = AddItem(displacedItem2, true);
                if (!result)
                {
                    DropItem(displacedItem2);
                }
            }
        }


        public PlayerProfile GetPlayerProfile()
        {
            PlayerProfile profile = new PlayerProfile()
            {
                Name = this.Name,
                Level = this.level,
                Id = this.id,
                Class = playerClass.name,
                Gold = this.gold,
                DungeonLevel = this.DungeonLevel,
                DungeonLevelsGenerated = this.DungeonLevelsGenerated,
                Experience = this.Experience,
                Damage = (int)this.TotalAttack,
                Armor = this.Armor,
                SpellDamage = this.SpellDamage,
                CriticalHitChance = (int)(this.CriticalHitChance * 100),
                MagicResistance = (int)(this.MagicResistance * 100),
                MovementSpeed = (int)(this.MoveSpeedAdjust * 100),
                AttackSpeed = (int)((this.NonBaseAttackSpeedAdjust * 100) + 100),
                CastSpeed = (int)((this.CastSpeedAdjust * 100) + 100),
                ManaCost = (int)((this.GetBaseManaCostAdjust() * 100)),
                MaxHealth = (int)this.MaxHealth,
                HealthRegen = (int)this.HealthRegen,
                MaxMana = (int)this.MaxMana,
                ManaRegen = (int)this.ManaRegen,
                Difficulty = this.difficulty
            };

            for (int i = 0; i < ItemSprite.EquipSlotNum; i++)
            {
                if (equippedItems[i, 0] != null)
                {
                    profile.Items.Add(equippedItems[i, 0].GetItemProfile());
                }
            }

            return profile;
        }


        public override void PlayAnimation(string name)
        {
            // add in what weapon we are using to the animation
            if (!name.Contains("Die"))
                name += GetWeaponAnimString();

            base.PlayAnimation(name);
        }

        public Ability[] Abilities
        {
            get { return abilities; }
        }


        public float HealAmount
        {
            get { return healAmount; }
        }


        public float AddManaAmount
        {
            get { return addManaAmount; }
        }


        public DateTime LastUnstuckTime
        {
            get { return lastUnstuckTime; }
            set { lastUnstuckTime = value; }
        }

        public bool IsRangedEquipped
        {
            get { return equippedItems[(int)EquipSlot.Hand, 0] != null && equippedItems[(int)EquipSlot.Hand, 0].IsRanged; }
        }

        public GameDifficulty GameDifficulty
        {
            get { return difficulty; }
            set { difficulty = value; }
        }

        public TownPortal TownPortal
        {
            get { return townPortal; }
        }


        public override float LifeStealPercent
        {
            get
            {
                return GetEquippedPropertyValue(Property.LifeSteal) / 100f;
            }
        }


        public StatsManager StatsManager
        {
            get { return statsManager; }
        }


        public override float ManaStealPercent
        {
            get
            {
                return GetEquippedPropertyValue(Property.ManaSteal) / 100f;
            }
        }



        public override float ReflectDamagePercent
        {
            get
            {
                return GetEquippedPropertyValue(Property.ReflectDamage) / 100f;
            }
        }
    }
}
