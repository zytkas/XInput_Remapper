// Services/SensitivityService/ExponentialCurveProvider.cs
using System;

namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Provides an exponential sensitivity curve
    /// </summary>
    public class ExponentialCurveProvider : ISensitivityProvider
    {
        /// <summary>
        /// Gets the name of the sensitivity curve
        /// </summary>
        public string Name => "Exponential";

        /// <summary>
        /// Gets the description of the sensitivity curve
        /// </summary>
        public string Description => "Exponential curve that increases sensitivity at higher input values";

        /// <summary>
        /// Processes an input value through an exponential curve
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">
        /// Optional parameters for the curve:
        /// params[0] = exponent (default: 2.0)
        /// </param>
        /// <returns>Processed value with exponential curve applied</returns>
        public double ProcessValue(double input, params double[] @params)
        {
            // Get the exponent parameter (default to 2.0 if not provided)
            double exponent = @params != null && @params.Length > 0 ? @params[0] : 2.0;

            // Ensure exponent is valid (minimum 1.0 for exponential curve)
            exponent = Math.Max(1.0, exponent);

            // Handle the sign separately
            double sign = Math.Sign(input);
            double absInput = Math.Abs(input);

            // Apply the exponential function
            double output = Math.Pow(absInput, exponent);

            // Reapply the sign and clamp to valid range
            return Math.Max(-1.0, Math.Min(1.0, sign * output));
        }

        /// <summary>
        /// Gets points representing the exponential curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">
        /// Optional parameters for the curve:
        /// params[0] = exponent (default: 2.0)
        /// </param>
        /// <returns>Array of points forming an exponential curve</returns>
        public (double X, double Y)[] GetCurvePoints(int pointCount, params double[] @params)
        {
            var points = new (double X, double Y)[pointCount];
            double step = 2.0 / (pointCount - 1);

            for (int i = 0; i < pointCount; i++)
            {
                double x = -1.0 + (i * step);
                points[i] = (x, ProcessValue(x, @params));
            }

            return points;
        }
    }
}