using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
//using AppLimit.CloudComputing.SharpBox;
//using AppLimit.CloudComputing.SharpBox.StorageProvider.DropBox;

#if !WIN8
using System.IO.IsolatedStorage;
#endif

namespace LegendsOfDescent
{
    public static class SaveGameManager
    {
        public const int maxSaveGameNum = 6;
        public const int timeSavedInterval = 5;

        const UInt32 dataVersion = 10;
        static string[] saveGameNames = new string[maxSaveGameNum];
        static bool[] existingGames = new bool[maxSaveGameNum];
        static PlayerSprite player;
        static int loadedIndex = 0;
        static int totalPlays = 0;
        static string username = string.Empty;
        static string password = string.Empty;
        static DateTime lastSavedTime = DateTime.Now;
        

        public static string Username { get { return username; } set { username = value; } }
        public static string Password { get { return password; } set { password = value; } }
        public static int TotalPlays { get { return totalPlays; } set { totalPlays = value; } }

        static public void Initialize()
        {
            for (int i = 0; i < maxSaveGameNum; i++)
            {
                PlayerSprite player = new PlayerSprite(Vector2.Zero);
                bool result = LoadPlayer(i, ref player, false);
                existingGames[i] = result;
                if (result)
                {
                    saveGameNames[i] = player.Name + "  " + player.GameDifficulty.ToDescription() + "-" + player.Level.ToString();
                }
                else
                {
                    saveGameNames[i] = "Empty";
                }
            }
        }


        static public String SaveName(int index)
        {
            return saveGameNames[index];
        }


        static public bool IsValidSave(int index)
        {
            return existingGames[index];
        }


        static public void RegisterNewPlayer(PlayerSprite player, int saveIndex)
        {
            SaveGameManager.player = player;
            PlayerSprite.loadedInstance = saveIndex;
            saveGameNames[saveIndex] = player.Name + "  " + player.GameDifficulty.ToDescription() + "-" + player.Level.ToString();
            existingGames[saveIndex] = true;
            PersistPlayer();
        }


        static public void Update()
        {
            if (DateTime.Now - lastSavedTime > new TimeSpan(0, 0, 0, timeSavedInterval))
            {
                PersistPlayer();
            }
        }

        static public bool LoadPlayer(int index, ref PlayerSprite player, bool setCurrentPlayer = true)
        {
            Stream isoStream = null;
            BinaryReader reader = null;

            // open isolated storage
            try
            {
                UInt32 fileDataVersion = OpenFileForRead("Player" + index.ToString(), out isoStream, out reader);

                using (reader)
                {
                    using (isoStream)
                    {
                        // read out data version
                        if (fileDataVersion == 1)
                        {
                            // wrote version twice in v1, so we need to throw that away
                            reader.ReadUInt32();
                        }

                        if (setCurrentPlayer)
                        {
                            SaveGameManager.player = player;
                            SaveGameManager.loadedIndex = index;
                        }
                        player.SaveIndex = index;
                        player.Load(reader, (int)fileDataVersion);
                    }
                }

                if (setCurrentPlayer)
                {
                    fileDataVersion = OpenFileForRead("Player" + index.ToString() + "HelpInfo", out isoStream, out reader);

                    using (reader)
                    {
                        using (isoStream)
                        {
                            HelpScreenManager.Instance.Load(reader, (int)fileDataVersion);
                        }
                    }
                }

                try
                {
                    // when transitioning from v5 to v7 data version this will fail
                    fileDataVersion = OpenFileForRead("Player" + index.ToString() + "Levels", out isoStream, out reader);
                }
                catch
                {
                    if (reader != null)
                        reader.Dispose();
                    if (isoStream != null)
                        isoStream.Dispose();

                    reader = null;
                    isoStream = null;
                }

                if (reader != null && isoStream != null)
                {
                    using (reader)
                    {
                        using (isoStream)
                        {
                            LevelOrchestrator.Load(reader, (int)fileDataVersion);
                        }
                    }
                }


                if (fileDataVersion < 7)
                {
                    PlayerSprite prevPlayer = SaveGameManager.player;

                    // we need to generate the town when transition to v7 data
                    DungeonLevel town = new DungeonLevel(LevelOrchestrator.GetConfig(0), true);
                    SaveGameManager.player = player;
                    new MapGenerator(town).GenerateMap();
                    town.PopulateDungeon(SaveGameManager.CurrentPlayer);
                    town.StartDialogueShown = true;
                    SaveGameManager.PersistDungeon(town);

                    if (!setCurrentPlayer)
                        SaveGameManager.player = prevPlayer;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message); 
                return false;
            }

#if WIN8
            DungeonGame.productsManager.RefreshLicenseState();
#endif
            return true;
        }



        static public bool PersistPlayer()
        {
            Stream isoStream = null;
            BinaryWriter writer = null;

            // open isolated storage
            try
            {
                OpenFileForWrite("Player" + PlayerSprite.loadedInstance, out isoStream, out writer);

                using (writer)
                {
                    using (isoStream)
                    {
                        

                        CurrentPlayer.Persist(writer);
                    }
                }

                OpenFileForWrite("Player" + PlayerSprite.loadedInstance + "HelpInfo", out isoStream, out writer);

                using (writer)
                {
                    using (isoStream)
                    {
                        HelpScreenManager.Instance.Persist(writer);
                    }
                }

                OpenFileForWrite("Player" + PlayerSprite.loadedInstance + "Levels", out isoStream, out writer);

                using (writer)
                {
                    using (isoStream)
                    {
                        LevelOrchestrator.Persist(writer);
                    }
                }

                saveGameNames[PlayerSprite.loadedInstance] = player.Name + "  " + player.GameDifficulty.ToDescription() + "-" + player.Level.ToString();

                lastSavedTime = DateTime.Now;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }



        static public bool LoadDungeon(ref DungeonLevel dungeon)
        {
            Stream isoStream = null;
            BinaryReader reader = null;

            // open isolated storage
            try
            {
                UInt32 fileDataVersion = OpenFileForRead("Player" + PlayerSprite.loadedInstance.ToString() + "-Dungeon" + CurrentPlayer.DungeonLevel.ToString(), out isoStream, out reader);

                using (reader)
                {
                    using (isoStream)
                    {
                        dungeon.Load(reader, (int)fileDataVersion);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }



        static public bool PersistDungeon(DungeonLevel dungeon)
        {
            Stream isoStream = null;
            BinaryWriter writer = null;

            // open isolated storage
            try
            {
                OpenFileForWrite("Player" + PlayerSprite.loadedInstance.ToString() + "-Dungeon" + dungeon.Level.ToString(), out isoStream, out writer);

                using (writer)
                {
                    using (isoStream)
                    {
                        dungeon.Persist(writer);
                        Debug.WriteLine("Dungeon " + dungeon.Level.ToString() + " Persisted. Total size of file: " + isoStream.Length.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }


        static public bool LoadSettings()
        {
            Stream isoStream = null;
            BinaryReader reader = null;

            // open isolated storage
            try
            {
                UInt32 fileVersion = OpenFileForRead("Settings", out isoStream, out reader);

                using (reader)
                {
                    using (isoStream)
                    {
                        AudioManager.audioManager.Load(reader, (int)fileVersion);
                        DungeonGame.portraitMode = reader.ReadBoolean();

                        Username = reader.ReadString();
                        Password = reader.ReadString();

                        if (fileVersion >= 9)
                            totalPlays = reader.ReadInt32();

                        DungeonGame.touchEnabled = reader.ReadBoolean();
                        DungeonGame.joystickEnabled = reader.ReadBoolean();
                        if (!DungeonGame.touchSupported)
                        {
                            DungeonGame.touchEnabled = false;
                            DungeonGame.joystickEnabled = false;
                        }

                        //Vector2 dimensions = new Vector2(reader.ReadInt32(), reader.ReadInt32());
                        //DungeonGame.fullScreen = reader.ReadBoolean();
                        //DungeonGame.Instance.SetResolution(dimensions);

                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }



        static public bool PersistSettings()
        {
            Stream isoStream = null;
            BinaryWriter writer = null;

            // open isolated storage
            try
            {
                OpenFileForWrite("Settings", out isoStream, out writer);

                using (writer)
                {
                    using (isoStream)
                    {
                        AudioManager.audioManager.Persist(writer);
                        writer.Write(DungeonGame.portraitMode);

                        writer.Write(Username);
                        writer.Write(Password);

                        writer.Write(totalPlays);

                        writer.Write(DungeonGame.touchEnabled);
                        writer.Write(DungeonGame.joystickEnabled);

                        //writer.Write(DungeonGame.ScreenSize.Width);
                        //writer.Write(DungeonGame.ScreenSize.Height);

                        //writer.Write(DungeonGame.fullScreen);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return false;
            }
            return true;
        }

        private static UInt32 OpenFileForRead(string fileName, out Stream stream, out BinaryReader reader)
        {
#if WINDOWS_PHONE || SILVERLIGHT
            OpenFileForReadBasic(fileName, out stream, out reader);
#else
            OpenFileForReadBasic("SaveGames\\" + fileName, out stream, out reader);
#endif

            // read out and check version
            UInt32 version = reader.ReadUInt32();

            return version;
        }


        public static void OpenFileForReadBasic(string fileName, out Stream stream, out BinaryReader reader)
        {
            stream = Storage.OpenRead(fileName);
            reader = new BinaryReader(stream);
        }


        public static void OpenFileForWriteBasic(string fileName, out Stream stream, out BinaryWriter writer)
        {
            stream = Storage.OpenCreate(fileName);
            writer = new BinaryWriter(stream);
        }


        private static void OpenFileForWrite(string fileName, out Stream stream, out BinaryWriter writer)
        {
#if WINDOWS_PHONE || SILVERLIGHT
            OpenFileForWriteBasic(fileName, out stream, out writer);
#else
             try
             {
#if !WIN8
                 Directory.CreateDirectory(".\\SaveGames");
#endif
             }
             catch { }

            OpenFileForWriteBasic("SaveGames\\" + fileName, out stream, out writer);
#endif
            // write version id
            writer.Write(dataVersion);
        }


        static public PlayerSprite CurrentPlayer
        {
            get { return player; }
        }


        static public void SetTestPlayer()
        {
            PlayerSprite player = new PlayerSprite(Vector2.Zero);
            player.Name = "Wolfgar";
            PlayerSprite.loadedInstance = 0;
            SaveGameManager.PersistPlayer();
            SaveGameManager.player = player;
            SaveGameManager.loadedIndex = 0;
        }

        public static List<PlayerProfile> GetPlayerProfiles()
        {
            List<PlayerProfile> profiles = new List<PlayerProfile>();

            if (DungeonGame.initialLoadComplete)
            {
                // read out all save game profiles
                for (int i = 0; i < maxSaveGameNum; i++)
                {
                    PlayerSprite player = new PlayerSprite(Vector2.Zero, false);
                    bool result = LoadPlayer(i, ref player, false);

                    if (result)
                    {
                        profiles.Add(player.GetPlayerProfile());
                    }
                }
            }

            return profiles;
        }

    }
}
