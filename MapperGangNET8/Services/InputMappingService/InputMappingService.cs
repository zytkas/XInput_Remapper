using System.Diagnostics;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputService;

namespace MapperGangNET8.Services.InputMappingService
{
    /// <summary>
    /// Service for mapping keyboard and mouse input to virtual controller
    /// </summary>
    public class InputMappingService : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly IControllerService _controllerService;
        private readonly IConfigService _configService;
        private readonly InputProcessorService _inputProcessor;

        private ConfigModel _currentConfig;
        private bool _isEnabled;
        private bool _disposed;

        // For tracking input state
        private readonly InputStateModel _inputState = new InputStateModel();
        private readonly ControllerState _controllerState = new ControllerState();

        // For performance measurement
        private readonly Stopwatch _frameTimer = new Stopwatch();
        private long _lastFrameTime;

        // For key bindings
        private Dictionary<int, ControllerButton> _keyButtonMappings = new Dictionary<int, ControllerButton>();
        private Dictionary<int, ControllerButton> _mouseButtonMappings = new Dictionary<int, ControllerButton>();

        /// <summary>
        /// Constructor
        /// </summary>
        public InputMappingService(
            IInputService inputService,
            IControllerService controllerService,
            IConfigService configService,
            InputProcessorService inputProcessor)
        {
            _inputService = inputService;
            _controllerService = controllerService;
            _configService = configService;
            _inputProcessor = inputProcessor;

            // Subscribe to input events
            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;

            // Load config asynchronously
            _ = LoadConfigAsync();

            // Start performance timer
            _frameTimer.Start();
            _lastFrameTime = _frameTimer.ElapsedMilliseconds;
        }

        /// <summary>
        /// Enable or disable mapping
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (enabled)
            {
                _inputService.Start();
                ConnectControllerAsync();
            }
            else
            {
                _inputService.Stop();
                DisconnectControllerAsync();
            }
        }

        /// <summary>
        /// Load configuration settings
        /// </summary>
        private async System.Threading.Tasks.Task LoadConfigAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();
            UpdateMappingsFromConfig();
        }

        /// <summary>
        /// Update input mappings from configuration
        /// </summary>
        private void UpdateMappingsFromConfig()
        {
            if (_currentConfig == null) return;

            // Load key bindings for keyboard
            _keyButtonMappings.Clear();

            foreach (var mapping in _currentConfig.KeyboardSettings.ButtonMappings)
            {
                // Convert key string to key code (simplified for example)
                int keyCode = GetKeyCodeFromString(mapping.KeyboardKey);

                // Convert controller button string to enum
                if (Enum.TryParse(RemoveSpaces(mapping.ControllerButton), out ControllerButton button))
                {
                    _keyButtonMappings[keyCode] = button;
                }
            }

            // Load key bindings for mouse
            _mouseButtonMappings.Clear();

            foreach (var mapping in _currentConfig.MouseSettings.ButtonMappings)
            {
                // Convert mouse button string to button code (simplified for example)
                int buttonCode = GetMouseButtonCodeFromString(mapping.MouseButton);

                // Convert controller button string to enum
                if (Enum.TryParse(RemoveSpaces(mapping.ControllerButton), out ControllerButton button))
                {
                    _mouseButtonMappings[buttonCode] = button;
                }
            }
        }

        /// <summary>
        /// Connect the virtual controller
        /// </summary>
        private async System.Threading.Tasks.Task ConnectControllerAsync()
        {
            try
            {
                // Get controller type from settings
                ControllerType controllerType = _currentConfig?.ControllerSettings?.SelectedControllerType == "DualShock 4 Controller"
                    ? ControllerType.DualShock4
                    : ControllerType.Xbox360;

                // Get controller number from settings
                int controllerNumber = 1;
                if (_currentConfig?.ControllerSettings?.ControllerNumber != null)
                {
                    string numberStr = _currentConfig.ControllerSettings.ControllerNumber.Replace("Controller ", "");
                    if (int.TryParse(numberStr, out int number))
                    {
                        controllerNumber = number;
                    }
                }

                // Connect controller
                await _controllerService.ConnectAsync(controllerType, controllerNumber);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error connecting controller: {ex.Message}");
            }
        }

        /// <summary>
        /// Disconnect the virtual controller
        /// </summary>
        private async System.Threading.Tasks.Task DisconnectControllerAsync()
        {
            try
            {
                await _controllerService.DisconnectAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error disconnecting controller: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle key down event
        /// </summary>
        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            _inputState.SetKeyState(e.KeyCode, true);

            // Check if this key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(e.KeyCode, out ControllerButton button))
            {
                _controllerState.SetButton(button, true);
                _controllerService.SetButton(button, true);
            }

            // Alternative way to handle directional input
            HandleDirectionalKeys();
        }

        /// <summary>
        /// Handle key up event
        /// </summary>
        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;

            _inputState.SetKeyState(e.KeyCode, false);

            // Check if this key is mapped to a controller button
            if (_keyButtonMappings.TryGetValue(e.KeyCode, out ControllerButton button))
            {
                _controllerState.SetButton(button, false);
                _controllerService.SetButton(button, false);
            }

            // Alternative way to handle directional input
            HandleDirectionalKeys();
        }

        /// <summary>
        /// Handle mouse state changed event
        /// </summary>
        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            if (!_isEnabled) return;

            // Calculate time since last frame for delta calculation
            long currentTime = _frameTimer.ElapsedMilliseconds;
            double dt = (currentTime - _lastFrameTime) / 1000.0; // Convert to seconds
            _lastFrameTime = currentTime;

            // Update mouse position
            int prevX = _inputState.MouseX;
            int prevY = _inputState.MouseY;

            _inputState.UpdateMousePosition(e.X, e.Y);

            // Calculate delta
            int deltaX = _inputState.MouseX - prevX;
            int deltaY = _inputState.MouseY - prevY;

            // Process mouse movement through sensitivity curves
            var processedDelta = _inputProcessor.ProcessMouseInput(deltaX, deltaY, dt);

            // Map to right stick of controller
            _controllerState.SetAxis(ControllerAxis.RightThumbX, processedDelta.X);
            _controllerState.SetAxis(ControllerAxis.RightThumbY, -processedDelta.Y); // Invert Y-axis

            _controllerService.SetAxis(ControllerAxis.RightThumbX, processedDelta.X);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, -processedDelta.Y);

            // Handle mouse buttons
            if (e.Button != 0)
            {
                bool isPressed = e.Button > 0;
                int absButton = Math.Abs(e.Button);

                _inputState.SetMouseButtonState(absButton, isPressed);

                // Check if this mouse button is mapped to a controller button
                if (_mouseButtonMappings.TryGetValue(absButton, out ControllerButton button))
                {
                    _controllerState.SetButton(button, isPressed);
                    _controllerService.SetButton(button, isPressed);
                }
            }
        }

        /// <summary>
        /// Handle directional keys for analog stick
        /// </summary>
        private void HandleDirectionalKeys()
        {
            // Example mapping for WASD keys
            bool wPressed = _inputState.IsKeyPressed(GetKeyCodeFromString("W"));
            bool aPressed = _inputState.IsKeyPressed(GetKeyCodeFromString("A"));
            bool sPressed = _inputState.IsKeyPressed(GetKeyCodeFromString("S"));
            bool dPressed = _inputState.IsKeyPressed(GetKeyCodeFromString("D"));

            // Calculate analog stick input
            double x = (dPressed ? 1.0 : 0.0) - (aPressed ? 1.0 : 0.0);
            double y = (wPressed ? 1.0 : 0.0) - (sPressed ? 1.0 : 0.0);

            // Normalize diagonal movement
            if (x != 0 && y != 0)
            {
                double length = Math.Sqrt(x * x + y * y);
                x /= length;
                y /= length;
            }

            // Process joystick input through deadzones and sensitivity
            var processed = _inputProcessor.ProcessJoystickInput(x, y);

            // Apply to left stick
            _controllerState.SetAxis(ControllerAxis.LeftThumbX, processed.X);
            _controllerState.SetAxis(ControllerAxis.LeftThumbY, processed.Y);

            _controllerService.SetAxis(ControllerAxis.LeftThumbX, processed.X);
            _controllerService.SetAxis(ControllerAxis.LeftThumbY, processed.Y);
        }

        /// <summary>
        /// Convert key string to key code (simplified for example)
        /// </summary>
        private int GetKeyCodeFromString(string keyString)
        {
            // This is a simplified version - in a real application, you'd map strings to actual key codes
            // For example, using Windows virtual key codes

            switch (keyString)
            {
                case "A": return 65;
                case "B": return 66;
                case "C": return 67;
                case "D": return 68;
                case "E": return 69;
                case "F": return 70;
                case "G": return 71;
                case "H": return 72;
                case "I": return 73;
                case "J": return 74;
                case "K": return 75;
                case "L": return 76;
                case "M": return 77;
                case "N": return 78;
                case "O": return 79;
                case "P": return 80;
                case "Q": return 81;
                case "R": return 82;
                case "S": return 83;
                case "T": return 84;
                case "U": return 85;
                case "V": return 86;
                case "W": return 87;
                case "X": return 88;
                case "Y": return 89;
                case "Z": return 90;
                case "Space": return 32;
                case "Left Ctrl": return 17;
                case "Left Shift": return 16;
                case "Left Alt": return 18;
                default: return 0;
            }
        }

        /// <summary>
        /// Convert mouse button string to button code (simplified for example)
        /// </summary>
        private int GetMouseButtonCodeFromString(string buttonString)
        {
            // This is a simplified version - in a real application, you'd map strings to actual button codes

            switch (buttonString)
            {
                case "Left Button": return 1;
                case "Right Button": return 2;
                case "Middle Button": return 3;
                case "Side Button 1": return 4;
                case "Side Button 2": return 5;
                default: return 0;
            }
        }

        /// <summary>
        /// Remove spaces from controller button string for enum parsing
        /// </summary>
        private string RemoveSpaces(string input)
        {
            return input.Replace(" ", "");
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                // Unsubscribe from events
                _inputService.KeyDown -= OnKeyDown;
                _inputService.KeyUp -= OnKeyUp;
                _inputService.MouseStateChanged -= OnMouseStateChanged;

                // Stop services
                _inputService.Stop();
                DisconnectControllerAsync().Wait();
            }

            _disposed = true;
        }
    }
}