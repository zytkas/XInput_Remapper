using Input;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using System.Collections.Generic;
using System.Linq;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps keyboard and mouse buttons to controller buttons
    /// </summary>
    public class KeyToControllerMapper
    {
        private readonly IControllerService _controllerService;

        // Keyboard mappings
        private readonly Dictionary<InputKeys, ControllerButton> _keyButtonMappings = new();
        private readonly Dictionary<InputKeys, ControllerAxis> _keyAxisMappings = new();
        private readonly HashSet<InputKeys> _pressedKeys = new();

        // Mouse button mappings
        private readonly Dictionary<InputButtons, ControllerButton> _mouseButtonMappings = new();
        private readonly Dictionary<InputButtons, ControllerAxis> _mouseAxisMappings = new();
        private readonly HashSet<InputButtons> _pressedMouseButtons = new();

        // WASD movement tracking for left stick (ходьба)
        private InputKeys _movementUpKey = InputKeys.W;
        private InputKeys _movementLeftKey = InputKeys.A;
        private InputKeys _movementDownKey = InputKeys.S;
        private InputKeys _movementRightKey = InputKeys.D;
        private readonly HashSet<InputKeys> _pressedMovementKeys = new();

        public KeyToControllerMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        /// <summary>
        /// Update key and mouse button mappings from configuration
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            // Clear all mappings
            _keyButtonMappings.Clear();
            _keyAxisMappings.Clear();
            _mouseButtonMappings.Clear();
            _mouseAxisMappings.Clear();

            // Load keyboard mappings
            if (config?.KeyboardSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.KeyboardSettings.ButtonMappings)
                {
                    InputKeys inputKey = GetInputKeyFromString(mapping.KeyboardKey);
                    string enumName = ConvertToEnumName(mapping.ControllerButton);

                    if (inputKey != InputKeys.None)
                    {
                        // Check if this is a trigger (should be mapped to axis)
                        if (enumName == "LeftTrigger")
                        {
                            _keyAxisMappings[inputKey] = ControllerAxis.LeftTrigger;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped key {inputKey} to left trigger axis");
                        }
                        else if (enumName == "RightTrigger")
                        {
                            _keyAxisMappings[inputKey] = ControllerAxis.RightTrigger;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped key {inputKey} to right trigger axis");
                        }
                        else if (System.Enum.TryParse<ControllerButton>(enumName, out var button))
                        {
                            _keyButtonMappings[inputKey] = button;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped key {inputKey} to button {button}");
                        }
                    }
                }
            }

            // Load mouse button mappings
            if (config?.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    InputButtons inputButton = GetInputMouseButtonFromString(mapping.MouseButton);
                    // Map mouse button names to codes
                    string enumName = ConvertToEnumName(mapping.ControllerButton);

                    if (inputButton != InputButtons.None)
                    {
                        // Check if this is a trigger (should be mapped to axis)
                        if (enumName == "LeftTrigger")
                        {
                            _mouseAxisMappings[inputButton] = ControllerAxis.LeftTrigger;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped mouse button {mapping.MouseButton} to left trigger axis");
                        }
                        else if (enumName == "RightTrigger")
                        {
                            _mouseAxisMappings[inputButton] = ControllerAxis.RightTrigger;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped mouse button {mapping.MouseButton} to right trigger axis");
                        }
                        else if (System.Enum.TryParse<ControllerButton>(enumName, out var button))
                        {
                            _mouseButtonMappings[inputButton] = button;
                            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Mapped mouse button {mapping.MouseButton} to button {button}");
                        }
                    }
                }
            }

            // Load WASD movement keys
            if (config?.KeyboardSettings != null)
            {
                _movementUpKey = GetInputKeyFromString(config.KeyboardSettings.MovementUp ?? "W");
                _movementLeftKey = GetInputKeyFromString(config.KeyboardSettings.MovementLeft ?? "A");
                _movementDownKey = GetInputKeyFromString(config.KeyboardSettings.MovementDown ?? "S");
                _movementRightKey = GetInputKeyFromString(config.KeyboardSettings.MovementRight ?? "D");

                System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Movement keys: Up={_movementUpKey}, Left={_movementLeftKey}, Down={_movementDownKey}, Right={_movementRightKey}");
            }

            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Loaded: {_keyButtonMappings.Count} key buttons, {_keyAxisMappings.Count} key axes, {_mouseButtonMappings.Count} mouse buttons, {_mouseAxisMappings.Count} mouse axes");
        }

        private InputButtons GetInputMouseButtonFromString(string buttonString)
        {
            return InputKeyMap.GetInputMouseButton(buttonString);
        }
        /// <summary>
        /// Process key down event
        /// </summary>
        public void ProcessKeyDown(int keyCode)
        {
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode))
            {
                return;
            }

            InputKeys inputKey = (InputKeys)(byte)keyCode;

            if (_pressedKeys.Contains(inputKey)) return; // Avoid repeat events
            _pressedKeys.Add(inputKey);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
            {
                _controllerService.SetButton(button, true);
                System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Key {inputKey} -> Button {button} pressed");
            }
            // Check if key is mapped to a controller axis (triggers)
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
            {
                _controllerService.SetAxis(axis, 1.0); // Full press for digital key
                System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Key {inputKey} -> Axis {axis} activated");
            }

            // Check if this is a movement key (WASD for left stick)
            if (IsMovementKey(inputKey))
            {
                _pressedMovementKeys.Add(inputKey);
                UpdateLeftStickFromMovement();
            }
        }

        /// <summary>
        /// Process key up event
        /// </summary>
        public void ProcessKeyUp(int keyCode)
        {
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode))
            {
                return;
            }

            InputKeys inputKey = (InputKeys)(byte)keyCode;

            _pressedKeys.Remove(inputKey);

            // Check if key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
            {
                _controllerService.SetButton(button, false);
                System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Key {inputKey} -> Button {button} released");
            }
            // Check if key is mapped to a controller axis (triggers)
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
            {
                _controllerService.SetAxis(axis, 0.0); // Release
                System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Key {inputKey} -> Axis {axis} released");
            }

            // Check if this is a movement key (WASD for left stick)
            if (IsMovementKey(inputKey))
            {
                _pressedMovementKeys.Remove(inputKey);
                UpdateLeftStickFromMovement();
            }
        }

        /// <summary>
        /// Process mouse button event
        /// </summary>

        /// <summary>
        /// Process mouse button press/release using InputButtons enum
        /// </summary>
        public void ProcessMouseButton(int button)
        {
            // Skip if button is 0 (no button pressed)
            if (button == 0) return;

            // Cast button code directly to InputButtons enum
            if (!System.Enum.IsDefined(typeof(InputButtons), (byte)button))
            {
                System.Diagnostics.Debug.WriteLine($"KeyToControllerMapper: Unknown InputButtons code: {button}");
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
        /// Convert keyboard string to InputKeys enum
        /// </summary>
        private InputKeys GetInputKeyFromString(string keyString)
        {
            return InputKeyMap.GetInputKey(keyString);
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
        /// Check if key is a movement key (WASD)
        /// </summary>
        private bool IsMovementKey(InputKeys key)
        {
            return key == _movementUpKey || key == _movementLeftKey || 
                   key == _movementDownKey || key == _movementRightKey;
        }

        /// <summary>
        /// Update left stick position based on pressed movement keys
        /// </summary>
        private void UpdateLeftStickFromMovement()
        {
            double x = 0.0;
            double y = 0.0;

            // Calculate X axis (left/right)
            if (_pressedMovementKeys.Contains(_movementLeftKey))
                x -= 1.0;
            if (_pressedMovementKeys.Contains(_movementRightKey))
                x += 1.0;

            // Calculate Y axis (up/down)
            if (_pressedMovementKeys.Contains(_movementUpKey))
                y += 1.0;
            if (_pressedMovementKeys.Contains(_movementDownKey))
                y -= 1.0;

            // Normalize diagonal movement to prevent faster movement
            if (x != 0.0 && y != 0.0)
            {
                double length = System.Math.Sqrt(x * x + y * y);
                x /= length;
                y /= length;
            }

            // Apply to left stick
            _controllerService.SetAxis(ControllerAxis.LeftThumbX, x);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, y);

            System.Diagnostics.Debug.WriteLine($"[KEY MAPPER] Left stick updated: X={x:F3}, Y={y:F3}");
        }

        /// <summary>
        /// Reset all pressed keys and buttons
        /// </summary>
        public void Reset()
        {
            // Release all pressed keys
            foreach (var key in _pressedKeys)
            {
                if (_keyButtonMappings.TryGetValue(key, out var button))
                {
                    _controllerService.SetButton(button, false);
                }
                else if (_keyAxisMappings.TryGetValue(key, out var axis))
                {
                    _controllerService.SetAxis(axis, 0.0);
                }
            }
            _pressedKeys.Clear();

            // Release all pressed mouse buttons
            foreach (var mouseButton in _pressedMouseButtons)
            {
                if (_mouseButtonMappings.TryGetValue(mouseButton, out var button))
                {
                    _controllerService.SetButton(button, false);
                }
                else if (_mouseAxisMappings.TryGetValue(mouseButton, out var axis))
                {
                    _controllerService.SetAxis(axis, 0.0);
                }
            }
            _pressedMouseButtons.Clear();

            // Reset movement keys and left stick
            _pressedMovementKeys.Clear();
            _controllerService.SetAxis(ControllerAxis.LeftThumbX, 0.0);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, 0.0);

            System.Diagnostics.Debug.WriteLine("[KEY MAPPER] Reset all inputs");
        }
    }
}