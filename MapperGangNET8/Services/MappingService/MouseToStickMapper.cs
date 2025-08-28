using System.Collections.Generic;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps mouse input to controller sticks and buttons - Step 7 implementation
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;
        private readonly Dictionary<int, ControllerButton> _mouseButtonMappings = new();
        
        // Mouse tracking
        private int _lastMouseX;
        private int _lastMouseY;
        private bool _isFirstMouseEvent = true;
        
        // Basic sensitivity (Step 7 - simple implementation)
        private double _mouseSensitivity = 1.0;

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

            if (config?.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    int buttonCode = GetMouseButtonCodeFromString(mapping.MouseButton);
                    string enumName = ConvertToEnumName(mapping.ControllerButton);
                    
                    System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Processing mapping - Mouse: {mapping.MouseButton} (Code: {buttonCode}) -> Controller: {mapping.ControllerButton} (Enum: {enumName})");
                    
                    if (buttonCode > 0 && System.Enum.TryParse<ControllerButton>(enumName, out var button))
                    {
                        _mouseButtonMappings[buttonCode] = button;
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Successfully mapped mouse button {buttonCode} to controller button {button}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Failed to map - ButtonCode: {buttonCode}, EnumName: {enumName}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("MouseToStickMapper: No mouse button mappings found in config");
            }

            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Total mappings loaded: {_mouseButtonMappings.Count}");

            // Update basic sensitivity from config
            if (config?.SensitivitySettings != null)
            {
                _mouseSensitivity = config.SensitivitySettings.MouseXAxisSensitivity / 100.0; // Convert percentage to decimal
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

            // Apply basic sensitivity and normalize
            double stickX = (deltaX * _mouseSensitivity) / 100.0; // Scale down movement
            double stickY = -(deltaY * _mouseSensitivity) / 100.0; // Invert Y axis, scale down

            // Clamp to valid stick range [-1.0, 1.0]
            stickX = System.Math.Max(-1.0, System.Math.Min(1.0, stickX));
            stickY = System.Math.Max(-1.0, System.Math.Min(1.0, stickY));

            // Apply to right stick (for camera/looking)
            _controllerService.SetAxis(ControllerAxis.RightThumbX, stickX);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, stickY);
        }

        /// <summary>
        /// Process mouse button press/release
        /// </summary>
        private void ProcessMouseButton(int button)
        {
            bool isPressed = button > 0;
            int buttonCode = System.Math.Abs(button);

            System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: ProcessMouseButton - Button: {button}, ButtonCode: {buttonCode}, Pressed: {isPressed}");

            // Check if mouse button is mapped to a controller button
            if (_mouseButtonMappings.TryGetValue(buttonCode, out var controllerButton))
            {
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Mouse button {buttonCode} mapped to controller button {controllerButton} - setting to {isPressed}");
                _controllerService.SetButton(controllerButton, isPressed);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"MouseToStickMapper: Mouse button {buttonCode} not mapped to any controller button");
            }
        }

        /// <summary>
        /// Convert mouse button string to button code
        /// </summary>
        private int GetMouseButtonCodeFromString(string buttonString)
        {
            return buttonString switch
            {
                "Left Button" => 1,
                "Right Button" => 2,
                "Middle Button" => 3,
                "Side Button 1" => 4,
                "Side Button 2" => 5,
                _ => 0
            };
        }

        /// <summary>
        /// Convert UI controller button names to enum names
        /// </summary>
        private string ConvertToEnumName(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            
            return input switch
            {
                "A Button" => "A",
                "B Button" => "B", 
                "X Button" => "X",
                "Y Button" => "Y",
                "Left Bumper" => "LeftShoulder",
                "Right Bumper" => "RightShoulder", 
                "Left Trigger" => "LeftTrigger",
                "Right Trigger" => "RightTrigger",
                "Left Stick Press" => "LeftThumb",
                "Right Stick Press" => "RightThumb",
                "D-Pad Up" => "DPadUp",
                "D-Pad Down" => "DPadDown", 
                "D-Pad Left" => "DPadLeft",
                "D-Pad Right" => "DPadRight",
                "Start Button" => "Start",
                "Back Button" => "Back",
                "Guide Button" => "Guide",
                _ => input.Replace(" ", "")
            };
        }

        /// <summary>
        /// Reset mouse tracking state
        /// </summary>
        public void Reset()
        {
            _isFirstMouseEvent = true;
            _lastMouseX = 0;
            _lastMouseY = 0;
        }
    }
}