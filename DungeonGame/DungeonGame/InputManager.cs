
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


#if WINDOWS_PHONE || WIN8
    using Microsoft.Xna.Framework.Input.Touch;
#endif


namespace LegendsOfDescent
{
    public enum keyboardActionSet
    {
        Inventory,
        Options,
        Abilities,
        Stats,
        Quest,
        HealthPot,
        ManaPot,
        Back,
        H1,
        H2,
        H3,
        H4,
        H5,
        H6,
        H7,
        H8,
        H9,
        Up,
        Down,
        Left,
        Right,
        ShowItemNames,
        Length
    }


    public struct TouchStart
    {
        public int touchID;
        public Vector2 startLocation;
    }



    public static class InputManager
    {
        public const int maxTouches = 21;
        public const int mouseTouchID = 9999999;
        public const int freeTouchID = 9999998;
#if WIN8
        private static readonly List<Microsoft.Xna.Framework.Input.Touch.GestureSample> XNAGestures = new List<Microsoft.Xna.Framework.Input.Touch.GestureSample>();
#endif
        private static TouchCollection TouchState = new TouchCollection();
        private static readonly List<GestureSample> Gestures = new List<GestureSample>();
        private static bool BackTriggered = false;
        private static int clearTimeElapsed = 0;
        private static int clearTimeDuration = 0;
        private static MouseState mouseState;
        private static MouseState prevMouseState;
        private static Vector2 mousePosition = new Vector2();
        private static TouchStart[] touchStarts = new TouchStart[maxTouches];

        // maps keys to the actions in the enum above
        private static Keys[] keyMap = {Keys.I,
                                        Keys.O,
                                        Keys.A,
                                        Keys.S,
                                        Keys.Q,
                                        Keys.H,
                                        Keys.M,
                                        Keys.Escape,
                                        Keys.D1,
                                        Keys.D2,
                                        Keys.D3,
                                        Keys.D4,
                                        Keys.D5,
                                        Keys.D6,
                                        Keys.D7,
                                        Keys.D8,
                                        Keys.D9,
                                        Keys.Up,
                                        Keys.Down,
                                        Keys.Left,
                                        Keys.Right,
                                        Keys.Z};

        // keyboard actions
        private static KeyboardState keyboardState;
        private static KeyboardState prevKeyboardState;
        private static bool[] keyboardActions = new bool[(int)keyboardActionSet.Length];

        private static ButtonState PreviousBackState = ButtonState.Released;

        // gesture detection
        private static bool startDragging = false;
        private const float minDragDistance = 15;


        static InputManager()
        {
            for (int j = 0; j < touchStarts.Length; j++)
            {
                touchStarts[j] = new TouchStart();
                touchStarts[j].touchID = freeTouchID;
            }
        }

 
        public static void Update(GameTime gameTime)
        {
            Gestures.Clear();
            prevKeyboardState = keyboardState;

            for (int i = 0; i < keyboardActions.Length; i++)
            {
                keyboardActions[i] = false;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed && PreviousBackState == ButtonState.Released)
            {
                BackTriggered = true;
            }
            else
            {
                BackTriggered = false;
            }
            PreviousBackState = GamePad.GetState(PlayerIndex.One).Buttons.Back;

#if WINDOWS_PHONE
            
            TouchState = TouchPanel.GetState();
            GetPress(ref lastDownPosition);

            while (TouchPanel.IsGestureAvailable)
            {
                Gestures.Add(TouchPanel.ReadGesture());
            }
#else
            XNAGestures.Clear();
            prevMouseState = mouseState;
            mouseState = Mouse.GetState();
            mousePosition.X = mouseState.X;
            mousePosition.Y = mouseState.Y;

            TouchState.Clear();
            var XNATouchState = TouchPanel.GetState();
            for (int i = 0; i < XNATouchState.Count; i++)
            {
                TouchState.Add(new TouchLocation(XNATouchState[i].Id, ConvertTouchState(XNATouchState[i].State), XNATouchState[i].Position));
            }

            GetPress(ref lastDownPosition);

            // Add the mouse as a touch
            if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Released)
            {
                TouchState.Add(new TouchLocation(mouseTouchID, TouchLocationState.Pressed, mousePosition));
                lastDownPosition = mousePosition;
            }
            else if (mouseState.LeftButton == ButtonState.Released && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                TouchState.Add(new TouchLocation(mouseTouchID, TouchLocationState.Released, mousePosition));
            }
            else if (mouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton == ButtonState.Pressed)
            {
                TouchState.Add(new TouchLocation(mouseTouchID, TouchLocationState.Moved, mousePosition));
            }

            // take note of touch start locations
            for (int i = 0; i < TouchState.Count; i++)
            {
                if (TouchState[i].State == TouchLocationState.Pressed)
                {
                    for (int j = 0; j < touchStarts.Length; j++)
                    {
                        if (touchStarts[j].touchID == freeTouchID)
                        {
                            touchStarts[j].touchID = TouchState[i].Id;
                            touchStarts[j].startLocation = TouchState[i].Position;
                            break;
                        }
                    }
                }
            }

            // detect gestures
            for (int i = 0; i < TouchState.Count; i++)
            {
                TouchLocation touch = TouchState[i];

                // find the start position
                Vector2 touchStart = Vector2.Zero;
                int touchStartIndex = 0;
                for (int j = 0; j < touchStarts.Length; j++)
                {
                    if (touchStarts[j].touchID == touch.Id)
                    {
                        touchStartIndex = j;
                        touchStart = touchStarts[j].startLocation;
                    }
                }

                if (touch.State == TouchLocationState.Released)
                {
                    touchStarts[touchStartIndex].touchID = freeTouchID;
                    
                    if (Vector2.Distance(touchStart, touch.Position) < minDragDistance && !startDragging)
                    {
                        if ((TouchPanel.EnabledGestures & Microsoft.Xna.Framework.Input.Touch.GestureType.Tap) == Microsoft.Xna.Framework.Input.Touch.GestureType.Tap)
                        {
                            Gestures.Add(new GestureSample(GestureType.Tap, gameTime.TotalGameTime, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                        }
                    }
                    else if (startDragging)
                    {
                        if ((TouchPanel.EnabledGestures & Microsoft.Xna.Framework.Input.Touch.GestureType.DragComplete) == Microsoft.Xna.Framework.Input.Touch.GestureType.DragComplete)
                        {
                            Gestures.Add(new GestureSample(GestureType.DragComplete, gameTime.TotalGameTime, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                            startDragging = false;
                        }
                    }
                }
                else if (touch.State == TouchLocationState.Moved)
                {
                    if ((TouchPanel.EnabledGestures & Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag) == Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag)
                    {
                        if (Vector2.Distance(touchStart, touch.Position) > minDragDistance && !startDragging)
                        {
                            startDragging = true;
                            Gestures.Add(new GestureSample(GestureType.FreeDrag, gameTime.TotalGameTime, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                        }
                        else if (startDragging)
                        {
                            Gestures.Add(new GestureSample(GestureType.FreeDrag, gameTime.TotalGameTime, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
                        }
                    }
                }
            }

#endif

            // due to delay in gesture detection, sometimes it is necessary for us to clear out
            // the gestures for a period of time during transitions
            if (clearTimeDuration > clearTimeElapsed)
            {
                Gestures.Clear();
                clearTimeElapsed += gameTime.ElapsedGameTime.Milliseconds;
                return;
            }

            // extract hotkey info
            keyboardState = Keyboard.GetState();
            for (int i = 0; i < (int)keyboardActionSet.Length; i++)
            {
                if (keyboardState.IsKeyDown(keyMap[i]) && prevKeyboardState.IsKeyUp(keyMap[i]))
                {
                    keyboardActions[i] = true;
                }
            }


            if (keyboardActions[(int)keyboardActionSet.Back])
            {
                BackTriggered = true;
            }

        }



        public static bool GetKeyboardAction(keyboardActionSet action)
        {
            return keyboardActions[(int)action];
        }



        public static bool IsKeyTriggered(Keys key)
        {
            if (prevKeyboardState.IsKeyUp(key) && keyboardState.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }

        public static bool IsKeyPressed(Keys key)
        {
            if (keyboardState.IsKeyDown(key))
            {
                return true;
            }
            return false;
        }


        public static void ClearInputForPeriod(int clearTimeInMilliSec)
        {
            Gestures.Clear();
            clearTimeDuration = clearTimeInMilliSec;
            clearTimeElapsed = 0;
        }


        public static bool IsLocationPressed(Rectangle loc)
        {
            for (int i = 0; i < TouchState.Count; i++)
            {
                if ((TouchState[i].State == TouchLocationState.Pressed || TouchState[i].State == TouchLocationState.Moved) && loc.Contains((int)TouchState[i].Position.X, (int)TouchState[i].Position.Y))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLocationPressed(Rectangle loc, ref Vector2 pressLoc)
        {
            for (int i = 0; i < TouchState.Count; i++)
            {
                if ((TouchState[i].State == TouchLocationState.Pressed || TouchState[i].State == TouchLocationState.Moved) && loc.Contains((int)TouchState[i].Position.X, (int)TouchState[i].Position.Y))
                {
                    if (pressLoc != null)
                    {
                        pressLoc.X = TouchState[i].Position.X;
                        pressLoc.Y = TouchState[i].Position.Y;
                    }
                    return true;
                }
            }
            return false;
        }



        public static bool IsLocationTapped(Rectangle loc)
        {
            for (int i = 0; i < Gestures.Count; i++)
            {
                if (Gestures[i].GestureType == GestureType.Tap && loc.Contains((int)Gestures[i].Position.X, (int)Gestures[i].Position.Y))
                {
                    return true;
                }
            }
            return false;
        }



        public static bool GetTouchPoint(ref Vector2 touchPoint)
        {
            for (int i = 0; i < TouchState.Count; i++)
            {
                if ((TouchState[i].State == TouchLocationState.Pressed || TouchState[i].State == TouchLocationState.Moved))
                {
                    touchPoint = TouchState[i].Position;
                    return true;
                }
            }
            return false;
        }

        public static bool GetTapPoint(ref Vector2 touchPoint)
        {
            for (int i = 0; i < Gestures.Count; i++)
            {
                // Note: we translate the coordinate plane for the rotation in line here...
                if (Gestures[i].GestureType == GestureType.Tap)
                {
                    touchPoint = Gestures[i].Position;
                    return true;
                }
            }
            return false;
        }


        public static bool GetPress(ref Vector2 pressLoc)
        {
            if (TouchState.Count == 1 && TouchState[0].State == TouchLocationState.Pressed)
            {
                pressLoc = TouchState[0].Position;
                return true;
            }
            return false;
        } 


        public static bool IsBackTriggered()
        {
            return BackTriggered;
        }



        static public TouchCollection GetTouchCollection()
        {
            return TouchState;
        }




        static bool isDragging = false;
        static Vector2 lastDownPosition = new Vector2();
        static Vector2 lastDragPosition = new Vector2();
        public static bool GetDrag(ref Vector2 position, ref bool isStart, ref bool isFinished)
        {
            for (int i = 0; i < Gestures.Count; i++)
            {
                if (Gestures[i].GestureType == GestureType.FreeDrag && !isDragging)
                {
                    Debug.WriteLine("Start drag detected");
                    position = lastDragPosition = lastDownPosition;
                    isStart = true;
                    isDragging = true;
                    break;
                }
                else if (Gestures[i].GestureType == GestureType.FreeDrag)
                {
                    lastDragPosition = position = Gestures[i].Position;
                    isDragging = true;
                }
                else if (Gestures[i].GestureType == GestureType.DragComplete && isDragging)
                {
                    Debug.WriteLine("End drag detected");
                    position = lastDragPosition;
                    isFinished = true;
                }
            }

            if (isFinished)
            {
                isDragging = false;
            }

            return isDragging;
        }


        public static bool SecondaryMousePressed
        {
            get { return prevMouseState.RightButton == ButtonState.Released && mouseState.RightButton == ButtonState.Pressed; }
        }


        public static Vector2 MousePosition
        {
            get { return mousePosition; }
        }


#if WINDOWS_PHONE || WIN8
        public static TouchLocationState ConvertTouchState(Microsoft.Xna.Framework.Input.Touch.TouchLocationState touch)
        {
            if (touch == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Invalid)
            {
                return TouchLocationState.Invalid;
            }
            else if (touch == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Moved)
            {
                return TouchLocationState.Moved;
            } 
            else if (touch == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed)
            {
                return TouchLocationState.Pressed;
            } 
            else
            {
                return TouchLocationState.Released;
            } 
        }


        public static GestureType ConvertGestureType(Microsoft.Xna.Framework.Input.Touch.GestureType gesture)
        {
            GestureType convertedGesture = GestureType.None;

            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.Tap) == Microsoft.Xna.Framework.Input.Touch.GestureType.Tap)
            {
                convertedGesture |= GestureType.Tap;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.DoubleTap) == Microsoft.Xna.Framework.Input.Touch.GestureType.DoubleTap)
            {
                convertedGesture |= GestureType.DoubleTap;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.Hold) == Microsoft.Xna.Framework.Input.Touch.GestureType.Hold)
            {
                convertedGesture |= GestureType.Hold;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.HorizontalDrag) == Microsoft.Xna.Framework.Input.Touch.GestureType.HorizontalDrag)
            {
                convertedGesture |= GestureType.HorizontalDrag;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.VerticalDrag) == Microsoft.Xna.Framework.Input.Touch.GestureType.VerticalDrag)
            {
                convertedGesture |= GestureType.VerticalDrag;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag) == Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag)
            {
                convertedGesture |= GestureType.FreeDrag;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.Pinch) == Microsoft.Xna.Framework.Input.Touch.GestureType.Pinch)
            {
                convertedGesture |= GestureType.Pinch;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.Flick) == Microsoft.Xna.Framework.Input.Touch.GestureType.Flick)
            {
                convertedGesture |= GestureType.Flick;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.DragComplete) == Microsoft.Xna.Framework.Input.Touch.GestureType.DragComplete)
            {
                convertedGesture |= GestureType.DragComplete;
            }
            if ((gesture & Microsoft.Xna.Framework.Input.Touch.GestureType.PinchComplete) == Microsoft.Xna.Framework.Input.Touch.GestureType.PinchComplete)
            {
                convertedGesture |= GestureType.PinchComplete;
            }

            return convertedGesture;
        }

#endif

#if WIN8

        public static Microsoft.Xna.Framework.Input.Touch.GestureType ConvertGestureType(GestureType gesture)
        {
            Microsoft.Xna.Framework.Input.Touch.GestureType convertedGesture = Microsoft.Xna.Framework.Input.Touch.GestureType.None;

            if ((gesture & GestureType.Tap) == GestureType.Tap)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
            }
            if ((gesture & GestureType.DoubleTap) == GestureType.DoubleTap)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.DoubleTap;
            }
            if ((gesture & GestureType.Hold) == GestureType.Hold)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.Hold;
            }
            if ((gesture & GestureType.HorizontalDrag) == GestureType.HorizontalDrag)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.HorizontalDrag;
            }
            if ((gesture & GestureType.VerticalDrag) == GestureType.VerticalDrag)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.VerticalDrag;
            }
            if ((gesture & GestureType.FreeDrag) == GestureType.FreeDrag)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag;
            }
            if ((gesture & GestureType.Pinch) == GestureType.Pinch)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.Pinch;
            }
            if ((gesture & GestureType.Flick) == GestureType.Flick)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.Flick;
            }
            if ((gesture & GestureType.DragComplete) == GestureType.DragComplete)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.DragComplete;
            }
            if ((gesture & GestureType.PinchComplete) == GestureType.PinchComplete)
            {
                convertedGesture |= Microsoft.Xna.Framework.Input.Touch.GestureType.PinchComplete;
            }

            return convertedGesture;
        }

#endif

    }
}
