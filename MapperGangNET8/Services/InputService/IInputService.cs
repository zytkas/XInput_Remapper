using System;

namespace MapperGangNET8.Services.InputService
{
    /// <summary>
    /// Interface for input service handling keyboard and mouse input
    /// </summary>
    public interface IInputService : IDisposable
    {
        /// <summary>
        /// Event triggered when a key is pressed
        /// </summary>
        event EventHandler<InputKeyEventArgs> KeyDown;

        /// <summary>
        /// Event triggered when a key is released
        /// </summary>
        event EventHandler<InputKeyEventArgs> KeyUp;

        /// <summary>
        /// Event triggered when mouse state changes (button press, movement)
        /// </summary>
        event EventHandler<InputMouseEventArgs> MouseStateChanged;

        /// <summary>
        /// Start capturing input
        /// </summary>
        void Start();

        /// <summary>
        /// Stop capturing input
        /// </summary>
        void Stop();

        /// <summary>
        /// Check if input service is currently capturing input
        /// </summary>
        bool IsCapturing { get; }

        /// <summary>
        /// Enable debugging of input hooks
        /// </summary>
        /// <param name="enable">True to enable, false to disable</param>
        void EnableDebug(bool enable);
    }
}