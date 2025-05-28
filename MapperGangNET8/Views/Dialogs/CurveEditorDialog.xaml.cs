using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MapperGangNET8.Services.SensitivityService;
using Wpf.Ui.Controls;

namespace MapperGangNET8.Views.Dialogs
{   
    public partial class CurveEditorDialog : FluentWindow
    {
        public string CurveType { get; set; }
        public double Exponent { get; set; }
        public double Strength { get; set; }
        public double Midpoint { get; set; }
        public List<Point> ControlPoints { get; set; }
        public ICommand CurveChangedCommand { get; set; }
        public ObservableCollection<string> AvailableCurveTypes { get; set; }

        private bool _isMouseCurve;
        private bool _dialogResult = false;

        /// <summary>
        /// Constructor for the curve editor dialog
        /// </summary>
        /// <param name="title">Dialog title</param>
        /// <param name="curveType">Current curve type</param>
        /// <param name="exponent">Exponent for exponential curve</param>
        /// <param name="strength">Strength for S-curve</param>
        /// <param name="midpoint">Midpoint for S-curve</param>
        /// <param name="controlPoints">Control points for custom curve</param>
        /// <param name="curveChangedCommand">Command to execute when curve changes</param>
        /// <param name="isMouseCurve">Whether this is for mouse or joystick</param>
        public CurveEditorDialog(
            string title,
            string curveType,
            double exponent,
            double strength,
            double midpoint,
            List<Point> controlPoints,
            ICommand curveChangedCommand,
            bool isMouseCurve = true)
        {
            InitializeComponent();

            // Set properties
            HeaderTextBlock.Text = title;
            CurveType = curveType;
            Exponent = exponent;
            Strength = strength;
            Midpoint = midpoint;
            ControlPoints = controlPoints ?? new List<Point>();
            CurveChangedCommand = curveChangedCommand;
            _isMouseCurve = isMouseCurve;

            // Create and initialize AvailableCurveTypes BEFORE setting DataContext
            AvailableCurveTypes = new ObservableCollection<string>();
            if (isMouseCurve)
            {
                AvailableCurveTypes.Add("Linear");
                AvailableCurveTypes.Add("Exponential");
                AvailableCurveTypes.Add("S-Curve");
                AvailableCurveTypes.Add("Custom");
            }
            else
            {
                AvailableCurveTypes.Add("Linear");
                AvailableCurveTypes.Add("Step");
                AvailableCurveTypes.Add("Custom");
            }

            // CRITICAL: Set DataContext AFTER initializing all properties
            CurveEditor.DataContext = this;

            // Log to debug
            System.Diagnostics.Debug.WriteLine("CurveEditorDialog initialized with DataContext set");
        }

        /// <summary>
        /// Get the current curve provider from the editor
        /// </summary>
        public ISensitivityProvider GetCurveProvider()
        {
            return CurveEditor.GetCurrentProvider();
        }

        /// <summary>
        /// Get the current curve parameters from the editor
        /// </summary>
        public double[] GetCurveParameters()
        {
            return CurveEditor.GetCurrentParameters();
        }

        /// <summary>
        /// Get the dialog result
        /// </summary>
        public bool GetDialogResult()
        {
            return _dialogResult;
        }

        private void CurveTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Update the curve type
            CurveType = CurveTypeComboBox.SelectedItem as string;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Apply clicked, dialog result will be true");
            _dialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Cancel clicked, dialog result will be false");
            _dialogResult = false;
            Close();
        }
    }
}