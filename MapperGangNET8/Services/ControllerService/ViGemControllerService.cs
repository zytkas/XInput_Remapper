using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MapperGangNET8.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MapperGangNET8.Services.ControllerService
{
    /// <summary>
    /// Implementation of the controller service using ViGem
    /// </summary>
    public class ViGemControllerService : IControllerService
    {
        private ViGEmClient _client;
        private IXbox360Controller _xbox360Controller;
        private IDualShock4Controller _dualShock4Controller;
        private ControllerState _currentState;
        private bool _isConnected;
        private ControllerType _controllerType;
        private int _controllerIndex;
        private bool _disposed;
        private readonly DispatcherTimer _updateTimer;

        /// <summary>
        /// Event triggered when controller connection state changes
        /// </summary>
        public event EventHandler<ControllerConnectionEventArgs> ConnectionStateChanged;

        /// <summary>
        /// Event triggered when controller state is updated
        /// </summary>
        public event EventHandler<ControllerStateEventArgs> ControllerStateUpdated;

        /// <summary>
        /// Gets whether the controller is currently connected
        /// </summary>
        public bool IsConnected => _isConnected;

        /// <summary>
        /// Gets the type of controller being emulated
        /// </summary>
        public ControllerType ControllerType => _controllerType;

        /// <summary>
        /// Gets the controller index (1-4)
        /// </summary>
        public int ControllerIndex => _controllerIndex;

        /// <summary>
        /// Constructor
        /// </summary>
        public ViGemControllerService()
        {
            _currentState = new ControllerState();
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(10) // 100Hz update rate
            };
            _updateTimer.Tick += OnUpdateTimerTick;
        }

        /// <summary>
        /// Connect the virtual controller
        /// </summary>
        /// <param name="controllerType">Type of controller to connect</param>
        /// <param name="controllerIndex">Controller index (1-4)</param>
        /// <returns>True if connection successful, false otherwise</returns>
        public async Task<bool> ConnectAsync(ControllerType controllerType, int controllerIndex)
        {
            if (_isConnected)
            {
                await DisconnectAsync();
            }

            return await Task.Run(() =>
            {
                try
                {
                    _controllerType = controllerType;
                    _controllerIndex = Math.Max(1, Math.Min(4, controllerIndex));

                    // Initialize ViGEm client
                    _client = new ViGEmClient();

                    // Create and connect the appropriate controller
                    switch (controllerType)
                    {
                        case ControllerType.Xbox360:
                            ConnectXbox360Controller();
                            break;

                        case ControllerType.DualShock4:
                            ConnectDualShock4Controller();
                            break;
                    }

                    // Start update timer
                    _updateTimer.Start();

                    _isConnected = true;
                    OnConnectionStateChanged(true);

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to connect controller: {ex.Message}");
                    CleanupResources();
                    OnConnectionStateChanged(false, ex.Message);
                    return false;
                }
            });
        }

        /// <summary>
        /// Disconnect the virtual controller
        /// </summary>
        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                if (!_isConnected)
                {
                    return;
                }

                _updateTimer.Stop();
                CleanupResources();

                _isConnected = false;
                OnConnectionStateChanged(false);
            });
        }

        /// <summary>
        /// Reset the controller state to default values
        /// </summary>
        public void ResetState()
        {
            _currentState.Reset();
            UpdateHardwareState();
            OnControllerStateUpdated();
        }

        /// <summary>
        /// Update the controller state
        /// </summary>
        /// <param name="state">New controller state</param>
        public void UpdateState(ControllerState state)
        {
            if (!_isConnected)
            {
                return;
            }

            _currentState = state.Clone();
            UpdateHardwareState();
            OnControllerStateUpdated();
        }

        /// <summary>
        /// Set a specific button state
        /// </summary>
        /// <param name="button">Button to set</param>
        /// <param name="pressed">Whether the button is pressed</param>
        public void SetButton(ControllerButton button, bool pressed)
        {
            if (!_isConnected)
            {
                return;
            }

            _currentState.SetButton(button, pressed);
            UpdateHardwareState();
            OnControllerStateUpdated();
        }

        /// <summary>
        /// Set a specific axis value
        /// </summary>
        /// <param name="axis">Axis to set</param>
        /// <param name="value">Value to set (-1.0 to 1.0 for sticks, 0.0 to 1.0 for triggers)</param>
        public void SetAxis(ControllerAxis axis, double value)
        {
            if (!_isConnected)
            {
                return;
            }

            _currentState.SetAxis(axis, value);
            UpdateHardwareState();
            OnControllerStateUpdated();
        }

        /// <summary>
        /// Get the current controller state
        /// </summary>
        /// <returns>Current controller state</returns>
        public ControllerState GetState()
        {
            return _currentState.Clone();
        }

        /// <summary>
        /// Connect an Xbox 360 controller
        /// </summary>
        private void ConnectXbox360Controller()
        {
            _xbox360Controller = _client.CreateXbox360Controller();

            // Set feedback callback
            _xbox360Controller.FeedbackReceived += (sender, args) =>
            {
                // We received vibration feedback from a game
                Debug.WriteLine($"Xbox 360 vibration: Large: {args.LargeMotor}, Small: {args.SmallMotor}");
            };

            // Connect the controller
            _xbox360Controller.Connect();
        }

        /// <summary>
        /// Connect a DualShock 4 controller
        /// </summary>
        private void ConnectDualShock4Controller()
        {
            _dualShock4Controller = _client.CreateDualShock4Controller();

            // Connect the controller
            _dualShock4Controller.Connect();
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        private void CleanupResources()
        {
            try
            {
                if (_xbox360Controller != null)
                {
                    _xbox360Controller.Disconnect();
                    _xbox360Controller = null;
                }

                if (_dualShock4Controller != null)
                {
                    _dualShock4Controller.Disconnect();
                    _dualShock4Controller = null;
                }

                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during controller cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Update the hardware controller state based on current state
        /// </summary>
        private void UpdateHardwareState()
        {
            if (!_isConnected)
            {
                return;
            }

            try
            {
                switch (_controllerType)
                {
                    case ControllerType.Xbox360:
                        UpdateXbox360ControllerState();
                        break;

                    case ControllerType.DualShock4:
                        UpdateDualShock4ControllerState();
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating controller state: {ex.Message}");
            }
        }

        /// <summary>
        /// Update Xbox 360 controller state
        /// </summary>
        private void UpdateXbox360ControllerState()
        {
            if (_xbox360Controller == null)
            {
                return;
            }

            // Convert axis values to Xbox360 format
            // For thumbsticks: -1.0 to 1.0 => -32768 to 32767
            // For triggers: 0.0 to 1.0 => 0 to 255
            _xbox360Controller.SetAxisValue(Xbox360Axis.LeftThumbX, ConvertAxisToShort(_currentState.LeftThumbX));
            _xbox360Controller.SetAxisValue(Xbox360Axis.LeftThumbY, ConvertAxisToShort(_currentState.LeftThumbY));
            _xbox360Controller.SetAxisValue(Xbox360Axis.RightThumbX, ConvertAxisToShort(_currentState.RightThumbX));
            _xbox360Controller.SetAxisValue(Xbox360Axis.RightThumbY, ConvertAxisToShort(_currentState.RightThumbY));
            _xbox360Controller.SetSliderValue(Xbox360Slider.LeftTrigger, ConvertTriggerToByte(_currentState.LeftTrigger));
            _xbox360Controller.SetSliderValue(Xbox360Slider.RightTrigger, ConvertTriggerToByte(_currentState.RightTrigger));

            // Set button states
            ushort buttonState = 0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.Start) ? Xbox360Button.Start.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.Back) ? Xbox360Button.Back.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.LeftThumb) ? Xbox360Button.LeftThumb.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.RightThumb) ? Xbox360Button.RightThumb.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.LeftShoulder) ? Xbox360Button.LeftShoulder.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.RightShoulder) ? Xbox360Button.RightShoulder.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.Guide) ? Xbox360Button.Guide.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.A) ? Xbox360Button.A.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.B) ? Xbox360Button.B.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.X) ? Xbox360Button.X.Value : (ushort)0;
            buttonState |= _currentState.IsButtonPressed(ControllerButton.Y) ? Xbox360Button.Y.Value : (ushort)0;

            _xbox360Controller.SetButtonsFull(buttonState);
            // Set D-pad states
            byte dPadValue = 0;
            if (_currentState.IsButtonPressed(ControllerButton.DPadUp))
                dPadValue |= 1;
            if (_currentState.IsButtonPressed(ControllerButton.DPadDown))
                dPadValue |= 2;
            if (_currentState.IsButtonPressed(ControllerButton.DPadLeft))
                dPadValue |= 4;
            if (_currentState.IsButtonPressed(ControllerButton.DPadRight))
                dPadValue |= 8;

            _xbox360Controller.SetButtonState(Xbox360Button.Up, (dPadValue & 1) != 0);
            _xbox360Controller.SetButtonState(Xbox360Button.Down, (dPadValue & 2) != 0);
            _xbox360Controller.SetButtonState(Xbox360Button.Left, (dPadValue & 4) != 0);
            _xbox360Controller.SetButtonState(Xbox360Button.Right, (dPadValue & 8) != 0);

            // Submit the report
            _xbox360Controller.SubmitReport();
        }

        /// <summary>
        /// Update DualShock 4 controller state
        /// </summary>
        private void UpdateDualShock4ControllerState()
        {
            // Implementation of DS4 controller state would go here
            // This is similar to Xbox 360 implementation but uses the DS4 API

            // This is a placeholder - a full implementation would map all buttons and axes
            if (_dualShock4Controller == null) return;

            // DualShock4 implementation would be added here
            // Since we prioritize Xbox 360 for this phase, this is left as a stub

            // A minimal implementation might look like:
            /*
            var report = new DualShock4Report();
            
            // Set stick values
            report.LeftThumbX = ConvertAxisToByte(_currentState.LeftThumbX);
            report.LeftThumbY = ConvertAxisToByte(_currentState.LeftThumbY);
            report.RightThumbX = ConvertAxisToByte(_currentState.RightThumbX);
            report.RightThumbY = ConvertAxisToByte(_currentState.RightThumbY);
            
            // Set trigger values
            report.LeftTrigger = ConvertTriggerToByte(_currentState.LeftTrigger);
            report.RightTrigger = ConvertTriggerToByte(_currentState.RightTrigger);
            
            // Set buttons
            if (_currentState.IsButtonPressed(ControllerButton.Cross))
                report.Buttons |= DualShock4Buttons.Cross;
            // ... other buttons
            
            // Submit report
            _dualShock4Controller.SubmitReport(report);
            */
        }

        /// <summary>
        /// Convert axis value from -1.0 to 1.0 to short range
        /// </summary>
        private short ConvertAxisToShort(double value)
        {
            // Convert -1.0 to 1.0 to -32768 to 32767
            double clampedValue = Math.Max(-1.0, Math.Min(1.0, value));

            if (clampedValue >= 0)
            {
                return (short)(clampedValue * 32767);
            }
            else
            {
                return (short)(clampedValue * 32768);
            }
        }

        /// <summary>
        /// Convert trigger value from 0.0 to 1.0 to byte range
        /// </summary>
        private byte ConvertTriggerToByte(double value)
        {
            // Convert 0.0 to 1.0 to 0 to 255
            double clampedValue = Math.Max(0.0, Math.Min(1.0, value));
            return (byte)(clampedValue * 255);
        }

        /// <summary>
        /// Timer tick handler
        /// </summary>
        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            // This is called periodically to ensure controller state is maintained
            // Some games need frequent updates
            if (_isConnected)
            {
                UpdateHardwareState();
            }
        }

        /// <summary>
        /// Raise connection state changed event
        /// </summary>
        private void OnConnectionStateChanged(bool isConnected, string errorMessage = null)
        {
            ConnectionStateChanged?.Invoke(this, new ControllerConnectionEventArgs(
                isConnected,
                _controllerType,
                _controllerIndex,
                errorMessage
            ));
        }

        /// <summary>
        /// Raise controller state updated event
        /// </summary>
        private void OnControllerStateUpdated()
        {
            ControllerStateUpdated?.Invoke(this, new ControllerStateEventArgs(_currentState));
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                _updateTimer.Stop();
                CleanupResources();
            }

            _disposed = true;
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~ViGemControllerService()
        {
            Dispose(false);
        }
    }
}