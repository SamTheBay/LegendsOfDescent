using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace LegendsOfDescent
{
    public enum TouchLocationState
    {
        Invalid,
        Moved,
        Pressed,
        Released
    }


    public class TouchLocation
    {
        public int Id;
        public Vector2 Position;
        public TouchLocationState State;

        public TouchLocation(int Id, TouchLocationState State, Vector2 Position)
        {
            this.Id = Id;
            this.Position = Position;
            this.State = State;
        }
    }

    public class TouchCollection : List<TouchLocation>
    {

    }


    public enum GestureType
    {
        None = 0x0,
        Tap = 0x1,
        DoubleTap = 0x2,
        Hold = 0x4,
        HorizontalDrag = 0x8,
        VerticalDrag = 0x10,
        FreeDrag = 0x20,
        Pinch = 0x40,
        Flick = 0x80,
        DragComplete = 0x100,
        PinchComplete = 0x200
    }


    public struct GestureSample
    {
        public Vector2 Delta;
        public Vector2 Delta2;
        public GestureType GestureType;
        public Vector2 Position;
        public Vector2 Position2;
        public TimeSpan Timestamp;

        public GestureSample(GestureType gestureType, TimeSpan timestamp, Vector2 position, Vector2 position2, Vector2 delta, Vector2 delta2)
        {
            this.GestureType = gestureType;
            this.Delta = delta;
            this.Delta2 = delta2;
            this.Position = position;
            this.GestureType = gestureType;
            this.Position2 = position2;
            this.Timestamp = timestamp;
        }
    }


}
