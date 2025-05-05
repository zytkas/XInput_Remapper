using System;
using System.Threading.Tasks;
using MapperGangNET8.Models;

namespace MapperGangNET8.Services.ControllerService
{
    /// <summary>
    /// Interface for the controller emulation service
    /// </summary>
    public interface IControllerService : IDisposable
    {
        /// <summary>
        /// Event triggered when controller connection state changes
        /// </summary>
        event EventHandler<ControllerConnectionEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Event triggered when controller state is updated
        /// </summary>
        event EventHandler<ControllerStateEventArgs> ControllerStateUpdated;

        /// <summary>
        /// Gets whether the controller is currently connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets the type of controller being emulated
        /// </summary>
        ControllerType ControllerType { get; }

        /// <summary>
        /// Gets the controller index (1-4)
        /// </summary>
        int ControllerIndex { get; }

        /// <summary>
        /// Connect the virtual controller
        /// </summary>
        /// <param name="controllerType">Type of controller to connect</param>
        /// <param name="controllerIndex">Controller index (1-4)</param>
        /// <returns>True if connection successful, false otherwise</returns>
        Task<bool> ConnectAsync(ControllerType controllerType, int controllerIndex);

        /// <summary>
        /// Disconnect the virtual controller
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Reset the controller state to default values
        /// </summary>
        void ResetState();

        /// <summary>
        /// Update the controller state
        /// </summary>
        /// <param name="state">New controller state</param>
        void UpdateState(ControllerState state);

        /// <summary>
        /// Set a specific button state
        /// </summary>
        /// <param name="button">Button to set</param>
        /// <param name="pressed">Whether the button is pressed</param>
        void SetButton(ControllerButton button, bool pressed);

        /// <summary>
        /// Set a specific axis value
        /// </summary>
        /// <param name="axis">Axis to set</param>
        /// <param name="value">Value to set (-1.0 to 1.0 for sticks, 0.0 to 1.0 for triggers)</param>
        void SetAxis(ControllerAxis axis, double value);

        /// <summary>
        /// Get the current controller state
        /// </summary>
        /// <returns>Current controller state</returns>
        ControllerState GetState();
    }
}