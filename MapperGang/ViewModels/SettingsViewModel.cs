// MapperGang/ViewModels/SettingsViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек приложения
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        #region Приватные поля
        private bool _startWithWindows;
        private bool _startMinimized;
        private bool _minimizeToTray;
        private bool _showNotifications;

        private string _theme;
        private string _accentColor;

        private bool _autoSwitchProfiles;
        private string _defaultProfile;
        private bool _cloudSyncProfiles;

        private bool _debugMode;
        private string _inputPollingRate;
        private string _processPriority;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Запускать приложение вместе с Windows
        /// </summary>
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set => SetProperty(ref _startWithWindows, value);
        }

        /// <summary>
        /// Запускать приложение в свернутом виде
        /// </summary>
        public bool StartMinimized
        {
            get => _startMinimized;
            set => SetProperty(ref _startMinimized, value);
        }

        /// <summary>
        /// Сворачивать в системный трей вместо панели задач
        /// </summary>
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set => SetProperty(ref _minimizeToTray, value);
        }

        /// <summary>
        /// Показывать уведомления
        /// </summary>
        public bool ShowNotifications
        {
            get => _showNotifications;
            set => SetProperty(ref _showNotifications, value);
        }

        /// <summary>
        /// Тема приложения (Light, Dark, System)
        /// </summary>
        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }

        /// <summary>
        /// Цвет акцента приложения
        /// </summary>
        public string AccentColor
        {
            get => _accentColor;
            set => SetProperty(ref _accentColor, value);
        }

        /// <summary>
        /// Автоматически переключать профили в зависимости от активного приложения
        /// </summary>
        public bool AutoSwitchProfiles
        {
            get => _autoSwitchProfiles;
            set => SetProperty(ref _autoSwitchProfiles, value);
        }

        /// <summary>
        /// Профиль по умолчанию
        /// </summary>
        public string DefaultProfile
        {
            get => _defaultProfile;
            set => SetProperty(ref _defaultProfile, value);
        }

        /// <summary>
        /// Синхронизировать профили через облако
        /// </summary>
        public bool CloudSyncProfiles
        {
            get => _cloudSyncProfiles;
            set => SetProperty(ref _cloudSyncProfiles, value);
        }

        /// <summary>
        /// Режим отладки
        /// </summary>
        public bool DebugMode
        {
            get => _debugMode;
            set => SetProperty(ref _debugMode, value);
        }

        /// <summary>
        /// Частота опроса устройств ввода
        /// </summary>
        public string InputPollingRate
        {
            get => _inputPollingRate;
            set => SetProperty(ref _inputPollingRate, value);
        }

        /// <summary>
        /// Приоритет процесса
        /// </summary>
        public string ProcessPriority
        {
            get => _processPriority;
            set => SetProperty(ref _processPriority, value);
        }

        /// <summary>
        /// Список доступных профилей
        /// </summary>
        public ObservableCollection<string> AvailableProfiles { get; }

        /// <summary>
        /// Список доступных частот опроса
        /// </summary>
        public ObservableCollection<string> AvailablePollingRates { get; }

        /// <summary>
        /// Список доступных приоритетов процесса
        /// </summary>
        public ObservableCollection<string> AvailablePriorities { get; }
        #endregion

        #region Свойства для определения темы
        /// <summary>
        /// Выбрана ли светлая тема
        /// </summary>
        public bool IsLightTheme
        {
            get => _theme == "Light";
            set
            {
                if (value)
                    Theme = "Light";
            }
        }

        /// <summary>
        /// Выбрана ли темная тема
        /// </summary>
        public bool IsDarkTheme
        {
            get => _theme == "Dark";
            set
            {
                if (value)
                    Theme = "Dark";
            }
        }

        /// <summary>
        /// Выбрана ли системная тема
        /// </summary>
        public bool IsSystemTheme
        {
            get => _theme == "System";
            set
            {
                if (value)
                    Theme = "System";
            }
        }
        #endregion

        #region Команды
        /// <summary>
        /// Команда сброса настроек на значения по умолчанию
        /// </summary>
        public ICommand ResetAllSettingsCommand { get; }

        /// <summary>
        /// Команда сохранения настроек
        /// </summary>
        public ICommand SaveSettingsCommand { get; }

        /// <summary>
        /// Команда экспорта настроек
        /// </summary>
        public ICommand ExportSettingsCommand { get; }

        /// <summary>
        /// Команда импорта настроек
        /// </summary>
        public ICommand ImportSettingsCommand { get; }
        #endregion

        /// <summary>
        /// Конструктор SettingsViewModel
        /// </summary>
        public SettingsViewModel()
        {
            // Инициализация свойств стандартными значениями
            StartWithWindows = true;
            StartMinimized = false;
            MinimizeToTray = true;
            ShowNotifications = true;

            Theme = "Dark";
            AccentColor = "Blue";

            AutoSwitchProfiles = true;
            DefaultProfile = "Default";
            CloudSyncProfiles = false;

            DebugMode = false;
            InputPollingRate = "1000 Hz";
            ProcessPriority = "High";

            // Инициализация коллекций
            AvailableProfiles = new ObservableCollection<string> { "Default", "Game", "Office", "Custom" };
            AvailablePollingRates = new ObservableCollection<string> { "125 Hz", "250 Hz", "500 Hz", "1000 Hz" };
            AvailablePriorities = new ObservableCollection<string> { "Low", "Normal", "High", "RealTime" };

            // Инициализация команд
            ResetAllSettingsCommand = new RelayCommand(OnResetAllSettings);
            SaveSettingsCommand = new RelayCommand(OnSaveSettings);
            ExportSettingsCommand = new RelayCommand(OnExportSettings);
            ImportSettingsCommand = new RelayCommand(OnImportSettings);
        }

        #region Обработчики команд
        private void OnResetAllSettings(object parameter)
        {
            // Заглушка для сброса настроек
        }

        private void OnSaveSettings(object parameter)
        {
            // Заглушка для сохранения настроек
        }

        private void OnExportSettings(object parameter)
        {
            // Заглушка для экспорта настроек
        }

        private void OnImportSettings(object parameter)
        {
            // Заглушка для импорта настроек
        }
        #endregion
    }
}