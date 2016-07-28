using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace LegendsOfDescent
{
    public enum StatType
    {
        TotalDamageTaken,
        TotalDamageDealt,
        TotalExperience,
        GoblinsKilled,
        OgresKilled,
        SkeletonKnightsKilled,
        SkeletonArchersKilled,
        ElementalsKilled,
        DrakesKilled,
        GhostsKilled,
        DragnorKilled,
        TotalEnemyKills,
        PoisonDamageDealt,
        PhysicalDamageDealt,
        LightningDamageDealt,
        IceDamageDealt,
        FireDamageDealt,
        PoisonDamageTaken,
        PhysicalDamageTaken,
        LightningDamageTaken,
        IceDamageTaken,
        FireDamageTaken,
        TotalDeaths,
        TotalGoldCollected,
        TotalGoldSpent,
        TotalItemsDropped,
        MagicItemsDropped,
        RareItemsDropped,
        HeroicItemsDropped,
        LegendaryItemsDropped,
        num
    }



    public class StatsManager : ISaveable
    {
        List<UInt64> stats;

        public StatsManager()
        {
            stats = new List<ulong>((int)StatType.num);
            for (int i = 0; i < (int)StatType.num; i++)
            {
                stats.Add(0);
            }
        }


        public void Persist(BinaryWriter writer)
        {
            Int32 number = (Int32)StatType.num;
            writer.Write((Int32)StatType.num);
            for (int i = 0; i < number; i++)
            {
                writer.Write(stats[i]);
            }
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            bool result = true;

            try
            {
                Int32 number = reader.ReadInt32();
                for (int i = 0; i < number; i++)
                {
                    stats[i] = reader.ReadUInt64();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Stats manager failed to load. " + e.Message);
            }

            return result;
        }


        public void Add(StatType type, UInt64 amount)
        {
            stats[(int)type] += amount;
        }

        public void AddDamage(DamageType type, UInt64 amount, bool isDealt)
        {
            if (isDealt)
            {
                Add(StatType.TotalDamageDealt, amount);
                switch (type)
                {
                    case DamageType.Fire:
                        Add(StatType.FireDamageDealt, amount);
                        break;
                    case DamageType.Ice:
                        Add(StatType.IceDamageDealt, amount);
                        break;
                    case DamageType.Lightning:
                        Add(StatType.LightningDamageDealt, amount);
                        break;
                    case DamageType.Physical:
                        Add(StatType.PhysicalDamageDealt, amount);
                        break;
                    case DamageType.Poison:
                        Add(StatType.PoisonDamageDealt, amount);
                        break;
                }
            }
            else
            {
                Add(StatType.TotalDamageTaken, amount);
                switch (type)
                {
                    case DamageType.Fire:
                        Add(StatType.FireDamageTaken, amount);
                        break;
                    case DamageType.Ice:
                        Add(StatType.IceDamageTaken, amount);
                        break;
                    case DamageType.Lightning:
                        Add(StatType.LightningDamageTaken, amount);
                        break;
                    case DamageType.Physical:
                        Add(StatType.PhysicalDamageTaken, amount);
                        break;
                    case DamageType.Poison:
                        Add(StatType.PoisonDamageTaken, amount);
                        break;
                }
            }
        }

        public void AddItemDrop(ItemSprite item)
        {
            Add(StatType.TotalItemsDropped, 1);

            switch (item.ItemClass)
            {
                case ItemClass.Magic:
                    Add(StatType.MagicItemsDropped, 1);
                    break;
                case ItemClass.Rare:
                    Add(StatType.RareItemsDropped, 1);
                    break;
                case ItemClass.Heroic:
                    Add(StatType.HeroicItemsDropped, 1);
                    break;
                case ItemClass.Legendary:
                    Add(StatType.LegendaryItemsDropped, 1);
                    break;
            }
        }


        public void AddEnemyDied(EnemyType type)
        {
            Add(StatType.TotalEnemyKills, 1);
            switch (type)
            {
                case EnemyType.Drake:
                    Add(StatType.DrakesKilled, 1);
                    break;
                case EnemyType.Ghost:
                    Add(StatType.GhostsKilled, 1);
                    break;
                case EnemyType.Goblin:
                    Add(StatType.GoblinsKilled, 1);
                    break;
                case EnemyType.LavaTroll:
                    Add(StatType.ElementalsKilled, 1);
                    break;
                case EnemyType.Ogre:
                    Add(StatType.OgresKilled, 1);
                    break;
                case EnemyType.SkeletonArcher:
                    Add(StatType.SkeletonArchersKilled, 1);
                    break;
                case EnemyType.SkeletonKnight:
                    Add(StatType.SkeletonKnightsKilled, 1);
                    break;
                case EnemyType.DwarfBoss | EnemyType.DwarfBoss2 | EnemyType.DwarfBoss3:
                    Add(StatType.DragnorKilled, 1);
                    break;
            }
        }


        public UInt64 Get(StatType type)
        {
            return stats[(int)type];
        }


        public string GetName(StatType type)
        {
            string name = "";
            switch (type)
            {
                case StatType.TotalDamageTaken:
                    name = "Total Damage Taken";
                    break;
                case StatType.TotalDamageDealt:
                    name = "Total Damage Dealt";
                    break;
                case StatType.TotalExperience:
                    name = "Total Experience Earned";
                    break;
                case StatType.GoblinsKilled:
                    name = "Goblins Killed";
                    break;
                case StatType.OgresKilled:
                    name = "Ogres Killed";
                    break;
                case StatType.SkeletonKnightsKilled:
                    name = "Skeleton Knights Killed";
                    break;
                case StatType.SkeletonArchersKilled:
                    name = "Skeleton Archers Killed";
                    break;
                case StatType.ElementalsKilled:
                    name = "Elementals Killed";
                    break;
                case StatType.DrakesKilled:
                    name = "Drakes Killed";
                    break;
                case StatType.GhostsKilled:
                    name = "Ghosts Killed";
                    break;
                case StatType.DragnorKilled:
                    name = "Dragnor Defeated";
                    break;
                case StatType.TotalEnemyKills:
                    name = "Total Enemies Killed";
                    break;
                case StatType.PoisonDamageDealt:
                    name = "Poison Damage Dealt";
                    break;
                case StatType.PhysicalDamageDealt:
                    name = "Physical Damage Dealt";
                    break;
                case StatType.LightningDamageDealt:
                    name = "Lightning Damage Dealt";
                    break;
                case StatType.IceDamageDealt:
                    name = "Ice Damage Dealt";
                    break;
                case StatType.FireDamageDealt:
                    name = "Fire Damage Dealt";
                    break;
                case StatType.PoisonDamageTaken:
                    name = "Poison Damage Taken";
                    break;
                case StatType.PhysicalDamageTaken:
                    name = "Physical Damage Taken";
                    break;
                case StatType.LightningDamageTaken:
                    name = "Lightning Damage Taken";
                    break;
                case StatType.IceDamageTaken:
                    name = "Ice Damage Taken";
                    break;
                case StatType.FireDamageTaken:
                    name = "Fire Damage Taken";
                    break;
                case StatType.TotalDeaths:
                    name = "Total Player Deaths";
                    break;
                case StatType.TotalGoldCollected:
                    name = "Total Gold Collected";
                    break;
                case StatType.TotalGoldSpent:
                    name = "Total Gold Spent";
                    break;
                case StatType.TotalItemsDropped:
                    name = "Total Items Found";
                    break;
                case StatType.MagicItemsDropped:
                    name = "Magic Items Found";
                    break;
                case StatType.RareItemsDropped:
                    name = "Rare Items Found";
                    break;
                case StatType.HeroicItemsDropped:
                    name = "Heroic Items Found";
                    break;
                case StatType.LegendaryItemsDropped:
                    name = "Legendary Items Found";
                    break;
            }

            return name;
        }

    }
}
