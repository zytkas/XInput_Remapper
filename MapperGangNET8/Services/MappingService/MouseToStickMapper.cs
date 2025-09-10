using System;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Maps mouse movement to controller right stick using Raw Input deltas
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;

        // Stick position accumulator
        private double _rightStickX = 0.0;
        private double _rightStickY = 0.0;

        // Settings
        private double _mouseSensitivity = 1.0;
        private bool _enableStickDecay = true;
        private double _stickDecayRate = 0.92; // Balanced decay rate

        // Smoothing for anti-jitter
        private double _smoothedStickX = 0.0;
        private double _smoothedStickY = 0.0;
        private const double SMOOTHING_FACTOR = 0.6; // 0.0 = no smoothing, 1.0 = maximum smoothing

        // Tracking
        private long _lastMouseMoveTime = 0;
        private const long DECAY_START_DELAY_MS = 80; // Slightly more time before decay starts

        public MouseToStickMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;
        }

        /// <summary>
        /// Process raw mouse delta and map to right stick
        /// </summary>
        public void ProcessMouseDelta(int deltaX, int deltaY)
        {
            // Skip if no movement
            if (deltaX == 0 && deltaY == 0) return;

            // Update last move time
            _lastMouseMoveTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // Apply sensitivity scaling with smoothing
            double rawX = (deltaX * _mouseSensitivity) / 60.0; // More conservative divisor
            double rawY = -(deltaY * _mouseSensitivity) / 60.0; // Invert Y for standard camera controls

            // Apply non-linear sensitivity curve for better feel
            double targetX = Math.Clamp(ApplySensitivityCurve(rawX), -1.0, 1.0);
            double targetY = Math.Clamp(ApplySensitivityCurve(rawY), -1.0, 1.0);

            // Apply exponential smoothing to reduce jitter
            _smoothedStickX = _smoothedStickX * SMOOTHING_FACTOR + targetX * (1.0 - SMOOTHING_FACTOR);
            _smoothedStickY = _smoothedStickY * SMOOTHING_FACTOR + targetY * (1.0 - SMOOTHING_FACTOR);

            // Apply smoothed values to controller
            _controllerService.SetAxis(ControllerAxis.RightThumbX, _smoothedStickX);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, _smoothedStickY);

            // Update internal state for decay
            _rightStickX = _smoothedStickX;
            _rightStickY = _smoothedStickY;

            System.Diagnostics.Debug.WriteLine($"[MOUSE MAPPER] Delta({deltaX},{deltaY}) -> Raw({rawX:F3},{rawY:F3}) -> Target({targetX:F3},{targetY:F3}) -> Smoothed({_smoothedStickX:F3},{_smoothedStickY:F3})");
        }

        /// <summary>
        /// Update stick decay - gradually return stick to center when no input
        /// </summary>
        public void UpdateStickDecay()
        {
            if (!_enableStickDecay) return;

            // Check if enough time has passed since last movement
            long timeSinceMove = DateTimeOffset.Now.ToUnixTimeMilliseconds() - _lastMouseMoveTime;

            if (timeSinceMove > DECAY_START_DELAY_MS)
            {
                // Gradual decay that gets faster over time for smooth feel
                if (timeSinceMove > 200) // After 200ms, faster decay
                {
                    _rightStickX *= 0.85;
                    _rightStickY *= 0.85;
                }
                else
                {
                    // Normal decay for first 120ms
                    _rightStickX *= _stickDecayRate;
                    _rightStickY *= _stickDecayRate;
                }

                // Snap to zero if very small to prevent drift
                if (Math.Abs(_rightStickX) < 0.03) _rightStickX = 0;
                if (Math.Abs(_rightStickY) < 0.03) _rightStickY = 0;

                // Update controller
                _controllerService.SetAxis(ControllerAxis.RightThumbX, _rightStickX);
                _controllerService.SetAxis(ControllerAxis.RightThumbY, _rightStickY);

                System.Diagnostics.Debug.WriteLine($"[MOUSE MAPPER] Decay: Stick({_rightStickX:F3},{_rightStickY:F3}) after {timeSinceMove}ms");
            }
        }

        /// <summary>
        /// Apply non-linear sensitivity curve for better mouse feel
        /// </summary>
        private double ApplySensitivityCurve(double input)
        {
            // Dead zone for very small movements
            if (Math.Abs(input) < 0.005)
                return 0.0;

            // Non-linear curve: slower for small movements, faster for large movements
            // Use a power curve for more natural feel
            double sign = Math.Sign(input);
            double absInput = Math.Abs(input);
            
            // Apply softer power curve (power of 1.1 for smoother large movements)
            double curved = Math.Pow(absInput, 1.1) * sign;
            
            return curved;
        }

        /// <summary>
        /// Reset stick position to center
        /// </summary>
        public void Reset()
        {
            _rightStickX = 0;
            _rightStickY = 0;
            _lastMouseMoveTime = 0;

            _controllerService.SetAxis(ControllerAxis.RightThumbX, 0);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, 0);

            System.Diagnostics.Debug.WriteLine("[MOUSE MAPPER] Reset stick to center");
        }

        /// <summary>
        /// Update configuration
        /// </summary>
        public void UpdateConfiguration(ConfigModel config)
        {
            if (config?.MouseSettings != null)
            {
                // Update sensitivity (convert from percentage if needed)
                _mouseSensitivity = config.MouseSettings.MouseSensitivity / 100.0;

                // Decay settings can be configured here
                _enableStickDecay = true; // Or from config
                _stickDecayRate = 0.85; // Or from config

                System.Diagnostics.Debug.WriteLine($"[MOUSE MAPPER] Config updated - Sensitivity: {_mouseSensitivity:F2}");
            }
        }

        /// <summary>
        /// Configure stick decay behavior
        /// </summary>
        public void SetStickDecaySettings(bool enabled, double decayRate)
        {
            _enableStickDecay = enabled;
            _stickDecayRate = Math.Clamp(decayRate, 0.0, 1.0);

            System.Diagnostics.Debug.WriteLine($"[MOUSE MAPPER] Decay settings - Enabled: {enabled}, Rate: {decayRate:F2}");
        }
    }
}