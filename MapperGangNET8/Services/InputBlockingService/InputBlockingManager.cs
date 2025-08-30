using System;
using System.Collections.Generic;
using MapperGangNET8.Models;
using Input;

namespace MapperGangNET8.Services.InputBlockingService
{
    /// <summary>
    /// Manages input blocking for mapped keys and mouse to prevent double input to games
    /// </summary>
    public class InputBlockingManager
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
        
        public event EventHandler<MouseDeltaEventArgs> MouseDeltaCaptured;
        
        /// <summary>
        /// Enable or disable input blocking
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
            if (!enabled)
            {
                // Reset mouse position tracking when disabled
                _lastMouseX = 0;
                _lastMouseY = 0;
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
            _blockMouseMovement = blockMovement;
            _blockMouseButtons = blockButtons;
            _captureMouseDeltas = captureDeltas;
            _blockMouse = blockMovement || blockButtons;
            
            System.Diagnostics.Debug.WriteLine($"InputBlockingManager: Mouse blocking - Movement: {blockMovement}, Buttons: {blockButtons}, Capture Deltas: {captureDeltas}");
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
            
            // Always capture mouse deltas for camera control if enabled
            if (_captureMouseDeltas && button == InputButtons.Move)
            {
                int deltaX = 0, deltaY = 0;
                
                if (_lastMouseX != 0 || _lastMouseY != 0)
                {
                    deltaX = x - _lastMouseX;
                    deltaY = y - _lastMouseY;
                    
                    // Only fire event if there's actual movement
                    if (deltaX != 0 || deltaY != 0)
                    {
                        MouseDeltaCaptured?.Invoke(this, new MouseDeltaEventArgs(deltaX, deltaY, x, y));
                    }
                }
                
                _lastMouseX = x;
                _lastMouseY = y;
            }
            
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