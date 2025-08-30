using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using MapperGangNET8.Models;
using MapperGangNET8.Services.InputService;
using Wpf.Ui.Controls;

namespace MapperGangNET8.Views
{
    public partial class InputDebugWindow : FluentWindow
    {
        private readonly IInputService _inputService;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly InputStateModel _inputState = new InputStateModel();
        private readonly DispatcherTimer _updateTimer;
        private int _logUpdateCounter = 0;
        private const int LOG_UPDATE_THROTTLE = 5; // Update UI every 5th log entry
        
        // Reuse StringBuilder instances to reduce GC pressure
        private readonly StringBuilder _kbStateBuilder = new StringBuilder();
        private readonly StringBuilder _mouseStateBuilder = new StringBuilder();

        public InputDebugWindow(IInputService inputService)
        {
            InitializeComponent();

            _inputService = inputService;

            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(250)
            };
            _updateTimer.Tick += UpdateUI;
            _updateTimer.Start();

            CaptureToggle.IsChecked = false;
            DebugToggle.IsChecked = true;

            _inputService.EnableDebug(true);

            WriteLog("Input Debug Window initialized. Toggle 'Capture Input' to begin...");
        }

        private void CaptureToggle_Checked(object sender, RoutedEventArgs e)
        {
            _inputService.Start();
            WriteLog("Input capturing started");
        }

        private void CaptureToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _inputService.Stop();
            WriteLog("Input capturing stopped");
        }


        private void DebugToggle_Checked(object sender, RoutedEventArgs e)
        {
            _inputService.EnableDebug(true);
            WriteLog("Debug mode enabled");
        }

        private void DebugToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _inputService.EnableDebug(false);
            WriteLog("Debug mode disabled");
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logBuilder.Clear();
            LogTextBox.Text = string.Empty;
            WriteLog("Log cleared");
        }

        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            _inputState.SetKeyState(e.KeyCode, true);
            WriteLog($"Key Down: {e.KeyCode}, Handled: {e.Handled}");
        }

        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            _inputState.SetKeyState(e.KeyCode, false);
            WriteLog($"Key Up: {e.KeyCode}, Handled: {e.Handled}");
        }

        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            _inputState.UpdateMousePosition(e.X, e.Y);

            if (e.Button != 0)
            {
                bool isPressed = e.Button > 0;
                int absButton = Math.Abs(e.Button);
                _inputState.SetMouseButtonState(absButton, isPressed);

                string action = isPressed ? "Down" : "Up";
                WriteLog($"Mouse Button {absButton} {action} at X={e.X}, Y={e.Y}, Handled: {e.Handled}");
            }
            else
            {
                // Only log mouse movement occasionally to reduce performance impact
                if (Math.Abs(_inputState.MouseDeltaX) > 10 || Math.Abs(_inputState.MouseDeltaY) > 10)
                {
                    WriteLog($"Mouse Move: X={e.X}, Y={e.Y}, DeltaX={_inputState.MouseDeltaX}, DeltaY={_inputState.MouseDeltaY}");
                }
            }
        }

        private void WriteLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"[{timestamp}] {message}");

            // Throttle UI updates to improve performance
            _logUpdateCounter++;
            if (_logUpdateCounter >= LOG_UPDATE_THROTTLE)
            {
                _logUpdateCounter = 0;
                
                // Limit log size more efficiently
                if (_logBuilder.Length > 50000) // ~500 lines
                {
                    string currentLog = _logBuilder.ToString();
                    int halfPoint = currentLog.Length / 2;
                    int newlineIndex = currentLog.IndexOf(Environment.NewLine, halfPoint);
                    if (newlineIndex > 0)
                    {
                        _logBuilder.Clear();
                        _logBuilder.Append(currentLog.Substring(newlineIndex + Environment.NewLine.Length));
                    }
                }

                LogTextBox.Text = _logBuilder.ToString();
                LogTextBox.ScrollToEnd();
            }
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Reuse StringBuilder to reduce GC pressure
            _kbStateBuilder.Clear();
            foreach (var key in _inputState.PressedKeys)
            {
                if (key.Value)
                {
                    _kbStateBuilder.AppendLine($"Key: {key.Key} is pressed");
                }
            }
            KeyboardStateTextBox.Text = _kbStateBuilder.ToString();

            _mouseStateBuilder.Clear();
            _mouseStateBuilder.AppendLine($"Position: X={_inputState.MouseX}, Y={_inputState.MouseY}");
            _mouseStateBuilder.AppendLine($"Delta: X={_inputState.MouseDeltaX}, Y={_inputState.MouseDeltaY}");

            foreach (var button in _inputState.PressedMouseButtons)
            {
                if (button.Value)
                {
                    _mouseStateBuilder.AppendLine($"Button: {button.Key} is pressed");
                }
            }
            MouseStateTextBox.Text = _mouseStateBuilder.ToString();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _inputService.Stop();
            _updateTimer.Stop();
            _inputService.KeyDown -= OnKeyDown;
            _inputService.KeyUp -= OnKeyUp;
            _inputService.MouseStateChanged -= OnMouseStateChanged;

            base.OnClosing(e);
        }
    }
}