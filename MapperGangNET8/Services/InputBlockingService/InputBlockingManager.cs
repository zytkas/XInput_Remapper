using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using MapperGangNET8.Models;
using Input;
using Linearstar.Windows.RawInput;

namespace MapperGangNET8.Services.InputBlockingService
{
    /// <summary>
    /// Manages input blocking for mapped keys and mouse to prevent double input to games
    /// </summary>
    public class InputBlockingManager : IDisposable
    {
        private readonly HashSet<int> _blockedKeys = new HashSet<int>();
        private bool _blockMouse = false;
        private bool _blockMouseMovement = false;
        private bool _blockMouseButtons = false;
        private bool _isEnabled = false;
        
        // Mouse delta capture for camera control
        private int _lastMouseX = 0;
        private int _lastMouseY = 0;
        private bool _captureMouseDeltas = false;
        
        // Raw Input for getting mouse deltas even when blocked
        private RawInputReceiverWindow? _rawInputWindow;
        private bool _rawInputEnabled = false;
        
        public event EventHandler<MouseDeltaEventArgs> MouseDeltaCaptured;
        
        /// <summary>
        /// Enable or disable input blocking
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] SetEnabled({enabled}) - Current: _isEnabled={_isEnabled}, _captureMouseDeltas={_captureMouseDeltas}, _rawInputEnabled={_rawInputEnabled}");
            
            _isEnabled = enabled;
            
            if (enabled && _captureMouseDeltas && !_rawInputEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Starting Raw Input because enabled=true and captureDeltas=true");
                StartRawInput();
            }
            else if (!enabled && _rawInputEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Stopping Raw Input because enabled=false");
                StopRawInput();
            }
            
            if (!enabled)
            {
                // Reset mouse position tracking when disabled
                _lastMouseX = 0;
                _lastMouseY = 0;
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Reset mouse tracking");
            }
        }
        
        /// <summary>
        /// Check if input blocking is enabled
        /// </summary>
        public bool IsEnabled => _isEnabled;
        
        /// <summary>
        /// Update blocked keys based on current key bindings
        /// </summary>
        public void UpdateBlockedKeys(IEnumerable<KeyBindingModel> keyBindings)
        {
            _blockedKeys.Clear();
            
            foreach (var binding in keyBindings)
            {
                if (binding.InputType == InputDeviceType.Keyboard)
                {
                    _blockedKeys.Add(binding.InputCode);
                }
                else if (binding.InputType == InputDeviceType.Mouse)
                {
                    // If any mouse buttons are mapped, block mouse buttons
                    _blockMouseButtons = true;
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"InputBlockingManager: Updated blocked keys count: {_blockedKeys.Count}");
        }
        
        /// <summary>
        /// Set mouse blocking configuration
        /// </summary>
        public void SetMouseBlocking(bool blockMovement, bool blockButtons, bool captureDeltas = true)
        {
            System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] SetMouseBlocking(movement={blockMovement}, buttons={blockButtons}, captureDeltas={captureDeltas}) - Current: _isEnabled={_isEnabled}, _rawInputEnabled={_rawInputEnabled}");
            
            _blockMouseMovement = blockMovement;
            _blockMouseButtons = blockButtons;
            _captureMouseDeltas = captureDeltas;
            _blockMouse = blockMovement || blockButtons;
            
            // Start/stop Raw Input based on capture deltas setting
            if (_isEnabled && captureDeltas && !_rawInputEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Starting Raw Input from SetMouseBlocking because enabled=true and captureDeltas=true");
                StartRawInput();
            }
            else if (!captureDeltas && _rawInputEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Stopping Raw Input from SetMouseBlocking because captureDeltas=false");
                StopRawInput();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] No Raw Input state change needed");
            }
            
            System.Diagnostics.Debug.WriteLine($"[INPUT BLOCKING] Mouse blocking configured - Movement: {blockMovement}, Buttons: {blockButtons}, Capture Deltas: {captureDeltas}");
        }
        
        /// <summary>
        /// Check if a keyboard key should be blocked
        /// </summary>
        public bool ShouldBlockKey(int keyCode)
        {
            if (!_isEnabled) return false;
            return _blockedKeys.Contains(keyCode);
        }
        
        /// <summary>
        /// Check if mouse input should be blocked and capture deltas if needed
        /// </summary>
        public bool ShouldBlockMouse(InputButtons button, int x, int y)
        {
            if (!_isEnabled) return false;

            // Check if this mouse input should be blocked
            if (button == InputButtons.Move && _blockMouseMovement)
            {
                return true;
            }
            
            if (button != InputButtons.Move && button != InputButtons.None && _blockMouseButtons)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Reset mouse position tracking
        /// </summary>
        public void ResetMouseTracking()
        {
            _lastMouseX = 0;
            _lastMouseY = 0;
        }
        
        /// <summary>
        /// Get current blocked keys for debugging
        /// </summary>
        public IReadOnlySet<int> GetBlockedKeys() => _blockedKeys;
        
        /// <summary>
        /// Get mouse blocking status for debugging
        /// </summary>
        public (bool movement, bool buttons, bool captureDeltas) GetMouseBlockingStatus() 
            => (_blockMouseMovement, _blockMouseButtons, _captureMouseDeltas);

        /// <summary>
        /// Start Raw Input for mouse delta capture
        /// </summary>
        private void StartRawInput()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] StartRawInput() called - _rawInputEnabled={_rawInputEnabled}"); 
                
                if (_rawInputEnabled) 
                {
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Already enabled, skipping");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Creating RawInputReceiverWindow...");
                _rawInputWindow = new RawInputReceiverWindow();
                _rawInputWindow.Input += OnRawInputReceived;
                
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Showing invisible window...");
               
                
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Window handle: {_rawInputWindow.Handle:X8}");
                
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Registering mouse device...");
                RawInputDevice.RegisterDevice(HidUsageAndPage.Mouse, 
                    RawInputDeviceFlags.ExInputSink, _rawInputWindow.Handle);
                
                _rawInputEnabled = true;
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] ✅ Raw Input started successfully for mouse deltas");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] ❌ Failed to start Raw Input - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Stop Raw Input
        /// </summary>
        private void StopRawInput()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] StopRawInput() called - _rawInputEnabled={_rawInputEnabled}");
                
                if (!_rawInputEnabled)
                {
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Already disabled, skipping");
                    return;
                }
                
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Unregistering mouse device...");
                RawInputDevice.UnregisterDevice(HidUsageAndPage.Mouse);
                
                if (_rawInputWindow != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Disposing window...");
                    _rawInputWindow.Input -= OnRawInputReceived;
                    _rawInputWindow.Dispose();
                    _rawInputWindow = null;
                }
                
                _rawInputEnabled = false;
                System.Diagnostics.Debug.WriteLine("[RAW INPUT] ✅ Raw Input stopped successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] ❌ Failed to stop Raw Input - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Handle raw input mouse data
        /// </summary>
        private void OnRawInputReceived(object? sender, RawInputEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[RAW INPUT] OnRawInputReceived called - Data type: {e.Data?.GetType().Name}");
            
            if (e.Data is RawInputMouseData mouseData)
            {
                var mouse = mouseData.Mouse;
                
                
                // Only process movement deltas
                if (mouse.LastX != 0 || mouse.LastY != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] 🎯 Firing MouseDeltaCaptured event - deltaX={mouse.LastX}, deltaY={mouse.LastY}");

                    MouseDeltaCaptured?.Invoke(this, new MouseDeltaEventArgs(mouse.LastX, mouse.LastY, 0, 0));
                    System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Event fired to {MouseDeltaCaptured?.GetInvocationList().Length ?? 0} subscribers");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[RAW INPUT] Not mouse data, ignoring");
            }
        }

        /// <summary>
        /// Handle raw mouse delta from external source
        /// </summary>
        public void OnRawMouseDelta(int deltaX, int deltaY)
        {
            if (_isEnabled && _captureMouseDeltas)
            {
                System.Diagnostics.Debug.WriteLine($"[EXTERNAL RAW] Mouse delta: X={deltaX}, Y={deltaY}");
                MouseDeltaCaptured?.Invoke(this, new MouseDeltaEventArgs(deltaX, deltaY, 0, 0));
            }
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            StopRawInput();
        }
    }

    /// <summary>
    /// Event args for raw input
    /// </summary>
    public class RawInputEventArgs : EventArgs
    {
        public RawInputData Data { get; }
        
        public RawInputEventArgs(RawInputData data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Invisible WPF window for receiving WM_INPUT messages
    /// </summary>
    internal sealed class RawInputReceiverWindow : IDisposable
    {
        public event EventHandler<RawInputEventArgs>? Input;
        private HwndSource? _hwndSource;

        public RawInputReceiverWindow()
        {
            var parameters = new HwndSourceParameters("RawInputWindow")
            {
                WindowStyle = unchecked((int)0x80000000), // WS_POPUP
                Width = 0,
                Height = 0,
                PositionX = -10000,
                PositionY = -10000,
                ParentWindow = IntPtr.Zero,
                UsesPerPixelOpacity = false
            };

            _hwndSource = new HwndSource(parameters);
            _hwndSource.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_INPUT = 0x00FF;
            if (msg == WM_INPUT)
            {
                var data = RawInputData.FromHandle(lParam);
                Input?.Invoke(this, new RawInputEventArgs(data));
                handled = true;
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


    /// <summary>
    /// Event args for mouse delta capture
    /// </summary>
    public class MouseDeltaEventArgs : EventArgs
    {
        public int DeltaX { get; }
        public int DeltaY { get; }
        public int AbsoluteX { get; }
        public int AbsoluteY { get; }
        
        public MouseDeltaEventArgs(int deltaX, int deltaY, int absoluteX, int absoluteY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
            AbsoluteX = absoluteX;
            AbsoluteY = absoluteY;
        }
    }
}