using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using MapperGang.Infrastructure.Commands;
using MapperGang.Models;
using MapperGang.Services.ConfigResetService;
using MapperGang.Services.ConfigService;

namespace MapperGang.ViewModels
{
    public class ControllerViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private ConfigModel _currentConfig;
        

        #region Приватные поля
        private string _selectedControllerType;
        private string _controllerNumber;
        private bool _vibrationEnabled;
        private double _vibrationStrength;
        private double _buttonPressureSensitivity;
        private double _triggerDeadzone;
        private bool _hidePhysicalControllers;
        private bool _exclusiveMode;
        private bool _passThroughMode;
        private bool _combineInputs;
        private bool _autoConnect;
        private string _buttonAssignmentMode;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Выбранный тип контроллера
        /// </summary>
        public string SelectedControllerType
        {
            get => _selectedControllerType;
            set
            {
                if (SetProperty(ref _selectedControllerType, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.SelectedControllerType = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Номер контроллера
        /// </summary>
        public string ControllerNumber
        {
            get => _controllerNumber;
            set
            {
                if (SetProperty(ref _controllerNumber, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.ControllerNumber = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Включена ли вибрация
        /// </summary>
        public bool VibrationEnabled
        {
            get => _vibrationEnabled;
            set
            {
                if (SetProperty(ref _vibrationEnabled, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.VibrationEnabled = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Сила вибрации (0-100%)
        /// </summary>
        public double VibrationStrength
        {
            get => _vibrationStrength;
            set
            {
                if (SetProperty(ref _vibrationStrength, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.VibrationStrength = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность нажатия кнопок (0-100%)
        /// </summary>
        public double ButtonPressureSensitivity
        {
            get => _buttonPressureSensitivity;
            set
            {
                if (SetProperty(ref _buttonPressureSensitivity, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.ButtonPressureSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Мертвая зона триггера (0-100%)
        /// </summary>
        public double TriggerDeadzone
        {
            get => _triggerDeadzone;
            set
            {
                if (SetProperty(ref _triggerDeadzone, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.TriggerDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Скрыть физические контроллеры
        /// </summary>
        public bool HidePhysicalControllers
        {
            get => _hidePhysicalControllers;
            set
            {
                if (SetProperty(ref _hidePhysicalControllers, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.HidePhysicalControllers = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Эксклюзивный режим управления
        /// </summary>
        public bool ExclusiveMode
        {
            get => _exclusiveMode;
            set
            {
                if (SetProperty(ref _exclusiveMode, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.ExclusiveMode = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Проходной режим для физического контроллера
        /// </summary>
        public bool PassThroughMode
        {
            get => _passThroughMode;
            set
            {
                if (SetProperty(ref _passThroughMode, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.PassThroughMode = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Комбинирование входных данных
        /// </summary>
        public bool CombineInputs
        {
            get => _combineInputs;
            set
            {
                if (SetProperty(ref _combineInputs, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.CombineInputs = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Автоматическое подключение при запуске
        /// </summary>
        public bool AutoConnect
        {
            get => _autoConnect;
            set
            {
                if (SetProperty(ref _autoConnect, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.AutoConnect = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Режим назначения кнопок (Стандартный/Пользовательский)
        /// </summary>
        public string ButtonAssignmentMode
        {
            get => _buttonAssignmentMode;
            set
            {
                if (SetProperty(ref _buttonAssignmentMode, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.ControllerSettings.ButtonAssignmentMode = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Available controller types
        /// </summary>
        public ObservableCollection<string> AvailableControllerTypes { get; } =
        [
            "Xbox 360 Controller",
            "PlayStation Controller"
        ];

        /// <summary>
        /// Available controller numbers
        /// </summary>
        public ObservableCollection<string> AvailableControllerNumbers { get; } =
        [
            "Controller 1",
            "Controller 2",
            "Controller 3",
            "Controller 4"
        ];
        #endregion

        #region Команды
        /// <summary>
        /// Команда перезапуска устройства
        /// </summary>
        public ICommand RestartDeviceCommand { get; }

        /// <summary>
        /// Команда сохранения настроек
        /// </summary>
        public ICommand SaveSettingsCommand { get; }
        #endregion

        public ControllerViewModel(IConfigService configService, IConfigResetService resetService)
        {

            _configService = configService;
            resetService.ConfigurationReset += async (s, e) => await LoadSettingsAsync();   
            RestartDeviceCommand = new RelayCommand(async _ => await OnRestartDevice());
            SaveSettingsCommand = new RelayCommand(async _ => await OnSaveSettings());
            _ = LoadSettingsAsync();
        }


        private async Task LoadSettingsAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();
            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Обновление свойств на основе загруженной конфигурации
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            if (_currentConfig == null) return;

            var controllerSettings = _currentConfig.ControllerSettings;

            SelectedControllerType = controllerSettings.SelectedControllerType;
            ControllerNumber = controllerSettings.ControllerNumber;
            VibrationEnabled = controllerSettings.VibrationEnabled;
            VibrationStrength = controllerSettings.VibrationStrength;
            ButtonPressureSensitivity = controllerSettings.ButtonPressureSensitivity;
            TriggerDeadzone = controllerSettings.TriggerDeadzone;
            HidePhysicalControllers = controllerSettings.HidePhysicalControllers;
            ExclusiveMode = controllerSettings.ExclusiveMode;
            PassThroughMode = controllerSettings.PassThroughMode;
            CombineInputs = controllerSettings.CombineInputs;
            AutoConnect = controllerSettings.AutoConnect;
            ButtonAssignmentMode = controllerSettings.ButtonAssignmentMode;
        }

        #region Обработчики команд
        /// <summary>
        /// Обработчик команды перезапуска устройства
        /// </summary>
        private async Task OnRestartDevice()
        {
            // Здесь будет реализация перезапуска виртуального устройства
            // На данном этапе просто сообщим пользователю
            System.Windows.MessageBox.Show("Функция перезапуска устройства будет реализована в будущих версиях.",
                          "Информация", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды сохранения настроек
        /// </summary>
        private async Task OnSaveSettings()
        {
            await SaveSettingsAsync();
            System.Windows.MessageBox.Show("Настройки успешно сохранены.", "Сохранение настроек",
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        #endregion

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        private async Task SaveSettingsAsync()
        {
            if (_currentConfig == null) return;

            // Сохраняем конфигурацию
            await _configService.SaveConfigAsync(_currentConfig);
        }
    }
}