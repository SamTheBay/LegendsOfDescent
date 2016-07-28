using System;
using System.Collections.Generic;
using ProtoBuf;

namespace LegendsOfDescent
{
    [ProtoContract]
    public class PlayerProfile
    {
        public PlayerProfile()
        {
            Items = new List<ItemProfile>(5);
        }

        [ProtoMember(1)]
        public Guid Id { get; set; }

        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(3)]
        public GameDifficulty Difficulty { get; set; }

        [ProtoMember(4)]
        public string Class { get; set; }

        [ProtoMember(5)]
        public int Level { get; set; }

        [ProtoMember(6)]
        public int Gold { get; set; }

        [ProtoMember(7)]
        public int DungeonLevel { get; set; }

        [ProtoMember(8)]
        public int DungeonLevelsGenerated { get; set; }

        [ProtoMember(9)]
        public int Experience { get; set; }

        [ProtoMember(10)]
        public int Damage { get; set; }

        [ProtoMember(11)]
        public int Armor { get; set; }

        [ProtoMember(12)]
        public int SpellDamage { get; set; }

        [ProtoMember(13)]
        public int CriticalHitChance { get; set; }

        [ProtoMember(14)]
        public int MagicResistance { get; set; }

        [ProtoMember(15)]
        public int ManaRegen { get; set; }

        [ProtoMember(16)]
        public int MaxMana { get; set; }

        [ProtoMember(17)]
        public int HealthRegen { get; set; }

        [ProtoMember(18)]
        public int MaxHealth { get; set; }

        [ProtoMember(19)]
        public int ManaCost { get; set; }

        [ProtoMember(20)]
        public int CastSpeed { get; set; }

        [ProtoMember(21)]
        public int AttackSpeed { get; set; }

        [ProtoMember(22)]
        public int MovementSpeed { get; set; }

        [ProtoMember(23)]
        public List<ItemProfile> Items { get; set; }
    }
}
