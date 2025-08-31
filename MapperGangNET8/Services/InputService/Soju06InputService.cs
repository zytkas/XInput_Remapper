using System;
using System.Diagnostics;
using Input;
using Input.Platforms.Windows;
using MapperGangNET8.Models;
using MapperGangNET8.Services.InputBlockingService;

namespace MapperGangNET8.Services.InputService
{

    public class Soju06InputService : IInputService
    {
        private readonly IKeyboardHook _keyboardHook;
        private readonly IMouseHook _mouseHook;
        private readonly InputStateModel _inputState = new InputStateModel();
        private bool _isCapturing = false;
        private bool _disposed = false;
        private InputBlockingManager _blockingManager;
        
        // Mouse event throttling to prevent performance issues
        private long _lastMouseEventTime = 0;
        private const long MOUSE_THROTTLE_MS = 1; // ~60fps throttling
        private int _lastMouseX = 0;
        private int _lastMouseY = 0;
        private const int MOUSE_MOVEMENT_THRESHOLD = 0; // Minimum pixel movement
        
        

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

        /// <summary>
        /// Set input blocking manager for controlling which inputs to block
        /// </summary>
        public void SetInputBlockingManager(InputBlockingManager blockingManager)
        {
            _blockingManager = blockingManager;
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


        public void EnableDebug(bool enable)
        {
            _keyboardHook.Debug = enable;
            _mouseHook.Debug = enable;
        }


        private bool KeyboardKeyDown(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;
            
            Debug.WriteLine($"Soju06InputService: KeyDown - Soju06 code: {soju06Code} ({key})");
            
            if (IsPanicKeyCombinationPressed(key))
            {
                Stop();

                Debug.WriteLine("EMERGENCY OVERRIDE ACTIVATED - Input capturing stopped");

                return true;
            }

            _inputState.SetKeyState(soju06Code, true);

            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyDown?.Invoke(this, args);

            // Check if this key should be blocked from reaching the game
            if (_blockingManager?.ShouldBlockKey(soju06Code) == true)
            {
                Debug.WriteLine($"Soju06InputService: Blocking key down {soju06Code} from game");
                return false; // Block from game
            }

            return true; // Allow pass-through to game
        }

        private bool IsPanicKeyCombinationPressed(InputKeys key)
        {
            // F9 key for emergency stop
            return key == InputKeys.F9;
        }

        private bool KeyboardKeyUp(object sender, InputKeys key, InputKeyState state)
        {
            int soju06Code = (int)key;
            
            Debug.WriteLine($"Soju06InputService: KeyUp - Soju06 code: {soju06Code} ({key})");
            
            _inputState.SetKeyState(soju06Code, false);

            var args = new InputKeyEventArgs(
                soju06Code,
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyUp?.Invoke(this, args);

            // Check if this key should be blocked from reaching the game
            if (_blockingManager?.ShouldBlockKey(soju06Code) == true)
            {
                Debug.WriteLine($"Soju06InputService: Blocking key up {soju06Code} from game");
                return false; // Block from game
            }

            return true; // Allow pass-through to game
        }

        private bool MouseState(object sender, InputButtons button, int x, int y)
        {
            _inputState.UpdateMousePosition(x, y);

            // Always handle button events immediately
            if (button != InputButtons.None && button != InputButtons.Move)
            {
                bool isPressed = button > 0;
                int absButton = Math.Abs((int)button);
                _inputState.SetMouseButtonState(absButton, isPressed);
                
                var buttonArgs = new InputMouseEventArgs(
                    (int)button,
                    x,
                    y,
                    DateTimeOffset.Now.ToUnixTimeMilliseconds()
                );
                MouseStateChanged?.Invoke(this, buttonArgs);
            }
            else if (button == InputButtons.Move)
            {
                // Throttle mouse movement events to prevent performance issues
                long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                int deltaX = Math.Abs(x - _lastMouseX);
                int deltaY = Math.Abs(y - _lastMouseY);
                
                // Only fire event if enough time passed OR significant movement occurred
                if ((currentTime - _lastMouseEventTime) >= MOUSE_THROTTLE_MS ||
                    deltaX >= MOUSE_MOVEMENT_THRESHOLD || deltaY >= MOUSE_MOVEMENT_THRESHOLD)
                {
                    _lastMouseEventTime = currentTime;
                    _lastMouseX = x;
                    _lastMouseY = y;
                    
                    var moveArgs = new InputMouseEventArgs(
                        (int)button,
                        x,
                        y,
                        currentTime
                    );
                    MouseStateChanged?.Invoke(this, moveArgs);
                }
            }

            // Check if mouse input should be blocked from reaching the game
            if (_blockingManager?.ShouldBlockMouse(button, x, y) == true)
            {
                Debug.WriteLine($"Soju06InputService: Blocking mouse {button} at ({x}, {y}) from game");
                return false; // Block from game
            }

            return true; // Allow pass-through to game
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
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Stop();

                GC.SuppressFinalize(this);
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