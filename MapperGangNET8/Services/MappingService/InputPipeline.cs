using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputCaptureService;
using MapperGangNET8.Services.InputService;
// В начало файла добавьте импорты:
using System.Runtime.InteropServices;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Main pipeline for processing input and sending to controller
    /// </summary>
    public class InputPipeline : IDisposable
    {
        // Добавьте эти Win32 API декларации в класс InputPipeline:
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("user32.dll")]
        private static extern bool ClipCursor(IntPtr lpRect);


        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // Добавьте эти поля в класс:
        private bool _mouseCenteringEnabled = false;
        private POINT _centerPoint;
        private int _screenWidth;
        private int _screenHeight;
        private const int SM_CXSCREEN = 0;  // Ширина экрана
        private const int SM_CYSCREEN = 1;  // Высота экрана

        private readonly IInputService _inputService;
        private readonly IControllerService _controllerService;
        private readonly IConfigService _configService;
        private readonly KeyToControllerMapper _keyMapper;
        private readonly MouseToStickMapper _mouseMapper;
        private readonly InputCaptureManager _captureManager;

        private bool _isControllerConnected = false;
        private bool _isEnabled = false;
        private bool _disposed = false;

        // Timer for stick decay (60 FPS)
        private readonly System.Timers.Timer _stickDecayTimer;

        public InputPipeline(
            IInputService inputService,
            IControllerService controllerService,
            IConfigService configService,
            KeyToControllerMapper keyMapper,
            MouseToStickMapper mouseMapper,
            InputCaptureManager captureManager)
        {
            _inputService = inputService;
            _controllerService = controllerService;
            _configService = configService;
            _keyMapper = keyMapper;
            _mouseMapper = mouseMapper;
            _captureManager = captureManager;

            // Setup input service with capture manager for key blocking
            if (_inputService is Soju06InputService soju06Service)
            {
                soju06Service.SetInputBlockingManager(_captureManager);
            }

            // Subscribe to keyboard events
            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;

            // Subscribe to mouse events
            _inputService.MouseStateChanged += OnMouseStateChanged;

            // Subscribe to raw mouse delta events
            _captureManager.MouseDeltaCaptured += OnMouseDeltaCaptured;

            // Setup stick decay timer (60 FPS)
            _stickDecayTimer = new System.Timers.Timer(16); // ~60 FPS
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
                System.Diagnostics.Debug.WriteLine("[PIPELINE] Enabling input pipeline...");
                _screenWidth = GetSystemMetrics(SM_CXSCREEN);
                _screenHeight = GetSystemMetrics(SM_CYSCREEN);
                _centerPoint = new POINT
                {
                    X = _screenWidth / 2,
                    Y = _screenHeight / 2
                };

                RECT clipRect = new RECT            
                {
                    Left = _centerPoint.X,
                    Top = _centerPoint.Y,
                    Right = _centerPoint.X + 1,
                    Bottom = _centerPoint.Y + 1
                };
                ClipCursor(ref clipRect);
                _mouseCenteringEnabled = true;
                SetCursorPos(_centerPoint.X, _centerPoint.Y);
                _captureManager.EnableMouInput(true);
                _inputService.Start();
                _stickDecayTimer.Start();

                System.Diagnostics.Debug.WriteLine($"[PIPELINE] ✅ Input pipeline enabled. Mouse centering at ({_centerPoint.X}, {_centerPoint.Y})");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[PIPELINE] Disabling input pipeline...");
                _mouseCenteringEnabled = false;
                ClipCursor(IntPtr.Zero);
                _stickDecayTimer.Stop();
                _inputService.Stop();
                _captureManager.EnableMouInput(false);

                System.Diagnostics.Debug.WriteLine("[PIPELINE] ✅ Input pipeline disabled");
            }
        }

        /// <summary>
        /// Update mapper configurations
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            // Update mappers
            _keyMapper.UpdateConfiguration(config);
            _mouseMapper.UpdateConfiguration(config);

            // Update blocked keys for keyboard
            var blockedKeys = new List<KeyBindingModel>();

            // Add keyboard button mappings
            if (config?.KeyboardSettings?.ButtonMappings != null)
            {
                foreach (var mapping in config.KeyboardSettings.ButtonMappings)
                {
                    if (!string.IsNullOrEmpty(mapping.KeyboardKey))
                    {
                        var keyCode = InputKeyMap.GetKeyCode(mapping.KeyboardKey);
                        if (keyCode != 0)
                        {
                            blockedKeys.Add(new KeyBindingModel
                            {
                                InputType = InputDeviceType.Keyboard,
                                InputCode = keyCode
                            });
                        }
                    }
                }
            }

            // Add WASD movement keys
            if (config?.KeyboardSettings != null)
            {
                var movementKeys = new[]
                {
                    config.KeyboardSettings.MovementUp ?? "W",
                    config.KeyboardSettings.MovementLeft ?? "A",
                    config.KeyboardSettings.MovementDown ?? "S",
                    config.KeyboardSettings.MovementRight ?? "D"
                };

                foreach (var key in movementKeys)
                {
                    var keyCode = InputKeyMap.GetKeyCode(key);
                    if (keyCode != 0)
                    {
                        blockedKeys.Add(new KeyBindingModel
                        {
                            InputType = InputDeviceType.Keyboard,
                            InputCode = keyCode
                        });
                    }
                }
            }

            // Update capture manager with blocked keys
            _captureManager.UpdateBlockedKeys(blockedKeys);

            // Update blocked mouse buttons
            if (config?.MouseSettings?.ButtonMappings != null)
            {
                _captureManager.UpdateBlockedMouseButtons(config.MouseSettings.ButtonMappings);
            }

            System.Diagnostics.Debug.WriteLine("[PIPELINE] Configuration updated");
        }

        /// <summary>
        /// Handle key down events
        /// </summary>
        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;
            _keyMapper.ProcessKeyDown(e.KeyCode);
        }

        /// <summary>
        /// Handle key up events
        /// </summary>
        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;
            _keyMapper.ProcessKeyUp(e.KeyCode);
        }

        /// <summary>
        /// Handle mouse state changes (button events)
        /// </summary>
        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            if (!_isEnabled) return;

            if (e.Button != 0)
            {
                _keyMapper.ProcessMouseButton(e.Button);
            }
        }

        /// <summary>
        /// Handle raw mouse delta events
        /// </summary>
        private void OnMouseDeltaCaptured(object sender, MouseDeltaEventArgs e)
        {
            if (!_isEnabled) return;
            // Process raw mouse deltas
            _mouseMapper.ProcessMouseDelta(e.DeltaX, e.DeltaY);
            if (_mouseCenteringEnabled)
            {
                // Возвращаем курсор в центр экрана
                SetCursorPos(_centerPoint.X, _centerPoint.Y);
            }

        }

        /// <summary>
        /// Handle stick decay timer
        /// </summary>
        private void OnStickDecayTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isEnabled) return;
            _mouseMapper.UpdateStickDecay();
        }

        /// <summary>
        /// Connect controller once per session
        /// </summary>
        public async Task<bool> ConnectControllerAsync()
        {
            if (_isControllerConnected) return true;

            try
            {
                var config = await _configService.LoadConfigAsync();

                ControllerType controllerType = config?.AppSettings?.SelectedControllerType == "DualShock 4 Controller"
                    ? ControllerType.DualShock4
                    : ControllerType.Xbox360;

                bool success = await _controllerService.ConnectAsync(controllerType, 1);
                _isControllerConnected = success;

                System.Diagnostics.Debug.WriteLine($"[PIPELINE] Controller connected: {success}");
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PIPELINE] ❌ Error connecting controller: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnect controller
        /// </summary>
        public async Task DisconnectControllerAsync()
        {
            if (!_isControllerConnected) return;

            SetEnabled(false);
            await _controllerService.DisconnectAsync();
            _isControllerConnected = false;

            System.Diagnostics.Debug.WriteLine("[PIPELINE] Controller disconnected");
        }

        /// <summary>
        /// Enable/disable mapping (controller stays connected)
        /// </summary>
        public async Task SetMappingEnabledAsync(bool enabled)
        {
            if (enabled && !_isControllerConnected)
            {
                var connected = await ConnectControllerAsync();
                if (!connected) return;
            }

            SetEnabled(enabled);
        }

        /// <summary>
        /// Refresh configuration from config service
        /// </summary>
        public async Task RefreshConfigurationAsync()
        {
            var config = await _configService.LoadConfigAsync();
            UpdateConfiguration(config);
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            // Stop timer
            _stickDecayTimer?.Stop();
            _stickDecayTimer?.Dispose();

            // Unsubscribe from events
            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _inputService.MouseStateChanged -= OnMouseStateChanged;
            _captureManager.MouseDeltaCaptured -= OnMouseDeltaCaptured;

            // Stop services
            if (_isEnabled)
            {
                SetEnabled(false);
            }

            // Dispose capture manager
            _captureManager?.Dispose();

            _disposed = true;

            System.Diagnostics.Debug.WriteLine("[PIPELINE] Disposed");
        }
    }
}