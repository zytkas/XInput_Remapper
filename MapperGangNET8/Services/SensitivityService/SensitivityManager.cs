using System;
using System.Collections.Generic;
using System.Linq;

namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Manager for sensitivity curves
    /// </summary>
    public class SensitivityManager
    {
        private readonly Dictionary<string, ISensitivityProvider> _providers;
        private ISensitivityProvider _currentProvider;
        private readonly CustomCurveProvider _customProvider;

        /// <summary>
        /// Constructor that initializes all available providers
        /// </summary>
        public SensitivityManager()
        {
            // Create providers
            var linearProvider = new LinearCurveProvider();
            var exponentialProvider = new ExponentialCurveProvider();
            var sCurveProvider = new SCurveSensitivityProvider();
            _customProvider = new CustomCurveProvider();

            // Register providers
            _providers = new Dictionary<string, ISensitivityProvider>
            {
                { linearProvider.Name, linearProvider },
                { exponentialProvider.Name, exponentialProvider },
                { sCurveProvider.Name, sCurveProvider },
                { _customProvider.Name, _customProvider }
            };

            // Set default provider
            _currentProvider = linearProvider;
        }

        /// <summary>
        /// Gets all available sensitivity curve types
        /// </summary>
        public IEnumerable<string> AvailableCurveTypes => _providers.Keys;

        /// <summary>
        /// Gets the current curve provider
        /// </summary>
        public ISensitivityProvider CurrentProvider => _currentProvider;

        /// <summary>
        /// Gets the name of the current curve provider
        /// </summary>
        public string CurrentCurveType => _currentProvider.Name;

        /// <summary>
        /// Sets the current curve type by name
        /// </summary>
        /// <param name="curveType">Name of the curve type</param>
        /// <returns>True if successful, false if the curve type doesn't exist</returns>
        public bool SetCurveType(string curveType)
        {
            if (_providers.TryGetValue(curveType, out var provider))
            {
                _currentProvider = provider;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Processes an input value through the current curve
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">Optional parameters for the curve</param>
        /// <returns>Processed value with current curve applied</returns>
        public double ProcessValue(double input, params double[] @params)
        {
            return _currentProvider.ProcessValue(input, @params);
        }

        /// <summary>
        /// Gets points representing the current curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">Optional parameters for the curve</param>
        /// <returns>Array of points representing the current curve</returns>
        public (double X, double Y)[] GetCurvePoints(int pointCount, params double[] @params)
        {
            return _currentProvider.GetCurvePoints(pointCount, @params);
        }

        /// <summary>
        /// Sets the control points for the custom curve
        /// </summary>
        /// <param name="controlPoints">Collection of control points</param>
        public void SetCustomCurveControlPoints(IEnumerable<(double X, double Y)> controlPoints)
        {
            _customProvider.SetControlPoints(controlPoints);
        }

        /// <summary>
        /// Gets the current control points for the custom curve
        /// </summary>
        /// <returns>Collection of control points</returns>
        public List<(double X, double Y)> GetCustomCurveControlPoints()
        {
            return _customProvider.GetControlPoints();
        }
    }
}