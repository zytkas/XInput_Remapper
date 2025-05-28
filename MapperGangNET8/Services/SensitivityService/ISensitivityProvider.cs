namespace MapperGangNET8.Services.SensitivityService
{
    /// <summary>
    /// Interface for sensitivity curve providers
    /// </summary>
    public interface ISensitivityProvider
    {
        /// <summary>
        /// Gets the name of the sensitivity curve
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the sensitivity curve
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Processes an input value through the sensitivity curve
        /// </summary>
        /// <param name="input">Raw input value in range [-1.0, 1.0]</param>
        /// <param name="params">Optional parameters for the curve</param>
        /// <returns>Processed value in range [-1.0, 1.0]</returns>
        double ProcessValue(double input, params double[] @params);

        /// <summary>
        /// Gets a collection of points representing the curve for visualization
        /// </summary>
        /// <param name="pointCount">Number of points to generate</param>
        /// <param name="params">Optional parameters for the curve</param>
        /// <returns>Array of points where X is input and Y is output</returns>
        (double X, double Y)[] GetCurvePoints(int pointCount, params double[] @params);
    }
}