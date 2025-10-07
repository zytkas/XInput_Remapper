using System;
using System.Collections.Generic;

namespace MapperGangNET8.Models
{
    /// <summary>
    /// Модель для настроек мыши
    /// </summary>
    [Serializable]
    public class MouseSettingsModel
    {
        // Mouse to Joystick Mode: Spring Mode, Absolute Position
        public string MouseJoystickMode { get; set; } = "Spring Mode";

        // Sensitivity: 0-200% (mapped to 0-27146 in MouseToStickMapper)
        public double MouseSensitivity { get; set; } = 100;
        public double MouseSensitivityX { get; set; } = 100;
        public double MouseSensitivityY { get; set; } = 100;

        public bool InvertXAxis { get; set; } = false;

        public bool InvertYAxis { get; set; } = true; // Y inverted by default for FPS

        // ScaleFactor: 0-200% (100% = 10000, mapped to 5-100000 in MouseToStickMapper)
        public double ScaleFactorX { get; set; } = 100;
        public double ScaleFactorY { get; set; } = 100;

        // Smoothing: 0-10 (exponential smoothing level)
        public double MouseSmoothing { get; set; } = 5;

        // NoiseFilter: 0-10 (threshold deadband for micro-movements)
        public double NoiseFilter { get; set; } = 0;

        // ReturnTime: auto-return time in milliseconds (5-255ms, default 30ms)
        public int ReturnTime { get; set; } = 30;

        // Response curve preset: Linear, Precision, Aggressive
        public string ResponseCurveType { get; set; } = "Linear";

        public List<MouseButtonMappingModel> ButtonMappings { get; set; } = new List<MouseButtonMappingModel>();
    }

    /// <summary>
    /// Модель для хранения маппинга кнопок мыши
    /// </summary>
    [Serializable]
    public class MouseButtonMappingModel
    {
        public string MouseButton { get; set; }

        public string ControllerButton { get; set; }
    }
}