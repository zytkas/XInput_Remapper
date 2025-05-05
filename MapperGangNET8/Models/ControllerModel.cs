using System;

namespace MapperGangNET8.Models
{
    /// <summary>
    /// Enum defining the type of controller to emulate
    /// </summary>
    public enum ControllerType
    {
        /// <summary>
        /// Xbox 360 Controller
        /// </summary>
        Xbox360,

        /// <summary>
        /// DualShock 4 Controller
        /// </summary>
        DualShock4
    }

    /// <summary>
    /// Enum defining controller buttons
    /// </summary>
    public enum ControllerButton
    {
        // Face buttons
        A,
        B,
        X,
        Y,

        // Shoulder buttons
        LeftShoulder,
        RightShoulder,

        // Stick buttons
        LeftThumb,
        RightThumb,

        // D-pad buttons
        DPadUp,
        DPadRight,
        DPadDown,
        DPadLeft,

        // Special buttons
        Start,
        Back,
        Guide
    }

    /// <summary>
    /// Enum defining controller axes
    /// </summary>
    public enum ControllerAxis
    {
        // Analog sticks
        LeftThumbX,
        LeftThumbY,
        RightThumbX,
        RightThumbY,

        // Triggers
        LeftTrigger,
        RightTrigger
    }

    /// <summary>
    /// Class representing the state of a controller
    /// </summary>
    [Serializable]
    public class ControllerState
    {
        // Button states - bit field for efficient storage
        private UInt16 _buttons;

        // Axis states
        public double LeftThumbX { get; set; }
        public double LeftThumbY { get; set; }
        public double RightThumbX { get; set; }
        public double RightThumbY { get; set; }
        public double LeftTrigger { get; set; }
        public double RightTrigger { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ControllerState()
        {
            Reset();
        }

        /// <summary>
        /// Reset state to default values
        /// </summary>
        public void Reset()
        {
            _buttons = 0;
            LeftThumbX = 0;
            LeftThumbY = 0;
            RightThumbX = 0;
            RightThumbY = 0;
            LeftTrigger = 0;
            RightTrigger = 0;
        }

        /// <summary>
        /// Check if a button is pressed
        /// </summary>
        /// <param name="button">Button to check</param>
        /// <returns>True if pressed, false otherwise</returns>
        public bool IsButtonPressed(ControllerButton button)
        {
            int bitPosition = (int)button;
            return (_buttons & (1 << bitPosition)) != 0;
        }

        /// <summary>
        /// Set button state
        /// </summary>
        /// <param name="button">Button to set</param>
        /// <param name="pressed">Pressed state</param>
        public void SetButton(ControllerButton button, bool pressed)
        {
            int bitPosition = (int)button;
            if (pressed)
            {
                _buttons |= (UInt16)(1 << bitPosition);
            }
            else
            {
                _buttons &= (UInt16)~(1 << bitPosition);
            }
        }

        /// <summary>
        /// Set axis value with clamping
        /// </summary>
        /// <param name="axis">Axis to set</param>
        /// <param name="value">Value to set</param>
        public void SetAxis(ControllerAxis axis, double value)
        {
            switch (axis)
            {
                case ControllerAxis.LeftThumbX:
                    LeftThumbX = ClampStickValue(value);
                    break;
                case ControllerAxis.LeftThumbY:
                    LeftThumbY = ClampStickValue(value);
                    break;
                case ControllerAxis.RightThumbX:
                    RightThumbX = ClampStickValue(value);
                    break;
                case ControllerAxis.RightThumbY:
                    RightThumbY = ClampStickValue(value);
                    break;
                case ControllerAxis.LeftTrigger:
                    LeftTrigger = ClampTriggerValue(value);
                    break;
                case ControllerAxis.RightTrigger:
                    RightTrigger = ClampTriggerValue(value);
                    break;
            }
        }

        /// <summary>
        /// Clamp stick value to valid range (-1.0 to 1.0)
        /// </summary>
        private double ClampStickValue(double value)
        {
            return Math.Max(-1.0, Math.Min(1.0, value));
        }

        /// <summary>
        /// Clamp trigger value to valid range (0.0 to 1.0)
        /// </summary>
        private double ClampTriggerValue(double value)
        {
            return Math.Max(0.0, Math.Min(1.0, value));
        }

        /// <summary>
        /// Create a deep copy of the controller state
        /// </summary>
        /// <returns>New instance with the same values</returns>
        public ControllerState Clone()
        {
            return new ControllerState
            {
                _buttons = this._buttons,
                LeftThumbX = this.LeftThumbX,
                LeftThumbY = this.LeftThumbY,
                RightThumbX = this.RightThumbX,
                RightThumbY = this.RightThumbY,
                LeftTrigger = this.LeftTrigger,
                RightTrigger = this.RightTrigger
            };
        }
    }

    /// <summary>
    /// Event arguments for controller connection state changes
    /// </summary>
    public class ControllerConnectionEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether the controller is connected
        /// </summary>
        public bool IsConnected { get; }

        /// <summary>
        /// Gets the controller type
        /// </summary>
        public ControllerType ControllerType { get; }

        /// <summary>
        /// Gets the controller index
        /// </summary>
        public int ControllerIndex { get; }

        /// <summary>
        /// Gets any error message (null if no error)
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerConnectionEventArgs(bool isConnected, ControllerType controllerType, int controllerIndex, string errorMessage = null)
        {
            IsConnected = isConnected;
            ControllerType = controllerType;
            ControllerIndex = controllerIndex;
            ErrorMessage = errorMessage;
        }
    }

    /// <summary>
    /// Event arguments for controller state updates
    /// </summary>
    public class ControllerStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the controller state
        /// </summary>
        public ControllerState State { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ControllerStateEventArgs(ControllerState state)
        {
            State = state.Clone();
        }
    }
}