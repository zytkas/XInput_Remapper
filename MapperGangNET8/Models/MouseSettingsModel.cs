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
        /// <summary>
        /// Режим преобразования движения мыши в джойстик
        /// </summary>
        public string MouseJoystickMode { get; set; } = "Absolute Position";

        /// <summary>
        /// Чувствительность мыши (0-100%)
        /// </summary>
        public double MouseSensitivity { get; set; } = 65;

        /// <summary>
        /// Инвертировать ось X
        /// </summary>
        public bool InvertXAxis { get; set; } = false;

        /// <summary>
        /// Инвертировать ось Y
        /// </summary>
        public bool InvertYAxis { get; set; } = false;

        /// <summary>
        /// Включить ускорение мыши
        /// </summary>
        public bool MouseAcceleration { get; set; } = false;

        /// <summary>
        /// Сглаживание движений мыши (0-100%)
        /// </summary>
        public double MouseSmoothing { get; set; } = 30;

        /// <summary>
        /// Маппинг колеса мыши
        /// </summary>
        public string MouseWheelMapping { get; set; } = "Right Stick Y-Axis";

        /// <summary>
        /// Коллекция маппингов кнопок мыши
        /// </summary>
        public List<MouseButtonMappingModel> ButtonMappings { get; set; } = new List<MouseButtonMappingModel>();
    }

    /// <summary>
    /// Модель для хранения маппинга кнопок мыши
    /// </summary>
    [Serializable]
    public class MouseButtonMappingModel
    {
        /// <summary>
        /// Кнопка мыши
        /// </summary>
        public string MouseButton { get; set; }

        /// <summary>
        /// Кнопка контроллера
        /// </summary>
        public string ControllerButton { get; set; }
    }
}