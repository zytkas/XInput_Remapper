using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Threading;
using MapperGangNET8.Models;

namespace MapperGangNET8.Services.InputService
{
    public class InputMonitorService : IDisposable
    {
        private readonly IInputService _inputService;
        private readonly InputStateModel _stateModel = new InputStateModel();
        private readonly TextBox _outputTextBox;
        private readonly DispatcherTimer _updateTimer;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private bool _disposed = false;
        private int _maxLogEntries = 100;


        public InputMonitorService(IInputService inputService, TextBox outputTextBox)
        {
            _inputService = inputService ?? throw new ArgumentNullException(nameof(inputService));
            _outputTextBox = outputTextBox ?? throw new ArgumentNullException(nameof(outputTextBox));

            _inputService.KeyDown += OnKeyDown;
            _inputService.KeyUp += OnKeyUp;
            _inputService.MouseStateChanged += OnMouseStateChanged;

            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _updateTimer.Tick += UpdateDisplay;
            _updateTimer.Start();
        }

        public void SetMaxLogEntries(int maxEntries)
        {
            if (maxEntries > 0)
                _maxLogEntries = maxEntries;
        }


        public void ClearLog()
        {
            _logBuilder.Clear();
            UpdateTextBox();
        }

        private void OnKeyDown(object sender, InputKeyEventArgs e)
        {
            _stateModel.SetKeyState(e.KeyCode, true);
            LogEvent($"KeyDown: Key={e.KeyCode}, State={e.KeyState}, Handled={e.Handled}");
        }

        private void OnKeyUp(object sender, InputKeyEventArgs e)
        {
            _stateModel.SetKeyState(e.KeyCode, false);
            LogEvent($"KeyUp: Key={e.KeyCode}, State={e.KeyState}, Handled={e.Handled}");
        }

        private void OnMouseStateChanged(object sender, InputMouseEventArgs e)
        {
            _stateModel.UpdateMousePosition(e.X, e.Y);
            if (e.Button != 0)
            {
                bool isPressed = e.Button > 0;
                int absButton = Math.Abs(e.Button);
                _stateModel.SetMouseButtonState(absButton, isPressed);

                string buttonAction = isPressed ? "Pressed" : "Released";
                LogEvent($"Mouse Button {absButton} {buttonAction} at X={e.X}, Y={e.Y}, Handled={e.Handled}");
            }
            else
            {
                LogEvent($"Mouse Move: X={e.X}, Y={e.Y}, DeltaX={_stateModel.MouseDeltaX}, DeltaY={_stateModel.MouseDeltaY}");
            }
        }

        private void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"[{timestamp}] {message}");

            // Limit log size
            string[] lines = _logBuilder.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            if (lines.Length > _maxLogEntries + 1)
            {
                _logBuilder.Clear();
                for (int i = lines.Length - _maxLogEntries; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                        _logBuilder.AppendLine(lines[i]);
                }
            }
        }

        private void UpdateDisplay(object sender, EventArgs e)
        {
            UpdateTextBox();
        }

        private void UpdateTextBox()
        {
            if (_outputTextBox != null && !_disposed)
            {
                _outputTextBox.Dispatcher.Invoke(() =>
                {
                    _outputTextBox.Text = _logBuilder.ToString();
                    _outputTextBox.ScrollToEnd();
                });
            }
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
                // Unsubscribe from events
                _inputService.KeyDown -= OnKeyDown;
                _inputService.KeyUp -= OnKeyUp;
                _inputService.MouseStateChanged -= OnMouseStateChanged;

                // Stop timer
                _updateTimer.Stop();
            }

            _disposed = true;
        }
    }
}