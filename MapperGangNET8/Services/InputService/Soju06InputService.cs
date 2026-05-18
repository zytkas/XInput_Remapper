using Input;
using Input.Platforms.Windows;
using MapperGangNET8.Models;
using MapperGangNET8.Services.InputCaptureService;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MapperGangNET8.Services.InputService
{
    public class Soju06InputService : IInputService
    {
        private readonly IKeyboardHook _keyboardHook;
        private readonly InputStateModel _inputState = new InputStateModel();
        private bool _isCapturing = false;
        private bool _disposed = false;
        private InputCaptureManager _captureManager;
        private bool _mouseCenteringEnabled = false;

        /// <summary>
        /// Event triggered when a key is pressed
        /// </summary>
        public event EventHandler<InputKeyEventArgs> KeyDown;

        /// <summary>
        /// Event triggered when a key is released
        /// </summary>
        public event EventHandler<InputKeyEventArgs> KeyUp;

        /// <summary>
        /// Event triggered when mouse state changes (not used with Raw Input)
        /// </summary>
        public event EventHandler<InputMouseEventArgs> MouseStateChanged;

        /// <summary>
        /// Check if input service is currently capturing
        /// </summary>
        public bool IsCapturing => _isCapturing;

        public Soju06InputService()
        {
            try
            {
                // Initialize keyboard and mouse hooks
                _keyboardHook = Inputs.Use<IKeyboardHook>();
                var keyboardModel = _keyboardHook.KeyboardModel;
                keyboardModel.KeyDown += KeyboardKeyDown;
                keyboardModel.KeyUp += KeyboardKeyUp;

                Debug.WriteLine("[SOJU06] Initialized keyboard hook simulation");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] ❌ Error initializing hooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Set input capture manager for key blocking
        /// </summary>
        public void SetInputBlockingManager(InputCaptureManager captureManager)
        {
            _captureManager = captureManager;
            Debug.WriteLine("[SOJU06] Capture manager set");
        }

        /// <summary>
        /// Start capturing keyboard and mouse input
        /// </summary>
        public void Start()
        {
            if (_isCapturing) return;

            try
            {
                _keyboardHook.HookStart();
                _isCapturing = true;

                if (Platform.IsWindows)
                {
                    StartWindowsMessagePump();
                }

                Debug.WriteLine("[SOJU06] ✅ Started keyboard and mouse capture");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] ❌ Error starting hooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Stop capturing keyboard and mouse input
        /// </summary>
        public void Stop()
        {
            if (!_isCapturing) return;

            try
            {
                _keyboardHook.HookStop();
                _isCapturing = false;
                Debug.WriteLine("[SOJU06] ✅ Stopped keyboard and mouse capture");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] ❌ Error stopping hooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Enable debug output
        /// </summary>
        public void EnableDebug(bool enable)
        {
            _keyboardHook.Debug = enable;
        }

        /// <summary>
        /// Handle keyboard key down
        /// </summary>
        private bool KeyboardKeyDown(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;

            // Emergency stop on F9
            if (IsPanicKeyCombinationPressed(key))
            {
                Stop();
                Debug.WriteLine("[SOJU06] 🛑 EMERGENCY STOP - F9 pressed");
                return true;
            }
            _inputState.SetKeyState(soju06Code, true);

            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyDown?.Invoke(this, args);

            if (_captureManager?.ShouldBlockKey(soju06Code) == true)
            {
                Debug.WriteLine($"[SOJU06] Blocking key {soju06Code} from game");
                return false;
            }

            return true; 
        }

        private bool KeyboardKeyUp(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;

            // Update state
            _inputState.SetKeyState(soju06Code, false);

            // Fire event
            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyUp?.Invoke(this, args);

            if (_captureManager?.ShouldBlockKey(soju06Code) == true)
            {
                Debug.WriteLine($"[SOJU06] Blocking key up {soju06Code} from game");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check for panic key combination
        /// </summary>
        private bool IsPanicKeyCombinationPressed(InputKeys key)
        {
            // F9 for emergency stop
            return key == InputKeys.F9;
        }

        /// <summary>
        /// Start Windows message pump for hooks
        /// </summary>
        private void StartWindowsMessagePump()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while (WindowsMessagePump.Pumping())
                {
                    System.Threading.Thread.Sleep(10);
                }

            });

        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            Stop();

            // Cleanup hooks
            if (_keyboardHook != null)
            {
                _keyboardHook.KeyboardModel.KeyDown -= KeyboardKeyDown;
                _keyboardHook.KeyboardModel.KeyUp -= KeyboardKeyUp;
                _keyboardHook.Dispose();
            }
            _disposed = true;

            Debug.WriteLine("[SOJU06] Disposed");
        }
    }
}