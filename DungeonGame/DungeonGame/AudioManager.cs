using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;
using System.IO;
using LegendsOfDescent;


namespace LegendsOfDescent
{
    public abstract class SoundInstance
    {
        public abstract void LoadEffect(ContentManager content);

        public abstract void UnloadEffect();

        public abstract int Play();

        public abstract void StopAll();

        public abstract String Name();
    }


    public class MusicInstance : SoundInstance
    {
        Song song;
        String name;
        String location;
        float volume;

        public MusicInstance(String EffectLocation, String Name)
        {
            Initialize(EffectLocation, Name, 1f);
        }

        public MusicInstance(String EffectLocation, String Name, float volume)
        {
            Initialize(EffectLocation, Name, volume);
        }

        public void Initialize(String Location, String Name, float volume)
        {
            this.volume = volume;
            this.location = Location;
            this.name = Name;
        }

        public override void LoadEffect(ContentManager content)
        {
            song = content.Load<Song>(location);
        }

        public override void UnloadEffect()
        {
            song.Dispose();
            song = null;
        }

        public override int Play()
        {     
            if (MediaPlayer.GameHasControl)
            {
                try
                {
                    // Check if we are already playing this song
                    if (MediaPlayer.State == MediaState.Playing && AudioManager.lastSong == song)
                        return 0;

                    MediaPlayer.Play(song);
                    //MediaPlayer.Volume = volume;
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.IsShuffled = false;
                    AudioManager.lastSong = song;

                }
                catch (Exception)
                {
                    // swallow exceptions that show up when debugger is attached as a result
                    // of WP7 idiosyncrasies. I think this issue is gone in Mango.
                }
            }
            return 0;
        }

        public override void StopAll()
        {
            if (MediaPlayer.GameHasControl)
            {
                MediaPlayer.Stop();
            }
        }

        public override String Name()
        {
            return name;
        }
    }


    public class SFXInstance : SoundInstance
    {
        SoundEffect effect;
        String name;
        String effectLocation;
        int maxIntanceCount;
        SoundEffectInstance [] instances;
        bool isMusic;
        float volume;

        public SFXInstance(String EffectLocation, String Name, int MaxInstances)
        {
            Initialize(EffectLocation, Name, MaxInstances, 1f, false);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, bool isMusic)
        {
            Initialize(EffectLocation, Name, MaxInstances, 1f, isMusic);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, float volume)
        {
            Initialize(EffectLocation, Name, MaxInstances, volume, false);
        }

        public SFXInstance(String EffectLocation, String Name, int MaxInstances, float volume, bool isMusic)
        {
            Initialize(EffectLocation, Name, MaxInstances, volume, isMusic);
        }

        public void Initialize(String EffectLocation, String Name, int MaxInstances, float volume, bool isMusic)
        {
            this.volume = volume;
            this.effectLocation = EffectLocation;
            this.isMusic = isMusic;
            this.name = Name;
            this.maxIntanceCount = MaxInstances;
        }

        public override void LoadEffect(ContentManager content)
        {
            if (effect == null)
                effect = content.Load<SoundEffect>(effectLocation);
            if (instances == null)
                instances = new SoundEffectInstance[maxIntanceCount];
            for (int i = 0; i < maxIntanceCount; i++)
            {
                if (instances[i] == null)
                {
                    instances[i] = effect.CreateInstance();
                    instances[i].Volume = volume;
                }
                
            }
        }

        public override void UnloadEffect()
        {
            if (instances != null)
            {
                for (int i = 0; i < maxIntanceCount; i++)
                {
                    if (instances[i] != null)
                    {
                        instances[i].Dispose();
                        instances[i] = null;
                    }
                }
                instances = null;
            }

            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
            GC.Collect();
        }

        public override int Play()
        {
            for (int i = 0; i < maxIntanceCount; i++)
            {
                if (instances[i].State == SoundState.Stopped)
                {
                    instances[i].Volume = volume;
                    instances[i].Play();
                    return i;
                }
            }
            return int.MaxValue;
        }

        public override void StopAll()
        {
            for (int i = 0; i < maxIntanceCount; i++)
            {
                if (instances != null)
                    instances[i].Stop();
            }
        }

        public override String Name()
        {
            return name;
        }
    }
    


    public class AudioManager : ISaveable
    {
        public static AudioManager audioManager = null;
        private List<SoundInstance> SFXList;
        public static bool sfxOn = true;
        public static bool musicOn = true;
        public static Song lastSong = null;


        private AudioManager()
        {
            // load up all of the sound files
            SFXList = new List<SoundInstance>();
            SFXList.Add(new SFXInstance("Audio\\Body_Hit_10", "Hit1", 4));
            SFXList.Add(new SFXInstance("Audio\\Body_Hit_11", "Hit2", 4));
            SFXList.Add(new SFXInstance("Audio\\Body_Hit_08", "Hit3", 4));
            SFXList.Add(new SFXInstance("Audio\\Body_Hit_43", "Hit4", 4)); // arrow hit
            SFXList.Add(new SFXInstance("Audio\\Trumpet", "Trumpet", 1));
            SFXList.Add(new SFXInstance("Audio\\Boom", "Boom", 4));
            SFXList.Add(new SFXInstance("Audio\\Drop", "Drop", 2));
            SFXList.Add(new SFXInstance("Audio\\MetalDrop", "MetalDrop", 2));
            SFXList.Add(new SFXInstance("Audio\\BowShoot", "BowShoot", 2));
            SFXList.Add(new SFXInstance("Audio\\Spark", "Spark", 4));
            SFXList.Add(new SFXInstance("Audio\\StairsDown", "StairsDown", 1));
            SFXList.Add(new SFXInstance("Audio\\StairsUp", "StairsUp", 1));
            SFXList.Add(new SFXInstance("Audio\\TownPort", "TownPort", 1));
            AddVariations("Swing", 2, 4);
            AddVariations("BoxOpen", 3, 2);
            AddVariations("Ork", 3, 2);
            AddVariations("Ghost", 5, 2);
            AddVariations("Chain", 3, 2);
            AddVariations("Skel", 4, 2);
            AddVariations("Element", 4, 2);
            AddVariations("Potion", 3, 2);
            AddVariations("Sword", 6, 2);
            AddVariations("Frozen", 3, 2);
            AddVariations("HeavyDrop", 3, 2);
            AddVariations("Magic", 5, 2);
            AddVariations("Fire", 3, 2);
            AddVariations("Gold", 3, 2);
            AddVariations("Invis", 5, 2);
            AddVariations("Fear", 5, 2);
            AddVariations("Slow", 3, 2);
            AddVariations("SpeedUp", 3, 2);
            AddVariations("Stomp", 4, 2);
            AddVariations("Poison", 3, 2);
            AddVariations("Shout", 3, 2);
            AddVariations("Stone", 4, 2);
            AddVariations("TrapSet", 5, 2);
            AddVariations("TrapSpring", 4, 2);
            AddVariations("SmallExplosion", 3, 4);
            AddVariations("DoorOpen", 3, 2);
            AddVariations("DoorClose", 4, 2);
            SFXList.Add(new SFXInstance("Audio\\menu_select", "MenuSelect", 1));
            SFXList.Add(new MusicInstance("Audio\\MainMusic", "MainMusic"));
            SFXList.Add(new MusicInstance("Audio\\MainMusic2", "MainMusic2"));
            SFXList.Add(new MusicInstance("Audio\\MainMusic3", "MainMusic3"));
        }


        private void AddVariations(string name, int variations, int instances)
        {
            for (int i = 1; i <= variations; i++)
            {
                string currentName = name + i.ToString();
                SFXList.Add(new SFXInstance("Audio\\" + currentName, currentName, instances));
            }
        }


        public static void Initialize()
        {
            audioManager = new AudioManager();

            SaveGameManager.LoadSettings();

            if (MediaPlayer.State == MediaState.Playing)
            {
                musicOn = false;
            }
        }



        public void PlaySFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    if ((SFXList[i] is SFXInstance && !sfxOn) ||
                        (SFXList[i] is MusicInstance && !musicOn))
                    {
                        // this effect is off in the options
                        return;
                    }

                    SFXList[i].Play();
                }
            }
        }


        public void StopAllSFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].StopAll();
                }
            }
        }

        public void LoadSFX(String soundName, ContentManager content)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].LoadEffect(content);
                    break;
                }
            }
        }

        public void LoadSFX(int index1, int index2)
        {
            for (int i = index1; i <= index2; i++)
                SFXList[i].LoadEffect(DungeonGame.Instance.Content);
        }


        public void LoadAllSFX()
        {
            LoadSFX(0, SFXList.Count - 1);
        }

        public void UnloadSFX(String soundName)
        {
            for (int i = 0; i < SFXList.Count; i++)
            {
                if (soundName == SFXList[i].Name())
                {
                    SFXList[i].UnloadEffect();
                    break;
                }
            }
        }

        public int SFXNum
        {
            get { return SFXList.Count; }
        }

        public void Persist(BinaryWriter writer)
        {
            writer.Write(musicOn);
            writer.Write(sfxOn);
        }


        public bool Load(BinaryReader reader, int dataVersion)
        {
            musicOn = reader.ReadBoolean();
            sfxOn = reader.ReadBoolean();
            return true;
        }

        public void PauseMusic()
        {
            if (musicOn)
            {
                MediaPlayer.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (musicOn)
            {
                MediaPlayer.Resume();
            }
        }
    }
}
