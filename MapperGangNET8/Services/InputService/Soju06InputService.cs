using System;
using System.Diagnostics;
using Input;
using Input.Platforms.Windows;
using MapperGangNET8.Models;

namespace MapperGangNET8.Services.InputService
{

    public class Soju06InputService : IInputService
    {
        private readonly IKeyboardHook _keyboardHook;
        private readonly IMouseHook _mouseHook;
        private readonly InputStateModel _inputState = new InputStateModel();
        private bool _isCapturing = false;
        private bool _isBlocking = false;
        private bool _disposed = false;
        
        private System.Collections.Generic.HashSet<int> _keysToBlock = new System.Collections.Generic.HashSet<int>();
        private System.Collections.Generic.HashSet<int> _mouseButtonsToBlock = new System.Collections.Generic.HashSet<int>();

        /// <summary>
        /// Event triggered when a key is pressed
        /// </summary>
        public event EventHandler<InputKeyEventArgs> KeyDown;

        /// <summary>
        /// Event triggered when a key is released
        /// </summary>
        public event EventHandler<InputKeyEventArgs> KeyUp;

        /// <summary>
        /// Event triggered when mouse state changes (button press, movement)
        /// </summary>
        public event EventHandler<InputMouseEventArgs> MouseStateChanged;

        /// <summary>
        /// Check if input service is currently capturing input
        /// </summary>
        public bool IsCapturing => _isCapturing;

        /// <summary>
        /// Gets whether input is being blocked
        /// </summary>
        public bool IsBlocking => _isBlocking;

        public Soju06InputService()
        {
            try
            {
                _keyboardHook = Inputs.Use<IKeyboardHook>();
                _mouseHook = Inputs.Use<IMouseHook>();

                var keyboardModel = _keyboardHook.KeyboardModel;
                var mouseModel = _mouseHook.MouseModel;

                keyboardModel.KeyDown += KeyboardKeyDown;
                keyboardModel.KeyUp += KeyboardKeyUp;
                mouseModel.State += MouseState;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing input hooks: {ex.Message}");
                throw;
            }
        }

        public void Start()
        {
            if (_isCapturing)
                return;

            try
            {
                _keyboardHook.HookStart();
                _mouseHook.HookStart();
                _isCapturing = true;

                if (Platform.IsWindows)
                {
                    StartWindowsMessagePump();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error starting input hooks: {ex.Message}");
                throw;
            }
        }

        public void Stop()
        {
            if (!_isCapturing)
                return;

            try
            {
                _keyboardHook.HookStop();
                _mouseHook.HookStop();
                _isCapturing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping input hooks: {ex.Message}");
                throw;
            }
        }

        public void SetInputBlocking(bool block)
        {
            _isBlocking = block;
        }

        public void EnableDebug(bool enable)
        {
            _keyboardHook.Debug = enable;
            _mouseHook.Debug = enable;
        }

        public void SetKeysToBlock(System.Collections.Generic.HashSet<int> keysToBlock, System.Collections.Generic.HashSet<int> mouseButtonsToBlock)
        {
            _keysToBlock = keysToBlock ?? new System.Collections.Generic.HashSet<int>();
            _mouseButtonsToBlock = mouseButtonsToBlock ?? new System.Collections.Generic.HashSet<int>();
            
            System.Diagnostics.Debug.WriteLine($"Soju06InputService: SetKeysToBlock - Keys to block: {string.Join(", ", _keysToBlock)}, Mouse buttons to block: {string.Join(", ", _mouseButtonsToBlock)}");
        }

        private bool KeyboardKeyDown(object sender, InputKeys key, InputKeyState state)
        {
            if (IsPanicKeyCombinationPressed((int)key))
            {
                SetInputBlocking(false);
                Stop();

                Debug.WriteLine("EMERGENCY OVERRIDE ACTIVATED - Input blocking disabled");

                return true;
            }

            _inputState.SetKeyState((int)key, true);

            var args = new InputKeyEventArgs(
                (int)key,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyDown?.Invoke(this, args);

            // Only block keys that are explicitly mapped or WASD
            if (_isBlocking && _keysToBlock.Contains((int)key))
            {
                return false; // Block this key
            }
            return true; // Allow pass-through
        }

        private bool IsPanicKeyCombinationPressed(int key)
        {
            //ctrl+alt+shift+esc
            return (key == (int)InputKeys.Escape &&
                    _inputState.IsKeyPressed((int)InputKeys.LeftControl) &&
                    _inputState.IsKeyPressed((int)InputKeys.LeftAlt) &&
                    _inputState.IsKeyPressed((int)InputKeys.LeftShift));
        }

        private bool KeyboardKeyUp(object sender, InputKeys key, InputKeyState state)
        {
            _inputState.SetKeyState((int)key, false);

            var args = new InputKeyEventArgs(
                (int)key,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyUp?.Invoke(this, args);

            // Only block keys that are explicitly mapped or WASD
            if (_isBlocking && _keysToBlock.Contains((int)key))
            {
                return false; // Block this key
            }
            return true; // Allow pass-through
        }

        private bool MouseState(object sender, InputButtons button, int x, int y)
        {
            _inputState.UpdateMousePosition(x, y);

            if (button != InputButtons.None)
            {
                bool isPressed = button > 0;
                int absButton = Math.Abs((int)button);
                _inputState.SetMouseButtonState(absButton, isPressed);
            }

            var args = new InputMouseEventArgs(
                (int)button,
                x,
                y,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            MouseStateChanged?.Invoke(this, args);

            // Always allow mouse movement (for right stick camera control)
            if (button == InputButtons.None) return true;
            
            // Only block mouse buttons that are explicitly mapped
            if (_isBlocking && _mouseButtonsToBlock.Contains(Math.Abs((int)button)))
            {
                return false; // Block this mouse button
            }
            return true; // Allow pass-through
        }

        private void StartWindowsMessagePump()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while (WindowsMessagePump.Pumping() && _isCapturing && !_disposed)
                {
                    System.Threading.Thread.Sleep(10);
                }
            });
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();

                if (_keyboardHook != null)
                {
                    _keyboardHook.KeyboardModel.KeyDown -= KeyboardKeyDown;
                    _keyboardHook.KeyboardModel.KeyUp -= KeyboardKeyUp;
                    _keyboardHook.Dispose();
                }

                if (_mouseHook != null)
                {
                    _mouseHook.MouseModel.State -= MouseState;
                    _mouseHook.Dispose();
                }
            }

            _disposed = true;
        }

        ~Soju06InputService()
        {
            Dispose(false);
        }
    }
}