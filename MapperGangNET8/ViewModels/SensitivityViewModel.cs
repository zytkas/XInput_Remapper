using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using MapperGangNET8.Infrastructure.Commands;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigResetService;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.SensitivityService;
using System.Collections.Generic;
using System.Windows;
using System;
using System.Linq;

namespace MapperGangNET8.ViewModels
{
    /// <summary>
    /// ViewModel for sensitivity settings tab
    /// </summary>
    public class SensitivityViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly SensitivityManager _sensitivityManager;
        private ConfigModel _currentConfig;

        #region Private fields
        private double _mouseXAxisSensitivity;
        private double _mouseYAxisSensitivity;
        private string _mouseResponseCurveType;
        private bool _mouseAcceleration;
        private double _mouseSmoothing;
        private bool _mouseAxisLock;

        private double _joystickSensitivity;
        private double _joystickDeadzone;
        private string _joystickResponseCurveType;
        private bool _joystickAntiDeadzone;
        private bool _joystickRotation;
        private bool _joystickRadialDeadzone;

        private double _mouseExponent;
        private double _mouseCurveStrength;
        private double _mouseCurveMidpoint;

        private List<System.Windows.Point> _mouseCustomCurvePoints;

        private bool _isEditingCurve;
        private string _editingCurveType; // "Mouse" or "Joystick"
        #endregion

        #region Public properties
        /// <summary>
        /// Mouse X-axis sensitivity (0-100%)
        /// </summary>
        public double MouseXAxisSensitivity
        {
            get => _mouseXAxisSensitivity;
            set
            {
                if (SetProperty(ref _mouseXAxisSensitivity, value))
                {
                    OnPropertyChanged(nameof(MouseSensitivityOverall));

                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseXAxisSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse Y-axis sensitivity (0-100%)
        /// </summary>
        public double MouseYAxisSensitivity
        {
            get => _mouseYAxisSensitivity;
            set
            {
                if (SetProperty(ref _mouseYAxisSensitivity, value))
                {
                    OnPropertyChanged(nameof(MouseSensitivityOverall));

                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseYAxisSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Overall mouse sensitivity (average of X and Y)
        /// </summary>
        public double MouseSensitivityOverall => (_mouseXAxisSensitivity + _mouseYAxisSensitivity) / 2;

        /// <summary>
        /// Mouse response curve type (Linear, S-Curve, Custom)
        /// </summary>
        public string MouseResponseCurveType
        {
            get => _mouseResponseCurveType;
            set
            {
                if (SetProperty(ref _mouseResponseCurveType, value))
                {
                    OnPropertyChanged(nameof(MouseLinearCurveAppearance));
                    OnPropertyChanged(nameof(MouseSCurveAppearance));
                    OnPropertyChanged(nameof(MouseCustomCurveAppearance));
                    OnPropertyChanged(nameof(MouseExponentialCurveAppearance));

                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseResponseCurveType = value;

                        // Update the sensitivity manager
                        _sensitivityManager.SetCurveType(value);

                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Appearance for Linear curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance MouseLinearCurveAppearance =>
            _mouseResponseCurveType == "Linear" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Appearance for S-Curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance MouseSCurveAppearance =>
            _mouseResponseCurveType == "S-Curve" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Appearance for Exponential curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance MouseExponentialCurveAppearance =>
            _mouseResponseCurveType == "Exponential" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Appearance for Custom curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance MouseCustomCurveAppearance =>
            _mouseResponseCurveType == "Custom" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Mouse acceleration enabled
        /// </summary>
        public bool MouseAcceleration
        {
            get => _mouseAcceleration;
            set
            {
                if (SetProperty(ref _mouseAcceleration, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseAcceleration = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse smoothing amount (0-100%)
        /// </summary>
        public double MouseSmoothing
        {
            get => _mouseSmoothing;
            set
            {
                if (SetProperty(ref _mouseSmoothing, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseSmoothing = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse smoothing enabled
        /// </summary>
        public bool MouseSmoothingEnabled
        {
            get => _mouseSmoothing > 0;
            set
            {
                MouseSmoothing = value ? 30 : 0; // Use 30% by default when enabling
            }
        }

        /// <summary>
        /// Mouse axis lock (restrict to one axis)
        /// </summary>
        public bool MouseAxisLock
        {
            get => _mouseAxisLock;
            set
            {
                if (SetProperty(ref _mouseAxisLock, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseAxisLock = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Joystick sensitivity (0-100%)
        /// </summary>
        public double JoystickSensitivity
        {
            get => _joystickSensitivity;
            set
            {
                if (SetProperty(ref _joystickSensitivity, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Joystick deadzone (0-100%)
        /// </summary>
        public double JoystickDeadzone
        {
            get => _joystickDeadzone;
            set
            {
                if (SetProperty(ref _joystickDeadzone, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Joystick response curve type (Linear, Step, Custom)
        /// </summary>
        public string JoystickResponseCurveType
        {
            get => _joystickResponseCurveType;
            set
            {
                if (SetProperty(ref _joystickResponseCurveType, value))
                {
                    OnPropertyChanged(nameof(JoystickLinearCurveAppearance));
                    OnPropertyChanged(nameof(JoystickStepCurveAppearance));
                    OnPropertyChanged(nameof(JoystickCustomCurveAppearance));

                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickResponseCurveType = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Appearance for Linear joystick curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance JoystickLinearCurveAppearance =>
            _joystickResponseCurveType == "Linear" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Appearance for Step joystick curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance JoystickStepCurveAppearance =>
            _joystickResponseCurveType == "Step" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Appearance for Custom joystick curve button
        /// </summary>
        public Wpf.Ui.Controls.ControlAppearance JoystickCustomCurveAppearance =>
            _joystickResponseCurveType == "Custom" ? Wpf.Ui.Controls.ControlAppearance.Primary : Wpf.Ui.Controls.ControlAppearance.Secondary;

        /// <summary>
        /// Joystick anti-deadzone
        /// </summary>
        public bool JoystickAntiDeadzone
        {
            get => _joystickAntiDeadzone;
            set
            {
                if (SetProperty(ref _joystickAntiDeadzone, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickAntiDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Joystick input rotation
        /// </summary>
        public bool JoystickRotation
        {
            get => _joystickRotation;
            set
            {
                if (SetProperty(ref _joystickRotation, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickRotation = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Joystick radial deadzone
        /// </summary>
        public bool JoystickRadialDeadzone
        {
            get => _joystickRadialDeadzone;
            set
            {
                if (SetProperty(ref _joystickRadialDeadzone, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickRadialDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse exponent for exponential curve
        /// </summary>
        public double MouseExponent
        {
            get => _mouseExponent;
            set
            {
                if (SetProperty(ref _mouseExponent, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseExponent = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse curve strength for S-curve
        /// </summary>
        public double MouseCurveStrength
        {
            get => _mouseCurveStrength;
            set
            {
                if (SetProperty(ref _mouseCurveStrength, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseCurveStrength = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse curve midpoint for S-curve
        /// </summary>
        public double MouseCurveMidpoint
        {
            get => _mouseCurveMidpoint;
            set
            {
                if (SetProperty(ref _mouseCurveMidpoint, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseCurveMidpoint = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Mouse custom curve control points
        /// </summary>
        public List<System.Windows.Point> MouseCustomCurvePoints
        {
            get => _mouseCustomCurvePoints;
            set
            {
                if (SetProperty(ref _mouseCustomCurvePoints, value))
                {
                    if (_currentConfig != null)
                    {
                        // Convert to serializable format
                        _currentConfig.SensitivitySettings.MouseCustomCurvePoints =
                            value.Select(p => new PointModel { X = p.X, Y = p.Y }).ToList();
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Whether we're currently editing a curve
        /// </summary>
        public bool IsEditingCurve
        {
            get => _isEditingCurve;
            set => SetProperty(ref _isEditingCurve, value);
        }

        /// <summary>
        /// Type of curve being edited ("Mouse" or "Joystick")
        /// </summary>
        public string EditingCurveType
        {
            get => _editingCurveType;
            set => SetProperty(ref _editingCurveType, value);
        }

        /// <summary>
        /// Available mouse response curve types
        /// </summary>
        public ObservableCollection<string> AvailableMouseCurveTypes { get; } = new ObservableCollection<string>
        {
            "Linear",
            "Exponential",
            "S-Curve",
            "Custom"
        };

        /// <summary>
        /// Available joystick response curve types
        /// </summary>
        public ObservableCollection<string> AvailableJoystickCurveTypes { get; } = new ObservableCollection<string>
        {
            "Linear",
            "Step",
            "Custom"
        };
        #endregion

        #region Commands
        /// <summary>
        /// Command to edit mouse curve
        /// </summary>
        public ICommand EditMouseCurveCommand { get; set; }

        /// <summary>
        /// Command to edit joystick curve
        /// </summary>
        public ICommand EditJoystickCurveCommand { get; set; }

        /// <summary>
        /// Command to select mouse curve preset
        /// </summary>
        public ICommand SelectMouseCurvePresetCommand { get; }

        /// <summary>
        /// Command to select joystick curve preset
        /// </summary>
        public ICommand SelectJoystickCurvePresetCommand { get; }

        /// <summary>
        /// Command to reset to default settings
        /// </summary>
        public ICommand ResetToDefaultsCommand { get; }

        /// <summary>
        /// Command to save settings
        /// </summary>
        public ICommand SaveMappingsCommand { get; }

        /// <summary>
        /// Command for handling curve changes
        /// </summary>
        public ICommand CurveChangedCommand { get; }

        /// <summary>
        /// Command to close curve editor
        /// </summary>
        public ICommand CloseCurveEditorCommand { get; }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SensitivityViewModel(IConfigService configService, IConfigResetService resetService)
        {
            _configService = configService;
            resetService.ConfigurationReset += async (sender, e) => await LoadSettingsAsync();

            // Initialize sensitivity manager
            _sensitivityManager = new SensitivityManager();

            // Initialize commands
            EditMouseCurveCommand = new RelayCommand(_ => OnEditMouseCurve());
            EditJoystickCurveCommand = new RelayCommand(_ => OnEditJoystickCurve());
            SelectMouseCurvePresetCommand = new RelayCommand(OnSelectMouseCurvePreset);
            SelectJoystickCurvePresetCommand = new RelayCommand(OnSelectJoystickCurvePreset);
            ResetToDefaultsCommand = new RelayCommand(async _ => await OnResetToDefaults());
            SaveMappingsCommand = new RelayCommand(async _ => await OnSaveSettings());
            CurveChangedCommand = new RelayCommand(_ => OnCurveChanged());
            CloseCurveEditorCommand = new RelayCommand(_ => OnCloseCurveEditor());

            // Initialize defaults
            _mouseExponent = 2.0;
            _mouseCurveStrength = 0.5;
            _mouseCurveMidpoint = 0.5;
            _mouseCustomCurvePoints = new List<System.Windows.Point>
            {
                new System.Windows.Point(0, 0),
                new System.Windows.Point(0.25, 0.25),
                new System.Windows.Point(0.5, 0.5),
                new System.Windows.Point(0.75, 0.75),
                new System.Windows.Point(1, 1)
            };

            // Load settings
            _ = LoadSettingsAsync();
        }

        /// <summary>
        /// Load settings from config service
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();
            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Update properties from loaded config
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            if (_currentConfig == null) return;

            var sensitivitySettings = _currentConfig.SensitivitySettings;

            // Update mouse settings
            MouseXAxisSensitivity = sensitivitySettings.MouseXAxisSensitivity;
            MouseYAxisSensitivity = sensitivitySettings.MouseYAxisSensitivity;
            MouseResponseCurveType = sensitivitySettings.MouseResponseCurveType;
            MouseAcceleration = sensitivitySettings.MouseAcceleration;
            MouseSmoothing = sensitivitySettings.MouseSmoothing;
            MouseAxisLock = sensitivitySettings.MouseAxisLock;

            // Update mouse curve parameters
            MouseExponent = sensitivitySettings.MouseExponent;
            MouseCurveStrength = sensitivitySettings.MouseCurveStrength;
            MouseCurveMidpoint = sensitivitySettings.MouseCurveMidpoint;

            // Update custom curve points
            if (sensitivitySettings.MouseCustomCurvePoints != null && sensitivitySettings.MouseCustomCurvePoints.Count > 0)
            {
                MouseCustomCurvePoints = sensitivitySettings.MouseCustomCurvePoints
                    .Select(p => new System.Windows.Point(p.X, p.Y))
                    .ToList();

                // Update the custom curve provider with these points
                if (_sensitivityManager.CurrentCurveType == "Custom")
                {
                    var controlPoints = MouseCustomCurvePoints
                        .Select(p => ((p.X * 2.0) - 1.0, (p.Y * 2.0) - 1.0))
                        .ToList();

                    _sensitivityManager.SetCustomCurveControlPoints(controlPoints);
                }
            }

            // Update joystick settings
            JoystickSensitivity = sensitivitySettings.JoystickSensitivity;
            JoystickDeadzone = sensitivitySettings.JoystickDeadzone;
            JoystickResponseCurveType = sensitivitySettings.JoystickResponseCurveType;
            JoystickAntiDeadzone = sensitivitySettings.JoystickAntiDeadzone;
            JoystickRotation = sensitivitySettings.JoystickRotation;
            JoystickRadialDeadzone = sensitivitySettings.JoystickRadialDeadzone;

            // Set the active curve type in the sensitivity manager
            _sensitivityManager.SetCurveType(MouseResponseCurveType);
        }

        #region Command handlers
        /// <summary>
        /// Handle Edit Mouse Curve command
        /// </summary>
        private void OnEditMouseCurve()
        {
            IsEditingCurve = true;
            EditingCurveType = "Mouse";
        }

        /// <summary>
        /// Handle Edit Joystick Curve command
        /// </summary>
        private void OnEditJoystickCurve()
        {
            IsEditingCurve = true;
            EditingCurveType = "Joystick";
        }

        /// <summary>
        /// Handle Select Mouse Curve Preset command
        /// </summary>
        private void OnSelectMouseCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                MouseResponseCurveType = presetType;
            }
        }

        /// <summary>
        /// Handle Select Joystick Curve Preset command
        /// </summary>
        private void OnSelectJoystickCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                JoystickResponseCurveType = presetType;
            }
        }

        /// <summary>
        /// Handle Reset to Defaults command
        /// </summary>
        private async Task OnResetToDefaults()
        {
            MessageBoxResult result = MessageBox.Show(
                "Reset sensitivity settings to default values?",
                "Reset Settings",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Create new sensitivity settings with default values
                SensitivitySettingsModel defaultSettings = new SensitivitySettingsModel();

                // Update current config
                if (_currentConfig != null)
                {
                    _currentConfig.SensitivitySettings = defaultSettings;
                    await SaveSettingsAsync();
                }

                // Update properties
                UpdatePropertiesFromConfig();

                MessageBox.Show("Sensitivity settings reset to default values.",
                              "Reset Settings",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handle Save Settings command
        /// </summary>
        private async Task OnSaveSettings()
        {
            if (_currentConfig != null)
            {
                await SaveSettingsAsync();

                MessageBox.Show("Sensitivity settings saved successfully.",
                              "Save Settings",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Handle Curve Changed command
        /// </summary>
        private void OnCurveChanged()
        {
            // This is called when a curve is modified in the editor
            SaveSettingsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Handle Close Curve Editor command
        /// </summary>
        private void OnCloseCurveEditor()
        {
            IsEditingCurve = false;
            EditingCurveType = null;
        }
        #endregion

        /// <summary>
        /// Save settings to config service
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            if (_currentConfig == null) return;

            // Save the configuration
            await _configService.SaveConfigAsync(_currentConfig);
        }

        /// <summary>
        /// Process an input value through the current sensitivity curve
        /// </summary>
        /// <param name="input">Input value (-1 to 1)</param>
        /// <param name="isMouse">Whether this is for mouse (true) or joystick (false)</param>
        /// <returns>Processed value (-1 to 1)</returns>
        public double ProcessInput(double input, bool isMouse = true)
        {
            if (isMouse)
            {
                // For mouse, use the sensitivity manager
                double[] parameters = GetCurveParameters(MouseResponseCurveType);
                return _sensitivityManager.ProcessValue(input, parameters);
            }
            else
            {
                // For joystick, we'll implement this later
                // For now, just pass through the input
                return input;
            }
        }

        /// <summary>
        /// Get the parameters for a specific curve type
        /// </summary>
        private double[] GetCurveParameters(string curveType)
        {
            switch (curveType)
            {
                case "Exponential":
                    return new double[] { MouseExponent };

                case "S-Curve":
                    return new double[] { MouseCurveStrength, MouseCurveMidpoint };

                default:
                    return Array.Empty<double>();
            }
        }
    }
}