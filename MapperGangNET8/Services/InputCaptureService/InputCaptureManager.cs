using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using MapperGangNET8.Models;
using Linearstar.Windows.RawInput;

namespace MapperGangNET8.Services.InputCaptureService
{
    /// <summary>
    /// Manages keyboard and mouse button blocking, plus raw mouse input capture
    /// </summary>
    public class InputCaptureManager : IDisposable
    {
        // Win32 API for mouse centering
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        private readonly HashSet<int> _blockedKeys = new HashSet<int>();
        private readonly HashSet<int> _blockedMouseButtons = new HashSet<int>();
        private RawInputReceiverWindow _rawInputWindow;
        private bool _rawInputEnabled = false;
        private bool _disposed = false;
        private bool _mouseCenteringEnabled = false;

        /// <summary>
        /// Event fired when mouse delta is captured from Raw Input
        /// </summary>
        public event EventHandler<MouseDeltaEventArgs> MouseDeltaCaptured;

        /// <summary>
        /// Event fired when mouse button is pressed/released
        /// </summary>
        public event EventHandler<MouseButtonEventArgs> MouseButtonEvent;

        /// <summary>
        /// Check if a keyboard key should be blocked
        /// </summary>
        public bool ShouldBlockKey(int keyCode)
        {
            return _blockedKeys.Contains(keyCode);
        }

        /// <summary>
        /// Check if a mouse button should be blocked
        /// </summary>
        public bool ShouldBlockMouseButton(int buttonCode)
        {
            return _blockedMouseButtons.Contains(buttonCode);
        }

        /// <summary>
        /// Update blocked keys and mouse buttons based on current bindings
        /// </summary>
        public void UpdateBlockedKeys(IEnumerable<KeyBindingModel> keyBindings)
        {
            _blockedKeys.Clear();
            _blockedMouseButtons.Clear();

            foreach (var binding in keyBindings)
            {
                if (binding.InputType == InputDeviceType.Keyboard)
                {
                    _blockedKeys.Add(binding.InputCode);
                    System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked key: {binding.InputCode}");
                }
                else if (binding.InputType == InputDeviceType.Mouse)
                {
                    _blockedMouseButtons.Add(binding.InputCode);
                    System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked mouse button: {binding.InputCode}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Blocked keys: {_blockedKeys.Count}, Blocked mouse buttons: {_blockedMouseButtons.Count}");
        }

        /// <summary>
        /// Update blocked mouse buttons from mouse button mappings
        /// </summary>
        public void UpdateBlockedMouseButtons(IEnumerable<MouseButtonMappingModel> mouseButtonMappings)
        {
            foreach (var mapping in mouseButtonMappings)
            {
                if (!string.IsNullOrEmpty(mapping.MouseButton))
                {
                    var buttonCode = InputKeyMap.GetMouseButtonCode(mapping.MouseButton);
                    if (buttonCode != 0)
                    {
                        _blockedMouseButtons.Add(buttonCode);
                        System.Diagnostics.Debug.WriteLine($"[INPUT CAPTURE] Added blocked mouse button from mapping: {mapping.MouseButton} -> {buttonCode}");
                    }
                }
            }
        }

        /// <summary>
        /// Enable or disable Raw Input capture for mouse deltas
        /// </summary>
        public void EnableRawInput(bool enable)
        {
            System.Diagnostics.Debug.WriteLine($"[RAW INPUT] EnableRawInput({enable}) - Current: _rawInputEnabled={_rawInputEnabled}");

            if (enable && !_rawInputEnabled)
            {
                StartRawInput();
            }
            else if (!enable && _rawInputEnabled)
            {
                StopRawInput();
            }
        }

        /// <summary>
        /// Start Raw Input for clean mouse delta capture
        /// </summary>
        private void StartRawInput()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] Starting Raw Input capture...");

                // Create invisible window for receiving WM_INPUT messages
                _rawInputWindow = new RawInputReceiverWindow();
                _rawInputWindow.Input += OnRawInputReceived;

                // Register for mouse input (non-exclusive - doesn't block)
                RawInputDevice.RegisterDevice(
                    HidUsageAndPage.Mouse,
                    RawInputDeviceFlags.InputSink, // Receives input even when not in foreground
                    _rawInputWindow.Handle
                );

                _rawInputEnabled = true;
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] ✅ Raw Input started successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] ❌ Failed to start: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop Raw Input capture
        /// </summary>
        private void StopRawInput()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] Stopping Raw Input capture...");

                if (_rawInputWindow != null)
                {
                    // Unregister device
                    RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);

                    // Cleanup window
                    _rawInputWindow.Input -= OnRawInputReceived;
                    _rawInputWindow.Dispose();
                    _rawInputWindow = null;
                }

                _rawInputEnabled = false;
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] ✅ Raw Input stopped successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] ❌ Failed to stop: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle raw input mouse data
        /// </summary>
        private void OnRawInputReceived(object sender, RawInputEventArgs e)
        {
            if (e.Data is RawInputMouseData mouseData)
            {
                var mouse = mouseData.Mouse;

                // Only process movement deltas
                if (mouse.LastX != 0 || mouse.LastY != 0)
                {
                    // Fire event with clean hardware deltas
                    MouseDeltaCaptured?.Invoke(this,
                        new MouseDeltaEventArgs(mouse.LastX, mouse.LastY));

                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Delta: X={mouse.LastX}, Y={mouse.LastY}");
                }
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            StopRawInput();
            _disposed = true;
        }
    }

    /// <summary>
    /// Event args for mouse delta capture
    /// </summary>
    public class MouseDeltaEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }

        public MouseDeltaEventArgs(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }

    /// <summary>
    /// Event args for mouse button events
    /// </summary>
    public class MouseButtonEventArgs : EventArgs
    {
        public int ButtonCode { get; }

        public MouseButtonEventArgs(int buttonCode)
        {
            ButtonCode = buttonCode;
        }
    }

    /// <summary>
    /// Event args for raw input
    /// </summary>
    internal class RawInputEventArgs : EventArgs
    {
        public RawInputData Data { get; }

        public RawInputEventArgs(RawInputData data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Invisible window for receiving WM_INPUT messages
    /// </summary>
    internal sealed class RawInputReceiverWindow : IDisposable
    {
        public event EventHandler<RawInputEventArgs> Input;
        private HwndSource _hwndSource;

        public RawInputReceiverWindow()
        {
            // Create invisible window
            var parameters = new HwndSourceParameters("RawInputReceiver")
            {
                WindowStyle = unchecked((int)0x80000000), // WS_POPUP
                Width = 0,
                Height = 0,
                PositionX = -10000,
                PositionY = -10000
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_INPUT = 0x00FF;

            if (msg == WM_INPUT)
            {
                try
                {
                    var data = RawInputData.FromHandle(lParam);
                    Input?.Invoke(this, new RawInputEventArgs(data));
                    handled = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Error processing WM_INPUT: {ex.Message}");
                }
            }

            return IntPtr.Zero;
        }

        public IntPtr Handle => _hwndSource?.Handle ?? IntPtr.Zero;

        public void Dispose()
        {
            _hwndSource?.RemoveHook(WndProc);
            _hwndSource?.Dispose();
            _hwndSource = null;
        }
    }
}