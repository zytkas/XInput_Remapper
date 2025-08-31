using System;
using System.Collections.Generic;
using Input;

namespace MapperGangNET8.Models
{
    /// <summary>
    /// Model for storing input state
    /// </summary>
    public class InputStateModel
    {
        /// <summary>
        /// Dictionary of currently pressed keys
        /// </summary>
        public Dictionary<int, bool> PressedKeys { get; } = new Dictionary<int, bool>();

        /// <summary>
        /// Dictionary of currently pressed mouse buttons
        /// </summary>
        public Dictionary<int, bool> PressedMouseButtons { get; } = new Dictionary<int, bool>();

        /// <summary>
        /// Current mouse X position
        /// </summary>
        public int MouseX { get; set; }

        /// <summary>
        /// Current mouse Y position
        /// </summary>
        public int MouseY { get; set; }

        /// <summary>
        /// Last change in mouse X position
        /// </summary>
        public int MouseDeltaX { get; set; }

        /// <summary>
        /// Last change in mouse Y position
        /// </summary>
        public int MouseDeltaY { get; set; }

        /// <summary>
        /// Last mouse wheel delta
        /// </summary>
        public int WheelDelta { get; set; }

        /// <summary>
        /// Timestamp of last input event
        /// </summary>
        public long LastInputTime { get; set; }

        /// <summary>
        /// Check if a key is currently pressed
        /// </summary>
        /// <param name="keyCode">Key code to check</param>
        /// <returns>True if pressed, false otherwise</returns>
        public bool IsKeyPressed(int keyCode)
        {
            return PressedKeys.TryGetValue(keyCode, out bool pressed) && pressed;
        }

        /// <summary>
        /// Check if a key is currently pressed using InputKeys enum
        /// </summary>
        /// <param name="key">InputKeys enum value to check</param>
        /// <returns>True if pressed, false otherwise</returns>
        public bool IsKeyPressed(InputKeys key)
        {
            return IsKeyPressed((int)key);
        }

        /// <summary>
        /// Check if a mouse button is currently pressed
        /// </summary>
        /// <param name="button">Button code to check</param>
        /// <returns>True if pressed, false otherwise</returns>
        public bool IsMouseButtonPressed(int button)
        {
            return PressedMouseButtons.TryGetValue(button, out bool pressed) && pressed;
        }

        /// <summary>
        /// Set key press state
        /// </summary>
        /// <param name="keyCode">Key code</param>
        /// <param name="isPressed">Whether key is pressed</param>
        public void SetKeyState(int keyCode, bool isPressed)
        {
            PressedKeys[keyCode] = isPressed;
            LastInputTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Set key press state using InputKeys enum
        /// </summary>
        /// <param name="key">InputKeys enum value</param>
        /// <param name="isPressed">Whether key is pressed</param>
        public void SetKeyState(InputKeys key, bool isPressed)
        {
            SetKeyState((int)key, isPressed);
        }

        /// <summary>
        /// Set mouse button press state
        /// </summary>
        /// <param name="button">Button code</param>
        /// <param name="isPressed">Whether button is pressed</param>
        public void SetMouseButtonState(int button, bool isPressed)
        {
            PressedMouseButtons[button] = isPressed;
            LastInputTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Update mouse position
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        public void UpdateMousePosition(int x, int y)
        {
            MouseDeltaX = x - MouseX;
            MouseDeltaY = y - MouseY;
            MouseX = x;
            MouseY = y;
            LastInputTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Update mouse wheel
        /// </summary>
        /// <param name="delta">Wheel delta</param>
        public void UpdateMouseWheel(int delta)
        {
            WheelDelta = delta;
            LastInputTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}