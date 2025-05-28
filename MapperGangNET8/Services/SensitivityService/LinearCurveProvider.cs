using System;

namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Provides a linear sensitivity curve (no modification)
    /// </summary>
    public class LinearCurveProvider : ISensitivityProvider
    {
        /// <summary>
        /// Gets the name of the sensitivity curve
        /// </summary>
        public string Name => "Linear";

        /// <summary>
        /// Gets the description of the sensitivity curve
        /// </summary>
        public string Description => "Direct 1:1 mapping of input to output with no modification";

        /// <summary>
        /// Processes an input value through a linear curve (no change)
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">Not used for linear curve</param>
        /// <returns>Same as input value</returns>
        public double ProcessValue(double input, params double[] @params)
        {
            // Clamp input to valid range
            return Math.Max(-1.0, Math.Min(1.0, input));
        }

        /// <summary>
        /// Gets points representing the linear curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">Not used for linear curve</param>
        /// <returns>Array of points forming a straight line</returns>
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