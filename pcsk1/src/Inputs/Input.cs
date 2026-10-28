using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace pcsk1.src.Inputs
{
    public static class Input
    {
        private static KeyboardState _currentKey;
        private static KeyboardState _previousKey;
        private static MouseState _currentMouse;
        private static MouseState _previousMouse;

        public static void Update()
        {
            _previousKey = _currentKey;
            _previousMouse = _currentMouse;

            _currentKey = Keyboard.GetState();
            _currentMouse = Mouse.GetState();
        }

        public static bool IsKeyDown(Keys key) => _currentKey.IsKeyDown(key);
        public static bool IsKeyPressed(Keys key) => _currentKey.IsKeyDown(key) && _previousKey.IsKeyUp(key);

        public static Point MousePosition => new(_currentMouse.Position.X, _currentMouse.Position.Y);
        public static bool IsLeftButtonDown() => _currentMouse.LeftButton == ButtonState.Pressed;
        public static bool IsLeftButtonPressed() => _currentMouse.LeftButton == ButtonState.Pressed && _previousMouse.LeftButton == ButtonState.Released;

        public static float GetAxis(string name)
        {
            name = name.ToLower();
            if (!name.Equals("horizontal") && !name.Equals("vertical"))
            { 
                throw new ArgumentException("Invalid axis name. Use 'horizontal' or 'vertical'.");
            }
            if(name.Equals("horizontal"))
            {
                float axis = 0f;
                if (IsKeyDown(Keys.A) || IsKeyDown(Keys.Left))
                    axis -= 1f;
                if (IsKeyDown(Keys.D) || IsKeyDown(Keys.Right))
                    axis += 1f;
                return axis;
            }
            else if (name.Equals("vertical"))
            {
                float axis = 0f;
                if (IsKeyDown(Keys.W) || IsKeyDown(Keys.Up))
                    axis -= 1f;
                if (IsKeyDown(Keys.S) || IsKeyDown(Keys.Down))
                    axis += 1f;
                return axis;
            }
            return 0f;
        }
    }
}
