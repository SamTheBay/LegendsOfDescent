using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.IO;
using Microsoft.Xna.Framework;

#if WINDOWS_PHONE || SILVERLIGHT
    using System.Windows;
#endif

namespace LegendsOfDescent
{
    public static class LevelOrchestrator
    {
        private static Dictionary<int, LevelConfig> levelToConfig;
        private static readonly EnemyType[] RandomEnemyTypes = 
        {
            EnemyType.Drake,
            EnemyType.Ghost,
            EnemyType.Goblin,
            EnemyType.LavaTroll,
            EnemyType.Ogre,
            EnemyType.SkeletonArcher,
            EnemyType.SkeletonKnight
        };

        internal static void Load(BinaryReader reader = null, int dataVersion = 0)
        {
            var serializer = new XmlSerializer(typeof(LevelConfigs));

#if WIN8
            using (var stream = TitleContainer.OpenStream("Assets/Data/Levels.xml"))
#else
            using (var stream = TitleContainer.OpenStream("Content/Data/Levels.xml"))
#endif
            {
                var configs = serializer.Deserialize(stream) as LevelConfigs;
                levelToConfig = configs.Levels.ToDictionary(l => l.Floor, l => l);
            }

            // now load randomly generated level info
            if (reader != null)
            {
                int levels = reader.ReadInt32();
                for (int i = 0; i < levels; i++)
                {
                    int level = reader.ReadInt32();
                    LevelConfig config = new LevelConfig();
                    config.Load(reader, dataVersion);

                    if (!levelToConfig.ContainsKey(level))
                    {
                        levelToConfig.Add(level, config);
                    }
                }
            }
        }

        internal static void Persist(BinaryWriter writer)
        {
            writer.Write((Int32)levelToConfig.Count);
            foreach (KeyValuePair<int,LevelConfig> level in levelToConfig)
            {
                writer.Write((Int32)level.Key);
                level.Value.Persist(writer);
            }
        }

        internal static LevelConfig GetConfig(int level)
        {
            LevelConfig config;
            if (!levelToConfig.TryGetValue(level, out config))
            {
                config = new LevelConfig();
                config.Floor = level;
                config.TileSet = new TileSetConfig();
                config.TileSet.Name = TileSet.RandomNames.Random();
                if (level % 5 == 0)
                {
                    // every 5 levels we have a boss encounter
                    config.Type = "Boss";
                    config.Enemies = new List<EnemyConfig>();
                    if (level <= 5)
                        config.Enemies.Add(new EnemyConfig() { Type = EnemyType.DwarfBoss, Count = 1 });
                    else if (level <= 15)
                        config.Enemies.Add(new EnemyConfig() { Type = EnemyType.DwarfBoss2, Count = 1 });
                    else
                        config.Enemies.Add(new EnemyConfig() { Type = EnemyType.DwarfBoss3, Count = 1 });
                    config.Enemies.Add(new EnemyConfig() { Type = EnemyType.SkeletonKnight, Count = 0 });

                    OpeningTextConfig text = new OpeningTextConfig();
                    text.Name = DwarfBoss.DwarfBossName;
                    text.Text = "My soul continues to haunt the dungeons hungering for revenge. Come and taste my wrath!";
                    config.OpeningText = text;
                }
                else
                {
                    config.Type = "Dungeon";
                    config.Enemies = CreateRandomListOfUniqueEnemyConfigs(2);
                    levelToConfig.Add(level, config);
                }


            }

            return config;
        }

        private static List<EnemyConfig> CreateRandomListOfUniqueEnemyConfigs(int length)
        {
            if (length > (int)EnemyType.Count)
            {
                throw new Exception("Not enough unique enemy types.");
            }

            List<EnemyConfig> enemies = new List<EnemyConfig>(length);

            for (int i = 0; i < length; i++)
            {
                EnemyType type = EnemyType.Ogre;
                while (enemies.Any(e => e.Type == (type = RandomEnemyTypes.Random()))) ;
                enemies.Add(new EnemyConfig() { Type = type, Count = Util.Random.Next(90, 121) });
            }

            return enemies;
        }
    }
}
