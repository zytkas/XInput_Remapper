using System;
using System.Collections.Generic;

namespace MapperGang.Models
{
    /// <summary>
    /// Модель для настроек мыши
    /// </summary>
    [Serializable]
    public class MouseSettingsModel
    {
        public string MouseJoystickMode { get; set; } = "Absolute Position";

        public double MouseSensitivity { get; set; } = 65;

        public bool InvertXAxis { get; set; } = false;

        public bool InvertYAxis { get; set; } = false;

        public bool MouseAcceleration { get; set; } = false;

        public double MouseSmoothing { get; set; } = 30;

        public string MouseWheelMapping { get; set; } = "Right Stick Y-Axis";

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