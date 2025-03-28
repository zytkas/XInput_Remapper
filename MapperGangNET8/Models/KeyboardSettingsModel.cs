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
        public string KeyboardLayout { get; set; } = "QWERTY";

        public bool KeyRepeatEnabled { get; set; } = true;

        public double KeyRepeatRate { get; set; } = 70;

        public double AnalogKeySensitivity { get; set; } = 65;

        public string MovementUp { get; set; } = "W";

        public string MovementLeft { get; set; } = "A";

        public string MovementDown { get; set; } = "S";

        public string MovementRight { get; set; } = "D";

        public string MovementStyle { get; set; } = "8-Way";

        public bool AnalogKeyboardEnabled { get; set; } = true;

        public List<KeyboardButtonMappingModel> ButtonMappings { get; set; } = new List<KeyboardButtonMappingModel>();
    }

    /// <summary>
    /// Модель для хранения маппинга клавиш клавиатуры
    /// </summary>
    [Serializable]
    public class KeyboardButtonMappingModel
    {
        public string KeyboardKey { get; set; }

        public string ControllerButton { get; set; }
    }
}