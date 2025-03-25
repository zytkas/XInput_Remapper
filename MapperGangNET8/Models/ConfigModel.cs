using System;
using System.Collections.Generic;

namespace MapperGang.Models
{
    /// <summary>
    /// Основная модель конфигурации, содержащая все настройки приложения
    /// </summary>
    [Serializable]
    public class ConfigModel
    {
        /// <summary>
        /// Версия конфигурации для обратной совместимости
        /// </summary>
        public string ConfigVersion { get; set; } = "1.0";

        /// <summary>
        /// Общие настройки приложения
        /// </summary>
        public AppSettingsModel AppSettings { get; set; } = new AppSettingsModel();

        /// <summary>
        /// Настройки контроллера
        /// </summary>
        public ControllerSettingsModel ControllerSettings { get; set; } = new ControllerSettingsModel();

        /// <summary>
        /// Настройки мыши
        /// </summary>
        public MouseSettingsModel MouseSettings { get; set; } = new MouseSettingsModel();

        /// <summary>
        /// Настройки клавиатуры
        /// </summary>
        public KeyboardSettingsModel KeyboardSettings { get; set; } = new KeyboardSettingsModel();

        /// <summary>
        /// Настройки чувствительности
        /// </summary>
        public SensitivitySettingsModel SensitivitySettings { get; set; } = new SensitivitySettingsModel();

        /// <summary>
        /// Профили настроек
        /// </summary>
        public Dictionary<string, ProfileModel> Profiles { get; set; } = new Dictionary<string, ProfileModel>();

        /// <summary>
        /// Активный профиль
        /// </summary>
        public string ActiveProfile { get; set; } = "Default";
    }
}