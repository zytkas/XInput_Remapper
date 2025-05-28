using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapperGangNET8.Services.SensitivityService;

namespace MapperGangNET8.Views.Controls
{
    /// <summary>
    /// Interaction logic for CurveEditor.xaml
    /// </summary>
    public partial class CurveEditor : UserControl
    {
        private readonly SolidColorBrush _gridBrush = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        private readonly SolidColorBrush _curveBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private readonly SolidColorBrush _controlPointBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230));
        private readonly SolidColorBrush _controlPointSelectedBrush = new SolidColorBrush(Color.FromRgb(255, 128, 0));

        private Polyline _curveLine;
        private readonly List<Ellipse> _controlPoints = new List<Ellipse>();
        private int _selectedControlPointIndex = -1;
        private bool _isDragging = false;

        private ISensitivityProvider _currentProvider;
        private string _currentCurveType = "Linear";
        private double[] _currentParams = new double[0];

        #region DependencyProperties

        public static readonly DependencyProperty CurveTypeProperty =
            DependencyProperty.Register("CurveType", typeof(string), typeof(CurveEditor),
                new PropertyMetadata("Linear", OnCurveTypeChanged));

        public static readonly DependencyProperty ExponentProperty =
            DependencyProperty.Register("Exponent", typeof(double), typeof(CurveEditor),
                new PropertyMetadata(2.0, OnParameterChanged));

        public static readonly DependencyProperty StrengthProperty =
            DependencyProperty.Register("Strength", typeof(double), typeof(CurveEditor),
                new PropertyMetadata(0.5, OnParameterChanged));

        public static readonly DependencyProperty MidpointProperty =
            DependencyProperty.Register("Midpoint", typeof(double), typeof(CurveEditor),
                new PropertyMetadata(0.5, OnParameterChanged));

        public static readonly DependencyProperty ControlPointsProperty =
            DependencyProperty.Register("ControlPoints", typeof(List<Point>), typeof(CurveEditor),
                new PropertyMetadata(null, OnControlPointsChanged));

        public static readonly DependencyProperty CurveChangedCommandProperty =
            DependencyProperty.Register("CurveChangedCommand", typeof(ICommand), typeof(CurveEditor));

        public string CurveType
        {
            get => (string)GetValue(CurveTypeProperty);
            set => SetValue(CurveTypeProperty, value);
        }

        public double Exponent
        {
            get => (double)GetValue(ExponentProperty);
            set => SetValue(ExponentProperty, value);
        }

        public double Strength
        {
            get => (double)GetValue(StrengthProperty);
            set => SetValue(StrengthProperty, value);
        }

        public double Midpoint
        {
            get => (double)GetValue(MidpointProperty);
            set => SetValue(MidpointProperty, value);
        }

        public List<Point> ControlPoints
        {
            get => (List<Point>)GetValue(ControlPointsProperty);
            set => SetValue(ControlPointsProperty, value);
        }

        public ICommand CurveChangedCommand
        {
            get => (ICommand)GetValue(CurveChangedCommandProperty);
            set => SetValue(CurveChangedCommandProperty, value);
        }

        #endregion

        public CurveEditor()
        {
            InitializeComponent();

            // Initialize providers for visualization
            InitializeProviders();

            // Set default values
            ExponentSlider.Value = Exponent;
            StrengthSlider.Value = Strength;
            MidpointSlider.Value = Midpoint;

            // Create empty curve line
            _curveLine = new Polyline
            {
                Stroke = _curveBrush,
                StrokeThickness = 2
            };

            CurveCanvas.SizeChanged += (s, e) => DrawCurve();

            // Initial drawing
            Loaded += (s, e) => DrawCurve();
        }

        private void InitializeProviders()
        {
            // Create providers based on the current curve type
            switch (CurveType)
            {
                case "Linear":
                    _currentProvider = new LinearCurveProvider();
                    break;

                case "Exponential":
                    _currentProvider = new ExponentialCurveProvider();
                    break;

                case "S-Curve":
                    _currentProvider = new SCurveSensitivityProvider();
                    break;

                case "Custom":
                    _currentProvider = new CustomCurveProvider();
                    if (ControlPoints != null && ControlPoints.Count > 0)
                    {
                        var controlPoints = ControlPoints.Select(p => ((p.X * 2.0) - 1.0, (p.Y * 2.0) - 1.0)).ToList();
                        ((CustomCurveProvider)_currentProvider).SetControlPoints(controlPoints);
                    }
                    break;

                default:
                    _currentProvider = new LinearCurveProvider();
                    break;
            }

            _currentCurveType = CurveType;
            UpdateParameterControls();
        }

        private void UpdateParameterControls()
        {
            // Hide all parameter panels
            ExponentialPanel.Visibility = Visibility.Collapsed;
            SCurvePanel.Visibility = Visibility.Collapsed;
            CustomCurveInstructions.Visibility = Visibility.Collapsed;
            ResetCustomCurveButton.Visibility = Visibility.Collapsed;

            // Show appropriate panel
            switch (_currentCurveType)
            {
                case "Exponential":
                    ExponentialPanel.Visibility = Visibility.Visible;
                    _currentParams = new double[] { Exponent };
                    break;

                case "S-Curve":
                    SCurvePanel.Visibility = Visibility.Visible;
                    _currentParams = new double[] { Strength, Midpoint };
                    break;

                case "Custom":
                    CustomCurveInstructions.Visibility = Visibility.Visible;
                    ResetCustomCurveButton.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void DrawCurve()
        {
            if (CurveCanvas.ActualWidth <= 0 || CurveCanvas.ActualHeight <= 0)
                return;

            CurveCanvas.Children.Clear();

            // Draw grid
            DrawGrid();

            // Draw curve
            DrawCurveLine();

            // Draw control points for custom curve
            if (_currentCurveType == "Custom")
            {
                DrawControlPoints();
            }
        }

        private void DrawGrid()
        {
            double width = CurveCanvas.ActualWidth;
            double height = CurveCanvas.ActualHeight;

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

            CurveCanvas.Children.Add(xAxis);
            CurveCanvas.Children.Add(yAxis);

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

                CurveCanvas.Children.Add(lineAbove);
                CurveCanvas.Children.Add(lineBelow);
                CurveCanvas.Children.Add(lineLeft);
                CurveCanvas.Children.Add(lineRight);
            }
        }

        private void DrawCurveLine()
        {
            double width = CurveCanvas.ActualWidth;
            double height = CurveCanvas.ActualHeight;

            // Get curve points
            var curvePoints = _currentProvider.GetCurvePoints(100, _currentParams);

            // Create a new polyline
            _curveLine = new Polyline
            {
                Stroke = _curveBrush,
                StrokeThickness = 2
            };

            // Convert curve points to canvas coordinates
            foreach (var point in curvePoints)
            {
                double x = (point.X + 1.0) / 2.0 * width;
                double y = height - ((point.Y + 1.0) / 2.0 * height);
                _curveLine.Points.Add(new Point(x, y));
            }

            CurveCanvas.Children.Add(_curveLine);
        }

        private void DrawControlPoints()
        {
            if (_currentCurveType != "Custom" || _currentProvider is not CustomCurveProvider customProvider)
                return;

            double width = CurveCanvas.ActualWidth;
            double height = CurveCanvas.ActualHeight;

            // Get control points
            var controlPoints = customProvider.GetControlPoints();
            _controlPoints.Clear();

            // Draw control points
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var point = controlPoints[i];

                // Convert to canvas coordinates
                double x = (point.X + 1.0) / 2.0 * width;
                double y = height - ((point.Y + 1.0) / 2.0 * height);

                // Create control point visual
                var ellipse = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = i == _selectedControlPointIndex ? _controlPointSelectedBrush : _controlPointBrush,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1,
                    Tag = i // Store index for hit testing
                };

                Canvas.SetLeft(ellipse, x - 5);
                Canvas.SetTop(ellipse, y - 5);

                ellipse.MouseLeftButtonDown += ControlPoint_MouseLeftButtonDown;
                ellipse.MouseLeftButtonUp += ControlPoint_MouseLeftButtonUp;
                ellipse.MouseMove += ControlPoint_MouseMove;

                _controlPoints.Add(ellipse);
                CurveCanvas.Children.Add(ellipse);
            }

            // Update bound property
            UpdateControlPointsProperty(controlPoints);
        }

        private void UpdateControlPointsProperty(List<(double X, double Y)> controlPoints)
        {
            var pointList = controlPoints.Select(p => new Point(
                (p.X + 1.0) / 2.0, // Convert from [-1,1] to [0,1]
                (p.Y + 1.0) / 2.0  // Convert from [-1,1] to [0,1]
            )).ToList();

            // Update property without triggering change notification
            ControlPoints = pointList;
        }

        #region Event Handlers

        private static void OnCurveTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveEditor editor)
            {
                editor._currentCurveType = (string)e.NewValue;
                editor.InitializeProviders();
                editor.DrawCurve();
                editor.NotifyCurveChanged();
            }
        }

        private static void OnParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveEditor editor)
            {
                editor.UpdateParameterControls();
                editor.DrawCurve();
                editor.NotifyCurveChanged();
            }
        }

        private static void OnControlPointsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CurveEditor editor && editor._currentCurveType == "Custom" &&
                editor._currentProvider is CustomCurveProvider customProvider)
            {
                var points = e.NewValue as List<Point>;
                if (points != null && points.Count > 0)
                {
                    var controlPoints = points.Select(p => (
                        (p.X * 2.0) - 1.0, // Convert from [0,1] to [-1,1]
                        (p.Y * 2.0) - 1.0  // Convert from [0,1] to [-1,1]
                    )).ToList();

                    customProvider.SetControlPoints(controlPoints);
                    editor.DrawCurve();
                }
            }
        }

        private void Parameter_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender == ExponentSlider)
            {
                Exponent = e.NewValue;
                _currentParams = new double[] { Exponent };
            }
            else if (sender == StrengthSlider)
            {
                Strength = e.NewValue;
                _currentParams = new double[] { Strength, Midpoint };
            }
            else if (sender == MidpointSlider)
            {
                Midpoint = e.NewValue;
                _currentParams = new double[] { Strength, Midpoint };
            }

            DrawCurve();
            NotifyCurveChanged();
        }

        private void CurveCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_currentCurveType != "Custom")
                return;

            Point pos = e.GetPosition(CurveCanvas);

            // Convert to curve space (-1 to 1)
            double x = (pos.X / CurveCanvas.ActualWidth) * 2.0 - 1.0;
            double y = ((CurveCanvas.ActualHeight - pos.Y) / CurveCanvas.ActualHeight) * 2.0 - 1.0;

            // Check if we clicked near an existing control point
            var customProvider = (CustomCurveProvider)_currentProvider;
            var controlPoints = customProvider.GetControlPoints();

            for (int i = 0; i < controlPoints.Count; i++)
            {
                var point = controlPoints[i];
                double distance = Math.Sqrt(Math.Pow(point.X - x, 2) + Math.Pow(point.Y - y, 2));

                if (distance < 0.1) // 10% of the curve space
                {
                    // Select this control point
                    _selectedControlPointIndex = i;
                    _isDragging = true;

                    CurveCanvas.CaptureMouse();
                    e.Handled = true;

                    DrawCurve(); // Redraw to show selection
                    return;
                }
            }

            // If we didn't click a control point, add a new one
            // Don't add points at the edges (those are fixed)
            if (x > -0.95 && x < 0.95)
            {
                var newPoint = (x, y);
                var newPoints = new List<(double X, double Y)>(controlPoints);

                // Insert at the correct position to maintain X-ordering
                int insertIndex = 0;
                while (insertIndex < newPoints.Count && newPoints[insertIndex].X < x)
                {
                    insertIndex++;
                }

                newPoints.Insert(insertIndex, newPoint);
                _selectedControlPointIndex = insertIndex;
                _isDragging = true;

                customProvider.SetControlPoints(newPoints);

                CurveCanvas.CaptureMouse();
                e.Handled = true;

                DrawCurve();
                NotifyCurveChanged();
            }
        }

        private void CurveCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _selectedControlPointIndex = -1;

                CurveCanvas.ReleaseMouseCapture();
                e.Handled = true;

                DrawCurve();
            }
        }

        private void CurveCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _selectedControlPointIndex >= 0 &&
                _currentProvider is CustomCurveProvider customProvider)
            {
                Point pos = e.GetPosition(CurveCanvas);

                // Convert to curve space (-1 to 1)
                double x = (pos.X / CurveCanvas.ActualWidth) * 2.0 - 1.0;
                double y = ((CurveCanvas.ActualHeight - pos.Y) / CurveCanvas.ActualHeight) * 2.0 - 1.0;

                // Clamp values
                x = Math.Max(-1.0, Math.Min(1.0, x));
                y = Math.Max(-1.0, Math.Min(1.0, y));

                // Get current control points
                var controlPoints = customProvider.GetControlPoints();

                // Check if we're trying to move edge points horizontally (not allowed)
                if ((_selectedControlPointIndex == 0 && controlPoints[0].X == -1.0) ||
                    (_selectedControlPointIndex == controlPoints.Count - 1 && controlPoints[controlPoints.Count - 1].X == 1.0))
                {
                    x = controlPoints[_selectedControlPointIndex].X;
                }

                // Check if we're trying to move a point past its neighbors
                if (_selectedControlPointIndex > 0 && x <= controlPoints[_selectedControlPointIndex - 1].X)
                {
                    x = controlPoints[_selectedControlPointIndex - 1].X + 0.01;
                }

                if (_selectedControlPointIndex < controlPoints.Count - 1 && x >= controlPoints[_selectedControlPointIndex + 1].X)
                {
                    x = controlPoints[_selectedControlPointIndex + 1].X - 0.01;
                }

                // Update the control point
                var updatedPoints = new List<(double X, double Y)>(controlPoints);
                updatedPoints[_selectedControlPointIndex] = (x, y);

                customProvider.SetControlPoints(updatedPoints);

                DrawCurve();
                NotifyCurveChanged();

                e.Handled = true;
            }
        }

        private void ControlPoint_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Ellipse ellipse && ellipse.Tag is int index)
            {
                _selectedControlPointIndex = index;
                _isDragging = true;

                ellipse.CaptureMouse();
                e.Handled = true;

                DrawCurve(); // Redraw to show selection
            }
        }

        private void ControlPoint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;

                if (sender is Ellipse ellipse)
                {
                    ellipse.ReleaseMouseCapture();
                }

                e.Handled = true;

                DrawCurve();
            }
        }

        private void ControlPoint_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && sender is Ellipse ellipse && ellipse.Tag is int index &&
                _currentProvider is CustomCurveProvider customProvider)
            {
                Point pos = e.GetPosition(CurveCanvas);

                // Convert to curve space (-1 to 1)
                double x = (pos.X / CurveCanvas.ActualWidth) * 2.0 - 1.0;
                double y = ((CurveCanvas.ActualHeight - pos.Y) / CurveCanvas.ActualHeight) * 2.0 - 1.0;

                // Clamp values
                x = Math.Max(-1.0, Math.Min(1.0, x));
                y = Math.Max(-1.0, Math.Min(1.0, y));

                // Get current control points
                var controlPoints = customProvider.GetControlPoints();

                // Check if we're trying to move edge points horizontally (not allowed)
                if ((index == 0 && controlPoints[0].X == -1.0) ||
                    (index == controlPoints.Count - 1 && controlPoints[controlPoints.Count - 1].X == 1.0))
                {
                    x = controlPoints[index].X;
                }

                // Check if we're trying to move a point past its neighbors
                if (index > 0 && x <= controlPoints[index - 1].X)
                {
                    x = controlPoints[index - 1].X + 0.01;
                }

                if (index < controlPoints.Count - 1 && x >= controlPoints[index + 1].X)
                {
                    x = controlPoints[index + 1].X - 0.01;
                }

                // Update the control point
                var updatedPoints = new List<(double X, double Y)>(controlPoints);
                updatedPoints[index] = (x, y);

                customProvider.SetControlPoints(updatedPoints);

                DrawCurve();
                NotifyCurveChanged();

                e.Handled = true;
            }
        }

        private void ResetCustomCurve_Click(object sender, RoutedEventArgs e)
        {
            if (_currentProvider is CustomCurveProvider customProvider)
            {
                // Reset to linear (5 points)
                var defaultPoints = new List<(double X, double Y)>
                {
                    (-1.0, -1.0),
                    (-0.5, -0.5),
                    (0.0, 0.0),
                    (0.5, 0.5),
                    (1.0, 1.0)
                };

                customProvider.SetControlPoints(defaultPoints);

                DrawCurve();
                NotifyCurveChanged();
            }
        }

        #endregion

        private void NotifyCurveChanged()
        {
            // Execute the command if it exists
            if (CurveChangedCommand != null && CurveChangedCommand.CanExecute(null))
            {
                CurveChangedCommand.Execute(null);
            }
        }

        /// <summary>
        /// Gets the current curve provider
        /// </summary>
        public ISensitivityProvider GetCurrentProvider()
        {
            return _currentProvider;
        }

        /// <summary>
        /// Gets the current curve parameters
        /// </summary>
        public double[] GetCurrentParameters()
        {
            return _currentParams;
        }
    }
}