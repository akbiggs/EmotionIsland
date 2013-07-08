using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace EmotionIsland
{
    public enum KeyboardControls
    {
        Up = 0, Down = 1, Left = 2, Right = 3, Attack1 = 4, Attack2 = 5
    }


    /// <summary>
    /// A wrapper class for all game input.
    /// It needs to be updated in the main update method, but can be accessed statically.
    /// </summary>
    public static class Input
    {
        private static MouseState mouse = Mouse.GetState();
        private static MouseState mousePrev = Mouse.GetState();
        private static KeyboardState keyboard = Keyboard.GetState();
        private static KeyboardState keyboardPrev = Keyboard.GetState();

#if KEYBOARD
        public class ButtonEmulator
        {
            private PlayerIndex index;

            public ButtonEmulator(PlayerIndex i)
            {
                index = i;
            }

            public ButtonState Start
            {
                get
                {
                    if (Input.IsKeyDown(KeyboardControls.Attack1, index))
                        return ButtonState.Pressed;
                    else
                        return ButtonState.Released;
                }
            }
            public ButtonState X
            {
                get
                {
                    if (Input.IsKeyDown(KeyboardControls.Attack1, index))
                        return ButtonState.Pressed;
                    else
                        return ButtonState.Released;
                }
            }

        }

        public class GamePadStateEmulator
        {
            private PlayerIndex index;

            public ThumbStickEmulator ThumbSticks;
            public TriggerEmulator Triggers;

            public ButtonEmulator Buttons;

            public bool IsConnected
            {
                get
                {
                    return true;
                }
            }

            public GamePadStateEmulator(PlayerIndex pIndex)
            {
                index = pIndex;
                Buttons = new ButtonEmulator(pIndex);
                ThumbSticks = new ThumbStickEmulator(pIndex);
                Triggers = new TriggerEmulator(pIndex);
            }
        }

        public class ThumbStickEmulator
        {
            private PlayerIndex index;
            public ThumbStickEmulator(PlayerIndex pIndex)
            {
                index = pIndex;
            }

            public Vector2 Left
            {
                get
                {
                    Vector2 direction = Vector2.Zero;
                    if (Input.IsKeyDown(KeyboardControls.Left, index))
                        direction.X = 0.8f;

                    if (Input.IsKeyDown(KeyboardControls.Right, index))
                        direction.X = -0.8f;

                    if (Input.IsKeyDown(KeyboardControls.Down, index))
                        direction.Y = -0.8f;

                    if (Input.IsKeyDown(KeyboardControls.Up, index))
                        direction.Y = 0.8f;

                    return direction;
                }
            }
            public Vector2 Right;
        }

        public class TriggerEmulator
        {
            private PlayerIndex index;
            public TriggerEmulator(PlayerIndex pIndex)
            {
                index = pIndex;
            }

            public float Left;
            public float Right;
        }
        public static GamePadStateEmulator[] gps = 
            new GamePadStateEmulator[] 
            { new GamePadStateEmulator(PlayerIndex.One),
              new GamePadStateEmulator(PlayerIndex.Two)
            };
#else
        public static GamePadState[] gps = new GamePadState[] { new GamePadState(), new GamePadState(), new GamePadState(), new GamePadState()};
#endif
        public static GamePadState[] pgps = new GamePadState[] { new GamePadState(), new GamePadState(), new GamePadState(), new GamePadState() };


        /// <summary>
        /// Keyboard controls for players.
        /// </summary>
        private static List<List<Keys>> KeyboardMappings;

        public static void init()
        {
#if ARCADE
            List<Keys> p1Controls = new List<Keys>();
            p1Controls.Add(Keys.Up);//Up
            p1Controls.Add(Keys.Down);//Down
            p1Controls.Add(Keys.Right);//Left
            p1Controls.Add(Keys.Left);//Right
            p1Controls.Add(Keys.Z);//Attack 1
            p1Controls.Add(Keys.X);//Attack 2


            List<Keys> p2Controls = new List<Keys>();
            p2Controls.Add(Keys.F);//Up
            p2Controls.Add(Keys.R);//Down
            p2Controls.Add(Keys.D);//Left
            p2Controls.Add(Keys.G);//Right
            p2Controls.Add(Keys.A);//Attack 1
            p2Controls.Add(Keys.S);//Attack 2
#else
            List<Keys> p1Controls = new List<Keys>();
            p1Controls.Add(Keys.W);//Up
            p1Controls.Add(Keys.S);//Down
            p1Controls.Add(Keys.D);//Left
            p1Controls.Add(Keys.A);//Right
            p1Controls.Add(Keys.G);//Attack 1
            p1Controls.Add(Keys.H);//Attack 2


            List<Keys> p2Controls = new List<Keys>();
            p2Controls.Add(Keys.Up);//Up
            p2Controls.Add(Keys.Down);//Down
            p2Controls.Add(Keys.Right);//Left
            p2Controls.Add(Keys.Left);//Right
            p2Controls.Add(Keys.OemComma);//Attack 1
            p2Controls.Add(Keys.OemPeriod);//Attack 2
#endif
            KeyboardMappings = new List<List<Keys>>();
            KeyboardMappings.Add(p1Controls);
            KeyboardMappings.Add(p2Controls);
        }

        /// <summary>
        /// Updates the input information..
        /// </summary>
        public static void Update()
        {
            mousePrev = mouse;
            keyboardPrev = keyboard;
            mouse = Mouse.GetState();
            keyboard = Keyboard.GetState();

#if !KEYBOARD
            for (int i = 0; i < 4; i++)
            {
                pgps[i] = gps[i];
                gps[i] = GamePad.GetState((PlayerIndex)i);
            }
#endif
        }

        public static KeyboardState GetKeyboardState()
        {
            return keyboard;
        }

        public static KeyboardState GetPreviousKeyboardState()
        {
            return keyboardPrev;
        }


        public static bool PressedUp(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Up, pIndex);
#else
            if (gps[(int)pIndex].DPad.Up == ButtonState.Pressed && pgps[(int)pIndex].DPad.Up == ButtonState.Released) return true;
            else if (gps[(int)pIndex].ThumbSticks.Left.Y > .5f && pgps[(int)pIndex].ThumbSticks.Left.Y < .5f) return true;
            else return false;
#endif
        }

        public static bool PressedRight(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Right, pIndex);
#else
            if (gps[(int)pIndex].DPad.Right == ButtonState.Pressed && pgps[(int)pIndex].DPad.Right == ButtonState.Released) return true;
            else if (gps[(int)pIndex].ThumbSticks.Left.X > .5f && pgps[(int)pIndex].ThumbSticks.Left.X < .5f) return true;
            else return false;
#endif
        }

        public static bool PressedLeft(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Left, pIndex);
#else
            if (gps[(int)pIndex].DPad.Left == ButtonState.Pressed && pgps[(int)pIndex].DPad.Left == ButtonState.Released) return true;
            else if (gps[(int)pIndex].ThumbSticks.Left.X < -.5f && pgps[(int)pIndex].ThumbSticks.Left.X > -.5f) return true;
            else return false;
#endif
        }

        public static bool PressedDown(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Down, pIndex);
#else
            if (gps[(int)pIndex].DPad.Down == ButtonState.Pressed && pgps[(int)pIndex].DPad.Down == ButtonState.Released) return true;
            else if (gps[(int)pIndex].ThumbSticks.Left.Y < -.5f && pgps[(int)pIndex].ThumbSticks.Left.Y > -.5f) return true;
            else return false;
#endif
        }

        public static bool PressedA(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Attack1, pIndex);
#else
            if (gps[(int)pIndex].Buttons.A == ButtonState.Pressed && pgps[(int)pIndex].Buttons.A == ButtonState.Released) return true;
            else return false;
#endif
        }

        public static bool PressedB(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Attack2, pIndex);
#else
            if (gps[(int)pIndex].Buttons.B == ButtonState.Pressed && pgps[(int)pIndex].Buttons.B == ButtonState.Released) return true;
            else return false;
#endif
        }

        public static bool PressedX(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Attack1, pIndex);
#else
            if (gps[(int)pIndex].Buttons.X == ButtonState.Pressed && pgps[(int)pIndex].Buttons.X == ButtonState.Released) return true;
            else return false;
#endif
        }

        public static bool PressedY(PlayerIndex pIndex)
        {
#if KEYBOARD
            return KeyPressed(KeyboardControls.Attack2, pIndex);
#else
            if (gps[(int)pIndex].Buttons.Y == ButtonState.Pressed && pgps[(int)pIndex].Buttons.Y == ButtonState.Released) return true;
            else return false;
#endif
        }


        const float TRIGGER_INPUT_THRESHHOLD = 0.50f;
        public static bool IsTriggerDown_Left(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Left >= TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }
        public static bool IsTriggerUp_Left(PlayerIndex pIndex)
        {
            return !IsTriggerDown_Left(pIndex);
        }
        public static bool PressedTrigger_Left(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Left >= TRIGGER_INPUT_THRESHHOLD && pgps[(int)pIndex].Triggers.Left < TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }
        public static bool ReleasedTrigger_Left(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Left < TRIGGER_INPUT_THRESHHOLD && pgps[(int)pIndex].Triggers.Left >= TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }


        public static bool IsTriggerDown_Right(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Right >= TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }
        public static bool IsTriggerUp_Right(PlayerIndex pIndex)
        {
            return !IsTriggerDown_Right(pIndex);
        }
        public static bool PressedTrigger_Right(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Right >= TRIGGER_INPUT_THRESHHOLD && pgps[(int)pIndex].Triggers.Right < TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }
        public static bool ReleasedTrigger_Right(PlayerIndex pIndex)
        {
            if (gps[(int)pIndex].Triggers.Right < TRIGGER_INPUT_THRESHHOLD && pgps[(int)pIndex].Triggers.Right >= TRIGGER_INPUT_THRESHHOLD) return true;
            else return false;
        }


        #region Keyboard Controls

        public static bool IsKeyDown(KeyboardControls key, PlayerIndex player)
        {
            if ((int)player >= KeyboardMappings.Count) return false;
            return IsKeyDown(KeyboardMappings[(int)player][(int)key]);
        }

        public static bool IsKeyUp(KeyboardControls key, PlayerIndex player)
        {
            if ((int)player >= KeyboardMappings.Count) return false;
            return IsKeyUp(KeyboardMappings[(int)player][(int)key]);
        }

        public static bool IsKeyDown(Keys key)
        {
            return keyboard.IsKeyDown(key);
        }

        public static bool IsKeyUp(Keys key)
        {
            return keyboard.IsKeyUp(key);
        }

        public static bool KeyPressed(KeyboardControls key, PlayerIndex player)
        {
            if ((int)player >= KeyboardMappings.Count) return false;
            return KeyPressed(KeyboardMappings[(int)player][(int)key]);
        }

        public static bool KeyReleased(KeyboardControls key, PlayerIndex player)
        {
            if ((int)player >= KeyboardMappings.Count) return false;
            return KeyReleased(KeyboardMappings[(int)player][(int)key]);
        }

        public static bool KeyPressed(Keys key)
        {
            return (keyboard.IsKeyDown(key) && keyboardPrev.IsKeyUp(key));
        }

        public static bool KeyReleased(Keys key)
        {
            return (keyboardPrev.IsKeyDown(key) && keyboard.IsKeyUp(key));
        }
        #endregion

        #region Mouse Controls
        public static bool MouseScrollDown
        {
            get { return (mouse.ScrollWheelValue < mousePrev.ScrollWheelValue); }
        }

        public static bool MouseScrollUp
        {
            get { return (mouse.ScrollWheelValue > mousePrev.ScrollWheelValue); }
        }

        public static bool MouseLeftButtonPressed
        {
            get { return (mouse.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released); }
        }

        public static bool MouseLeftButtonTapped
        {
            get { return (mouse.LeftButton == ButtonState.Pressed && mousePrev.LeftButton == ButtonState.Released); }
        }

        public static bool MouseRightButtonPressed
        {
            get { return (mouse.RightButton == ButtonState.Pressed && mousePrev.RightButton == ButtonState.Released); }
        }

        public static bool MouseRightButtonTapped
        {
            get { return (mouse.RightButton == ButtonState.Pressed && mousePrev.RightButton == ButtonState.Released); }
        }

        public static bool MouseRightButtonReleased
        {
            get { return (mouse.RightButton == ButtonState.Released && mousePrev.RightButton == ButtonState.Pressed); }
        }

        public static bool MouseMiddleButtonTapped
        {
            get { return (mouse.MiddleButton == ButtonState.Pressed && mousePrev.MiddleButton == ButtonState.Released); }
        }

        public static bool MouseLeftButtonDown
        {
            get { return mouse.LeftButton == ButtonState.Pressed; }
        }

        public static bool MouseRightButtonDown
        {
            get { return mouse.RightButton == ButtonState.Pressed; }
        }

        public static bool MouseMiddleButtonDown
        {
            get { return mouse.MiddleButton == ButtonState.Pressed; }
        }

        public static Vector2 MousePosition
        {
            get { return new Vector2(mouse.X, mouse.Y); }
        }

        public static Vector2 MousePrevPosition
        {
            get { return new Vector2(mousePrev.X, mousePrev.Y); }
        }
        #endregion

    }
}