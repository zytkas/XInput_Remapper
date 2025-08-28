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

        /// <summary>
        /// Selected controller type for emulation
        /// </summary>
        public string SelectedControllerType { get; set; } = "Xbox 360 Controller";
    }


    /// <summary>
    /// Model for a 2D point
    /// </summary>
    [Serializable]
    public class PointModel
    {
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Model for sensitivity settings
    /// </summary>
    [Serializable]
    public class SensitivitySettingsModel
    {
        /// <summary>
        /// Mouse X-axis sensitivity (0-100%)
        /// </summary>
        public double MouseXAxisSensitivity { get; set; } = 65;

        /// <summary>
        /// Mouse Y-axis sensitivity (0-100%)
        /// </summary>
        public double MouseYAxisSensitivity { get; set; } = 60;

        /// <summary>
        /// Type of mouse response curve
        /// </summary>
        public string MouseResponseCurveType { get; set; } = "Linear";

        /// <summary>
        /// Mouse acceleration enabled
        /// </summary>
        public bool MouseAcceleration { get; set; } = false;

        /// <summary>
        /// Mouse smoothing amount (0-100%)
        /// </summary>
        public double MouseSmoothing { get; set; } = 30;

        /// <summary>
        /// Mouse axis lock (restrict to a single axis)
        /// </summary>
        public bool MouseAxisLock { get; set; } = false;

        /// <summary>
        /// Joystick sensitivity (0-100%)
        /// </summary>
        public double JoystickSensitivity { get; set; } = 80;

        /// <summary>
        /// Controller trigger deadzone (0-100%)
        /// </summary>
        public double TriggerDeadzone { get; set; } = 10;

        /// <summary>
        /// Joystick deadzone (0-100%)
        /// </summary>
        public double JoystickDeadzone { get; set; } = 10;

        /// <summary>
        /// Type of joystick response curve
        /// </summary>
        public string JoystickResponseCurveType { get; set; } = "Linear";

        /// <summary>
        /// Joystick anti-deadzone compensation
        /// </summary>
        public bool JoystickAntiDeadzone { get; set; } = false;

        /// <summary>
        /// Joystick input rotation
        /// </summary>
        public bool JoystickRotation { get; set; } = false;

        /// <summary>
        /// Use radial deadzone for joystick
        /// </summary>
        public bool JoystickRadialDeadzone { get; set; } = true;

        // New curve parameters

        /// <summary>
        /// Exponent for mouse exponential curve (1.0-5.0)
        /// </summary>
        public double MouseExponent { get; set; } = 2.0;

        /// <summary>
        /// Strength parameter for mouse S-curve (0.0-1.0)
        /// </summary>
        public double MouseCurveStrength { get; set; } = 0.5;

        /// <summary>
        /// Midpoint parameter for mouse S-curve (0.1-0.9)
        /// </summary>
        public double MouseCurveMidpoint { get; set; } = 0.5;

        /// <summary>
        /// Control points for mouse custom curve
        /// </summary>
        public List<PointModel> MouseCustomCurvePoints { get; set; } = new List<PointModel>
        {
            new PointModel { X = 0, Y = 0 },
            new PointModel { X = 0.25, Y = 0.25 },
            new PointModel { X = 0.5, Y = 0.5 },
            new PointModel { X = 0.75, Y = 0.75 },
            new PointModel { X = 1, Y = 1 }
        };

        /// <summary>
        /// Strength parameter for joystick S-curve (0.0-1.0)
        /// </summary>
        public double JoystickCurveStrength { get; set; } = 0.5;

        /// <summary>
        /// Midpoint parameter for joystick S-curve (0.1-0.9)
        /// </summary>
        public double JoystickCurveMidpoint { get; set; } = 0.5;

        /// <summary>
        /// Control points for joystick custom curve
        /// </summary>
        public List<PointModel> JoystickCustomCurvePoints { get; set; } = new List<PointModel>
        {
            new PointModel { X = 0, Y = 0 },
            new PointModel { X = 0.25, Y = 0.25 },
            new PointModel { X = 0.5, Y = 0.5 },
            new PointModel { X = 0.75, Y = 0.75 },
            new PointModel { X = 1, Y = 1 }
        };
    }

    /// <summary>
    /// Модель для профиля настроек
    /// </summary>
    [Serializable]
    public class ProfileModel
    {
        public string Name { get; set; } = "Default";

        public string Description { get; set; } = "Профиль по умолчанию";


        public MouseSettingsModel MouseSettings { get; set; } = new MouseSettingsModel();

        public KeyboardSettingsModel KeyboardSettings { get; set; } = new KeyboardSettingsModel();

        public SensitivitySettingsModel SensitivitySettings { get; set; } = new SensitivitySettingsModel();

        public List<string> AssociatedApplications { get; set; } = new List<string>();
    }
}