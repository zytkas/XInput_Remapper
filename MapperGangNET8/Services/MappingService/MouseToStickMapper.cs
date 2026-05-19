using System;
using System.Diagnostics;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Services.MappingService
{
    /// <summary>
    /// Response curve point for stick deflection interpolation.
    /// </summary>
    public struct ResponseCurvePoint
    {
        public float TravelDistance;
        public float NewValue;

        public ResponseCurvePoint(float travelDistance, float newValue)
        {
            TravelDistance = travelDistance;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Mouse → right-stick mapper inspired by the reWASD pipeline:
    /// noise filter → sensitivity/scale → spring decay → response curve → exponential smoothing → inversion.
    /// <see cref="ProcessMouseDelta"/> accumulates impulses on the input thread; <see cref="Update"/>
    /// is driven by the pipeline tick and emits the stick value.
    /// </summary>
    public class MouseToStickMapper
    {
        private readonly IControllerService _controllerService;
        private readonly object _stateLock = new object();
        private long _lastTick;
        private readonly double _ticksToMs = 1000.0 / Stopwatch.Frequency;

        private float _xSensitivity = 13573f;
        private float _ySensitivity = 13573f;

        private float _scaleFactorX = 10000f;
        private float _scaleFactorY = 10000f;

        private uint _smoothing = 5;
        private uint _noiseFilter = 0;

        private bool _springMode = true;
        private byte _returnTime = 30;

        private bool _isXInvert = false;
        private bool _isYInvert = true;

        private ResponseCurvePoint[] _horizontalCurve;
        private ResponseCurvePoint[] _verticalCurve;

        private const float GLOBAL_GAIN = 100.0f;
        private const float DEFAULT_SENSITIVITY = 13573f;
        private const float MAX_SENSITIVITY = 27146f;
        private const float MIN_STICK_VALUE = -32767f;
        private const float MAX_STICK_VALUE = 32767f;

        private float _accumulatedX = 0f;
        private float _accumulatedY = 0f;
        private float _prevOutputX = 0f;
        private float _prevOutputY = 0f;

        public MouseToStickMapper(IControllerService controllerService)
        {
            _controllerService = controllerService;
            _lastTick = Stopwatch.GetTimestamp();

            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(10922, 10922),
                new ResponseCurvePoint(21845, 21845),
                new ResponseCurvePoint(32767, 32767)
            };

            _verticalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(10922, 10922),
                new ResponseCurvePoint(21845, 21845),
                new ResponseCurvePoint(32767, 32767)
            };
        }

        /// <summary>
        /// Called from the mouse-read thread (high frequency). Accumulates impulse into the stick state.
        /// </summary>
        public void ProcessMouseDelta(long deltaX, long deltaY)
        {
            float dx = deltaX;
            float dy = deltaY;

            if (Math.Abs(dx) < _noiseFilter) dx = 0;
            if (Math.Abs(dy) < _noiseFilter) dy = 0;

            float sensitivityX = (_xSensitivity / DEFAULT_SENSITIVITY) * (_scaleFactorX / 10000.0f) * GLOBAL_GAIN;
            float sensitivityY = (_ySensitivity / DEFAULT_SENSITIVITY) * (_scaleFactorY / 10000.0f) * GLOBAL_GAIN;

            lock (_stateLock)
            {
                _accumulatedX += dx * sensitivityX;
                _accumulatedY += dy * sensitivityY;
            }
        }

        /// <summary>
        /// Called by the pipeline tick. Applies spring physics / clamping, curve, smoothing, inversion,
        /// and writes the normalized value to the right thumbstick. Does not submit the report.
        /// </summary>
        public void Update()
        {
            long currentTick = Stopwatch.GetTimestamp();
            float deltaTimeMs = (float)((currentTick - _lastTick) * _ticksToMs);
            _lastTick = currentTick;

            if (deltaTimeMs <= 0) return;

            float currentX, currentY;

            lock (_stateLock)
            {
                currentX = _accumulatedX;
                currentY = _accumulatedY;

                if (_springMode)
                {
                    float decay = deltaTimeMs / (_returnTime + 1.0f);
                    decay = Math.Min(decay, 1.0f);

                    _accumulatedX *= (1.0f - decay);
                    _accumulatedY *= (1.0f - decay);

                    if (Math.Abs(_accumulatedX) < 1.0f) _accumulatedX = 0;
                    if (Math.Abs(_accumulatedY) < 1.0f) _accumulatedY = 0;
                }
                else
                {
                    _accumulatedX = Clamp(_accumulatedX, MIN_STICK_VALUE, MAX_STICK_VALUE);
                    _accumulatedY = Clamp(_accumulatedY, MIN_STICK_VALUE, MAX_STICK_VALUE);
                }
            }

            float curvedX = ApplyCurve(currentX, _horizontalCurve);
            float curvedY = ApplyCurve(currentY, _verticalCurve);

            float alpha = 1.0f - (_smoothing / 20.0f);

            float smoothX = _prevOutputX + alpha * (curvedX - _prevOutputX);
            float smoothY = _prevOutputY + alpha * (curvedY - _prevOutputY);

            _prevOutputX = smoothX;
            _prevOutputY = smoothY;

            if (_isXInvert) smoothX = -smoothX;
            if (_isYInvert) smoothY = -smoothY;

            double finalX = Clamp(smoothX, MIN_STICK_VALUE, MAX_STICK_VALUE) / MAX_STICK_VALUE;
            double finalY = Clamp(smoothY, MIN_STICK_VALUE, MAX_STICK_VALUE) / MAX_STICK_VALUE;

            _controllerService.SetAxis(ControllerAxis.RightThumbX, finalX);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, finalY);
        }

        private float ApplyCurve(float inputValue, ResponseCurvePoint[] curvePoints)
        {
            float absInput = Math.Abs(inputValue);
            float sign = Math.Sign(inputValue);

            for (int i = 0; i < curvePoints.Length - 1; i++)
            {
                if (absInput >= curvePoints[i].TravelDistance &&
                    absInput <= curvePoints[i + 1].TravelDistance)
                {
                    float t1 = curvePoints[i].TravelDistance;
                    float t2 = curvePoints[i + 1].TravelDistance;
                    float v1 = curvePoints[i].NewValue;
                    float v2 = curvePoints[i + 1].NewValue;

                    float factor = (t2 - t1) > 0 ? (absInput - t1) / (t2 - t1) : 0;
                    float output = v1 + (v2 - v1) * factor;

                    return output * sign;
                }
            }

            return curvePoints[curvePoints.Length - 1].NewValue * sign;
        }

        private float Clamp(float value, float min, float max) =>
            Math.Max(min, Math.Min(max, value));

        /// <summary>
        /// Reset accumulated state and center the right stick.
        /// </summary>
        public void Reset()
        {
            lock (_stateLock)
            {
                _accumulatedX = 0f;
                _accumulatedY = 0f;
            }
            _prevOutputX = 0f;
            _prevOutputY = 0f;

            _controllerService.SetAxis(ControllerAxis.RightThumbX, 0);
            _controllerService.SetAxis(ControllerAxis.RightThumbY, 0);
        }

        /// <summary>
        /// Set sensitivity (0-27146, default 13573 = 100%).
        /// </summary>
        public void SetSensitivity(float x, float y)
        {
            _xSensitivity = Clamp(x, 0, MAX_SENSITIVITY);
            _ySensitivity = Clamp(y, 0, MAX_SENSITIVITY);
        }

        /// <summary>
        /// Set sensitivity from percentage (0-200%).
        /// </summary>
        public void SetSensitivityPercent(float xPercent, float yPercent)
        {
            _xSensitivity = (xPercent / 100.0f) * DEFAULT_SENSITIVITY;
            _ySensitivity = (yPercent / 100.0f) * DEFAULT_SENSITIVITY;
        }

        /// <summary>
        /// Set scale factors (5-100000, default 10000).
        /// </summary>
        public void SetScaleFactor(float x, float y)
        {
            _scaleFactorX = Clamp(x, 5, 100000);
            _scaleFactorY = Clamp(y, 5, 100000);
        }

        /// <summary>
        /// Set smoothing level (0-10, default 5).
        /// </summary>
        public void SetSmoothing(uint smoothing) => _smoothing = Math.Min(smoothing, 10);

        /// <summary>
        /// Set noise filter level (0-10, default 0).
        /// </summary>
        public void SetNoiseFilter(uint noiseFilter) => _noiseFilter = Math.Min(noiseFilter, 10);

        /// <summary>
        /// True = virtual joystick with auto-return (spring). False = absolute positioning.
        /// </summary>
        public void SetSpringMode(bool enabled) => _springMode = enabled;

        /// <summary>
        /// Auto-return time in milliseconds (default 30).
        /// </summary>
        public void SetReturnTime(byte milliseconds) => _returnTime = milliseconds;

        /// <summary>
        /// Per-axis inversion.
        /// </summary>
        public void SetInversion(bool xInvert, bool yInvert)
        {
            _isXInvert = xInvert;
            _isYInvert = yInvert;
        }

        /// <summary>
        /// Custom horizontal response curve (≥ 4 points).
        /// </summary>
        public void SetHorizontalCurve(ResponseCurvePoint[] curve)
        {
            if (curve.Length >= 4) _horizontalCurve = curve;
        }

        /// <summary>
        /// Custom vertical response curve (≥ 4 points).
        /// </summary>
        public void SetVerticalCurve(ResponseCurvePoint[] curve)
        {
            if (curve.Length >= 4) _verticalCurve = curve;
        }

        /// <summary>
        /// Precision preset (slow at low deflection, ramps up at high).
        /// </summary>
        public void SetPrecisionCurve()
        {
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(16384, 8192),
                new ResponseCurvePoint(24576, 20480),
                new ResponseCurvePoint(32767, 32767)
            };
            _verticalCurve = (ResponseCurvePoint[])_horizontalCurve.Clone();
        }

        /// <summary>
        /// Aggressive preset (fast response at low deflection).
        /// </summary>
        public void SetAggressiveCurve()
        {
            _horizontalCurve = new ResponseCurvePoint[]
            {
                new ResponseCurvePoint(0, 0),
                new ResponseCurvePoint(8192, 16384),
                new ResponseCurvePoint(20480, 28672),
                new ResponseCurvePoint(32767, 32767)
            };
            _verticalCurve = (ResponseCurvePoint[])_horizontalCurve.Clone();
        }

        /// <summary>
        /// Default linear curve.
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
    }
}
