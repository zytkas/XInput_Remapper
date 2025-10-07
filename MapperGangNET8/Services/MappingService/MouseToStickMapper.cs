using System;
using System.Diagnostics;
using System.IO;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Response curve point for stick deflection interpolation
    /// </summary>
    public struct ResponseCurvePoint
    {
        public float TravelDistance; // Input value (0-32767)
        public float NewValue;        // Output value after curve (0-32767)

        public ResponseCurvePoint(float travelDistance, float newValue)
        {
            TravelDistance = travelDistance;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Mouse → Stick mapper based on reWASD algorithm
    /// Two-stage processing: Delta Processing → Stick Processing
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;

        // ==================== CONFIGURATION PARAMETERS ====================

        // Sensitivity (0-27146, default 13573 = 100%)
        private float _xSensitivity = 13573f;
        private float _ySensitivity = 13573f;

        // Scale factors (5-100000, default 30000 for high DPI direct input)
        private float _scaleFactorX = 10000f;
        private float _scaleFactorY = 10000f;

        // Smoothing level (0-10, default 0 for precise micro-movements)
        private uint _smoothing = 5;

        // Noise filter level (0-10, default 0 for direct mouse input)
        private uint _noiseFilter = 0;

        // Spring mode (default true = virtual joystick with auto-return)
        private bool _springMode = true;

        // Auto-return time in milliseconds (default 20ms balanced)
        private byte _returnTime = 30;

        // Axis inversion
        private bool _isXInvert = false;
        private bool _isYInvert = true; // Y inverted by default for FPS

        // Response curves (4 points each)
        private ResponseCurvePoint[] _horizontalCurve;
        private ResponseCurvePoint[] _verticalCurve;

        // ==================== STATE VARIABLES ====================

        // Target position (Spring Mode) or Accumulated position (Absolute Mode)
        private float _targetX = 0f;
        private float _targetY = 0f;

        // Previous smoothed values for exponential smoothing
        private float _previousSmoothX = 0f;
        private float _previousSmoothY = 0f;

        // Timing
        private DateTime _lastDeltaTime = DateTime.Now;

        // Debug logging
        private System.IO.StreamWriter? _debugLog;
        private readonly object _logLock = new object();

        // ==================== CONSTANTS ====================

        private const float DEFAULT_SENSITIVITY = 13573f;  // 100% (1.0x)
        private const float MAX_SENSITIVITY = 27146f;      // 200% (2.0x)
        private const float MIN_STICK_VALUE = -32767f;
        private const float MAX_STICK_VALUE = 32767f;

        public MouseToStickMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;

            // Initialize default linear response curves
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),           // Dead zone
                new ResponseCurvePoint(10922, 10922),   // 33%
                new ResponseCurvePoint(21845, 21845),   // 66%
                new ResponseCurvePoint(32767, 32767)    // 100%
            };

            _verticalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),           // Dead zone
                new ResponseCurvePoint(10922, 10922),   // 33%
                new ResponseCurvePoint(21845, 21845),   // 66%
                new ResponseCurvePoint(32767, 32767)    // 100%
            };
        }

        /// <summary>
        /// Process raw mouse delta and update controller stick
        /// Complete reWASD algorithm implementation
        /// </summary>
        public void ProcessMouseDelta(long deltaX, long deltaY)
        {
            float deltaTimeMs = (float)(DateTime.Now - _lastDeltaTime).TotalMilliseconds;
            _lastDeltaTime = DateTime.Now;

            // ==================== STAGE 1: DELTA PROCESSING (PRE-ACCUMULATION) ====================

            float dx = (float)deltaX;
            float dy = (float)deltaY;

            bool hasMovement = (dx != 0 || dy != 0);
            if (hasMovement)
                LogDebug($"[1] RAW: dx={dx:F1}, dy={dy:F1}");

            // 1. Apply NoiseFilter (threshold deadband)
            float noiseThreshold = _noiseFilter * 10.0f; // Estimated multiplier
            if (Math.Abs(dx) < noiseThreshold) dx = 0;
            if (Math.Abs(dy) < noiseThreshold) dy = 0;

            // 2. Apply ScaleFactor BEFORE accumulation
            dx *= _scaleFactorX / 1000.0f;  // 1500 → 1.5x
            dy *= _scaleFactorY / 1000.0f;

            // 3. Apply Sensitivity BEFORE accumulation
            float sensitivityMultiplierX = _xSensitivity / DEFAULT_SENSITIVITY;
            float sensitivityMultiplierY = _ySensitivity / DEFAULT_SENSITIVITY;
            dx *= sensitivityMultiplierX;
            dy *= sensitivityMultiplierY;

            if (hasMovement)
                LogDebug($"[2] SCALED: dx={dx:F2}, dy={dy:F2}");

            // ==================== STAGE 2: POSITION CALCULATION & MODE LOGIC ====================

            bool noMovement = (Math.Abs(dx) < 0.1f && Math.Abs(dy) < 0.1f);

            if (_springMode)
            {
                // Spring Mode: Virtual joystick with auto-return
                _targetX += dx;
                _targetY += dy;

                // Clamp to stick range
                _targetX = Clamp(_targetX, MIN_STICK_VALUE, MAX_STICK_VALUE);
                _targetY = Clamp(_targetY, MIN_STICK_VALUE, MAX_STICK_VALUE);

                if (hasMovement)
                    LogDebug($"[3] TARGET: x={_targetX:F2}, y={_targetY:F2}");

                // Auto-return to center if no new movement
                if (noMovement && deltaTimeMs > 0)
                {
                    // Smooth exponential decay
                    float decayFactor = Math.Min(deltaTimeMs / _returnTime, 1.0f);

                    _targetX *= (1.0f - decayFactor);
                    _targetY *= (1.0f - decayFactor);

                    // Snap to zero when very close (small threshold for micro precision)
                    if (Math.Abs(_targetX) < 5.0f) _targetX = 0f;
                    if (Math.Abs(_targetY) < 5.0f) _targetY = 0f;
                }
            }
            else
            {
                // Absolute Mode: Accumulate position permanently (no auto-return)
                _targetX += dx;
                _targetY += dy;

                // Clamp to stick range
                _targetX = Clamp(_targetX, MIN_STICK_VALUE, MAX_STICK_VALUE);
                _targetY = Clamp(_targetY, MIN_STICK_VALUE, MAX_STICK_VALUE);
            }

            // ==================== STAGE 3: RESPONSE CURVE APPLICATION ====================

            float curvedX = ApplyCurve(_targetX, _horizontalCurve);
            float curvedY = ApplyCurve(_targetY, _verticalCurve);

            if (hasMovement)
                LogDebug($"[4] CURVED: x={curvedX:F2}, y={curvedY:F2}");

            // ==================== STAGE 4: FINAL PROCESSING ====================

            // 1. Apply Smoothing (exponential smoothing)
            float smoothX, smoothY;

            if (_smoothing > 0)
            {
                float smoothingFactor = _smoothing / 10.0f;
                smoothX = _previousSmoothX * smoothingFactor + curvedX * (1.0f - smoothingFactor);
                smoothY = _previousSmoothY * smoothingFactor + curvedY * (1.0f - smoothingFactor);

                _previousSmoothX = smoothX;
                _previousSmoothY = smoothY;
            }
            else
            {
                smoothX = curvedX;
                smoothY = curvedY;
            }

            // 2. Apply inversion if enabled
            if (_isXInvert) smoothX = -smoothX;
            if (_isYInvert) smoothY = -smoothY;

            if (hasMovement)
                LogDebug($"[5] SMOOTH: x={smoothX:F2}, y={smoothY:F2}");

            // 3. Final range limiting
            int finalX = (int)Clamp(smoothX, MIN_STICK_VALUE, MAX_STICK_VALUE);
            int finalY = (int)Clamp(smoothY, MIN_STICK_VALUE, MAX_STICK_VALUE);

            // 4. Normalize to -1.0 to 1.0 range for controller service
            double normalizedX = finalX / 32767.0;
            double normalizedY = finalY / 32767.0;

            if (hasMovement)
                LogDebug($"[6] FINAL: x={finalX}, y={finalY} -> norm({normalizedX:F4}, {normalizedY:F4})\n");

            // 5. Output to virtual controller stick
            _controllerService.SetAxis(ControllerAxis.RightThumbX, normalizedX);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, normalizedY);
        }

        /// <summary>
        /// Update method for timer-based updates (120 FPS recommended)
        /// Call this even when there's no mouse movement for auto-return
        /// </summary>
        public void Update()
        {
            // Process with zero deltas to handle auto-return
            ProcessMouseDelta(0, 0);
        }

        /// <summary>
        /// Apply response curve interpolation to stick position
        /// </summary>
        private float ApplyCurve(float inputValue, ResponseCurvePoint[] curvePoints)
        {
            float absInput = Math.Abs(inputValue);
            float sign = Math.Sign(inputValue);

            // Find curve segment for linear interpolation
            for (int i = 0; i < curvePoints.Length - 1; i++)
            {
                if (absInput >= curvePoints[i].TravelDistance &&
                    absInput <= curvePoints[i + 1].TravelDistance)
                {
                    float t1 = curvePoints[i].TravelDistance;
                    float t2 = curvePoints[i + 1].TravelDistance;
                    float v1 = curvePoints[i].NewValue;
                    float v2 = curvePoints[i + 1].NewValue;

                    // Linear interpolation
                    float factor = (t2 - t1) > 0 ? (absInput - t1) / (t2 - t1) : 0;
                    float output = v1 + (v2 - v1) * factor;

                    return output * sign; // Preserve direction
                }
            }

            // If beyond curve range, return maximum
            return curvePoints[curvePoints.Length - 1].NewValue * sign;
        }

        private float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Reset all state to center position
        /// </summary>
        public void Reset()
        {
            _targetX = 0f;
            _targetY = 0f;
            _previousSmoothX = 0f;
            _previousSmoothY = 0f;
            _lastDeltaTime = DateTime.Now;

            _controllerService.SetAxis(ControllerAxis.RightThumbX, 0);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, 0);
        }

        // ==================== CONFIGURATION METHODS ====================

        /// <summary>
        /// Set sensitivity (0-27146, default 13573 = 100%)
        /// </summary>
        public void SetSensitivity(float x, float y)
        {
            _xSensitivity = Clamp(x, 0, MAX_SENSITIVITY);
            _ySensitivity = Clamp(y, 0, MAX_SENSITIVITY);
        }

        /// <summary>
        /// Set sensitivity from percentage (0-200%)
        /// </summary>
        public void SetSensitivityPercent(float xPercent, float yPercent)
        {
            _xSensitivity = (xPercent / 100.0f) * DEFAULT_SENSITIVITY;
            _ySensitivity = (yPercent / 100.0f) * DEFAULT_SENSITIVITY;
        }

        /// <summary>
        /// Set scale factors (5-100000, default 30000 for high DPI direct input)
        /// </summary>
        public void SetScaleFactor(float x, float y)
        {
            _scaleFactorX = Clamp(x, 5, 100000);
            _scaleFactorY = Clamp(y, 5, 100000);
        }

        /// <summary>
        /// Set smoothing level (0-10, default 3)
        /// </summary>
        public void SetSmoothing(uint smoothing)
        {
            _smoothing = Math.Min(smoothing, 10);
        }

        /// <summary>
        /// Set noise filter level (0-10, default 3)
        /// </summary>
        public void SetNoiseFilter(uint noiseFilter)
        {
            _noiseFilter = Math.Min(noiseFilter, 10);
        }

        /// <summary>
        /// Set spring mode (true = virtual joystick with auto-return, false = absolute positioning)
        /// </summary>
        public void SetSpringMode(bool enabled)
        {
            _springMode = enabled;
        }

        /// <summary>
        /// Set auto-return time in milliseconds (default 30ms)
        /// </summary>
        public void SetReturnTime(byte milliseconds)
        {
            _returnTime = milliseconds;
        }

        /// <summary>
        /// Set axis inversion
        /// </summary>
        public void SetInversion(bool xInvert, bool yInvert)
        {
            _isXInvert = xInvert;
            _isYInvert = yInvert;
        }

        /// <summary>
        /// Set custom response curve for horizontal axis
        /// </summary>
        public void SetHorizontalCurve(ResponseCurvePoint[] curve)
        {
            if (curve.Length >= 4)
                _horizontalCurve = curve;
        }

        /// <summary>
        /// Set custom response curve for vertical axis
        /// </summary>
        public void SetVerticalCurve(ResponseCurvePoint[] curve)
        {
            if (curve.Length >= 4)
                _verticalCurve = curve;
        }

        /// <summary>
        /// Set precision curve preset (slower movement in low deflection zones)
        /// </summary>
        public void SetPrecisionCurve()
        {
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(16384, 8192),    // 50% input → 25% output
                new ResponseCurvePoint(24576, 20480),   // 75% input → 62% output
                new ResponseCurvePoint(32767, 32767)
            };

            _verticalCurve = (ResponseCurvePoint[])_horizontalCurve.Clone();
        }

        /// <summary>
        /// Set aggressive curve preset (faster movement in low deflection zones)
        /// </summary>
        public void SetAggressiveCurve()
        {
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(8192, 16384),    // 25% input → 50% output
                new ResponseCurvePoint(20480, 28672),   // 62% input → 87% output
                new ResponseCurvePoint(32767, 32767)
            };

            _verticalCurve = (ResponseCurvePoint[])_horizontalCurve.Clone();
        }

        /// <summary>
        /// Reset to default linear curve
        /// </summary>
        public void SetLinearCurve()
        {
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(10922, 10922),
                new ResponseCurvePoint(21845, 21845),
                new ResponseCurvePoint(32767, 32767)
            };

            _verticalCurve = (ResponseCurvePoint[])_horizontalCurve.Clone();
        }

        // Legacy compatibility methods
        [Obsolete("Use SetSensitivity or SetSensitivityPercent instead")]
        public void SetCapFactor(int cap) { /* Deprecated */ }

        [Obsolete("Use SetSmoothing instead")]
        public void SetLerpSpeed(double speed) { /* Deprecated */ }

        [Obsolete("Use Update() instead")]
        public void UpdateStickDecay() => Update();

        // ==================== DEBUG LOGGING ====================

        /// <summary>
        /// Enable detailed debug logging to file
        /// </summary>
        public void EnableDebugLog(string? filePath = null)
        {
            lock (_logLock)
            {
                if (_debugLog != null)
                    return; // Already enabled

                string path = filePath ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"MouseToStick_Debug_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );

                _debugLog = new System.IO.StreamWriter(path, false) { AutoFlush = true };
                _debugLog.WriteLine($"=== Mouse To Stick Debug Log ===");
                _debugLog.WriteLine($"Started: {DateTime.Now}");
                _debugLog.WriteLine($"Settings: ScaleFactor={_scaleFactorX}, Sens={_xSensitivity}, Smooth={_smoothing}, Spring={_springMode}");
                _debugLog.WriteLine($"===================================\n");
            }
        }

        /// <summary>
        /// Disable debug logging
        /// </summary>
        public void DisableDebugLog()
        {
            lock (_logLock)
            {
                if (_debugLog != null)
                {
                    _debugLog.WriteLine($"\n=== Log ended: {DateTime.Now} ===");
                    _debugLog.Close();
                    _debugLog.Dispose();
                    _debugLog = null;
                }
            }
        }

        private void LogDebug(string message)
        {
            if (_debugLog != null)
            {
                lock (_logLock)
                {
                    _debugLog?.WriteLine(message);
                }
            }
        }
    }
}
