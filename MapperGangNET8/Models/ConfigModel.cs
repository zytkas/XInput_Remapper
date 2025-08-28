using System;
using System.Collections.Generic;

namespace MapperGangNET8.Models
{
    /// <summary>
    /// Основная модель конфигурации, содержащая все настройки приложения
    /// </summary>
    [Serializable]
    public class ConfigModel
    {
        public string ConfigVersion { get; set; } = "1.0";

        public AppSettingsModel AppSettings { get; set; } = new AppSettingsModel();


        public MouseSettingsModel MouseSettings { get; set; } = new MouseSettingsModel();

        public KeyboardSettingsModel KeyboardSettings { get; set; } = new KeyboardSettingsModel();

        public SensitivitySettingsModel SensitivitySettings { get; set; } = new SensitivitySettingsModel();

        public Dictionary<string, ProfileModel> Profiles { get; set; } = new Dictionary<string, ProfileModel>();

        public string ActiveProfile { get; set; } = "Default";
    }
}