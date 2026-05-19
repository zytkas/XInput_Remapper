using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MapperGangNET8.Models;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MapperGangNET8.Services.ControllerService
{
    /// <summary>
    /// Implementation of the controller service using ViGEm (Xbox 360 emulation only).
    /// </summary>
    public class ViGemControllerService : IControllerService
    {
        private ViGEmClient _client;
        private IXbox360Controller _xbox360Controller;
        private ControllerState _currentState;
        private bool _isConnected;
        private ControllerType _controllerType;
        private int _controllerIndex;
        private bool _disposed;
        private int _dirty;

        /// <inheritdoc/>
        public event EventHandler<ControllerConnectionEventArgs> ConnectionStateChanged;

        /// <inheritdoc/>
        public event EventHandler<ControllerStateEventArgs> ControllerStateUpdated;

        /// <inheritdoc/>
        public bool IsConnected => _isConnected;

        /// <inheritdoc/>
        public ControllerType ControllerType => _controllerType;

        /// <inheritdoc/>
        public int ControllerIndex => _controllerIndex;

        public ViGemControllerService()
        {
            _currentState = new ControllerState();
        }

        /// <inheritdoc/>
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

                    _client = new ViGEmClient();
                    ConnectXbox360Controller();

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

        /// <inheritdoc/>
        public async Task DisconnectAsync()
        {
            await Task.Run(() =>
            {
                if (!_isConnected) return;

                CleanupResources();
                _isConnected = false;
                OnConnectionStateChanged(false);
            });
        }

        /// <inheritdoc/>
        public void ResetState()
        {
            _currentState.Reset();
            _dirty = 1;
            Submit();
            OnControllerStateUpdated();
        }

        /// <inheritdoc/>
        public void UpdateState(ControllerState state)
        {
            if (!_isConnected) return;

            _currentState = state.Clone();
            _dirty = 1;
            OnControllerStateUpdated();
        }

        /// <inheritdoc/>
        public void SetButton(ControllerButton button, bool pressed)
        {
            if (!_isConnected) return;

            _currentState.SetButton(button, pressed);
            _dirty = 1;
            OnControllerStateUpdated();
        }

        /// <inheritdoc/>
        public void SetAxis(ControllerAxis axis, double value)
        {
            if (!_isConnected) return;

            _currentState.SetAxis(axis, value);
            _dirty = 1;
            OnControllerStateUpdated();
        }

        /// <inheritdoc/>
        public ControllerState GetState() => _currentState.Clone();

        /// <inheritdoc/>
        public void Submit()
        {
            if (!_isConnected || _xbox360Controller == null) return;
            if (System.Threading.Interlocked.Exchange(ref _dirty, 0) == 0) return;

            try
            {
                _xbox360Controller.SetAxisValue(Xbox360Axis.LeftThumbX, ConvertAxisToShort(_currentState.LeftThumbX));
                _xbox360Controller.SetAxisValue(Xbox360Axis.LeftThumbY, ConvertAxisToShort(_currentState.LeftThumbY));
                _xbox360Controller.SetAxisValue(Xbox360Axis.RightThumbX, ConvertAxisToShort(_currentState.RightThumbX));
                _xbox360Controller.SetAxisValue(Xbox360Axis.RightThumbY, ConvertAxisToShort(_currentState.RightThumbY));
                _xbox360Controller.SetSliderValue(Xbox360Slider.LeftTrigger, ConvertTriggerToByte(_currentState.LeftTrigger));
                _xbox360Controller.SetSliderValue(Xbox360Slider.RightTrigger, ConvertTriggerToByte(_currentState.RightTrigger));

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

                _xbox360Controller.SetButtonState(Xbox360Button.Up, _currentState.IsButtonPressed(ControllerButton.DPadUp));
                _xbox360Controller.SetButtonState(Xbox360Button.Down, _currentState.IsButtonPressed(ControllerButton.DPadDown));
                _xbox360Controller.SetButtonState(Xbox360Button.Left, _currentState.IsButtonPressed(ControllerButton.DPadLeft));
                _xbox360Controller.SetButtonState(Xbox360Button.Right, _currentState.IsButtonPressed(ControllerButton.DPadRight));

                _xbox360Controller.SubmitReport();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error submitting controller report: {ex.Message}");
            }
        }

        private void ConnectXbox360Controller()
        {
            _xbox360Controller = _client.CreateXbox360Controller();
            _xbox360Controller.FeedbackReceived += (sender, args) =>
            {
                Debug.WriteLine($"Xbox 360 vibration: Large: {args.LargeMotor}, Small: {args.SmallMotor}");
            };
            _xbox360Controller.Connect();
        }

        private void CleanupResources()
        {
            try
            {
                if (_xbox360Controller != null)
                {
                    _xbox360Controller.Disconnect();
                    _xbox360Controller = null;
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

        private short ConvertAxisToShort(double value)
        {
            double clampedValue = Math.Max(-1.0, Math.Min(1.0, value));
            return clampedValue >= 0
                ? (short)(clampedValue * 32767)
                : (short)(clampedValue * 32768);
        }

        private byte ConvertTriggerToByte(double value)
        {
            double clampedValue = Math.Max(0.0, Math.Min(1.0, value));
            return (byte)(clampedValue * 255);
        }

        private void OnConnectionStateChanged(bool isConnected, string errorMessage = null)
        {
            ConnectionStateChanged?.Invoke(this, new ControllerConnectionEventArgs(
                isConnected, _controllerType, _controllerIndex, errorMessage));
        }

        private void OnControllerStateUpdated()
        {
            ControllerStateUpdated?.Invoke(this, new ControllerStateEventArgs(_currentState));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing) CleanupResources();
            _disposed = true;
        }

        ~ViGemControllerService()
        {
            Dispose(false);
        }
    }
}
