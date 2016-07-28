using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using LegendsOfDescent;
using Microsoft.Xna.Framework;

#if SILVERLIGHT
using System.Windows;
#endif

namespace LegendsOfDescent
{

    public enum ItemClass
    {
        Normal,
        Magic,
        Rare,
        Heroic,
        Legendary,
        Num
    }


    public enum DropType
    {
        Normal,
        Champion,
        Boss,
        Merchant
    }


    public class MagicProperty
    {
        public string name;
        public float valueAdjust = 1f;
        public bool addNameToStart = true;
        public List<PropertyModifier> modifiers = new List<PropertyModifier>();
        public bool showParticles = false;
        public ParticleType particleType;
        public Color particleColor;
    }

    public class ItemManager
    {
        static public ItemManager Instance;
        List<ItemSprite> items = new List<ItemSprite>();
        List<ItemSprite>[] itemsByLevel = new List<ItemSprite>[25]; // list of items for each level that they are available in
        Dictionary<MerchantType, List<ItemSprite>[]> itemsByMerchantType = new Dictionary<MerchantType, List<ItemSprite>[]>();
        List<MagicProperty>[] wepMagicProperties = new List<MagicProperty>[(int)ItemClass.Num];
        List<MagicProperty>[] armorMagicProperties = new List<MagicProperty>[(int)ItemClass.Num];
        List<MagicProperty>[] allMagicProperties = new List<MagicProperty>[(int)ItemClass.Num];
        List<ItemSprite> legendaryItems = new List<ItemSprite>();

        //string[] qualifiers = {"Flimsy",
        //                       "Simple",
        //                       "Good",
        //                       "Artisan",
        //                       "Great",
        //                       "Glorious",
        //                       "Fantastic",
        //                       "Epic",
        //                       "Mystical",
        //                       "Legendary"};


        string[] UsableQualifiers = {"Minor",
                                     "Small",
                                     "",
                                     "Large",
                                     "Major",
                                     "Potent",
                                     "Giant",
                                     "Massive",
                                     "Behemoth",
                                     "Ancient",
                                     "Arcane",
                                     "Impossible",
                                     "Heroic",
                                     "Runic",
                                     "Mythic"};

        float[] UsableModifierAdjust = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 10f, 13f, 16f, 20f, 25f, 30f, 40f, 50f};
        int[] UsableLevelScale = { 1, 5, 9, 13, 17, 21, 25, 30, 35, 40, 50, 60, 80, 100, 120, 140, 170, 200 };

        public ItemManager()
        {
            Instance = this;
            for (int i = 0; i < (int)ItemClass.Num; i++)
            {
                wepMagicProperties[i] = new List<MagicProperty>();
                armorMagicProperties[i] = new List<MagicProperty>();
                allMagicProperties[i] = new List<MagicProperty>();
            }

            for (int i = 0; i < itemsByLevel.Length; i++)
            {
                itemsByLevel[i] = new List<ItemSprite>();
            }

            for (int i = 0; i < (int)MerchantType.Num; i++)
            {
                List<ItemSprite>[] lists = new List<ItemSprite>[25];
                for (int j = 0; j < lists.Length; j++)
                {
                    lists[j] = new List<ItemSprite>();
                }
                itemsByMerchantType.Add((MerchantType)i, lists);
            }

        }

        public void Load()
        {
            // Load the Magic Properties
#if WIN8
            XDocument root = XmlHelper.Load("Assets/Data/MagicProperties.xml");
#else
            XDocument root = XmlHelper.Load("Content/Data/MagicProperties.xml");
#endif
            IEnumerable<XElement> childrenNodes = root.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "MagicProperties")
                {
                    ParseMagicProperties(element);
                }
            }

            // Load the base items
#if WIN8
            root = XmlHelper.Load("Assets/Data/Items.xml");
#else
            root = XmlHelper.Load("Content/Data/Items.xml");
#endif
            childrenNodes = root.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Items")
                {
                    ParseItems(element);
                }
            }

            // Load the legendary items
#if WIN8
            root = XmlHelper.Load("Assets/Data/LegendaryItems.xml");
#else
            root = XmlHelper.Load("Content/Data/LegendaryItems.xml");
#endif
            childrenNodes = root.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Items")
                {
                    ParseLegendaryItems(element);
                }
            }
        }


        public void ParseMagicProperties(XElement node)
        {
            IEnumerable<XElement> childrenNodes = node.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "MagicProperty")
                {
                    ParseMagicProperty(element);
                }
            }
        }


        public void ParseMagicProperty(XElement node)
        {
            MagicProperty mp = new MagicProperty();
            string type = null;
            ItemClass itemClass = ItemClass.Num;

            IEnumerable<XAttribute> attributes = node.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name == "Name")
                {
                    mp.name = attribute.Value;
                }
                else if (attribute.Name == "Class")
                {
                    for (int i = 0; i < (int)ItemClass.Num; i++)
                    {
                        if (((ItemClass)i).ToString() == attribute.Value)
                        {
                            itemClass = (ItemClass)i;
                            break;
                        }
                    }
                }
                else if (attribute.Name == "Type")
                {
                    type = attribute.Value;
                }
                else if (attribute.Name == "AddNameToStart")
                {
                    mp.addNameToStart = Convert.ToBoolean(attribute.Value);
                }
                else if (attribute.Name == "ValueAdjust")
                {
                    mp.valueAdjust = Convert.ToSingle(attribute.Value, new CultureInfo("en-US"));
                }
                else if (attribute.Name == "ParticleType")
                {
                    mp.particleType = StringToParticleType(attribute.Value);
                    mp.showParticles = true;
                }
                else if (attribute.Name == "ParticleColor")
                {
                    mp.particleColor = StringToColor(attribute.Value);
                }
                else
                {
                    throw new Exception();
                }
            }

            IEnumerable<XElement> childrenNodes = node.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Modifier")
                {
                    ParseProperty(element, mp.modifiers);
                }
            }

            if (type == "Weapon" || type == "All")
            {
                wepMagicProperties[(int)itemClass].Add(mp);
            }
            if (type == "Armor" || type == "All")
            {
                armorMagicProperties[(int)itemClass].Add(mp);
            }
            allMagicProperties[(int)itemClass].Add(mp);
        }

        public void ParseItems(XElement node)
        {
            IEnumerable<XElement> childrenNodes = node.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Equipable")
                {
                    ParseEquipables(element);
                }
                else if (element.Name == "Usable")
                {
                    ParseUsables(element);
                }
            }
        }


        public void ParseEquipables(XElement node)
        {
            string name="", texture="", description="";
            int level = 0, framesPerRow = 0, frame = 0, minLevel = 1, maxLevel = 999;
            float value = 1f;

            IEnumerable<XElement> children = node.Descendants();
            foreach (XElement element in children)
            {
                if (element.Name == "Item")
                {
                    ParseItem(element, ref name, ref level, ref texture, ref framesPerRow, ref frame, ref value, ref description, ref minLevel, ref maxLevel);
                    EquipableItem item = new EquipableItem(name, texture, framesPerRow, frame, level, value, description, 0);

                    ParseEquipable(element, item);

                    items.Add(item);
                    for (int i = minLevel; i < itemsByLevel.Length && i <= maxLevel; i++)
                    {
                        itemsByLevel[i].Add(item);
                        itemsByMerchantType[MerchantType.General][i].Add(item);
                        itemsByMerchantType[MerchantType.Equipable][i].Add(item);
                        if (item.MerchantType != MerchantType.General)
                        {
                            itemsByMerchantType[item.MerchantType][i].Add(item);
                        }

                    }

                    description = "";
                }
            }
        }


        public void ParseUsables(XElement node)
        {
            string name = "", texture = "", description = "";
            int level = 0, framesPerRow = 0, frame = 0, minLevel = 1, maxLevel = 999;
            float value = 1f;

            IEnumerable<XElement> children = node.Descendants();
            foreach (XElement element in children)
            {
                if (element.Name == "Item")
                {
                    ParseItem(element, ref name, ref level, ref texture, ref framesPerRow, ref frame, ref value, ref description, ref minLevel, ref maxLevel);
                    UsableItem item = new UsableItem(name, texture, framesPerRow, frame, level, value, description, 0);

                    ParseUsable(element, item);

                    items.Add(item);
                    for (int i = minLevel; i < itemsByLevel.Length && i <= maxLevel; i++)
                    {
                        itemsByLevel[i].Add(item);
                        itemsByMerchantType[MerchantType.Potion][i].Add(item);
                        itemsByMerchantType[MerchantType.General][i].Add(item);
                    }

                    description = "";
                }
            }
        }

        public void ParseLegendaryItems(XElement node)
        {
            IEnumerable<XElement> childrenNodes = node.Descendants();
            foreach (XElement element in childrenNodes)
            {
                if (element.Name == "Equipable")
                {
                    ParseLegendaryEquipables(element);
                }
            }
        }


        public void ParseLegendaryEquipables(XElement node)
        {
            string name = "", texture = "", description = "";
            int level = 0, framesPerRow = 0, frame = 0, minLevel = 1, maxLevel = 999;
            float value = 1f;

            IEnumerable<XElement> children = node.Descendants();
            foreach (XElement element in children)
            {
                if (element.Name == "Item")
                {
                    ParseItem(element, ref name, ref level, ref texture, ref framesPerRow, ref frame, ref value, ref description, ref minLevel, ref maxLevel);
                    EquipableItem item = new EquipableItem(name, texture, framesPerRow, frame, level, value, description, 0);

                    ParseEquipable(element, item);

                    legendaryItems.Add(item);
                    description = "";
                }
            }
        }

        void ParseEquipable(XElement node, EquipableItem item)
        {
            IEnumerable<XElement> children = node.Descendants();
            foreach (XElement element in children)
            {
                if (element.Name == "Slot")
                {
                    item.EquipSlot = ParseEquipSlot(element.Value);
                }
                else if (element.Name == "Icon")
                {
                    ParseIcon(element, item);
                }
                else if (element.Name == "Modifier")
                {
                    ParseProperty(element, item.Modifiers);
                }
                else if (element.Name == "Ranged")
                {
                    item.IsRanged = Convert.ToBoolean(element.Value);
                }
                else if (element.Name == "MerchantType")
                {
                    item.MerchantType = StringToMerchantType(element.Value);
                }
                else if (element.Name == "Particle")
                {
                    ParseParticleType(element, item);
                }
                else
                {
                    throw new Exception();
                }
            }
        }


        void ParseParticleType(XElement node, EquipableItem item)
        {
            ParticleType partType = ParticleType.ExplosionWhite;
            Color color = Color.White;

            IEnumerable<XAttribute> attributes = node.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name == "ParticleType")
                {
                    partType = StringToParticleType(attribute.Value);
                }
                else if (attribute.Name == "ParticleColor")
                {
                    color = StringToColor(attribute.Value);
                }
            }

            item.SetParticleGlow(partType, color);
        }


        void ParseProperty(XElement node, List<PropertyModifier> list)
        {
            PropertyModifier pm = new PropertyModifier();

            IEnumerable<XAttribute> attributes = node.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name == "Stat")
                {
                    for (int i = 0; i < (int)Property.Num; i++)
                    {
                        if (((Property)i).ToString() == attribute.Value)
                        {
                            pm.property = ((Property)i);
                            break;
                        }
                    }
                }
                else if (attribute.Name == "Amount")
                {
                    pm.amount = Convert.ToInt32(attribute.Value);
                }
                else
                {
                    throw new Exception();
                }
            }

            list.Add(pm);
        }


        public void ParseIcon(XElement node, ItemSprite item)
        {
            String texture = "";
            int framesPerRow = 0, frame = 0;

            IEnumerable<XAttribute> attributes = node.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name == "Texture")
                {
                    texture = attribute.Value;
                }
                else if (attribute.Name == "FramesPerRow")
                {
                    framesPerRow = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "Frame")
                {
                    frame = Convert.ToInt32(attribute.Value);
                }
                else
                {
                    throw new Exception();
                }
            }

            item.SetIcon(texture, framesPerRow, frame);
        }

        public EquipSlot ParseEquipSlot(String slot)
        {
            for (int i = 0; i < (int)EquipSlot.Num; i++)
            {
                if (((EquipSlot)i).ToString() == slot)
                {
                    return ((EquipSlot)i);
                }
            }
            throw new Exception();
        }

        public void ParseItem(XElement node, ref string name, ref int level, ref string texture, ref int framesPerRow,
            ref int frame, ref float value, ref string description, ref int minlevel, ref int maxlevel)
        {
            IEnumerable<XAttribute> attributes = node.Attributes();
            foreach (XAttribute attribute in attributes)
            {
                if (attribute.Name == "Name")
                {
                    name = attribute.Value;
                }
                else if (attribute.Name == "Description")
                {
                    description = attribute.Value;
                }
                else if (attribute.Name == "Level")
                {
                    level = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "Texture")
                {
                    texture = attribute.Value;
                }
                else if (attribute.Name == "FramesPerRow")
                {
                    framesPerRow = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "Frame")
                {
                    frame = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "ValueAdjust")
                {
                    value = Convert.ToSingle(attribute.Value, new CultureInfo("en-US"));
                }
                else if (attribute.Name == "MinLevel")
                {
                    minlevel = Convert.ToInt32(attribute.Value);
                }
                else if (attribute.Name == "MaxLevel")
                {
                    maxlevel = Convert.ToInt32(attribute.Value);
                }
                else
                {
                    throw new Exception();
                }
            }
        }


        public void ParseUsable(XElement node, UsableItem item)
        {
            IEnumerable<XElement> children = node.Descendants();
            foreach (XElement element in children)
            {
                if (element.Name == "Duration")
                {
                    item.Duration = Convert.ToInt32(element.Value);
                }
                else if (element.Name == "Icon")
                {
                    ParseIcon(element, item);
                }
                else if (element.Name == "Modifier")
                {
                    ParseProperty(element, item.Modifiers);
                }
                else if (element.Name == "MaxGroup")
                {
                    item.MaxGroup = Convert.ToInt32(element.Value);
                }
                else
                {
                    throw new Exception();
                }
            }
        }


        public ItemSprite GetItem(String name, int level)
        {
            ItemSprite item = GetItem(name);
            ApplyItemLevel(item, level);
            return item;
        }


        public ItemSprite GetItemForDrop(EnemySprite enemy)
        {
            MerchantType type = MerchantType.General;

            DropType dropType = DropType.Normal;
            if (enemy.IsBoss)
                dropType = DropType.Boss;
            else if (enemy.IsChampion)
                dropType = DropType.Champion;


            if (enemy.IsChampion || enemy.IsBoss)
                type = MerchantType.Equipable;

            return GetItemForDrop(enemy.Level, dropType, type);
        }



        public List<ItemSprite> GenerateAllItems(int level)
        {
            List<ItemSprite> allItems = new List<ItemSprite>();
            for (int i = 0; i < items.Count; i++)
            {
                allItems.Add(GetItem(items[i].Name, level));
            }
            return allItems;
        }


        public ItemSprite GetBasicItem(int level, MerchantType merchantType = MerchantType.General)
        {
            List<ItemSprite> levelList = itemsByMerchantType[merchantType][Math.Min(itemsByLevel.Length - 1, level)];
            ItemSprite item = levelList.Random().CopyItem();

            ApplyItemLevel(item, level);

            return item;
        }

        public ItemSprite GetItemForDrop(int level, DropType dropType = DropType.Normal, MerchantType merchantType = MerchantType.General)
        {
            // generate a random drop item based on enemies level
            ItemSprite item = null;

            if (level < 1)
                level = 1;

            // select a base item
            List<ItemSprite> levelList = itemsByMerchantType[merchantType][Math.Min(itemsByLevel.Length - 1, level)];
            item = levelList.Random().CopyItem();
            item.Level = level;

            if (item is EquipableItem)
            {
                EquipableItem ei = (EquipableItem)item;

                // roll item level chances
                int roll = Util.Random.Next(0, 1000);
                if (roll > 950)
                {
                    item.Level += 2;
                }
                else if (roll > 800)
                {
                    item.Level++;
                }
                else if (roll < 100)
                {
                    item.Level--;
                    if (item.Level < 1)
                        item.Level = 1;
                }

                // random roll for a magic property
                roll = Util.Random.Next(0, 1000);
                float heroic = 0, rare = 0, magic = 0, legendary = 0;
                CalculateMagicChances(dropType, out heroic, out rare, out magic, out legendary);
                if (roll < legendary)
                {
                    item = legendaryItems.Random().CopyItem();  // have to repick item in this case since legendaries are unique
                    item.ItemClass = ItemClass.Legendary;
                    item.Level = level;
                }
                else if (roll < heroic + legendary)
                {
                    item.ItemClass = ItemClass.Heroic;
                }
                else if (roll < heroic + rare + legendary)
                {
                    item.ItemClass = ItemClass.Rare;
                }
                else if (roll < heroic + rare + magic + legendary)
                {
                    item.ItemClass = ItemClass.Magic;
                }
            }

            ApplyItemLevel(item, item.Level);

            return item;
        }


        private void CalculateMagicChances(DropType dropType, out float heroic, out float rare, out float magic, out float legendary)
        {
            // normal drop chance
            legendary = 5;
            heroic = 30;
            rare = 100;
            magic = 200;

            // adjust for special creeps
            if (dropType == DropType.Champion || dropType == DropType.Merchant)
            {
                legendary = 10;
                heroic = 60;
                rare = 200;
                magic = 1000;
            }
            else if (dropType == DropType.Boss)
            {
                legendary = 300;
                heroic = 1000;
            }

            // merchants do not carry legendary items and are not effected by magic drop buffs
            if (dropType == DropType.Merchant)
            {
                legendary = 0;
            }
            else
            {
                // adjust for magic properties
                legendary = (int)((float)legendary * (((float)SaveGameManager.CurrentPlayer.GetEquippedPropertyValue(Property.MagicDrop) + 100f) / 100f));
                heroic = (int)((float)heroic * (((float)SaveGameManager.CurrentPlayer.GetEquippedPropertyValue(Property.MagicDrop) + 100f) / 100f));
                rare = (int)((float)rare * (((float)SaveGameManager.CurrentPlayer.GetEquippedPropertyValue(Property.MagicDrop) + 100f) / 100f));
                magic = (int)((float)magic * (((float)SaveGameManager.CurrentPlayer.GetEquippedPropertyValue(Property.MagicDrop) + 100f) / 100f));
            }

            // TODO: remove
            // legendary = 1000;
        }




        private void ApplyItemLevel(ItemSprite item, int level)
        {
            item.Level = level;

            if (item is EquipableItem)
            {
                EquipableItem ei = (EquipableItem)item;
                ei.Value = (int)(ei.ValueAdjust * BalanceManager.GetBaseItemValue(level));

                if (ei.EquipSlot == EquipSlot.Augment && item.ItemClass == ItemClass.Normal)
                {
                    // augments are always at least magic
                    item.ItemClass = ItemClass.Magic;
                }

                // build item based on level
                MagicProperty mp = null;
                if (item.ItemClass != ItemClass.Legendary && item.ItemClass != ItemClass.Normal)
                {
                    if (ei.EquipSlot == EquipSlot.Hand || 
                        ei.EquipSlot == EquipSlot.MainHand || 
                        ei.EquipSlot == EquipSlot.TwoHand || 
                        ei.EquipSlot == EquipSlot.OffHand)
                    {
                        mp = wepMagicProperties[(int)item.ItemClass][Util.Random.Next(0, wepMagicProperties[(int)item.ItemClass].Count)];
                    }
                    else if (ei.EquipSlot == EquipSlot.Chest ||
                             ei.EquipSlot == EquipSlot.Legs ||
                             ei.EquipSlot == EquipSlot.Head)
                    {
                        mp = armorMagicProperties[(int)item.ItemClass][Util.Random.Next(0, armorMagicProperties[(int)item.ItemClass].Count)];
                    }
                    else if (ei.EquipSlot == EquipSlot.Augment)
                    {
                        mp = allMagicProperties[(int)item.ItemClass][Util.Random.Next(0, wepMagicProperties[(int)item.ItemClass].Count)];
                    }
                }

                // apply magic property value change
                if (mp != null)
                {
                    item.Value = (int)((float)item.Value * mp.valueAdjust);
                }


                // Translate item from percents to absolutes
                for (int i = 0; i < ei.Modifiers.Count; i++)
                {
                    ei.Modifiers[i] = TranslateProperty(ei.Modifiers[i], ei.Level, ei.EquipSlot);
                }

                if (mp != null)
                {
                    for (int i = 0; i < mp.modifiers.Count; i++)
                    {
                        ei.Modifiers.Add(TranslateProperty(mp.modifiers[i], ei.Level, ei.EquipSlot));
                    }

                    if (mp.addNameToStart)
                    {
                        ei.Name = mp.name + " " + ei.Name;
                    }
                    else
                    {
                        ei.Name = ei.Name + " of " + mp.name;
                    }
                }


                // collapse like modifiers
                for (int i = 0; i < item.Modifiers.Count; i++)
                {
                    for (int j = i + 1; j < item.Modifiers.Count; j++)
                    {
                        if (item.Modifiers[i].property == item.Modifiers[j].property)
                        {
                            item.Modifiers[i] = new PropertyModifier(item.Modifiers[i].property, item.Modifiers[i].amount + item.Modifiers[j].amount);
                            item.Modifiers.RemoveAt(j);
                        }
                    }
                }

                if (mp != null && mp.showParticles)
                {
                    ei.SetParticleGlow(mp.particleType, mp.particleColor);
                }

            }
            if (item is UsableItem)
            {
                UsableItem usable = item as UsableItem;

                // apply usable item modifiers
                int group = 0;
                while (group < UsableModifierAdjust.Length - 1 && level >= UsableLevelScale[group + 1])
                {
                    group++;
                }
                if (group >= UsableModifierAdjust.Length)
                {
                    group = UsableModifierAdjust.Length - 1;
                }
                if (group >= UsableQualifiers.Length)
                {
                    group = UsableQualifiers.Length - 1;
                }
                if (group > usable.MaxGroup)
                {
                    group = usable.MaxGroup;
                }

                // adjust potency
                for (int i = 0; i < item.Modifiers.Count; i++)
                {
                    item.Modifiers[i] = new PropertyModifier(item.Modifiers[i].property, (int)((float)item.Modifiers[i].amount * UsableModifierAdjust[group]));
                }

                // adjust name
                if (usable.MaxGroup > 1)
                {
                    if (!string.IsNullOrEmpty(UsableQualifiers[group]))
                        item.Name = UsableQualifiers[group] + " " + item.Name;

                    // adjust texture
                    item.SetTextureOffset(Math.Min(group, 4));
                    item.SetIcon(Math.Min(group, 4));
                }


                // adjust value
                item.Value = (int)(item.ValueAdjust * (float)BalanceManager.GetBaseItemValue((group * 4) + 1));
            }
        }


        public PropertyModifier TranslateProperty(PropertyModifier prop, int level, EquipSlot slot)
        {
            PropertyModifier newProp = new PropertyModifier();
            newProp.property = prop.property;
            if (prop.property == Property.Damage ||
                prop.property == Property.FireDamage ||
                prop.property == Property.ColdDamage ||
                prop.property == Property.LightningDamage ||
                prop.property == Property.PoisonDamage ||
                prop.property == Property.BurnDamage ||
                prop.property == Property.BleedDamage)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseDamage(level));
            }
            else if (prop.property == Property.Armor)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseArmor(level, slot));
            }
            else if (prop.property == Property.Health)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseHealth(level));
            }
            else if (prop.property == Property.Mana)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseMana(level));
            }
            else if (prop.property == Property.SpellDamage)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseSpellDamage(level));
            }
            else if (prop.property == Property.HealthRegen)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseHealthRegen(level));
            }
            else if (prop.property == Property.ManaRegen)
            {
                newProp.amount = (int)((prop.amount / 100f) * BalanceManager.GetBaseManaRegen(level));
            }
            else
            {
                newProp.amount = prop.amount;
            }

            if (newProp.amount == 0)
            {
                newProp.amount = 1;
            }

            return newProp;
        }


        private ItemSprite GetItem(String name)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Name == name)
                    return items[i].CopyItem();
            }

            return null;
        }



        //public string GetQualifier(int level)
        //{
        //    level /= 2;
        //    if (level < qualifiers.Length)
        //    {
        //        return qualifiers[level];
        //    }
        //    return qualifiers[qualifiers.Length - 1];
        //}


        public static string PropertyToString(Property property)
        {
            if (property == Property.AttackSpeed)
            {
                return "Attack Speed";
            }
            else if (property == Property.CastSpeed)
            {
                return "Cast Speed";
            }
            else if (property == Property.ColdDamage)
            {
                return "Cold Damage";
            }
            else if (property == Property.FireDamage)
            {
                return "Fire Damage";
            }
            else if (property == Property.PoisonDamage)
            {
                return "Poison Damage";
            }
            else if (property == Property.BurnDamage)
            {
                return "Burn Damage";
            }
            else if (property == Property.BleedDamage)
            {
                return "Bleed Damage";
            }
            else if (property == Property.LightningDamage)
            {
                return "Lightning Damage";
            }
            else if (property == Property.MoveSpeed)
            {
                return "Move Speed";
            }
            else if (property == Property.SpellDamage)
            {
                return "Spell Damage";
            }
            else if (property == Property.HealthRegen)
            {
                return "Health Regeneration";
            }
            else if (property == Property.ManaRegen)
            {
                return "Mana Regeneration";
            }
            else if (property == Property.CriticalHitChance)
            {
                return "Critical Hit Chance";
            }
            else if (property == Property.MagicResistance)
            {
                return "Magic Resistance";
            }
            else if (property == Property.GoldDrop)
            {
                return "Gold Drops";
            }
            else if (property == Property.MagicDrop)
            {
                return "Magic Item Drop";
            }
            else if (property == Property.StunChance)
            {
                return "Stun Chance";
            }
            else if (property == Property.SlowChance)
            {
                return "Slow Chance";
            }
            else if (property == Property.FearChance)
            {
                return "Fear Chance";
            }
            else if (property == Property.LifeSteal)
            {
                return "Life Steal";
            }
            else if (property == Property.ManaSteal)
            {
                return "Mana Steal";
            }
            else if (property == Property.ReflectDamage)
            {
                return "Reflect Damage";
            }
            else
            {
                return property.ToString();
            }
        }



        static public ParticleType StringToParticleType(string name)
        {
            if (name == "ExplosionWhite")
                return ParticleType.ExplosionWhite;
            else if (name == "Starburst")
                return ParticleType.Starburst;
            else if (name == "Explosion")
                return ParticleType.Explosion;
            else if (name == "Glow")
                return ParticleType.Glow;
            else
                return ParticleType.ExplosionWhite;
        }


        static public MerchantType StringToMerchantType(string type)
        {
            for (int i = 0; i < (int)MerchantType.Num; i++)
            {
                if (type == ((MerchantType)i).ToString())
                {
                    return (MerchantType)i;
                }
            }
            return MerchantType.General;
        }


        static public Color StringToColor(string name)
        {
            int r = 0, g = 0, b = 0;

            // format will be r;g;b
            char[] sep = {';'};
            string[] values = name.Split(sep);
            r = Convert.ToInt32(values[0]);
            g = Convert.ToInt32(values[1]);
            b = Convert.ToInt32(values[2]);

            return new Color(r, g, b);
        }

    }
}
