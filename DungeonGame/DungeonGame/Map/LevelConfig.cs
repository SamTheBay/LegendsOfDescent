using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using LegendsOfDescent;

namespace LegendsOfDescent
{
    [XmlRoot("Levels")]
    public class LevelConfigs
    {
        public LevelConfigs()
        {
            Levels = new List<LevelConfig>();
        }

        [XmlElement("Level")]
        public List<LevelConfig> Levels { get; set; }
    }

    public class LevelConfig : ISaveable
    {
        [XmlAttribute]
        public int Floor { get; set; }

        [XmlAttribute]
        public string Type { get { return type; } set { type = value; } }
        public string type = "Dungeon";

        public TileSetConfig TileSet { get; set; }

        public OpeningTextConfig OpeningText { get; set; } 

        [XmlArrayItem("Enemy")]
        public List<EnemyConfig> Enemies { get; set; }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(Floor);
            writer.Write(TileSet);
            writer.Write(Enemies);
            writer.Write(Type);
            writer.Write(OpeningText != null);
            if (OpeningText != null)
                OpeningText.Persist(writer);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            Floor = reader.ReadInt32();
            TileSet = reader.Read<TileSetConfig>(dataVersion);
            Enemies = reader.ReadList<EnemyConfig>(dataVersion);
            if (dataVersion >= 6)
            {
                Type = reader.ReadString();
                if (reader.ReadBoolean())
                {
                    OpeningText = new OpeningTextConfig();
                    OpeningText.Load(reader, dataVersion);
                }
            }
            else
            {
                Type = "Dungeon";
            }
            return true;
        }
    }

    public class OpeningTextConfig : ISaveable
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string Text { get; set; }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write(Text);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            Name = reader.ReadString();
            Text = reader.ReadString();
            return true;
        }
    }

    public class TileSetConfig : ISaveable
    {
        [XmlAttribute]
        public string Name { get; set; }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(Name);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            Name = reader.ReadString();
            return true;
        }
    }

    public class EnemyConfig : ISaveable
    {
        [XmlAttribute]
        public EnemyType Type { get; set; }

        [XmlAttribute]
        public ElementalType Element { get; set; }

        [XmlAttribute]
        public int Count { get; set; }

        public void Persist(BinaryWriter writer)
        {
            writer.Write((int)Type);
            writer.Write((int)Element);
            writer.Write(Count);
        }

        public bool Load(BinaryReader reader, int dataVersion)
        {
            Type = (EnemyType)reader.ReadInt32();
            Element = (ElementalType)reader.ReadInt32();
            Count = reader.ReadInt32();
            return true;
        }
    }
}
