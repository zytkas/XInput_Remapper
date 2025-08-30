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
        private bool _disposed = false;
        
        // Mouse event throttling to prevent performance issues
        private long _lastMouseEventTime = 0;
        private const long MOUSE_THROTTLE_MS = 16; // ~60fps throttling
        private int _lastMouseX = 0;
        private int _lastMouseY = 0;
        private const int MOUSE_MOVEMENT_THRESHOLD = 2; // Minimum pixel movement
        
        /// <summary>
        /// Convert soju06 library key codes to standard Win32 Virtual-Key codes
        /// soju06 returns sequential codes (A=1, B=2, etc.) but we need Win32 VK codes (A=65, B=66, etc.)
        /// </summary>
        private static readonly Dictionary<int, int> Soju06ToWin32KeyMap = new Dictionary<int, int>()
        {
            // Soju06 sequential codes -> Win32 VK codes
            // A-Z mapping (soju06: 1-26, Win32: 65-90)
            {1, 65},   // A
            {2, 66},   // B  
            {3, 67},   // C
            {4, 68},   // D
            {5, 69},   // E
            {6, 70},   // F
            {7, 71},   // G
            {8, 72},   // H
            {9, 73},   // I
            {10, 74},  // J
            {11, 75},  // K
            {12, 76},  // L
            {13, 77},  // M
            {14, 78},  // N
            {15, 79},  // O
            {16, 80},  // P
            {17, 81},  // Q
            {18, 82},  // R
            {19, 83},  // S
            {20, 84},  // T
            {21, 85},  // U
            {22, 86},  // V
            {23, 87},  // W
            {24, 88},  // X
            {25, 89},  // Y
            {26, 90},  // Z
            
            // Special keys that soju06 maps differently
            {66, 32},  // Space (soju06: 66 -> Win32: 32)
            
            // Numbers 0-9 (if they're also mapped incorrectly)
            {27, 48},  // 0
            {28, 49},  // 1
            {29, 50},  // 2
            {30, 51},  // 3
            {31, 52},  // 4
            {32, 53},  // 5
            {33, 54},  // 6
            {34, 55},  // 7
            {35, 56},  // 8
            {36, 57},  // 9
            
            // Common control keys
            {37, 13},  // Enter
            {38, 27},  // Escape
            {39, 8},   // Backspace
            {40, 9},   // Tab
            {41, 16},  // Shift
            {42, 17},  // Ctrl
            {43, 18},  // Alt
        };
        
        /// <summary>
        /// Convert soju06 key code to Win32 Virtual-Key code
        /// </summary>
        private static int ConvertSoju06KeyToWin32(int soju06KeyCode)
        {
            // Check if we have a mapping for this key
            if (Soju06ToWin32KeyMap.TryGetValue(soju06KeyCode, out int win32Code))
            {
                return win32Code;
            }
            
            // If no mapping found, return original code (might be correct already)
            // Add debug info for unmapped keys
            System.Diagnostics.Debug.WriteLine($"Soju06InputService: Unknown key code {soju06KeyCode}, using as-is");
            return soju06KeyCode;
        }
        

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
            // Convert soju06 key code to Win32 VK code
            int soju06Code = (int)key;
            int win32Code = ConvertSoju06KeyToWin32(soju06Code);
            
            Debug.WriteLine($"Soju06InputService: KeyDown - Soju06 code: {soju06Code} -> Win32 code: {win32Code}");
            
            if (IsPanicKeyCombinationPressed(win32Code))
            {
                Stop();

                Debug.WriteLine("EMERGENCY OVERRIDE ACTIVATED - Input capturing stopped");

                return true;
            }

            _inputState.SetKeyState(win32Code, true);

            var args = new InputKeyEventArgs(
                soju06Code,  // Use soju06 code directly (InputKeys enum values)
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyDown?.Invoke(this, args);

            return true; // Always allow pass-through
        }

        private bool IsPanicKeyCombinationPressed(int win32KeyCode)
        {
            //ctrl+alt+shift+esc (using Win32 VK codes)
            return (win32KeyCode == 27 && // VK_ESCAPE
                    _inputState.IsKeyPressed(17) && // VK_CONTROL
                    _inputState.IsKeyPressed(18) && // VK_MENU (Alt)
                    _inputState.IsKeyPressed(16)); // VK_SHIFT
        }

        private bool KeyboardKeyUp(object sender, InputKeys key, InputKeyState state)
        {
            // Convert soju06 key code to Win32 VK code
            int soju06Code = (int)key;
            int win32Code = ConvertSoju06KeyToWin32(soju06Code);
            
            Debug.WriteLine($"Soju06InputService: KeyUp - Soju06 code: {soju06Code} -> Win32 code: {win32Code}");
            
            _inputState.SetKeyState(win32Code, false);

            var args = new InputKeyEventArgs(
                soju06Code,  // Use soju06 code directly (InputKeys enum values)
                (int)state,
                DateTimeOffset.Now.ToUnixTimeMilliseconds()
            );

            KeyUp?.Invoke(this, args);

            return true; // Always allow pass-through
        }

        private bool MouseState(object sender, InputButtons button, int x, int y)
        {
            _inputState.UpdateMousePosition(x, y);

            // Always handle button events immediately
            if (button != InputButtons.None)
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
            else
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

            return true; // Always allow pass-through
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