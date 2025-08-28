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
            return InputKeyMap.GetKeyCode(keyString);
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
                    ControllerAction.AButton => "A",
                    ControllerAction.BButton => "B",
                    ControllerAction.XButton => "X",
                    ControllerAction.YButton => "Y",
                    ControllerAction.LeftBumper => "LeftShoulder",
                    ControllerAction.RightBumper => "RightShoulder",
                    ControllerAction.LeftTrigger => "LeftTrigger",
                    ControllerAction.RightTrigger => "RightTrigger",
                    ControllerAction.LeftStickPress => "LeftThumb",
                    ControllerAction.RightStickPress => "RightThumb",
                    ControllerAction.DPadUp => "DPadUp",
                    ControllerAction.DPadDown => "DPadDown",
                    ControllerAction.DPadLeft => "DPadLeft",
                    ControllerAction.DPadRight => "DPadRight",
                    ControllerAction.Start => "Start",
                    ControllerAction.Back => "Back",
                    ControllerAction.Guide => "Guide",
                    _ => action.Value.ToString()
                };
            }
            
            return input.Replace(" ", "");
        }
    }
}