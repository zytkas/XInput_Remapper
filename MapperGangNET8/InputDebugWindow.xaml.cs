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

        public InputDebugWindow(IInputService inputService)
        {
            InitializeComponent();

            _inputService = inputService;

            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _updateTimer.Tick += UpdateUI;
            _updateTimer.Start();

            CaptureToggle.IsChecked = false;
            BlockToggle.IsChecked = false;
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

        private void BlockToggle_Checked(object sender, RoutedEventArgs e)
        {
            _inputService.SetInputBlocking(true);
            WriteLog("Input blocking enabled");
        }

        private void BlockToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            _inputService.SetInputBlocking(false);
            WriteLog("Input blocking disabled");
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
                if (_inputState.MouseDeltaX % 5 == 0 || _inputState.MouseDeltaY % 5 == 0)
                {
                    WriteLog($"Mouse Move: X={e.X}, Y={e.Y}, DeltaX={_inputState.MouseDeltaX}, DeltaY={_inputState.MouseDeltaY}");
                }
            }
        }

        private void WriteLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"[{timestamp}] {message}");

            string[] lines = _logBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length > 500)
            {
                _logBuilder.Clear();
                for (int i = lines.Length - 500; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                        _logBuilder.AppendLine(lines[i]);
                }
            }

            LogTextBox.Text = _logBuilder.ToString();
            LogTextBox.ScrollToEnd();
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            StringBuilder kbState = new StringBuilder();
            foreach (var key in _inputState.PressedKeys)
            {
                if (key.Value)
                {
                    kbState.AppendLine($"Key: {key.Key} is pressed");
                }
            }
            KeyboardStateTextBox.Text = kbState.ToString();

            StringBuilder mouseState = new StringBuilder();
            mouseState.AppendLine($"Position: X={_inputState.MouseX}, Y={_inputState.MouseY}");
            mouseState.AppendLine($"Delta: X={_inputState.MouseDeltaX}, Y={_inputState.MouseDeltaY}");

            foreach (var button in _inputState.PressedMouseButtons)
            {
                if (button.Value)
                {
                    mouseState.AppendLine($"Button: {button.Key} is pressed");
                }
            }
            MouseStateTextBox.Text = mouseState.ToString();
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