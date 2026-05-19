using Input;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using System.Collections.Generic;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps keyboard keys and mouse buttons to controller buttons / trigger axes,
    /// plus drives the left thumbstick from WASD-style movement keys.
    /// </summary>
    public class KeyToControllerMapper
    {
        private readonly IControllerService _controllerService;

        private readonly Dictionary<InputKeys, ControllerButton> _keyButtonMappings = new();
        private readonly Dictionary<InputKeys, ControllerAxis> _keyAxisMappings = new();
        private readonly HashSet<InputKeys> _pressedKeys = new();

        private readonly Dictionary<InputButtons, ControllerButton> _mouseButtonMappings = new();
        private readonly Dictionary<InputButtons, ControllerAxis> _mouseAxisMappings = new();
        private readonly HashSet<InputButtons> _pressedMouseButtons = new();

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
        /// Rebuild keyboard / mouse-button mappings and movement-key set from the config.
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyButtonMappings.Clear();
            _keyAxisMappings.Clear();
            _mouseButtonMappings.Clear();
            _mouseAxisMappings.Clear();

            if (config?.KeyboardSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.KeyboardSettings.ButtonMappings)
                {
                    InputKeys inputKey = InputKeyMap.GetInputKey(mapping.KeyboardKey);
                    if (inputKey == InputKeys.None) continue;

                    AssignMapping(mapping.ControllerButton,
                        b => _keyButtonMappings[inputKey] = b,
                        a => _keyAxisMappings[inputKey] = a);
                }
            }

            if (config?.MouseSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.MouseSettings.ButtonMappings)
                {
                    InputButtons inputButton = InputKeyMap.GetInputMouseButton(mapping.MouseButton);
                    if (inputButton == InputButtons.None) continue;

                    AssignMapping(mapping.ControllerButton,
                        b => _mouseButtonMappings[inputButton] = b,
                        a => _mouseAxisMappings[inputButton] = a);
                }
            }

            if (config?.KeyboardSettings != null)
            {
                _movementUpKey = InputKeyMap.GetInputKey(config.KeyboardSettings.MovementUp ?? "W");
                _movementLeftKey = InputKeyMap.GetInputKey(config.KeyboardSettings.MovementLeft ?? "A");
                _movementDownKey = InputKeyMap.GetInputKey(config.KeyboardSettings.MovementDown ?? "S");
                _movementRightKey = InputKeyMap.GetInputKey(config.KeyboardSettings.MovementRight ?? "D");
            }
        }

        private static void AssignMapping(
            string controllerButton,
            System.Action<ControllerButton> onButton,
            System.Action<ControllerAxis> onAxis)
        {
            var action = InputKeyMap.GetControllerAction(controllerButton);
            if (!action.HasValue) return;

            switch (action.Value)
            {
                case ControllerButton.LeftTrigger:
                    onAxis(ControllerAxis.LeftTrigger);
                    break;
                case ControllerButton.RightTrigger:
                    onAxis(ControllerAxis.RightTrigger);
                    break;
                default:
                    onButton(action.Value);
                    break;
            }
        }

        /// <summary>
        /// Handle key-down from the input service.
        /// </summary>
        public void ProcessKeyDown(int keyCode)
        {
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode)) return;

            InputKeys inputKey = (InputKeys)(byte)keyCode;
            if (_pressedKeys.Contains(inputKey)) return;
            _pressedKeys.Add(inputKey);

            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
                _controllerService.SetButton(button, true);
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
                _controllerService.SetAxis(axis, 1.0);

            if (IsMovementKey(inputKey))
            {
                _pressedMovementKeys.Add(inputKey);
                UpdateLeftStickFromMovement();
            }
        }

        /// <summary>
        /// Handle key-up from the input service.
        /// </summary>
        public void ProcessKeyUp(int keyCode)
        {
            if (!System.Enum.IsDefined(typeof(InputKeys), (byte)keyCode)) return;

            InputKeys inputKey = (InputKeys)(byte)keyCode;
            _pressedKeys.Remove(inputKey);

            if (_keyButtonMappings.TryGetValue(inputKey, out var button))
                _controllerService.SetButton(button, false);
            else if (_keyAxisMappings.TryGetValue(inputKey, out var axis))
                _controllerService.SetAxis(axis, 0.0);

            if (IsMovementKey(inputKey))
            {
                _pressedMovementKeys.Remove(inputKey);
                UpdateLeftStickFromMovement();
            }
        }

        /// <summary>
        /// Handle a mouse-button transition from the capture manager.
        /// </summary>
        public void ProcessMouseButton(InputButtons button, bool isPressed)
        {
            if (button == InputButtons.None || button == InputButtons.Move) return;

            if (isPressed)
            {
                if (_pressedMouseButtons.Contains(button)) return;
                _pressedMouseButtons.Add(button);
            }
            else
            {
                if (!_pressedMouseButtons.Contains(button)) return;
                _pressedMouseButtons.Remove(button);
            }

            if (_mouseButtonMappings.TryGetValue(button, out var controllerButton))
                _controllerService.SetButton(controllerButton, isPressed);
            else if (_mouseAxisMappings.TryGetValue(button, out var controllerAxis))
                _controllerService.SetAxis(controllerAxis, isPressed ? 1.0 : 0.0);
        }

        private bool IsMovementKey(InputKeys key) =>
            key == _movementUpKey || key == _movementLeftKey ||
            key == _movementDownKey || key == _movementRightKey;

        private void UpdateLeftStickFromMovement()
        {
            double x = 0.0;
            double y = 0.0;

            if (_pressedMovementKeys.Contains(_movementLeftKey)) x -= 1.0;
            if (_pressedMovementKeys.Contains(_movementRightKey)) x += 1.0;
            if (_pressedMovementKeys.Contains(_movementUpKey)) y += 1.0;
            if (_pressedMovementKeys.Contains(_movementDownKey)) y -= 1.0;

            if (x != 0.0 && y != 0.0)
            {
                double length = System.Math.Sqrt(x * x + y * y);
                x /= length;
                y /= length;
            }

            _controllerService.SetAxis(ControllerAxis.LeftThumbX, x);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, y);
        }

        /// <summary>
        /// Release every pressed input and zero the left thumbstick.
        /// </summary>
        public void Reset()
        {
            foreach (var key in _pressedKeys)
            {
                if (_keyButtonMappings.TryGetValue(key, out var button))
                    _controllerService.SetButton(button, false);
                else if (_keyAxisMappings.TryGetValue(key, out var axis))
                    _controllerService.SetAxis(axis, 0.0);
            }
            _pressedKeys.Clear();

            foreach (var mouseButton in _pressedMouseButtons)
            {
                if (_mouseButtonMappings.TryGetValue(mouseButton, out var button))
                    _controllerService.SetButton(button, false);
                else if (_mouseAxisMappings.TryGetValue(mouseButton, out var axis))
                    _controllerService.SetAxis(axis, 0.0);
            }
            _pressedMouseButtons.Clear();

            _pressedMovementKeys.Clear();
            _controllerService.SetAxis(ControllerAxis.LeftThumbX, 0.0);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, 0.0);
        }
    }
}
