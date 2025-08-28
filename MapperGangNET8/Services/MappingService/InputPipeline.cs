using System;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Main pipeline for processing input and sending to controller - Step 7 implementation
    /// </summary>
    public class InputPipeline : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly IControllerService _controllerService;
        private readonly KeyToControllerMapper _keyMapper;
        private readonly MouseToStickMapper _mouseMapper;
        
        private bool _isEnabled;
        private bool _disposed;

        public InputPipeline(
            IInputService inputService,
            IControllerService controllerService,
            KeyToControllerMapper keyMapper,
            MouseToStickMapper mouseMapper)
        {
            _inputService = inputService;
            _controllerService = controllerService;
            _keyMapper = keyMapper;
            _mouseMapper = mouseMapper;

            // Subscribe to input events
            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;
        }

        /// <summary>
        /// Enable or disable the input pipeline
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (enabled)
            {
                // Ensure WASD is always blocked for left stick control
                EnsureWASDBlocked();
                
                // Start input capture
                _inputService.Start();
                // Enable input blocking (reWASD-like behavior)
                _inputService.SetInputBlocking(true);
            }
            else
            {
                // Disable input blocking first (restore normal input)
                _inputService.SetInputBlocking(false);
                // Stop input capture
                _inputService.Stop();
                // Reset controller state when disabled
                _controllerService.ResetState();
            }
        }

        /// <summary>
        /// Update mapper configurations
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyMapper.UpdateConfiguration(config);
            _mouseMapper.UpdateConfiguration(config);
            
            // Update which keys should be blocked
            UpdateBlockedKeys(config);
        }
        
        /// <summary>
        /// Ensure WASD keys are always blocked for left stick control
        /// </summary>
        private void EnsureWASDBlocked()
        {
            var keysToBlock = new System.Collections.Generic.HashSet<int>();
            var mouseButtonsToBlock = new System.Collections.Generic.HashSet<int>();
            
            // Always block WASD for left stick control
            keysToBlock.Add(87); // W
            keysToBlock.Add(65); // A  
            keysToBlock.Add(83); // S
            keysToBlock.Add(68); // D
            
            System.Diagnostics.Debug.WriteLine("InputPipeline: Ensuring WASD keys are blocked for left stick control");
            _inputService.SetKeysToBlock(keysToBlock, mouseButtonsToBlock);
        }
        
        /// <summary>
        /// Update the list of keys and mouse buttons to block based on configuration
        /// </summary>
        private void UpdateBlockedKeys(ConfigModel config)
        {
            var keysToBlock = new System.Collections.Generic.HashSet<int>();
            var mouseButtonsToBlock = new System.Collections.Generic.HashSet<int>();
            
            // Always block WASD for left stick control
            keysToBlock.Add(87); // W
            keysToBlock.Add(65); // A  
            keysToBlock.Add(83); // S
            keysToBlock.Add(68); // D
            
            // Add mapped keyboard keys
            if (config?.KeyboardSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.KeyboardSettings.ButtonMappings)
                {
                    int keyCode = InputKeyMap.GetKeyCode(mapping.KeyboardKey);
                    if (keyCode > 0)
                    {
                        keysToBlock.Add(keyCode);
                    }
                }
            }
            
            // Add mapped mouse buttons
            if (config?.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    int mouseButtonCode = InputKeyMap.GetMouseButtonCode(mapping.MouseButton);
                    if (mouseButtonCode > 0)
                    {
                        mouseButtonsToBlock.Add(mouseButtonCode);
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"InputPipeline: Updating blocked keys - Total keys: {keysToBlock.Count}, Total mouse buttons: {mouseButtonsToBlock.Count}");
            
            // Update input service with keys to block
            _inputService.SetKeysToBlock(keysToBlock, mouseButtonsToBlock);
        }

        /// <summary>
        /// Handle key down events
        /// </summary>
        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            // Process key through mapper
            _keyMapper.ProcessKeyDown(e.KeyCode);
        }

        /// <summary>
        /// Handle key up events
        /// </summary>
        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            // Process key through mapper
            _keyMapper.ProcessKeyUp(e.KeyCode);
        }

        /// <summary>
        /// Handle mouse state changes
        /// </summary>
        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            if (!_isEnabled) return;

            // Process mouse through mapper
            _mouseMapper.ProcessMouseInput(e.X, e.Y, e.Button);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Unsubscribe from events
            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _inputService.MouseStateChanged -= OnMouseStateChanged;

            // Stop input service
            if (_isEnabled)
            {
                _inputService.Stop();
            }

            _disposed = true;
        }
    }
}