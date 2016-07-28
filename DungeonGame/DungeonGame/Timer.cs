using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using LegendsOfDescent;

namespace LegendsOfDescent
{
    public enum TimerState
    {
        Running,
        Paused,
        Stopped
    }


    public enum TimerType
    {
        Auto,
        Manual
    }


    public class Timer : ISaveable
    {
        int duration;
        int elapsed;
        TimerState timerState;
        TimerType timerType;

        public Timer(int time, TimerState state = TimerState.Running, TimerType type = TimerType.Auto)
        {
            duration = time;
            elapsed = 0;
            timerState = state;
            timerType = type;
        }


        public void Persist(System.IO.BinaryWriter writer)
        {
            writer.Write((Int32)duration);
            writer.Write((Int32)elapsed);
            writer.Write((Int32)timerState);
            writer.Write((Int32)timerType);
        }


        public bool Load(System.IO.BinaryReader reader, int dataVersion)
        {
            duration = reader.ReadInt32();
            elapsed = reader.ReadInt32();
            timerState = (TimerState)reader.ReadInt32();
            timerType = (TimerType)reader.ReadInt32();

            return true;
        }

        public bool Update(GameTime gameTime)
        {
            if (timerState == TimerState.Running)
            {
                elapsed += gameTime.ElapsedGameTime.Milliseconds;
                if (elapsed > duration)
                {
                    elapsed = 0;

                    if (timerType == TimerType.Manual)
                    {
                        State = TimerState.Stopped;
                    }

                    return true;
                }
            }
            return false;
        }


        public void ResetTimerAndRun(int duration = 0)
        {
            if (duration != 0)
                this.duration = duration;

            elapsed = 0;
            timerState = TimerState.Running;
        }



        public TimerState State
        {
            get { return timerState; }
            set
            {
                timerState = value;
                if (timerState == TimerState.Stopped)
                {
                    elapsed = 0;
                }
            }
        }


        public int ExpireDuration
        {
            get { return duration; }
            set { duration = value; }
        }


        public float PercentComplete
        {
            get { return (float)elapsed / (float)duration; }
        }


        public int Elapsed
        {
            get { return elapsed; }
        }

        public int RemainingDuration
        {
            get 
            {
                if (State == TimerState.Running)
                    return Math.Max(duration - elapsed, 0);
                else
                    return 0;
            }
        }


        public void ExpireNextUpdate()
        {
            elapsed = duration - 1;
        }

        public void AddDuration(int increase)
        {
            duration += increase;
        }
    }
}
