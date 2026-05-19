using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;

namespace MapperGangNET8.Views
{
    public partial class ControllerDebugWindow : FluentWindow
    {
        private readonly IControllerService _controllerService;
        private readonly StringBuilder _logBuilder = new StringBuilder();
        private readonly SolidColorBrush _activeButtonBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private readonly SolidColorBrush _activeTriggerBrush = new SolidColorBrush(Color.FromRgb(230, 80, 0));
        private readonly SolidColorBrush _inactiveButtonBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));
        private readonly DispatcherTimer _updateTimer;
        private ControllerState _currentState = new ControllerState();

        // For interactive testing
        private Dictionary<ControllerButton, object> _controllerElements = new Dictionary<ControllerButton, object>();
        private bool _testButtonPressed = false;
        private ControllerButton? _currentlyTestedButton = null;
        private Point _leftStickCenter;
        private Point _rightStickCenter;
        private bool _leftStickDragging = false;
        private bool _rightStickDragging = false;

        public ControllerDebugWindow(IControllerService controllerService)
        {
            InitializeComponent();

            _controllerService = controllerService;

            // Subscribe to controller events
            _controllerService.ConnectionStateChanged += OnConnectionStateChanged;
            _controllerService.ControllerStateUpdated += OnControllerStateUpdated;

            // Initialize timer for UI updates
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _updateTimer.Tick += UpdateUI;
            _updateTimer.Start();

            // Set initial state
            ConnectToggle.IsChecked = _controllerService.IsConnected;
            WriteLog("Controller Debug Window initialized.");

            // Store controller visual elements for easy access
            _controllerElements[ControllerButton.A] = ButtonA;
            _controllerElements[ControllerButton.B] = ButtonB;
            _controllerElements[ControllerButton.X] = ButtonX;
            _controllerElements[ControllerButton.Y] = ButtonY;
            _controllerElements[ControllerButton.LeftShoulder] = LeftShoulder;
            _controllerElements[ControllerButton.RightShoulder] = RightShoulder;
            _controllerElements[ControllerButton.LeftTrigger] = LeftTrigger;
            _controllerElements[ControllerButton.RightTrigger] = RightTrigger;
            _controllerElements[ControllerButton.Start] = ButtonStart;
            _controllerElements[ControllerButton.Back] = ButtonBack;
            _controllerElements[ControllerButton.Guide] = ButtonGuide;
            _controllerElements[ControllerButton.DPadUp] = DPadUp;
            _controllerElements[ControllerButton.DPadDown] = DPadDown;
            _controllerElements[ControllerButton.DPadLeft] = DPadLeft;
            _controllerElements[ControllerButton.DPadRight] = DPadRight;
            _controllerElements[ControllerButton.LeftThumb] = LeftStick;
            _controllerElements[ControllerButton.RightThumb] = RightStick;

            // Calculate thumbstick centers for interaction
            _leftStickCenter = new Point(
                Canvas.GetLeft(LeftStickBase) + LeftStickBase.Width / 2,
                Canvas.GetTop(LeftStickBase) + LeftStickBase.Height / 2
            );

            _rightStickCenter = new Point(
                Canvas.GetLeft(RightStickBase) + RightStickBase.Width / 2,
                Canvas.GetTop(RightStickBase) + RightStickBase.Height / 2
            );

            // Set up interaction events for thumbsticks
            SetupThumbstickInteraction();
        }

        private void SetupThumbstickInteraction()
        {
            // Left thumbstick
            LeftStick.MouseLeftButtonDown += (sender, e) =>
            {
                _leftStickDragging = true;
                LeftStick.CaptureMouse();
                e.Handled = true;
            };

            LeftStick.MouseLeftButtonUp += (sender, e) =>
            {
                if (_leftStickDragging)
                {
                    _leftStickDragging = false;
                    LeftStick.ReleaseMouseCapture();
                    ResetThumbstickPosition(LeftStick, _leftStickCenter);
                    UpdateThumbstickState(ControllerAxis.LeftThumbX, 0);
                    UpdateThumbstickState(ControllerAxis.LeftThumbY, 0);
                    e.Handled = true;
                }
            };

            LeftStick.MouseMove += (sender, e) =>
            {
                if (_leftStickDragging)
                {
                    Point pos = e.GetPosition(ControllerCanvas);
                    UpdateThumbstickPosition(LeftStick, _leftStickCenter, pos, 15);
                    e.Handled = true;
                }
            };

            // Right thumbstick
            RightStick.MouseLeftButtonDown += (sender, e) =>
            {
                _rightStickDragging = true;
                RightStick.CaptureMouse();
                e.Handled = true;
            };

            RightStick.MouseLeftButtonUp += (sender, e) =>
            {
                if (_rightStickDragging)
                {
                    _rightStickDragging = false;
                    RightStick.ReleaseMouseCapture();
                    ResetThumbstickPosition(RightStick, _rightStickCenter);
                    UpdateThumbstickState(ControllerAxis.RightThumbX, 0);
                    UpdateThumbstickState(ControllerAxis.RightThumbY, 0);
                    e.Handled = true;
                }
            };

            RightStick.MouseMove += (sender, e) =>
            {
                if (_rightStickDragging)
                {
                    Point pos = e.GetPosition(ControllerCanvas);
                    UpdateThumbstickPosition(RightStick, _rightStickCenter, pos, 15);
                    e.Handled = true;
                }
            };

            // Make thumbsticks look interactive
            LeftStick.Cursor = Cursors.Hand;
            RightStick.Cursor = Cursors.Hand;
        }

        private void UpdateThumbstickPosition(UIElement thumbstick, Point center, Point currentPos, double maxDistance)
        {
            // Calculate distance from center
            double deltaX = currentPos.X - center.X;
            double deltaY = currentPos.Y - center.Y;
            double distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

            // Limit to max distance
            if (distance > maxDistance)
            {
                deltaX = deltaX * maxDistance / distance;
                deltaY = deltaY * maxDistance / distance;
            }

            // Update position
            Canvas.SetLeft(thumbstick, center.X - thumbstick.DesiredSize.Width / 2 + deltaX);
            Canvas.SetTop(thumbstick, center.Y - thumbstick.DesiredSize.Height / 2 + deltaY);

            // Calculate and update axis values (-1 to 1)
            double xAxis = deltaX / maxDistance;
            double yAxis = -deltaY / maxDistance; // Invert Y since canvas coordinates are top-down

            // Update controller state
            if (thumbstick == LeftStick)
            {
                UpdateThumbstickState(ControllerAxis.LeftThumbX, xAxis);
                UpdateThumbstickState(ControllerAxis.LeftThumbY, yAxis);
            }
            else if (thumbstick == RightStick)
            {
                UpdateThumbstickState(ControllerAxis.RightThumbX, xAxis);
                UpdateThumbstickState(ControllerAxis.RightThumbY, yAxis);
            }
        }

        private void ResetThumbstickPosition(UIElement thumbstick, Point center)
        {
            Canvas.SetLeft(thumbstick, center.X - thumbstick.DesiredSize.Width / 2);
            Canvas.SetTop(thumbstick, center.Y - thumbstick.DesiredSize.Height / 2);
        }

        private void UpdateThumbstickState(ControllerAxis axis, double value)
        {
            _controllerService.SetAxis(axis, value);
            _controllerService.Submit();
            _currentState.SetAxis(axis, value);
        }

        private void ConnectToggle_Checked(object sender, RoutedEventArgs e)
        {
            ConnectController();
        }

        private void ConnectToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            DisconnectController();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            _logBuilder.Clear();
            LogTextBox.Text = string.Empty;
            WriteLog("Log cleared");
        }

        private async void ConnectController()
        {
            ControllerType controllerType = ControllerType.Xbox360;
            int controllerIndex = ControllerIndexComboBox.SelectedIndex + 1;

            // Disable UI while connecting
            ConnectToggle.IsEnabled = false;
            ControllerTypeComboBox.IsEnabled = false;
            ControllerIndexComboBox.IsEnabled = false;

            WriteLog($"Connecting {controllerType} controller (index: {controllerIndex})...");

            try
            {
                bool success = true;
                if (success)
                {
                    WriteLog("Controller connected successfully");
                }
                else
                {
                    WriteLog("Failed to connect controller");
                    ConnectToggle.IsChecked = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Error connecting controller: {ex.Message}");
                ConnectToggle.IsChecked = false;
            }
            finally
            {
                // Re-enable UI
                ConnectToggle.IsEnabled = true;
                ControllerTypeComboBox.IsEnabled = !_controllerService.IsConnected;
                ControllerIndexComboBox.IsEnabled = !_controllerService.IsConnected;
            }
        }

        private async void DisconnectController()
        {
            // Disable toggle while disconnecting
            ConnectToggle.IsEnabled = false;

            WriteLog("Disconnecting controller...");

            try
            {
                await _controllerService.DisconnectAsync();
                WriteLog("Controller disconnected");
            }
            catch (Exception ex)
            {
                WriteLog($"Error disconnecting controller: {ex.Message}");
            }
            finally
            {
                // Re-enable toggle and type/index selectors
                ConnectToggle.IsEnabled = true;
                ControllerTypeComboBox.IsEnabled = true;
                ControllerIndexComboBox.IsEnabled = true;
            }
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_controllerService.IsConnected) return;

            if (sender is Button button && button.Tag is string buttonTag)
            {
                _testButtonPressed = !_testButtonPressed;

                // Parse the tag to get the corresponding controller button
                if (Enum.TryParse(buttonTag, out ControllerButton controllerButton))
                {
                    if (_testButtonPressed)
                    {
                        _currentlyTestedButton = controllerButton;
                        WriteLog($"Test Button {buttonTag} pressed");
                        _controllerService.SetButton(controllerButton, true);
                        _controllerService.Submit();

                        string originalContent = button.Content.ToString();
                        button.Content = $"{originalContent}";

                        if (button.Style == null)
                        {
                            button.Background = new SolidColorBrush(Color.FromRgb(0, 120, 215));
                            button.Foreground = Brushes.White;
                        }

                        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
                        timer.Tick += (s, args) => {
                            _testButtonPressed = false;
                            _controllerService.SetButton(controllerButton, false);
                            _controllerService.Submit();
                            button.Content = originalContent;
                            WriteLog($"Test Button {buttonTag} released");

                            if (button.Style == null)
                            {
                                button.ClearValue(Button.BackgroundProperty);
                                button.ClearValue(Button.ForegroundProperty);
                            }

                            timer.Stop();
                        };
                        timer.Start();
                    }
                }
            }
        }

        private void TriggerSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_controllerService.IsConnected) return;

            if (sender is Slider slider && slider.Tag is string triggerTag)
            {
                double value = slider.Value;

                if (triggerTag == "LeftTrigger")
                {
                    _controllerService.SetAxis(ControllerAxis.LeftTrigger, value);
                    _controllerService.Submit();
                    WriteLog($"Left Trigger value: {value:F2}");
                }
                else if (triggerTag == "RightTrigger")
                {
                    _controllerService.SetAxis(ControllerAxis.RightTrigger, value);
                    _controllerService.Submit();
                    WriteLog($"Right Trigger value: {value:F2}");
                }
            }
        }

        private void ResetController_Click(object sender, RoutedEventArgs e)
        {
            if (!_controllerService.IsConnected) return;

            _controllerService.ResetState();
            WriteLog("Controller state reset");

            // Reset UI
            LeftTriggerSlider.Value = 0;
            RightTriggerSlider.Value = 0;

            // Reset thumbstick positions
            ResetThumbstickPosition(LeftStick, _leftStickCenter);
            ResetThumbstickPosition(RightStick, _rightStickCenter);
        }

        private void OpenWindowsGameControllerTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open Windows Game Controller Test panel
                Process.Start("control", "joy.cpl");
                WriteLog("Opened Windows Game Controller Test panel");
            }
            catch (Exception ex)
            {
                WriteLog($"Failed to open Game Controller panel: {ex.Message}");
            }
        }

        private void OnConnectionStateChanged(object sender, ControllerConnectionEventArgs e)
        {
            // Update UI based on connection state
            Dispatcher.Invoke(() =>
            {
                ConnectToggle.IsChecked = e.IsConnected;
                ControllerTypeComboBox.IsEnabled = !e.IsConnected;
                ControllerIndexComboBox.IsEnabled = !e.IsConnected;

                if (e.IsConnected)
                {
                    WriteLog($"Controller connected: {e.ControllerType}, Index: {e.ControllerIndex}");
                }
                else
                {
                    if (!string.IsNullOrEmpty(e.ErrorMessage))
                    {
                        WriteLog($"Controller disconnected: {e.ErrorMessage}");
                    }
                    else
                    {
                        WriteLog("Controller disconnected");
                    }
                }
            });
        }

        private void OnControllerStateUpdated(object sender, ControllerStateEventArgs e)
        {
            // Update our state copy
            _currentState = e.State.Clone();
        }

        private void WriteLog(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            _logBuilder.AppendLine($"[{timestamp}] {message}");

            // Limit log size to avoid performance issues
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

            // Update the log text box
            LogTextBox.Text = _logBuilder.ToString();
            LogTextBox.ScrollToEnd();
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            UpdateButtonStateText();
            UpdateAxisStateText();
            UpdateControllerVisualization();
        }

        private void UpdateButtonStateText()
        {
            StringBuilder buttonState = new StringBuilder();

            // Add button states
            if (_currentState.IsButtonPressed(ControllerButton.A))
                buttonState.AppendLine("A: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.B))
                buttonState.AppendLine("B: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.X))
                buttonState.AppendLine("X: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.Y))
                buttonState.AppendLine("Y: Pressed");

            if (_currentState.IsButtonPressed(ControllerButton.LeftShoulder))
                buttonState.AppendLine("Left Shoulder: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.RightShoulder))
                buttonState.AppendLine("Right Shoulder: Pressed");

            if (_currentState.IsButtonPressed(ControllerButton.LeftThumb))
                buttonState.AppendLine("Left Thumb: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.RightThumb))
                buttonState.AppendLine("Right Thumb: Pressed");

            if (_currentState.IsButtonPressed(ControllerButton.DPadUp))
                buttonState.AppendLine("D-Pad Up: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.DPadDown))
                buttonState.AppendLine("D-Pad Down: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.DPadLeft))
                buttonState.AppendLine("D-Pad Left: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.DPadRight))
                buttonState.AppendLine("D-Pad Right: Pressed");

            if (_currentState.IsButtonPressed(ControllerButton.Start))
                buttonState.AppendLine("Start: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.Back))
                buttonState.AppendLine("Back: Pressed");
            if (_currentState.IsButtonPressed(ControllerButton.Guide))
                buttonState.AppendLine("Guide: Pressed");

            ButtonStateTextBox.Text = buttonState.ToString();
        }

        private void UpdateAxisStateText()
        {
            StringBuilder axisState = new StringBuilder();

            // Add axis states
            axisState.AppendLine($"Left Thumb X: {_currentState.LeftThumbX:F2}");
            axisState.AppendLine($"Left Thumb Y: {_currentState.LeftThumbY:F2}");
            axisState.AppendLine($"Right Thumb X: {_currentState.RightThumbX:F2}");
            axisState.AppendLine($"Right Thumb Y: {_currentState.RightThumbY:F2}");
            axisState.AppendLine($"Left Trigger: {_currentState.LeftTrigger:F2}");
            axisState.AppendLine($"Right Trigger: {_currentState.RightTrigger:F2}");

            AxisStateTextBox.Text = axisState.ToString();
        }

        private void UpdateControllerVisualization()
        {
            // Update button colors based on state
            ButtonA.Fill = _currentState.IsButtonPressed(ControllerButton.A) ? _activeButtonBrush : _inactiveButtonBrush;
            ButtonB.Fill = _currentState.IsButtonPressed(ControllerButton.B) ? _activeButtonBrush : _inactiveButtonBrush;
            ButtonX.Fill = _currentState.IsButtonPressed(ControllerButton.X) ? _activeButtonBrush : _inactiveButtonBrush;
            ButtonY.Fill = _currentState.IsButtonPressed(ControllerButton.Y) ? _activeButtonBrush : _inactiveButtonBrush;

            LeftShoulder.Fill = _currentState.IsButtonPressed(ControllerButton.LeftShoulder) ? _activeButtonBrush : _inactiveButtonBrush;
            RightShoulder.Fill = _currentState.IsButtonPressed(ControllerButton.RightShoulder) ? _activeButtonBrush : _inactiveButtonBrush;

            // Update trigger visuals based on value
            LeftTrigger.Fill = _currentState.LeftTrigger > 0.1 ? _activeTriggerBrush : _inactiveButtonBrush;
            LeftTrigger.Opacity = 0.5 + (_currentState.LeftTrigger * 0.5); // Make more opaque as value increases

            RightTrigger.Fill = _currentState.RightTrigger > 0.1 ? _activeTriggerBrush : _inactiveButtonBrush;
            RightTrigger.Opacity = 0.5 + (_currentState.RightTrigger * 0.5);

            // Update D-pad
            DPadUp.Fill = _currentState.IsButtonPressed(ControllerButton.DPadUp) ? _activeButtonBrush : _inactiveButtonBrush;
            DPadDown.Fill = _currentState.IsButtonPressed(ControllerButton.DPadDown) ? _activeButtonBrush : _inactiveButtonBrush;
            DPadLeft.Fill = _currentState.IsButtonPressed(ControllerButton.DPadLeft) ? _activeButtonBrush : _inactiveButtonBrush;
            DPadRight.Fill = _currentState.IsButtonPressed(ControllerButton.DPadRight) ? _activeButtonBrush : _inactiveButtonBrush;

            // Update special buttons
            ButtonStart.Fill = _currentState.IsButtonPressed(ControllerButton.Start) ? _activeButtonBrush : _inactiveButtonBrush;
            ButtonBack.Fill = _currentState.IsButtonPressed(ControllerButton.Back) ? _activeButtonBrush : _inactiveButtonBrush;
            ButtonGuide.Fill = _currentState.IsButtonPressed(ControllerButton.Guide) ? _activeButtonBrush : _inactiveButtonBrush;

            // Only update thumbstick positions from state if not actively dragging
            if (!_leftStickDragging)
            {
                double leftStickX = _currentState.LeftThumbX;
                double leftStickY = -_currentState.LeftThumbY; // Invert Y for visual

                Canvas.SetLeft(LeftStick, _leftStickCenter.X - LeftStick.Width / 2 + (leftStickX * 15));
                Canvas.SetTop(LeftStick, _leftStickCenter.Y - LeftStick.Height / 2 + (leftStickY * 15));
            }

            if (!_rightStickDragging)
            {
                double rightStickX = _currentState.RightThumbX;
                double rightStickY = -_currentState.RightThumbY; // Invert Y for visual

                Canvas.SetLeft(RightStick, _rightStickCenter.X - RightStick.Width / 2 + (rightStickX * 15));
                Canvas.SetTop(RightStick, _rightStickCenter.Y - RightStick.Height / 2 + (rightStickY * 15));
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Clean up
            _updateTimer.Stop();
            _controllerService.ConnectionStateChanged -= OnConnectionStateChanged;
            _controllerService.ControllerStateUpdated -= OnControllerStateUpdated;

            if (_controllerService.IsConnected)
            {
                // Disconnect on window close
                _controllerService.DisconnectAsync().ConfigureAwait(false);
            }

            base.OnClosing(e);
        }
    }
}