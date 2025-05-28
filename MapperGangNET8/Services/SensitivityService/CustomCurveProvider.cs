// Services/SensitivityService/CustomCurveProvider.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Provides a custom sensitivity curve with user-defined control points
    /// </summary>
    public class CustomCurveProvider : ISensitivityProvider
    {
        private List<(double X, double Y)> _controlPoints;

        /// <summary>
        /// Constructor with default control points
        /// </summary>
        public CustomCurveProvider()
        {
            // Default to linear curve
            _controlPoints = new List<(double X, double Y)>
            {
                (-1.0, -1.0),
                (-0.5, -0.5),
                (0.0, 0.0),
                (0.5, 0.5),
                (1.0, 1.0)
            };
        }

        /// <summary>
        /// Gets the name of the sensitivity curve
        /// </summary>
        public string Name => "Custom";

        /// <summary>
        /// Gets the description of the sensitivity curve
        /// </summary>
        public string Description => "User-defined custom curve with adjustable control points";

        /// <summary>
        /// Sets the control points for the custom curve
        /// </summary>
        /// <param name="controlPoints">Collection of control points</param>
        public void SetControlPoints(IEnumerable<(double X, double Y)> controlPoints)
        {
            _controlPoints = controlPoints.OrderBy(p => p.X).ToList();

            // Ensure we have at least two control points
            if (_controlPoints.Count < 2)
            {
                _controlPoints = new List<(double X, double Y)> { (-1.0, -1.0), (1.0, 1.0) };
            }

            // Ensure range is from -1 to 1
            if (_controlPoints.First().X > -1.0)
            {
                _controlPoints.Insert(0, (-1.0, _controlPoints.First().Y));
            }

            if (_controlPoints.Last().X < 1.0)
            {
                _controlPoints.Add((1.0, _controlPoints.Last().Y));
            }
        }

        /// <summary>
        /// Gets the current control points
        /// </summary>
        /// <returns>Collection of control points</returns>
        public List<(double X, double Y)> GetControlPoints()
        {
            return new List<(double X, double Y)>(_controlPoints);
        }

        /// <summary>
        /// Processes an input value through the custom curve
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">Not used for custom curve</param>
        /// <returns>Processed value with custom curve applied</returns>
        public double ProcessValue(double input, params double[] @params)
        {
            // Clamp input to valid range
            input = Math.Max(-1.0, Math.Min(1.0, input));

            // Edge cases
            if (input <= _controlPoints.First().X)
                return _controlPoints.First().Y;

            if (input >= _controlPoints.Last().X)
                return _controlPoints.Last().Y;

            // Find the two control points that bound the input value
            int i = 0;
            while (i < _controlPoints.Count - 1 && _controlPoints[i + 1].X < input)
            {
                i++;
            }

            // Linear interpolation between control points
            double x0 = _controlPoints[i].X;
            double y0 = _controlPoints[i].Y;
            double x1 = _controlPoints[i + 1].X;
            double y1 = _controlPoints[i + 1].Y;

            // Calculate the interpolation factor
            double t = (input - x0) / (x1 - x0);

            // Linear interpolation
            return y0 + (y1 - y0) * t;
        }

        /// <summary>
        /// Gets points representing the custom curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">Not used for custom curve</param>
        /// <returns>Array of points forming the custom curve</returns>
        public (double X, double Y)[] GetCurvePoints(int pointCount, params double[] @params)
        {
            var points = new (double X, double Y)[pointCount];
            double step = 2.0 / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double x = -1.0 + (i * step);
                points[i] = (x, ProcessValue(x));
            }

            return points;
        }
    }
}