using System;
using System.Collections.Generic;

namespace MapperGangNET8.Models
{
    /// <summary>
    /// Модель для общих настроек приложения
    /// </summary>
    [Serializable]
    public class AppSettingsModel
    {
        public bool StartWithWindows { get; set; } = false;

        public bool StartMinimized { get; set; } = false;

        public bool MinimizeToTray { get; set; } = true;

        public bool ShowNotifications { get; set; } = true;

        public string Theme { get; set; } = "Dark";

        public string AccentColor { get; set; } = "Blue";

        public bool AutoSwitchProfiles { get; set; } = true;

        public string DefaultProfile { get; set; } = "Default";

        public bool CloudSyncProfiles { get; set; } = false;

        public bool DebugMode { get; set; } = false;

        public string InputPollingRate { get; set; } = "1000 Hz";

        public string ProcessPriority { get; set; } = "High";
    }

    /// <summary>
    /// Модель для настроек контроллера
    /// </summary>
    [Serializable]
    public class ControllerSettingsModel
    {

        public string SelectedControllerType { get; set; } = "Xbox 360 Controller";

        public string ControllerNumber { get; set; } = "Controller 1";

        public bool VibrationEnabled { get; set; } = true;

        public double VibrationStrength { get; set; } = 80;

        public double ButtonPressureSensitivity { get; set; } = 75;

        public double TriggerDeadzone { get; set; } = 10;

        public bool HidePhysicalControllers { get; set; } = false;

        public bool ExclusiveMode { get; set; } = true;

        public bool PassThroughMode { get; set; } = false;

        public bool CombineInputs { get; set; } = false;

        public bool AutoConnect { get; set; } = true;

        public string ButtonAssignmentMode { get; set; } = "Standard";
    }

    /// <summary>
    /// Модель для настроек чувствительности
    /// </summary>
    [Serializable]
    public class SensitivitySettingsModel
    {
        public double MouseXAxisSensitivity { get; set; } = 65;

        public double MouseYAxisSensitivity { get; set; } = 60;

        public string MouseResponseCurveType { get; set; } = "Linear";

        public bool MouseAcceleration { get; set; } = false;

        public double MouseSmoothing { get; set; } = 30;

        public bool MouseAxisLock { get; set; } = false;

        public double JoystickSensitivity { get; set; } = 80;

        public double JoystickDeadzone { get; set; } = 10;

        public string JoystickResponseCurveType { get; set; } = "Linear";

        public bool JoystickAntiDeadzone { get; set; } = false;

        public bool JoystickRotation { get; set; } = false;

        public bool JoystickRadialDeadzone { get; set; } = true;
    }

    /// <summary>
    /// Модель для профиля настроек
    /// </summary>
    [Serializable]
    public class ProfileModel
    {
        public string Name { get; set; } = "Default";

        public string Description { get; set; } = "Профиль по умолчанию";

        public ControllerSettingsModel ControllerSettings { get; set; } = new ControllerSettingsModel();

        public MouseSettingsModel MouseSettings { get; set; } = new MouseSettingsModel();

        public KeyboardSettingsModel KeyboardSettings { get; set; } = new KeyboardSettingsModel();

        public SensitivitySettingsModel SensitivitySettings { get; set; } = new SensitivitySettingsModel();

        public List<string> AssociatedApplications { get; set; } = new List<string>();
    }
}