using System.Collections.Generic;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using Input;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps keyboard input to controller buttons - Step 7 implementation
    /// </summary>
    public class KeyToControllerMapper
    {
        private readonly IControllerService _controllerService;
        private readonly Dictionary<InputKeys, ControllerButton> _keyButtonMappings = new();
        private readonly Dictionary<InputKeys, ControllerAxis> _keyAxisMappings = new();
        private readonly HashSet<InputKeys> _pressedKeys = new();

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
            _keyAxisMappings.Clear();

            if (config?.KeyboardSettings?.ButtonMappings == null) 
            {
                System.Diagnostics.Debug.WriteLine("KeyToControllerMapper: No keyboard button mappings found in config");
                return;
            }

            foreach (var mapping in config.KeyboardSettings.ButtonMappings)
            {
                InputKeys inputKey = GetInputKeyFromString(mapping.KeyboardKey);
                string enumName = ConvertToEnumName(mapping.ControllerButton);
                
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Processing mapping - Key: {mapping.KeyboardKey} (InputKey: {inputKey}) -> Controller: {mapping.ControllerButton} (Enum: {enumName})");
                
                // Check what GetControllerAction returns
                var controllerAction = InputKeyMap.GetControllerAction(mapping.ControllerButton);
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: GetControllerAction(\"{mapping.ControllerButton}\") returned: {controllerAction}");
                
                if (inputKey != InputKeys.None)
                {
                    // Check if this is a trigger (should be mapped to axis)
                    if (enumName == "LeftTrigger")
                    {
                        _keyAxisMappings[inputKey] = ControllerAxis.LeftTrigger;
                        System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Successfully mapped key {inputKey} to left trigger axis");
                    }
                    else if (enumName == "RightTrigger")
                    {
                        _keyAxisMappings[inputKey] = ControllerAxis.RightTrigger;
                        System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Successfully mapped key {inputKey} to right trigger axis");
                    }
                    else if (System.Enum.TryParse<ControllerButton>(enumName, out var button))
                    {
                        _keyButtonMappings[inputKey] = button;
                        System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Successfully mapped key {inputKey} to controller button {button}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Failed to map - InputKey: {inputKey}, EnumName: {enumName}");
                    }
                }
            }
            
            System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Total button mappings loaded: {_keyButtonMappings.Count}, axis mappings: {_keyAxisMappings.Count}");
        }

        /// <summary>
        /// Process key down event using InputKeys enum
        /// </summary>
        public void ProcessKeyDown(int keyCode)
        {
            // Convert keyCode to InputKeys enum
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Unknown InputKeys code: {keyCode}");
                return;
            }
            
            InputKeys inputKey = (InputKeys)(byte)keyCode;
            string keyName = GetKeyNameFromInputKey(inputKey);
            System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: ProcessKeyDown - InputKey: {inputKey} ({keyName})");
            
            if (_pressedKeys.Contains(inputKey)) return; // Avoid repeat events
            
            _pressedKeys.Add(inputKey);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Key {inputKey} ({keyName}) mapped to controller button {button} - pressing");
                _controllerService.SetButton(button, true);
            }
            // Check if key is mapped to a controller axis (like triggers)
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Key {inputKey} ({keyName}) mapped to controller axis {axis} - activating");
                _controllerService.SetAxis(axis, 1.0); // Full press for digital key
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Key {inputKey} ({keyName}) not mapped to any controller input");
            }

            // Handle directional keys for analog sticks (WASD)
            UpdateAnalogSticks();
        }

        /// <summary>
        /// Process key up event using InputKeys enum
        /// </summary>
        public void ProcessKeyUp(int keyCode)
        {
            // Convert keyCode to InputKeys enum
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Unknown InputKeys code: {keyCode}");
                return;
            }
            
            InputKeys inputKey = (InputKeys)(byte)keyCode;
            _pressedKeys.Remove(inputKey);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
            {
                _controllerService.SetButton(button, false);
            }
            // Check if key is mapped to a controller axis (like triggers)
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
            {
                _controllerService.SetAxis(axis, 0.0); // Release trigger
            }

            // Handle directional keys for analog sticks (WASD)
            UpdateAnalogSticks();
        }

        /// <summary>
        /// Update analog stick positions based on currently pressed directional keys
        /// </summary>
        private void UpdateAnalogSticks()
        {
            // WASD mapping for left stick using InputKeys enum
            bool w = _pressedKeys.Contains(InputKeys.W);
            bool a = _pressedKeys.Contains(InputKeys.A);
            bool s = _pressedKeys.Contains(InputKeys.S);
            bool d = _pressedKeys.Contains(InputKeys.D);

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
        /// Convert key string to InputKeys enum
        /// </summary>
        private InputKeys GetInputKeyFromString(string keyString)
        {
            return InputKeyMap.GetInputKey(keyString);
        }

        /// <summary>
        /// Get key name from InputKeys enum for debugging
        /// </summary>
        private string GetKeyNameFromInputKey(InputKeys inputKey)
        {
            return InputKeyMap.GetKeyboardKeyName(inputKey);
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
    }
}