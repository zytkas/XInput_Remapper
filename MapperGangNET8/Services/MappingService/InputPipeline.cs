using System;
using System.Linq;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputService;
using MapperGangNET8.Services.InputBlockingService;

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
        private readonly InputBlockingManager _blockingManager;
        
        private bool _isEnabled;
        private bool _disposed;
        
        // Timer for mouse stick decay updates
        private readonly System.Timers.Timer _stickDecayTimer;

        public InputPipeline(
            IInputService inputService,
            IControllerService controllerService,
            KeyToControllerMapper keyMapper,
            MouseToStickMapper mouseMapper,
            InputBlockingManager blockingManager)
        {
            _inputService = inputService;
            _controllerService = controllerService;
            _keyMapper = keyMapper;
            _mouseMapper = mouseMapper;
            _blockingManager = blockingManager;

            // Set blocking manager on input service if it supports it
            if (_inputService is Soju06InputService soju06Service)
            {
                soju06Service.SetInputBlockingManager(_blockingManager);
            }

            // Subscribe to input events
            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;
            
            // Subscribe to mouse delta events for camera control
            _blockingManager.MouseDeltaCaptured += OnMouseDeltaCaptured;
            
            // Setup stick decay timer (60 FPS updates)
            _stickDecayTimer = new System.Timers.Timer(64); // ~60 FPS
            _stickDecayTimer.Elapsed += OnStickDecayTimerElapsed;
            _stickDecayTimer.AutoReset = true;
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
                // Enable input blocking
                _blockingManager.SetEnabled(true);
                // Start input capture
                _inputService.Start();
                // Start stick decay timer
                _stickDecayTimer.Start();
            }
            else
            {
                // Stop stick decay timer
                _stickDecayTimer.Stop();
                // Stop input capture
                _inputService.Stop();
                // Disable input blocking
                _blockingManager.SetEnabled(false);
                // Reset controller state when disabled
                _controllerService.ResetState();
                // Reset mouse mapper
                _mouseMapper.Reset();
            }
        }

        /// <summary>
        /// Update mapper configurations
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyMapper.UpdateConfiguration(config);
            _mouseMapper.UpdateConfiguration(config);
            
            // Update input blocking based on configuration
            // Convert keyboard and mouse mappings to unified KeyBindingModel list for blocking
            var allBindings = new List<KeyBindingModel>();
            
            // Add keyboard button mappings
            if (config.KeyboardSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.KeyboardSettings.ButtonMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.KeyboardKey))
                    {
                        // Convert keyboard key string to soju06 InputKeys enum code
                        var keyCode = InputKeyMap.GetKeyCode(mapping.KeyboardKey);
                        if (keyCode != 0)
                        {
                            allBindings.Add(new KeyBindingModel
                            {
                                InputType = InputDeviceType.Keyboard,
                                InputCode = keyCode,
                                Action = ControllerButton.A // Placeholder - actual action doesn't matter for blocking
                            });
                        }
                    }
                }
            }
            
            // Add WASD movement keys (always blocked)
            var movementKeys = new[]
            {
                config.KeyboardSettings?.MovementUp ?? "W",
                config.KeyboardSettings?.MovementLeft ?? "A", 
                config.KeyboardSettings?.MovementDown ?? "S",
                config.KeyboardSettings?.MovementRight ?? "D"
            };
            
            foreach (var keyStr in movementKeys)
            {
                var keyCode = InputKeyMap.GetKeyCode(keyStr);
                if (keyCode != 0)
                {
                    allBindings.Add(new KeyBindingModel
                    {
                        InputType = InputDeviceType.Keyboard,
                        InputCode = keyCode,
                        Action = ControllerButton.A // Placeholder
                    });
                }
            }
            
            // Add mouse button mappings
            if (config.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.MouseButton))
                    {
                        allBindings.Add(new KeyBindingModel
                        {
                            InputType = InputDeviceType.Mouse,
                            InputCode = 0, // Mouse buttons handled differently
                            Action = ControllerButton.A // Placeholder
                        });
                    }
                }
            }
            
            _blockingManager.UpdateBlockedKeys(allBindings);
            
            bool hasMouseButtonBindings = config.MouseSettings?.ButtonMappings?.Any() ?? false;
            _blockingManager.SetMouseBlocking(
                blockMovement: true, // Always block mouse movement for right stick control
                blockButtons: hasMouseButtonBindings, // Block mouse buttons if they are mapped
                captureDeltas: true // Always capture mouse deltas for camera control
            );
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
            if (e.Button != 0)
            {
                _mouseMapper.ProcessMouseInput(e.X, e.Y, e.Button);
            }
        }

        /// <summary>
        /// Handle mouse delta events for camera control
        /// </summary>
        private void OnMouseDeltaCaptured(object sender, MouseDeltaEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[PIPELINE] Mouse delta captured: X={e.DeltaX}, Y={e.DeltaY}, Enabled={_isEnabled}");

            if (!_isEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[PIPELINE] Skipped - pipeline disabled");
                return;
            }

            // Process mouse deltas for camera/stick mapping
            _mouseMapper.ProcessMouseDelta(e.DeltaX, e.DeltaY);
        }

        /// <summary>
        /// Handle stick decay timer events
        /// </summary>
        private void OnStickDecayTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (_isEnabled)
            {
                System.Diagnostics.Debug.WriteLine($"[DECAY TIMER] Tick at {DateTimeOffset.Now:HH:mm:ss.fff}");
                _mouseMapper.UpdateStickDecay();
            }
        }
        
        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Stop and dispose timer
            _stickDecayTimer?.Stop();
            _stickDecayTimer?.Dispose();
            
            // Unsubscribe from events
            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _inputService.MouseStateChanged -= OnMouseStateChanged;
            _blockingManager.MouseDeltaCaptured -= OnMouseDeltaCaptured;

            // Stop input service
            if (_isEnabled)
            {
                _inputService.Stop();
            }

            // Dispose blocking manager
            _blockingManager?.Dispose();

            _disposed = true;
        }
    }
}