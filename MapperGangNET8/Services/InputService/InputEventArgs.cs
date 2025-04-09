using System;

namespace MapperGangNET8.Services.InputService
{
    /// <summary>
    /// Base class for input event arguments
    /// </summary>
    public abstract class InputEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets whether the event should be handled (blocked from system)
        /// </summary>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the timestamp of the event
        /// </summary>
        public long Timestamp { get; }

        protected InputEventArgs(long timestamp)
        {
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Event arguments for keyboard input events
    /// </summary>
    public class InputKeyEventArgs : InputEventArgs
    {
        /// <summary>
        /// Gets the key code
        /// </summary>
        public int KeyCode { get; }

        /// <summary>
        /// Gets the key state
        /// </summary>
        public int KeyState { get; }

        public InputKeyEventArgs(int keyCode, int keyState, long timestamp)
            : base(timestamp)
        {
            KeyCode = keyCode;
            KeyState = keyState;
        }
    }

    /// <summary>
    /// Event arguments for mouse button events
    /// </summary>
    public class InputMouseEventArgs : InputEventArgs
    {
        /// <summary>
        /// Gets the mouse button
        /// </summary>
        public int Button { get; }

        /// <summary>
        /// Gets the X coordinate of the mouse
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Gets the Y coordinate of the mouse
        /// </summary>
        public int Y { get; }

        public InputMouseEventArgs(int button, int x, int y, long timestamp)
            : base(timestamp)
        {
            Button = button;
            X = x;
            Y = y;
        }
    }
}