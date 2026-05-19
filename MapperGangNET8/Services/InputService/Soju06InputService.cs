using Input;
using Input.Platforms.Windows;
using MapperGangNET8.Models;
using MapperGangNET8.Services.InputCaptureService;
using System;
using System.Diagnostics;

namespace MapperGangNET8.Services.InputService
{
    /// <summary>
    /// Keyboard hook implementation backed by the Soju06 Input library.
    /// Forwards <see cref="KeyDown"/>/<see cref="KeyUp"/> events and consults the
    /// shared <see cref="InputCaptureManager"/> to decide whether to swallow the
    /// system event (i.e. block the game/OS from seeing the key).
    /// </summary>
    public class Soju06InputService : IInputService
    {
        private readonly IKeyboardHook _keyboardHook;
        private readonly InputStateModel _inputState = new InputStateModel();
        private bool _isCapturing = false;
        private bool _disposed = false;
        private InputCaptureManager _captureManager;

        /// <inheritdoc/>
        public event EventHandler<InputKeyEventArgs> KeyDown;

        /// <inheritdoc/>
        public event EventHandler<InputKeyEventArgs> KeyUp;

        /// <inheritdoc/>
        public event EventHandler<InputMouseEventArgs> MouseStateChanged;

        /// <inheritdoc/>
        public bool IsCapturing => _isCapturing;

        public Soju06InputService()
        {
            try
            {
                _keyboardHook = Inputs.Use<IKeyboardHook>();
                var keyboardModel = _keyboardHook.KeyboardModel;
                keyboardModel.KeyDown += KeyboardKeyDown;
                keyboardModel.KeyUp += KeyboardKeyUp;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] Error initializing hooks: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Wire up the shared capture manager so the hook can ask whether to block a key.
        /// </summary>
        public void SetInputBlockingManager(InputCaptureManager captureManager)
        {
            _captureManager = captureManager;
        }

        /// <inheritdoc/>
        public void Start()
        {
            if (_isCapturing) return;

            try
            {
                _keyboardHook.HookStart();
                _isCapturing = true;

                if (Platform.IsWindows)
                    StartWindowsMessagePump();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] Error starting hooks: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            if (!_isCapturing) return;

            try
            {
                _keyboardHook.HookStop();
                _isCapturing = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[SOJU06] Error stopping hooks: {ex.Message}");
                throw;
            }
        }

        /// <inheritdoc/>
        public void EnableDebug(bool enable) => _keyboardHook.Debug = enable;

        private bool KeyboardKeyDown(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;

            if (IsPanicKeyCombinationPressed(key))
            {
                Stop();
                return true;
            }

            _inputState.SetKeyState(soju06Code, true);

            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds());
            KeyDown?.Invoke(this, args);

            return _captureManager?.ShouldBlockKey(soju06Code) != true;
        }

        private bool KeyboardKeyUp(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;
            _inputState.SetKeyState(soju06Code, false);

            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds());
            KeyUp?.Invoke(this, args);

            return _captureManager?.ShouldBlockKey(soju06Code) != true;
        }

        private bool IsPanicKeyCombinationPressed(InputKeys key) => key == InputKeys.F9;

        private void StartWindowsMessagePump()
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                while (WindowsMessagePump.Pumping())
                    System.Threading.Thread.Sleep(10);
            });
        }

        public void Dispose()
        {
            if (_disposed) return;

            Stop();

            if (_keyboardHook != null)
            {
                _keyboardHook.KeyboardModel.KeyDown -= KeyboardKeyDown;
                _keyboardHook.KeyboardModel.KeyUp -= KeyboardKeyUp;
                _keyboardHook.Dispose();
            }
            _disposed = true;
        }
    }
}
