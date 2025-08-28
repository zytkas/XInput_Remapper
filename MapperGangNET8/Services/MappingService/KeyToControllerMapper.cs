using System.Collections.Generic;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps keyboard input to controller buttons - Step 7 implementation
    /// </summary>
    public class KeyToControllerMapper
    {
        private readonly IControllerService _controllerService;
        private readonly Dictionary<int, ControllerButton> _keyButtonMappings = new();
        private readonly HashSet<int> _pressedKeys = new();

        public KeyToControllerMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        /// <summary>
        /// Update key mappings from configuration
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyButtonMappings.Clear();

            if (config?.KeyboardSettings?.ButtonMappings == null) 
            {
                System.Diagnostics.Debug.WriteLine("KeyToControllerMapper: No keyboard button mappings found in config");
                return;
            }

            foreach (var mapping in config.KeyboardSettings.ButtonMappings)
            {
                int keyCode = GetKeyCodeFromString(mapping.KeyboardKey);
                string enumName = ConvertToEnumName(mapping.ControllerButton);
                
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Processing mapping - Key: {mapping.KeyboardKey} (Code: {keyCode}) -> Controller: {mapping.ControllerButton} (Enum: {enumName})");
                
                if (keyCode > 0 && System.Enum.TryParse<ControllerButton>(enumName, out var button))
                {
                    _keyButtonMappings[keyCode] = button;
                    System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Successfully mapped key {keyCode} to controller button {button}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Failed to map - KeyCode: {keyCode}, EnumName: {enumName}");
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Total mappings loaded: {_keyButtonMappings.Count}");
        }

        /// <summary>
        /// Process key down event
        /// </summary>
        public void ProcessKeyDown(int keyCode)
        {
            System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: ProcessKeyDown - KeyCode: {keyCode}");
            
            if (_pressedKeys.Contains(keyCode)) return; // Avoid repeat events
            
            _pressedKeys.Add(keyCode);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(keyCode, out var button))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Key {keyCode} mapped to controller button {button} - pressing");
                _controllerService.SetButton(button, true);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Key {keyCode} not mapped to any controller button");
            }

            // Handle directional keys for analog sticks (WASD)
            UpdateAnalogSticks();
        }

        /// <summary>
        /// Process key up event
        /// </summary>
        public void ProcessKeyUp(int keyCode)
        {
            _pressedKeys.Remove(keyCode);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(keyCode, out var button))
            {
                _controllerService.SetButton(button, false);
            }

            // Handle directional keys for analog sticks (WASD)
            UpdateAnalogSticks();
        }

        /// <summary>
        /// Update analog stick positions based on currently pressed directional keys
        /// </summary>
        private void UpdateAnalogSticks()
        {
            // WASD mapping for left stick
            bool w = _pressedKeys.Contains(GetKeyCodeFromString("W"));
            bool a = _pressedKeys.Contains(GetKeyCodeFromString("A"));
            bool s = _pressedKeys.Contains(GetKeyCodeFromString("S"));
            bool d = _pressedKeys.Contains(GetKeyCodeFromString("D"));

            // Calculate stick position
            double x = (d ? 1.0 : 0.0) - (a ? 1.0 : 0.0);
            double y = (w ? 1.0 : 0.0) - (s ? 1.0 : 0.0);

            // Normalize diagonal movement to maintain consistent speed
            if (x != 0 && y != 0)
            {
                double length = System.Math.Sqrt(x * x + y * y);
                x /= length;
                y /= length;
            }

            // Apply to left stick
            _controllerService.SetAxis(ControllerAxis.LeftThumbX, x);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, y);
        }

        /// <summary>
        /// Convert key string to virtual key code
        /// </summary>
        private int GetKeyCodeFromString(string keyString)
        {
            // Basic key mapping - can be expanded
            return keyString switch
            {
                "A" => 65, "B" => 66, "C" => 67, "D" => 68, "E" => 69,
                "F" => 70, "G" => 71, "H" => 72, "I" => 73, "J" => 74,
                "K" => 75, "L" => 76, "M" => 77, "N" => 78, "O" => 79,
                "P" => 80, "Q" => 81, "R" => 82, "S" => 83, "T" => 84,
                "U" => 85, "V" => 86, "W" => 87, "X" => 88, "Y" => 89, "Z" => 90,
                "Space" => 32,
                "Left Ctrl" => 17,
                "Left Shift" => 16,
                "Left Alt" => 18,
                "Enter" => 13,
                "Tab" => 9,
                "Escape" => 27,
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
    }
}