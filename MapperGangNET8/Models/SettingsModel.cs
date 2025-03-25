using System;
using System.Collections.Generic;

namespace MapperGang.Models
{
    /// <summary>
    /// Модель для общих настроек приложения
    /// </summary>
    [Serializable]
    public class AppSettingsModel
    {
        /// <summary>
        /// Запуск приложения вместе с Windows
        /// </summary>
        public bool StartWithWindows { get; set; } = false;

        /// <summary>
        /// Запуск приложения в свернутом виде
        /// </summary>
        public bool StartMinimized { get; set; } = false;

        /// <summary>
        /// Сворачивать в системный трей вместо панели задач
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// Показывать уведомления
        /// </summary>
        public bool ShowNotifications { get; set; } = true;

        /// <summary>
        /// Тема приложения (Light, Dark, System)
        /// </summary>
        public string Theme { get; set; } = "Dark";

        /// <summary>
        /// Цвет акцента приложения
        /// </summary>
        public string AccentColor { get; set; } = "Blue";

        /// <summary>
        /// Автоматически переключать профили в зависимости от активного приложения
        /// </summary>
        public bool AutoSwitchProfiles { get; set; } = true;

        /// <summary>
        /// Профиль по умолчанию
        /// </summary>
        public string DefaultProfile { get; set; } = "Default";

        /// <summary>
        /// Синхронизировать профили через облако
        /// </summary>
        public bool CloudSyncProfiles { get; set; } = false;

        /// <summary>
        /// Режим отладки
        /// </summary>
        public bool DebugMode { get; set; } = false;

        /// <summary>
        /// Частота опроса устройств ввода
        /// </summary>
        public string InputPollingRate { get; set; } = "1000 Hz";

        /// <summary>
        /// Приоритет процесса
        /// </summary>
        public string ProcessPriority { get; set; } = "High";
    }

    /// <summary>
    /// Модель для настроек контроллера
    /// </summary>
    [Serializable]
    public class ControllerSettingsModel
    {
        /// <summary>
        /// Выбранный тип контроллера
        /// </summary>
        public string SelectedControllerType { get; set; } = "Xbox 360 Controller";

        /// <summary>
        /// Номер контроллера
        /// </summary>
        public string ControllerNumber { get; set; } = "Controller 1";

        /// <summary>
        /// Включена ли вибрация
        /// </summary>
        public bool VibrationEnabled { get; set; } = true;

        /// <summary>
        /// Сила вибрации (0-100%)
        /// </summary>
        public double VibrationStrength { get; set; } = 80;

        /// <summary>
        /// Чувствительность нажатия кнопок (0-100%)
        /// </summary>
        public double ButtonPressureSensitivity { get; set; } = 75;

        /// <summary>
        /// Мертвая зона триггера (0-100%)
        /// </summary>
        public double TriggerDeadzone { get; set; } = 10;

        /// <summary>
        /// Скрыть физические контроллеры
        /// </summary>
        public bool HidePhysicalControllers { get; set; } = false;

        /// <summary>
        /// Эксклюзивный режим управления
        /// </summary>
        public bool ExclusiveMode { get; set; } = true;

        /// <summary>
        /// Проходной режим для физического контроллера
        /// </summary>
        public bool PassThroughMode { get; set; } = false;

        /// <summary>
        /// Комбинирование входных данных
        /// </summary>
        public bool CombineInputs { get; set; } = false;

        /// <summary>
        /// Автоматическое подключение при запуске
        /// </summary>
        public bool AutoConnect { get; set; } = true;

        /// <summary>
        /// Режим назначения кнопок (Стандартный/Пользовательский)
        /// </summary>
        public string ButtonAssignmentMode { get; set; } = "Standard";
    }

    /// <summary>
    /// Модель для настроек чувствительности
    /// </summary>
    [Serializable]
    public class SensitivitySettingsModel
    {
        /// <summary>
        /// Чувствительность оси X мыши (0-100%)
        /// </summary>
        public double MouseXAxisSensitivity { get; set; } = 65;

        /// <summary>
        /// Чувствительность оси Y мыши (0-100%)
        /// </summary>
        public double MouseYAxisSensitivity { get; set; } = 60;

        /// <summary>
        /// Тип кривой отклика мыши (Linear, S-Curve, Custom)
        /// </summary>
        public string MouseResponseCurveType { get; set; } = "Linear";

        /// <summary>
        /// Включено ли ускорение мыши
        /// </summary>
        public bool MouseAcceleration { get; set; } = false;

        /// <summary>
        /// Сглаживание движений мыши (0-100%)
        /// </summary>
        public double MouseSmoothing { get; set; } = 30;

        /// <summary>
        /// Блокировка движения по одной оси
        /// </summary>
        public bool MouseAxisLock { get; set; } = false;

        /// <summary>
        /// Чувствительность джойстика (0-100%)
        /// </summary>
        public double JoystickSensitivity { get; set; } = 80;

        /// <summary>
        /// Мертвая зона джойстика (0-100%)
        /// </summary>
        public double JoystickDeadzone { get; set; } = 10;

        /// <summary>
        /// Тип кривой отклика джойстика (Linear, Step, Custom)
        /// </summary>
        public string JoystickResponseCurveType { get; set; } = "Linear";

        /// <summary>
        /// Компенсация мертвой зоны джойстика
        /// </summary>
        public bool JoystickAntiDeadzone { get; set; } = false;

        /// <summary>
        /// Поворот ввода джойстика
        /// </summary>
        public bool JoystickRotation { get; set; } = false;

        /// <summary>
        /// Использование радиальной мертвой зоны
        /// </summary>
        public bool JoystickRadialDeadzone { get; set; } = true;
    }

    /// <summary>
    /// Модель для профиля настроек
    /// </summary>
    [Serializable]
    public class ProfileModel
    {
        /// <summary>
        /// Название профиля
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Описание профиля
        /// </summary>
        public string Description { get; set; } = "Профиль по умолчанию";

        /// <summary>
        /// Настройки контроллера для этого профиля
        /// </summary>
        public ControllerSettingsModel ControllerSettings { get; set; } = new ControllerSettingsModel();

        /// <summary>
        /// Настройки мыши для этого профиля
        /// </summary>
        public MouseSettingsModel MouseSettings { get; set; } = new MouseSettingsModel();

        /// <summary>
        /// Настройки клавиатуры для этого профиля
        /// </summary>
        public KeyboardSettingsModel KeyboardSettings { get; set; } = new KeyboardSettingsModel();

        /// <summary>
        /// Настройки чувствительности для этого профиля
        /// </summary>
        public SensitivitySettingsModel SensitivitySettings { get; set; } = new SensitivitySettingsModel();

        /// <summary>
        /// Список приложений, для которых используется этот профиль
        /// </summary>
        public List<string> AssociatedApplications { get; set; } = new List<string>();
    }
}