using System;

namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Provides an S-shaped sensitivity curve
    /// </summary>
    public class SCurveSensitivityProvider : ISensitivityProvider
    {
        /// <summary>
        /// Gets the name of the sensitivity curve
        /// </summary>
        public string Name => "S-Curve";

        /// <summary>
        /// Gets the description of the sensitivity curve
        /// </summary>
        public string Description => "S-shaped curve with reduced sensitivity for small movements and increased sensitivity for large movements";

        /// <summary>
        /// Processes an input value through an S-shaped curve
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">
        /// Optional parameters for the curve:
        /// params[0] = strength (0.0-1.0, default: 0.5)
        /// params[1] = midpoint (0.0-1.0, default: 0.5)
        /// </param>
        /// <returns>Processed value with S-curve applied</returns>
        public double ProcessValue(double input, params double[] @params)
        {
            // Get parameters (defaults if not provided)
            double strength = @params != null && @params.Length > 0 ? @params[0] : 0.5;
            double midpoint = @params != null && @params.Length > 1 ? @params[1] : 0.5;

            // Clamp parameters to valid ranges
            strength = Math.Max(0.0, Math.Min(1.0, strength));
            midpoint = Math.Max(0.1, Math.Min(0.9, midpoint));

            // Handle the sign separately
            double sign = Math.Sign(input);
            double absInput = Math.Abs(input);

            // Apply sigmoid function: 1 / (1 + e^(-k(x - midpoint)))
            double k = 8.0 * strength; // Adjusts the steepness of the curve
            double output = 1.0 / (1.0 + Math.Exp(-k * (absInput - midpoint)));

            // Scale output from [0,1] to [0,1]
            double scale = 1.0 / (1.0 / (1.0 + Math.Exp(-k * (1.0 - midpoint))) -
                                 1.0 / (1.0 + Math.Exp(-k * (0.0 - midpoint))));
            double offset = 1.0 / (1.0 + Math.Exp(-k * (0.0 - midpoint)));
            output = (output - offset) * scale;

            // Reapply the sign and clamp to valid range
            return Math.Max(-1.0, Math.Min(1.0, sign * output));
        }

        /// <summary>
        /// Gets points representing the S-curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">
        /// Optional parameters for the curve:
        /// params[0] = strength (0.0-1.0, default: 0.5)
        /// params[1] = midpoint (0.0-1.0, default: 0.5)
        /// </param>
        /// <returns>Array of points forming an S-curve</returns>
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