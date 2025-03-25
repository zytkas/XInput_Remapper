using System;
using System.Collections.Generic;

namespace MapperGang.Models
{
    /// <summary>
    /// Модель для настроек клавиатуры
    /// </summary>
    [Serializable]
    public class KeyboardSettingsModel
    {
        /// <summary>
        /// Выбранная раскладка клавиатуры
        /// </summary>
        public string KeyboardLayout { get; set; } = "QWERTY";

        /// <summary>
        /// Включен ли повтор клавиш
        /// </summary>
        public bool KeyRepeatEnabled { get; set; } = true;

        /// <summary>
        /// Скорость повтора клавиш (0-100%)
        /// </summary>
        public double KeyRepeatRate { get; set; } = 70;

        /// <summary>
        /// Чувствительность аналоговых клавиш (0-100%)
        /// </summary>
        public double AnalogKeySensitivity { get; set; } = 65;

        /// <summary>
        /// Клавиша движения вверх
        /// </summary>
        public string MovementUp { get; set; } = "W";

        /// <summary>
        /// Клавиша движения влево
        /// </summary>
        public string MovementLeft { get; set; } = "A";

        /// <summary>
        /// Клавиша движения вниз
        /// </summary>
        public string MovementDown { get; set; } = "S";

        /// <summary>
        /// Клавиша движения вправо
        /// </summary>
        public string MovementRight { get; set; } = "D";

        /// <summary>
        /// Стиль движения (8-Way/4-Way)
        /// </summary>
        public string MovementStyle { get; set; } = "8-Way";

        /// <summary>
        /// Включить аналоговый режим клавиатуры
        /// </summary>
        public bool AnalogKeyboardEnabled { get; set; } = true;

        /// <summary>
        /// Коллекция маппингов кнопок клавиатуры
        /// </summary>
        public List<KeyboardButtonMappingModel> ButtonMappings { get; set; } = new List<KeyboardButtonMappingModel>();
    }

    /// <summary>
    /// Модель для хранения маппинга клавиш клавиатуры
    /// </summary>
    [Serializable]
    public class KeyboardButtonMappingModel
    {
        /// <summary>
        /// Клавиша клавиатуры
        /// </summary>
        public string KeyboardKey { get; set; }

        /// <summary>
        /// Кнопка контроллера
        /// </summary>
        public string ControllerButton { get; set; }
    }
}