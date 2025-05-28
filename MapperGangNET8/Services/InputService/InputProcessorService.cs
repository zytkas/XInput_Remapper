using System;
using System.Collections.Generic;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.SensitivityService;

namespace MapperGangNET8.Services.InputService
{
    /// <summary>
    /// Service for processing raw input through various transformations like sensitivity curves
    /// </summary>
    public class InputProcessorService
    {
        private readonly IConfigService _configService;
        private readonly SensitivityManager _sensitivityManager;
        private ConfigModel _currentConfig;
        private ISensitivityProvider _mouseCurveProvider;
        private ISensitivityProvider _joystickCurveProvider;
        private double[] _mouseCurveParams;
        private double[] _joystickCurveParams;

        // Cache for performance
        private double _mouseXSensitivity;
        private double _mouseYSensitivity;
        private double _joystickSensitivity;
        private double _joystickDeadzone;
        private bool _mouseAcceleration;
        private double _mouseSmoothing;
        private bool _mouseAxisLock;
        private bool _joystickRadialDeadzone;

        // For smoothing
        private readonly Queue<(double X, double Y)> _mouseSmoothingBuffer = new Queue<(double X, double Y)>();
        private const int SmoothingBufferSize = 10;

        /// <summary>
        /// Constructor
        /// </summary>
        public InputProcessorService(IConfigService configService)
        {
            _configService = configService;
            _sensitivityManager = new SensitivityManager();

            // Initialize default providers
            _mouseCurveProvider = new LinearCurveProvider();
            _joystickCurveProvider = new LinearCurveProvider();
            _mouseCurveParams = Array.Empty<double>();
            _joystickCurveParams = Array.Empty<double>();

            // Load config asynchronously
            _ = LoadConfigAsync();
        }

        /// <summary>
        /// Load configuration settings
        /// </summary>
        private async System.Threading.Tasks.Task LoadConfigAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();
            UpdateProvidersFromConfig();
        }

        /// <summary>
        /// Update sensitivity providers from loaded config
        /// </summary>
        private void UpdateProvidersFromConfig()
        {
            if (_currentConfig == null) return;

            var settings = _currentConfig.SensitivitySettings;

            // Update mouse settings
            _mouseXSensitivity = settings.MouseXAxisSensitivity / 100.0; // Convert to 0.0-1.0 range
            _mouseYSensitivity = settings.MouseYAxisSensitivity / 100.0;
            _mouseAcceleration = settings.MouseAcceleration;
            _mouseSmoothing = settings.MouseSmoothing / 100.0;
            _mouseAxisLock = settings.MouseAxisLock;

            // Update joystick settings
            _joystickSensitivity = settings.JoystickSensitivity / 100.0;
            _joystickDeadzone = settings.JoystickDeadzone / 100.0;
            _joystickRadialDeadzone = settings.JoystickRadialDeadzone;

            // Update mouse curve
            _sensitivityManager.SetCurveType(settings.MouseResponseCurveType);
            _mouseCurveProvider = _sensitivityManager.CurrentProvider;

            // Set mouse curve parameters
            if (settings.MouseResponseCurveType == "Exponential")
            {
                _mouseCurveParams = new double[] { settings.MouseExponent };
            }
            else if (settings.MouseResponseCurveType == "S-Curve")
            {
                _mouseCurveParams = new double[] { settings.MouseCurveStrength, settings.MouseCurveMidpoint };
            }
            else if (settings.MouseResponseCurveType == "Custom" && settings.MouseCustomCurvePoints != null)
            {
                var points = new List<(double X, double Y)>();

                foreach (var point in settings.MouseCustomCurvePoints)
                {
                    // Convert from [0,1] to [-1,1]
                    points.Add(((point.X * 2.0) - 1.0, (point.Y * 2.0) - 1.0));
                }

                _sensitivityManager.SetCustomCurveControlPoints(points);
            }

            // For simplicity, joystick uses linear curve for now
            // This will be expanded in the future to support different joystick curves
        }

        /// <summary>
        /// Force a refresh of the configuration
        /// </summary>
        public async System.Threading.Tasks.Task RefreshConfigAsync()
        {
            await LoadConfigAsync();
        }

        /// <summary>
        /// Process raw mouse input to apply sensitivity and response curves
        /// </summary>
        /// <param name="deltaX">Raw X movement</param>
        /// <param name="deltaY">Raw Y movement</param>
        /// <param name="dt">Time since last input (seconds)</param>
        /// <returns>Processed mouse movement</returns>
        public (double X, double Y) ProcessMouseInput(double deltaX, double deltaY, double dt)
        {
            // Scale by sensitivity
            double x = deltaX * _mouseXSensitivity;
            double y = deltaY * _mouseYSensitivity;

            // Apply axis lock if enabled
            if (_mouseAxisLock && Math.Abs(x) > 0 && Math.Abs(y) > 0)
            {
                // Only keep the larger axis movement
                if (Math.Abs(x) > Math.Abs(y))
                {
                    y = 0;
                }
                else
                {
                    x = 0;
                }
            }

            // Normalize to [-1, 1] range
            double maxDelta = 100.0; // Adjust based on expected maximum movement
            x = Math.Max(-1.0, Math.Min(1.0, x / maxDelta));
            y = Math.Max(-1.0, Math.Min(1.0, y / maxDelta));

            // Apply mouse acceleration if enabled
            if (_mouseAcceleration)
            {
                double speed = Math.Sqrt(x * x + y * y);
                double multiplier = speed > 0.1 ? 1.0 + (speed - 0.1) * 2.0 : 1.0;

                x *= multiplier;
                y *= multiplier;

                // Re-clamp to [-1, 1] after acceleration
                x = Math.Max(-1.0, Math.Min(1.0, x));
                y = Math.Max(-1.0, Math.Min(1.0, y));
            }

            // Apply response curve
            x = _mouseCurveProvider.ProcessValue(x, _mouseCurveParams);
            y = _mouseCurveProvider.ProcessValue(y, _mouseCurveParams);

            // Apply smoothing if enabled
            if (_mouseSmoothing > 0)
            {
                // Add current point to buffer
                _mouseSmoothingBuffer.Enqueue((x, y));

                // Keep buffer at max size
                while (_mouseSmoothingBuffer.Count > SmoothingBufferSize)
                {
                    _mouseSmoothingBuffer.Dequeue();
                }

                // Calculate weighted average
                double sumX = 0;
                double sumY = 0;
                double sumWeights = 0;
                double weight = 1.0;
                double decay = 0.8; // Lower values = more smoothing

                foreach (var point in _mouseSmoothingBuffer)
                {
                    sumX += point.X * weight;
                    sumY += point.Y * weight;
                    sumWeights += weight;
                    weight *= decay;
                }

                // Apply smoothing based on smoothing amount
                x = x * (1.0 - _mouseSmoothing) + (sumX / sumWeights) * _mouseSmoothing;
                y = y * (1.0 - _mouseSmoothing) + (sumY / sumWeights) * _mouseSmoothing;
            }

            // Scale back to actual movement
            x *= maxDelta;
            y *= maxDelta;

            return (x, y);
        }

        /// <summary>
        /// Process raw joystick input to apply sensitivity, deadzone, and response curves
        /// </summary>
        /// <param name="x">Raw X position (-1.0 to 1.0)</param>
        /// <param name="y">Raw Y position (-1.0 to 1.0)</param>
        /// <returns>Processed joystick position</returns>
        public (double X, double Y) ProcessJoystickInput(double x, double y)
        {
            // Ensure input is in valid range
            x = Math.Max(-1.0, Math.Min(1.0, x));
            y = Math.Max(-1.0, Math.Min(1.0, y));

            // Apply deadzone
            if (_joystickDeadzone > 0)
            {
                // Radial deadzone
                if (_joystickRadialDeadzone)
                {
                    double magnitude = Math.Sqrt(x * x + y * y);

                    if (magnitude < _joystickDeadzone)
                    {
                        return (0, 0);
                    }
                    else
                    {
                        // Rescale values outside deadzone to fill 0-1 range
                        double scale = Math.Min(1.0, (magnitude - _joystickDeadzone) / (1.0 - _joystickDeadzone));
                        double angle = Math.Atan2(y, x);

                        x = Math.Cos(angle) * scale;
                        y = Math.Sin(angle) * scale;
                    }
                }
                // Rectangular deadzone (per-axis)
                else
                {
                    if (Math.Abs(x) < _joystickDeadzone)
                    {
                        x = 0;
                    }
                    else
                    {
                        // Rescale values outside deadzone
                        double sign = Math.Sign(x);
                        x = sign * (Math.Abs(x) - _joystickDeadzone) / (1.0 - _joystickDeadzone);
                    }

                    if (Math.Abs(y) < _joystickDeadzone)
                    {
                        y = 0;
                    }
                    else
                    {
                        // Rescale values outside deadzone
                        double sign = Math.Sign(y);
                        y = sign * (Math.Abs(y) - _joystickDeadzone) / (1.0 - _joystickDeadzone);
                    }
                }
            }

            // Apply response curve (basic linear curve for now)
            // This will be expanded in the future to support different joystick curves

            // Apply sensitivity
            x *= _joystickSensitivity;
            y *= _joystickSensitivity;

            // Ensure output is in valid range
            x = Math.Max(-1.0, Math.Min(1.0, x));
            y = Math.Max(-1.0, Math.Min(1.0, y));

            return (x, y);
        }
    }
}