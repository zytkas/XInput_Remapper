using Input;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputCaptureService;
using MapperGangNET8.Services.InputService;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Main pipeline for processing input and sending to controller.
    /// Owns the high-frequency tick that drives mouse-to-stick physics and flushes ViGEm reports.
    /// </summary>
    public class InputPipeline : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

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

        private POINT _centerPoint;
        private int _screenWidth;
        private int _screenHeight;
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        private readonly IInputService _inputService;
        private readonly IControllerService _controllerService;
        private readonly IConfigService _configService;
        private readonly KeyToControllerMapper _keyMapper;
        private readonly MouseToStickMapper _mouseMapper;
        private readonly InputCaptureManager _captureManager;

        private bool _isControllerConnected = false;
        private bool _isEnabled = false;
        private bool _disposed = false;

        private readonly System.Timers.Timer _updateTimer;

        /// <summary>
        /// Tick period in ms. 2 ms ≈ 500 Hz — drives stick physics and controller report flush.
        /// </summary>
        private const int UPDATE_INTERVAL_MS = 2;

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

            if (_inputService is Soju06InputService soju06Service)
            {
                soju06Service.SetInputBlockingManager(_captureManager);
            }

            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _captureManager.MouseButtonAction += OnMouseButtonFromDriver;
            _captureManager.MouseDeltaAction += OnMouseDeltaAction;

            _updateTimer = new System.Timers.Timer(UPDATE_INTERVAL_MS);
            _updateTimer.Elapsed += OnUpdateTimerElapsed;
            _updateTimer.AutoReset = true;
        }

        /// <summary>
        /// Enable or disable the input pipeline.
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (_isEnabled == enabled) return;

            _isEnabled = enabled;

            if (enabled)
            {
                _screenWidth = GetSystemMetrics(SM_CXSCREEN);
                _screenHeight = GetSystemMetrics(SM_CYSCREEN);
                _centerPoint = new POINT { X = _screenWidth / 2, Y = _screenHeight / 2 };

                RECT clipRect = new RECT
                {
                    Left = _centerPoint.X,
                    Top = _centerPoint.Y,
                    Right = _centerPoint.X + 1,
                    Bottom = _centerPoint.Y + 1
                };
                ClipCursor(ref clipRect);
                SetCursorPos(_centerPoint.X, _centerPoint.Y);

                _captureManager.EnableMouInput(true);
                _inputService.Start();
                _updateTimer.Start();
            }
            else
            {
                ClipCursor(IntPtr.Zero);
                _updateTimer.Stop();
                _inputService.Stop();
                _captureManager.EnableMouInput(false);
                _keyMapper.Reset();
                _mouseMapper.Reset();
                _controllerService.Submit();
            }
        }

        /// <summary>
        /// Update mapper configurations from the config model.
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            _keyMapper.UpdateConfiguration(config);

            if (config?.MouseSettings != null)
            {
                var mouseSettings = config.MouseSettings;

                float sensX = (float)mouseSettings.MouseSensitivityX;
                float sensY = (float)mouseSettings.MouseSensitivityY;
                _mouseMapper.SetSensitivityPercent(sensX, sensY);

                float scaleX = (float)(mouseSettings.ScaleFactorX * 100.0);
                float scaleY = (float)(mouseSettings.ScaleFactorY * 100.0);
                _mouseMapper.SetScaleFactor(scaleX, scaleY);

                _mouseMapper.SetSmoothing((uint)mouseSettings.MouseSmoothing);
                _mouseMapper.SetNoiseFilter((uint)mouseSettings.NoiseFilter);
                _mouseMapper.SetSpringMode(mouseSettings.MouseJoystickMode == "Spring Mode");
                _mouseMapper.SetReturnTime((byte)mouseSettings.ReturnTime);
                _mouseMapper.SetInversion(mouseSettings.InvertXAxis, mouseSettings.InvertYAxis);

                switch (mouseSettings.ResponseCurveType)
                {
                    case "Precision":
                        _mouseMapper.SetPrecisionCurve();
                        break;
                    case "Aggressive":
                        _mouseMapper.SetAggressiveCurve();
                        break;
                    case "Linear":
                    default:
                        _mouseMapper.SetLinearCurve();
                        break;
                }
            }

            var blockedKeys = new List<KeyBindingModel>();

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

            _captureManager.UpdateBlockedKeys(blockedKeys);

            if (config?.MouseSettings?.ButtonMappings != null)
            {
                _captureManager.UpdateBlockedMouseButtons(config.MouseSettings.ButtonMappings);
            }
        }

        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;
            _keyMapper.ProcessKeyDown(e.KeyCode);
        }

        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            if (!_isEnabled) return;
            _keyMapper.ProcessKeyUp(e.KeyCode);
        }

        private void OnMouseButtonFromDriver(InputButtons button, bool isPressed)
        {
            if (!_isEnabled) return;
            _keyMapper.ProcessMouseButton(button, isPressed);
        }

        private void OnMouseDeltaAction(int deltaX, int deltaY)
        {
            if (!_isEnabled) return;
            _mouseMapper.ProcessMouseDelta(deltaX, deltaY);
        }

        private void OnUpdateTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!_isEnabled) return;
            _mouseMapper.Update();
            _controllerService.Submit();
        }

        /// <summary>
        /// Connect controller once per session.
        /// </summary>
        public async Task<bool> ConnectControllerAsync()
        {
            if (_isControllerConnected) return true;

            try
            {
                await _configService.LoadConfigAsync();
                bool success = await _controllerService.ConnectAsync(ControllerType.Xbox360, 1);
                _isControllerConnected = success;
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PIPELINE] Error connecting controller: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnect controller.
        /// </summary>
        public async Task DisconnectControllerAsync()
        {
            if (!_isControllerConnected) return;

            SetEnabled(false);
            await _controllerService.DisconnectAsync();
            _isControllerConnected = false;
        }

        /// <summary>
        /// Enable/disable mapping (controller stays connected across toggles).
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
        /// Refresh configuration from config service.
        /// </summary>
        public async Task RefreshConfigurationAsync()
        {
            var config = await _configService.LoadConfigAsync();
            UpdateConfiguration(config);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _updateTimer?.Stop();
            _updateTimer?.Dispose();

            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _captureManager.MouseButtonAction -= OnMouseButtonFromDriver;
            _captureManager.MouseDeltaAction -= OnMouseDeltaAction;

            if (_isEnabled) SetEnabled(false);

            _captureManager?.Dispose();
            _disposed = true;
        }
    }
}
