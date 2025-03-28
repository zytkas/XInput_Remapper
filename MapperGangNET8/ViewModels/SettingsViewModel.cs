using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Win32;
using MapperGang.Infrastructure.Commands;
using MapperGang.Models;
using MapperGang.Services.ConfigService;
using System.Windows;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек приложения
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private ConfigModel _currentConfig;

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
        /// Запуск приложения вместе с Windows
        /// </summary>
        public bool StartWithWindows
        {
            get => _startWithWindows;
            set
            {
                if (SetProperty(ref _startWithWindows, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.StartWithWindows = value;
                    UpdateAutoStartRegistry(value);
                }
            }
        }

        /// <summary>
        /// Запуск приложения в свернутом виде
        /// </summary>
        public bool StartMinimized
        {
            get => _startMinimized;
            set
            {
                if (SetProperty(ref _startMinimized, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.StartMinimized = value;
                }
            }
        }

        /// <summary>
        /// Сворачивать в системный трей вместо панели задач
        /// </summary>
        public bool MinimizeToTray
        {
            get => _minimizeToTray;
            set
            {
                if (SetProperty(ref _minimizeToTray, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.MinimizeToTray = value;
                }
            }
        }

        /// <summary>
        /// Показывать уведомления
        /// </summary>
        public bool ShowNotifications
        {
            get => _showNotifications;
            set
            {
                if (SetProperty(ref _showNotifications, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.ShowNotifications = value;
                }
            }
        }

        /// <summary>
        /// Тема приложения (Light, Dark, System)
        /// </summary>
        public string Theme
        {
            get => _theme;
            set
            {
                if (SetProperty(ref _theme, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.Theme = value;
                    OnPropertyChanged(nameof(IsLightTheme));
                    OnPropertyChanged(nameof(IsDarkTheme));
                    OnPropertyChanged(nameof(IsSystemTheme));
                    ApplyTheme(value);
                }
            }
        }

        /// <summary>
        /// Цвет акцента приложения
        /// </summary>
        public string AccentColor
        {
            get => _accentColor;
            set
            {
                if (SetProperty(ref _accentColor, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.AccentColor = value;
                    ApplyAccentColor(value);
                }
            }
        }

        /// <summary>
        /// Автоматически переключать профили в зависимости от активного приложения
        /// </summary>
        public bool AutoSwitchProfiles
        {
            get => _autoSwitchProfiles;
            set
            {
                if (SetProperty(ref _autoSwitchProfiles, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.AutoSwitchProfiles = value;
                }
            }
        }

        /// <summary>
        /// Профиль по умолчанию
        /// </summary>
        public string DefaultProfile
        {
            get => _defaultProfile;
            set
            {
                if (SetProperty(ref _defaultProfile, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.DefaultProfile = value;
                }
            }
        }

        /// <summary>
        /// Синхронизировать профили через облако
        /// </summary>
        public bool CloudSyncProfiles
        {
            get => _cloudSyncProfiles;
            set
            {
                if (SetProperty(ref _cloudSyncProfiles, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.CloudSyncProfiles = value;
                }
            }
        }

        /// <summary>
        /// Режим отладки
        /// </summary>
        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                if (SetProperty(ref _debugMode, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.DebugMode = value;
                }
            }
        }

        /// <summary>
        /// Частота опроса устройств ввода
        /// </summary>
        public string InputPollingRate
        {
            get => _inputPollingRate;
            set
            {
                if (SetProperty(ref _inputPollingRate, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.InputPollingRate = value;
                }
            }
        }

        /// <summary>
        /// Приоритет процесса
        /// </summary>
        public string ProcessPriority
        {
            get => _processPriority;
            set
            {
                if (SetProperty(ref _processPriority, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    _currentConfig.AppSettings.ProcessPriority = value;
                }
            }
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
        public SettingsViewModel(IConfigService configService)
        {
            _configService = configService;

            // Инициализация коллекций
            AvailableProfiles = new ObservableCollection<string> { "Default", "Game", "Office", "Custom" };
            AvailablePollingRates = new ObservableCollection<string> { "125 Hz", "250 Hz", "500 Hz", "1000 Hz" };
            AvailablePriorities = new ObservableCollection<string> { "Low", "Normal", "High", "RealTime" };

            // Инициализация команд
            ResetAllSettingsCommand = new RelayCommand(async _ => await OnResetAllSettings());
            SaveSettingsCommand = new RelayCommand(async _ => await OnSaveSettings());
            ExportSettingsCommand = new RelayCommand(async _ => await OnExportSettings());
            ImportSettingsCommand = new RelayCommand(async _ => await OnImportSettings());

            // Загрузка настроек
            _ = LoadSettings();
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private async Task LoadSettings()
        {
            // Загружаем конфигурацию
            _currentConfig = await _configService.LoadConfigAsync();

            // Обновляем свойства
            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Обновление свойств на основе загруженной конфигурации
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            var appSettings = _currentConfig.AppSettings;

            // Обновляем свойства
            StartWithWindows = appSettings.StartWithWindows;
            StartMinimized = appSettings.StartMinimized;
            MinimizeToTray = appSettings.MinimizeToTray;
            ShowNotifications = appSettings.ShowNotifications;
            Theme = appSettings.Theme;
            AccentColor = appSettings.AccentColor;
            AutoSwitchProfiles = appSettings.AutoSwitchProfiles;
            DefaultProfile = appSettings.DefaultProfile;
            CloudSyncProfiles = appSettings.CloudSyncProfiles;
            DebugMode = appSettings.DebugMode;
            InputPollingRate = appSettings.InputPollingRate;
            ProcessPriority = appSettings.ProcessPriority;

            // Проверяем, есть ли профиль по умолчанию в списке доступных профилей
            if (!AvailableProfiles.Contains(DefaultProfile))
            {
                AvailableProfiles.Add(DefaultProfile);
            }
        }

        #region Обработчики команд
        /// <summary>
        /// Обработчик команды сброса настроек
        /// </summary>
        private async Task OnResetAllSettings()
        {
            // Запрашиваем подтверждение
            MessageBoxResult result = MessageBox.Show(
                "Вы уверены, что хотите сбросить все настройки к значениям по умолчанию?",
                "Подтверждение сброса",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Сбрасываем конфигурацию
                _currentConfig = await _configService.ResetConfigAsync();

                // Обновляем свойства
                UpdatePropertiesFromConfig();

                MessageBox.Show("Настройки успешно сброшены.", "Сброс настроек",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработчик команды сохранения настроек
        /// </summary>
        private async Task OnSaveSettings()
        {
            // Сохраняем конфигурацию
            await _configService.SaveConfigAsync(_currentConfig);

            MessageBox.Show("Настройки успешно сохранены.", "Сохранение настроек",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды экспорта настроек
        /// </summary>
        private async Task OnExportSettings()
        {
            // Создаем диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                Title = "Экспорт настроек",
                FileName = "mapper_gang_config.json"
            };

            // Если пользователь выбрал файл, экспортируем настройки
            if (saveFileDialog.ShowDialog() == true)
            {
                await _configService.ExportConfigAsync(_currentConfig, saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// Обработчик команды импорта настроек
        /// </summary>
        private async Task OnImportSettings()
        {
            // Создаем диалог открытия файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "JSON файлы (*.json)|*.json",
                Title = "Импорт настроек"
            };

            // Если пользователь выбрал файл, импортируем настройки
            if (openFileDialog.ShowDialog() == true)
            {
                _currentConfig = await _configService.ImportConfigAsync(openFileDialog.FileName);
                UpdatePropertiesFromConfig();
            }
        }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Обновление записи автозапуска в реестре Windows
        /// </summary>
        private void UpdateAutoStartRegistry(bool enable)
        {
            try
            {
                // Открываем раздел реестра для автозапуска
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    if (key != null)
                    {
                        if (enable)
                        {
                            // Получаем путь к исполняемому файлу приложения
                            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                            key.SetValue("MapperGang", appPath);
                        }
                        else
                        {
                            // Удаляем запись из реестра
                            if (key.GetValue("MapperGang") != null)
                            {
                                key.DeleteValue("MapperGang", false);
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении автозапуска: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применение темы
        /// </summary>
        private void ApplyTheme(string theme)
        {
            // В будущем здесь будет реализована логика изменения темы
        }

        /// <summary>
        /// Применение цвета акцента
        /// </summary>
        private void ApplyAccentColor(string accentColor)
        {
            // В будущем здесь будет реализована логика изменения цвета акцента
        }
        #endregion
    }
}