using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapperGangNET8.Services.SensitivityService;
using MapperGangNET8.ViewModels;
using MapperGangNET8.Views.Dialogs;

namespace MapperGangNET8.Views
{
    /// <summary>
    /// Interaction logic for SensitivityView.xaml
    /// </summary>
    public partial class SensitivityView : UserControl
    {
        private readonly SolidColorBrush _gridBrush = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        private readonly SolidColorBrush _curveBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private SensitivityViewModel _viewModel;

        public SensitivityView()
        {
            InitializeComponent();

            Loaded += SensitivityView_Loaded;
        }

        private void SensitivityView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as SensitivityViewModel;

            if (_viewModel != null)
            {
                // Subscribe to property changes
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;

                // Initial curve visualization
                DrawMouseCurve();
                DrawJoystickCurve();

                // Event handler for editing curves
                _viewModel.EditMouseCurveCommand = new RelayCommand(() => ShowCurveEditor("Mouse"));
                _viewModel.EditJoystickCurveCommand = new RelayCommand(() => ShowCurveEditor("Joystick"));
            }
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update curve visualizations when relevant properties change
            if (e.PropertyName == nameof(_viewModel.MouseResponseCurveType) ||
                e.PropertyName == nameof(_viewModel.MouseExponent) ||
                e.PropertyName == nameof(_viewModel.MouseCurveStrength) ||
                e.PropertyName == nameof(_viewModel.MouseCurveMidpoint) ||
                e.PropertyName == nameof(_viewModel.MouseCustomCurvePoints))
            {
                DrawMouseCurve();
            }

            if (e.PropertyName == nameof(_viewModel.JoystickResponseCurveType))
            {
                DrawJoystickCurve();
            }
        }

        private void DrawMouseCurve()
        {
            if (_viewModel == null) return;

            MouseCurveCanvas.Children.Clear();

            // Draw grid
            DrawGrid(MouseCurveCanvas);

            // Get a sensitivity provider based on the current curve type
            ISensitivityProvider provider = GetMouseCurveProvider();

            // Get curve parameters
            double[] parameters = GetMouseCurveParameters();

            // Draw the curve
            DrawCurve(MouseCurveCanvas, provider, parameters);
        }

        private void DrawJoystickCurve()
        {
            if (_viewModel == null) return;

            JoystickCurveCanvas.Children.Clear();

            // Draw grid
            DrawGrid(JoystickCurveCanvas);

            // For now, just draw a linear curve for joystick
            // This will be expanded in the future to support different joystick curves
            ISensitivityProvider provider = new LinearCurveProvider();

            if (_viewModel.JoystickResponseCurveType == "Step")
            {
                // Simple step function implementation for visualization
                provider = new CustomCurveProvider();
                ((CustomCurveProvider)provider).SetControlPoints(new List<(double X, double Y)>
                {
                    (-1.0, -1.0),
                    (-0.5, -1.0),
                    (-0.5, -0.5),
                    (0.0, -0.5),
                    (0.0, 0.0),
                    (0.5, 0.0),
                    (0.5, 0.5),
                    (1.0, 0.5),
                    (1.0, 1.0)
                });
            }

            // Draw the curve
            DrawCurve(JoystickCurveCanvas, provider, new double[0]);
        }

        private ISensitivityProvider GetMouseCurveProvider()
        {
            switch (_viewModel.MouseResponseCurveType)
            {
                case "Linear":
                    return new LinearCurveProvider();

                case "Exponential":
                    return new ExponentialCurveProvider();

                case "S-Curve":
                    return new SCurveSensitivityProvider();

                case "Custom":
                    var customProvider = new CustomCurveProvider();

                    // If we have custom points, set them
                    if (_viewModel.MouseCustomCurvePoints != null && _viewModel.MouseCustomCurvePoints.Count > 0)
                    {
                        var controlPoints = new List<(double X, double Y)>();

                        foreach (var point in _viewModel.MouseCustomCurvePoints)
                        {
                            // Convert from [0,1] to [-1,1]
                            controlPoints.Add(((point.X * 2.0) - 1.0, (point.Y * 2.0) - 1.0));
                        }

                        customProvider.SetControlPoints(controlPoints);
                    }

                    return customProvider;

                default:
                    return new LinearCurveProvider();
            }
        }

        private double[] GetMouseCurveParameters()
        {
            switch (_viewModel.MouseResponseCurveType)
            {
                case "Exponential":
                    return new double[] { _viewModel.MouseExponent };

                case "S-Curve":
                    return new double[] { _viewModel.MouseCurveStrength, _viewModel.MouseCurveMidpoint };

                default:
                    return new double[0];
            }
        }

        private void DrawGrid(Canvas canvas)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            // Draw axes
            var xAxis = new Line
            {
                X1 = 0,
                Y1 = height / 2,
                X2 = width,
                Y2 = height / 2,
                Stroke = _gridBrush,
                StrokeThickness = 1
            };

            var yAxis = new Line
            {
                X1 = width / 2,
                Y1 = 0,
                X2 = width / 2,
                Y2 = height,
                Stroke = _gridBrush,
                StrokeThickness = 1
            };

            canvas.Children.Add(xAxis);
            canvas.Children.Add(yAxis);

            // Draw grid lines
            for (int i = 1; i < 4; i++)
            {
                // Horizontal lines (above and below center)
                var lineAbove = new Line
                {
                    X1 = 0,
                    Y1 = height / 2 - (height / 4) * (i / 2.0),
                    X2 = width,
                    Y2 = height / 2 - (height / 4) * (i / 2.0),
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5
                };

                var lineBelow = new Line
                {
                    X1 = 0,
                    Y1 = height / 2 + (height / 4) * (i / 2.0),
                    X2 = width,
                    Y2 = height / 2 + (height / 4) * (i / 2.0),
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5
                };

                // Vertical lines (left and right of center)
                var lineLeft = new Line
                {
                    X1 = width / 2 - (width / 4) * (i / 2.0),
                    Y1 = 0,
                    X2 = width / 2 - (width / 4) * (i / 2.0),
                    Y2 = height,
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5
                };

                var lineRight = new Line
                {
                    X1 = width / 2 + (width / 4) * (i / 2.0),
                    Y1 = 0,
                    X2 = width / 2 + (width / 4) * (i / 2.0),
                    Y2 = height,
                    Stroke = _gridBrush,
                    StrokeThickness = 0.5
                };

                canvas.Children.Add(lineAbove);
                canvas.Children.Add(lineBelow);
                canvas.Children.Add(lineLeft);
                canvas.Children.Add(lineRight);
            }
        }

        private void DrawCurve(Canvas canvas, ISensitivityProvider provider, double[] parameters)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            if (width <= 0 || height <= 0 || provider == null)
                return;

            // Get curve points
            var curvePoints = provider.GetCurvePoints(100, parameters);

            // Create polyline
            var curveLine = new Polyline
            {
                Stroke = _curveBrush,
                StrokeThickness = 2
            };

            // Convert curve points to canvas coordinates
            foreach (var point in curvePoints)
            {
                double x = (point.X + 1.0) / 2.0 * width;
                double y = height - ((point.Y + 1.0) / 2.0 * height);
                curveLine.Points.Add(new Point(x, y));
            }

            canvas.Children.Add(curveLine);
        }

        private void ShowCurveEditor(string curveType)
        {
            if (_viewModel == null)
                return;

            string title;
            string currentCurveType;
            double exponent;
            double strength;
            double midpoint;
            List<Point> controlPoints;
            bool isMouseCurve;

            if (curveType == "Mouse")
            {
                title = "Edit Mouse Response Curve";
                currentCurveType = _viewModel.MouseResponseCurveType;
                exponent = _viewModel.MouseExponent;
                strength = _viewModel.MouseCurveStrength;
                midpoint = _viewModel.MouseCurveMidpoint;
                controlPoints = _viewModel.MouseCustomCurvePoints;
                isMouseCurve = true;
            }
            else // Joystick
            {
                title = "Edit Joystick Response Curve";
                currentCurveType = _viewModel.JoystickResponseCurveType;
                exponent = 2.0; // Default
                strength = 0.5; // Default
                midpoint = 0.5; // Default
                controlPoints = new List<Point>(); // Empty for now
                isMouseCurve = false;
            }

            // Create dialog
            var dialog = new CurveEditorDialog(
                title,
                currentCurveType,
                exponent,
                strength,
                midpoint,
                controlPoints,
                new RelayCommand(() => { }), // Placeholder
                isMouseCurve);

            // Show dialog
            dialog.Owner = Window.GetWindow(this);
            dialog.ShowDialog();

            // If OK was clicked, update the view model
            if (dialog.GetDialogResult())
            {
                if (curveType == "Mouse")
                {
                    _viewModel.MouseResponseCurveType = dialog.CurveType;
                    _viewModel.MouseExponent = dialog.Exponent;
                    _viewModel.MouseCurveStrength = dialog.Strength;
                    _viewModel.MouseCurveMidpoint = dialog.Midpoint;

                    if (dialog.CurveType == "Custom" && dialog.ControlPoints != null)
                    {
                        _viewModel.MouseCustomCurvePoints = dialog.ControlPoints;
                    }
                }
                else // Joystick
                {
                    _viewModel.JoystickResponseCurveType = dialog.CurveType;

                    // Will add more joystick curve parameters in the future
                }
            }
        }

        /// <summary>
        /// Simple RelayCommand implementation for the code-behind
        /// </summary>
        private class RelayCommand : ICommand
        {
            private readonly Action _execute;

            public RelayCommand(Action execute)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            }

            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter)
            {
                _execute();
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }
        }
    }
}