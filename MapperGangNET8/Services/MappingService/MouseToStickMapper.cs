using System.Collections.Generic;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using Input;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps mouse input to controller sticks and buttons - Step 7 implementation
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;
        private readonly Dictionary<InputButtons, ControllerButton> _mouseButtonMappings = new();
        private readonly Dictionary<InputButtons, ControllerAxis> _mouseAxisMappings = new();
        private readonly HashSet<InputButtons> _pressedMouseButtons = new(); // Track pressed mouse buttons
        
        // Mouse tracking
        private int _lastMouseX;
        private int _lastMouseY;
        private bool _isFirstMouseEvent = true;
        
        // Right stick position accumulation (for camera control)
        private double _rightStickX = 0.0;
        private double _rightStickY = 0.0;
        
        // Stick behavior settings
        private double _mouseSensitivity = 1.0;
        private double _stickDecayRate = 0.85; // How fast stick returns to center (0.0 = instant, 1.0 = never)
        private bool _enableStickDecay = true; // Whether stick should return to center when mouse stops
        private long _lastMouseMoveTime = 0;
        private const long STICK_DECAY_DELAY_MS = 100; // Wait before starting decay
        
        

        public MouseToStickMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        /// <summary>
        /// Update mouse mappings from configuration
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _mouseButtonMappings.Clear();
            _mouseAxisMappings.Clear();

            if (config?.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    InputButtons inputButton = GetInputMouseButtonFromString(mapping.MouseButton);
                    string enumName = ConvertToEnumName(mapping.ControllerButton);
                    
                    System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Processing mapping - Mouse: {mapping.MouseButton} (InputButton: {inputButton}) -> Controller: {mapping.ControllerButton} (Enum: {enumName})");
                    
                    // Check what GetControllerAction returns
                    var controllerAction = InputKeyMap.GetControllerAction(mapping.ControllerButton);
                    System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: GetControllerAction(\"{mapping.ControllerButton}\") returned: {controllerAction}");
                    
                    if (inputButton != InputButtons.None)
                    {
                        // Check if this is a trigger (should be mapped to axis)
                        if (enumName == "LeftTrigger")
                        {
                            _mouseAxisMappings[inputButton] = ControllerAxis.LeftTrigger;
                            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Successfully mapped mouse button {inputButton} to left trigger axis");
                        }
                        else if (enumName == "RightTrigger")
                        {
                            _mouseAxisMappings[inputButton] = ControllerAxis.RightTrigger;
                            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Successfully mapped mouse button {inputButton} to right trigger axis");
                        }
                        else if (System.Enum.TryParse<ControllerButton>(enumName, out var button))
                        {
                            _mouseButtonMappings[inputButton] = button;
                            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Successfully mapped mouse button {inputButton} to controller button {button}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Failed to map - InputButton: {inputButton}, EnumName: {enumName}");
                        }
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MouseToStickMapper: No mouse button mappings found in config");
            }

            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Total button mappings loaded: {_mouseButtonMappings.Count}, axis mappings: {_mouseAxisMappings.Count}");

            // Update mouse settings from config
            if (config?.SensitivitySettings != null)
            {
                _mouseSensitivity = config.SensitivitySettings.MouseXAxisSensitivity / 100.0; // Convert percentage to decimal
            }
            
            // Update mouse behavior settings
            if (config?.MouseSettings != null)
            {
                // Check if we should enable stick decay (return to center)
                // For now, enable by default with moderate decay rate
                SetStickDecaySettings(true, 0.85);
                
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Updated settings - Sensitivity: {_mouseSensitivity:F2}, Decay: {_enableStickDecay}, Rate: {_stickDecayRate:F2}");
            }
        }

        /// <summary>
        /// Process mouse input
        /// </summary>
        public void ProcessMouseInput(int x, int y, int button)
        {
            // Handle mouse movement
            ProcessMouseMovement(x, y);
            
            // Handle mouse buttons
            if (button != 0)
            {
                ProcessMouseButton(button);
            }
        }

        /// <summary>
        /// Process mouse movement and map to right stick
        /// </summary>
        private void ProcessMouseMovement(int x, int y)
        {
            // Skip first event to establish baseline
            if (_isFirstMouseEvent)
            {
                _lastMouseX = x;
                _lastMouseY = y;
                _isFirstMouseEvent = false;
                return;
            }

            // Calculate movement delta
            int deltaX = x - _lastMouseX;
            int deltaY = y - _lastMouseY;

            // Update last position
            _lastMouseX = x;
            _lastMouseY = y;
            
            // Record time of mouse movement
            _lastMouseMoveTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Only process if there's actual movement
            if (deltaX != 0 || deltaY != 0)
            {
                // Apply sensitivity and scale
                double moveX = (deltaX * _mouseSensitivity) / 50.0; // Adjust scale for better feel
                double moveY = -(deltaY * _mouseSensitivity) / 50.0; // Invert Y axis

                // Accumulate movement to stick position
                _rightStickX += moveX;
                _rightStickY += moveY;

                // Clamp accumulated position to valid stick range [-1.0, 1.0]
                _rightStickX = System.Math.Max(-1.0, System.Math.Min(1.0, _rightStickX));
                _rightStickY = System.Math.Max(-1.0, System.Math.Min(1.0, _rightStickY));

                // Apply to right stick (for camera/looking)
                _controllerService.SetAxis(ControllerAxis.RightThumbX, _rightStickX);
                _controllerService.SetAxis(ControllerAxis.RightThumbY, _rightStickY);
                
               // System.Diagnostics.Debug.WriteLine($"MouseToStick: Delta({deltaX},{deltaY}) -> Stick({_rightStickX:F3},{_rightStickY:F3})");
            }
        }

        /// <summary>
        /// Process mouse button press/release using InputButtons enum
        /// </summary>
        private void ProcessMouseButton(int button)
        {
            // Skip if button is 0 (no button pressed)
            if (button == 0) return;
            
            // Cast button code directly to InputButtons enum
            if (!System.Enum.IsDefined(typeof(InputButtons), (byte)button))
            {
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Unknown InputButtons code: {button}");
                return;
            }
            
            InputButtons inputButton = (InputButtons)(byte)button;
            
            // Skip mouse movement events
            if (inputButton == InputButtons.Move)
            {
                return;
            }
            
            // Determine if this is a press or release event based on InputButtons enum
            bool isPressed = false;
            InputButtons mappingKey = InputButtons.None;
            
            switch (inputButton)
            {
                case InputButtons.LeftMouseDown:
                    isPressed = true;
                    mappingKey = InputButtons.LeftMouseDown;
                    break;
                case InputButtons.LeftMouseUp:
                    isPressed = false;
                    mappingKey = InputButtons.LeftMouseDown; // Use down as mapping key for tracking
                    break;
                case InputButtons.RightMouseDown:
                    isPressed = true;
                    mappingKey = InputButtons.RightMouseDown;
                    break;
                case InputButtons.RightMouseUp:
                    isPressed = false;
                    mappingKey = InputButtons.RightMouseDown; // Use down as mapping key for tracking
                    break;
                case InputButtons.LeftDoubleClick:
                    // Handle double click as press event
                    isPressed = true;
                    mappingKey = InputButtons.LeftDoubleClick;
                    break;
                case InputButtons.WheelUp:
                case InputButtons.WheelDown:
                case InputButtons.WheelMoveUp:
                case InputButtons.WheelMoveDown:
                    // Handle wheel events as momentary presses
                    isPressed = true;
                    mappingKey = inputButton;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Unhandled InputButtons: {inputButton}");
                    return;
            }
            
            string buttonName = inputButton.ToString();

            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: ProcessMouseButton - InputButton: {inputButton} ({buttonName}), MappingKey: {mappingKey}, Pressed: {isPressed}");

            // Track button state to avoid duplicate events (except for wheel events which are momentary)
            if (inputButton != InputButtons.WheelUp && inputButton != InputButtons.WheelDown && 
                inputButton != InputButtons.WheelMoveUp && inputButton != InputButtons.WheelMoveDown)
            {
                if (isPressed)
                {
                    if (_pressedMouseButtons.Contains(mappingKey))
                    {
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Button {mappingKey} already pressed, ignoring repeat event");
                        return;
                    }
                    _pressedMouseButtons.Add(mappingKey);
                }
                else
                {
                    if (!_pressedMouseButtons.Contains(mappingKey))
                    {
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Button {mappingKey} not in pressed state, ignoring release event");
                        return;
                    }
                    _pressedMouseButtons.Remove(mappingKey);
                }
            }

            // Check if mouse button is mapped to a controller button
            if (_mouseButtonMappings.TryGetValue(mappingKey, out var controllerButton))
            {
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Mouse button {mappingKey} ({buttonName}) mapped to controller button {controllerButton} - setting to {isPressed}");
                _controllerService.SetButton(controllerButton, isPressed);
                
                // For wheel events, immediately release after pressing
                if (inputButton == InputButtons.WheelUp || inputButton == InputButtons.WheelDown || 
                    inputButton == InputButtons.WheelMoveUp || inputButton == InputButtons.WheelMoveDown)
                {
                    System.Threading.Tasks.Task.Delay(50).ContinueWith(_ => 
                    {
                        _controllerService.SetButton(controllerButton, false);
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Released wheel button {controllerButton}");
                    });
                }
            }
            // Check if mouse button is mapped to a controller axis (like triggers)
            else if (_mouseAxisMappings.TryGetValue(mappingKey, out var controllerAxis))
            {
                double axisValue = isPressed ? 1.0 : 0.0; // Full press for digital mouse button
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Mouse button {mappingKey} ({buttonName}) mapped to controller axis {controllerAxis} - setting to {axisValue}");
                _controllerService.SetAxis(controllerAxis, axisValue);
                
                // For wheel events, immediately release after pressing
                if (inputButton == InputButtons.WheelUp || inputButton == InputButtons.WheelDown || 
                    inputButton == InputButtons.WheelMoveUp || inputButton == InputButtons.WheelMoveDown)
                {
                    System.Threading.Tasks.Task.Delay(50).ContinueWith(_ => 
                    {
                        _controllerService.SetAxis(controllerAxis, 0.0);
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Released wheel axis {controllerAxis}");
                    });
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Mouse button {mappingKey} ({buttonName}) not mapped to any controller input");
            }
        }

        /// <summary>
        /// Convert mouse button string to InputButtons enum
        /// </summary>
        private InputButtons GetInputMouseButtonFromString(string buttonString)
        {
            return InputKeyMap.GetInputMouseButton(buttonString);
        }

        /// <summary>
        /// Get mouse button name from InputButtons enum for debugging
        /// </summary>
        private string GetMouseButtonName(InputButtons inputButton)
        {
            return InputKeyMap.GetMouseButtonName(inputButton);
        }

        /// <summary>
        /// Convert UI controller button names to enum names
        /// </summary>
        private string ConvertToEnumName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            
            var action = InputKeyMap.GetControllerAction(input);
            if (action.HasValue)
            {
                return action.Value switch
                {
                    ControllerButton.A => "A",
                    ControllerButton.B => "B",
                    ControllerButton.X => "X",
                    ControllerButton.Y => "Y",
                    ControllerButton.LeftShoulder => "LeftShoulder",
                    ControllerButton.RightShoulder => "RightShoulder",
                    ControllerButton.LeftTrigger => "LeftTrigger",
                    ControllerButton.RightTrigger => "RightTrigger",
                    ControllerButton.LeftThumb => "LeftThumb",
                    ControllerButton.RightThumb => "RightThumb",
                    ControllerButton.DPadUp => "DPadUp",
                    ControllerButton.DPadDown => "DPadDown",
                    ControllerButton.DPadLeft => "DPadLeft",
                    ControllerButton.DPadRight => "DPadRight",
                    ControllerButton.Start => "Start",
                    ControllerButton.Back => "Back",
                    ControllerButton.Guide => "Guide",
                    _ => action.Value.ToString()
                };
            }
            
            return input.Replace(" ", "");
        }

        /// <summary>
        /// Update stick decay (should be called regularly, e.g. on a timer)
        /// </summary>
        public void UpdateStickDecay()
        {
            if (!_enableStickDecay)
                return;
                
            long currentTime = System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
            
            // Only start decay if mouse hasn't moved recently
            if (currentTime - _lastMouseMoveTime > STICK_DECAY_DELAY_MS)
            {
                // Apply decay to return stick to center
                _rightStickX *= _stickDecayRate;
                _rightStickY *= _stickDecayRate;
                
                // Snap to zero if very close to center
                if (System.Math.Abs(_rightStickX) < 0.01) _rightStickX = 0.0;
                if (System.Math.Abs(_rightStickY) < 0.01) _rightStickY = 0.0;
                
                // Update controller
                _controllerService.SetAxis(ControllerAxis.RightThumbX, _rightStickX);
                _controllerService.SetAxis(ControllerAxis.RightThumbY, _rightStickY);
            }
        }
        
        /// <summary>
        /// Set stick decay settings
        /// </summary>
        public void SetStickDecaySettings(bool enabled, double decayRate = 0.85)
        {
            _enableStickDecay = enabled;
            _stickDecayRate = System.Math.Max(0.0, System.Math.Min(1.0, decayRate));
        }
        
        /// <summary>
        /// Reset mouse tracking state
        /// </summary>
        public void Reset()
        {
            _isFirstMouseEvent = true;
            _lastMouseX = 0;
            _lastMouseY = 0;
            _rightStickX = 0.0;
            _rightStickY = 0.0;
            _lastMouseMoveTime = 0;
            _pressedMouseButtons.Clear();
            
            // Reset controller stick to center
            _controllerService.SetAxis(ControllerAxis.RightThumbX, 0.0);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, 0.0);
        }
    }
}