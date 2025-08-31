using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using MapperGangNET8.Infrastructure.Commands;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ProfileService;
using MapperGangNET8.Services.InputMappingService;

namespace MapperGangNET8.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly IProfileService _profileService;
        private readonly InputMappingService _inputMappingService;
        private ConfigModel _currentConfig;

        #region Приватные поля
        private bool _isDeviceActive;
        private string _deviceType;
        private string _deviceId;
        private string _activeProfile;
        private double _mouseSensitivity;
        private double _joystickSensitivity;
        private ViewModelBase _currentViewModel;
        private ObservableCollection<string> _availableProfiles;
        #endregion

        #region Публичные свойства

        /// <summary>
        /// Текущая активная ViewModel
        /// </summary>
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        /// <summary>
        /// Статус активности устройства
        /// </summary>
        public bool IsDeviceActive
        {
            get => _isDeviceActive;
            set => SetProperty(ref _isDeviceActive, value);
        }

        /// <summary>
        /// Тип эмулируемого устройства
        /// </summary>
        public string DeviceType
        {
            get => _deviceType;
            set => SetProperty(ref _deviceType, value);
        }

        /// <summary>
        /// Идентификатор устройства
        /// </summary>
        public string DeviceId
        {
            get => _deviceId;
            set => SetProperty(ref _deviceId, value);
        }

        /// <summary>
        /// Активный профиль настроек
        /// </summary>
        public string ActiveProfile
        {
            get => _activeProfile;
            set
            {
                if (SetProperty(ref _activeProfile, value))
                {
                    _ = SwitchProfileAsync(value);
                }
            }
        }

        /// <summary>
        /// Доступные профили
        /// </summary>
        public ObservableCollection<string> AvailableProfiles
        {
            get => _availableProfiles;
            set => SetProperty(ref _availableProfiles, value);
        }

        /// <summary>
        /// Чувствительность мыши (0-100%)
        /// </summary>
        public double MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                if (SetProperty(ref _mouseSensitivity, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseXAxisSensitivity = value;
                        _currentConfig.SensitivitySettings.MouseYAxisSensitivity = value;

                        // Автоматически сохраняем настройки
                        _ = SaveSettingsAsync();
                        
                        // Уведомляем InputMappingService об изменении конфигурации
                        _ = _inputMappingService.RefreshConfigurationAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность джойстика (0-100%)
        /// </summary>
        public double JoystickSensitivity
        {
            get => _joystickSensitivity;
            set
            {
                if (SetProperty(ref _joystickSensitivity, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickSensitivity = value;

                        // Автоматически сохраняем настройки
                        _ = SaveSettingsAsync();
                        
                        // Уведомляем InputMappingService об изменении конфигурации
                        _ = _inputMappingService.RefreshConfigurationAsync();
                    }
                }
            }
        }
        #endregion

        #region Команды
        /// <summary>
        /// Команда перезапуска устройства
        /// </summary>
        public ICommand RestartDeviceCommand { get; }

        /// <summary>
        /// Команда создания нового профиля
        /// </summary>
        public ICommand NewProfileCommand { get; }

        /// <summary>
        /// Команда сохранения настроек
        /// </summary>
        public ICommand SaveSettingsCommand { get; }

        /// <summary>
        /// Команда навигации
        /// </summary>
        public ICommand NavigateCommand { get; }
        
        /// <summary>
        /// Команда запуска/остановки маппинга
        /// </summary>
        public ICommand ToggleMappingCommand { get; }
        #endregion

        // Зависимости для навигации

        /// <summary>
        /// Конструктор MainViewModel
        /// </summary>
        public MainViewModel(IConfigService configService,
                            IProfileService profileService,
                            InputMappingService inputMappingService)
        {
            _configService = configService;
            _profileService = profileService;
            _inputMappingService = inputMappingService;
            // Инициализация свойств по умолчанию
            AvailableProfiles = new ObservableCollection<string>();

            // Инициализация команд
            RestartDeviceCommand = new RelayCommand(async _ => await OnRestartDevice());
            NewProfileCommand = new RelayCommand(async _ => await OnNewProfile());
            SaveSettingsCommand = new RelayCommand(async _ => await OnSaveSettings());
            ToggleMappingCommand = new RelayCommand(async _ => await OnToggleMapping());
            //NavigateCommand = new RelayCommand(OnNavigate);

            // Загрузка настроек и профилей
            _ = InitializeAsync();
            
            // Подключить контроллер при запуске приложения (но не активировать маппинг)
            _ = InitializeControllerAsync();
        }

        /// <summary>
        /// Асинхронная инициализация
        /// </summary>
        private async Task InitializeAsync()
        {
            // Загружаем конфигурацию
            _currentConfig = await _configService.LoadConfigAsync();

            // Загружаем список профилей
            var profiles = await _profileService.GetProfilesAsync();
            AvailableProfiles.Clear();
            foreach (var profile in profiles)
            {
                AvailableProfiles.Add(profile);
            }

            // Получаем активный профиль
            string activeProfileName = await _profileService.GetActiveProfileNameAsync();
            ActiveProfile = activeProfileName;

            // Обновляем свойства
            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Обновление свойств на основе загруженной конфигурации
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            if (_currentConfig == null) return;

            // Базовые свойства
            DeviceType = _currentConfig.AppSettings.SelectedControllerType;
            DeviceId = "VID_045E&PID_028E"; // Пример идентификатора Xbox-контроллера
            IsDeviceActive = true; // По умолчанию устройство активно

            // Настройки чувствительности
            MouseSensitivity = _currentConfig.SensitivitySettings.MouseXAxisSensitivity;
            JoystickSensitivity = _currentConfig.SensitivitySettings.JoystickSensitivity;
        }

        #region Обработчики команд
        /// <summary>
        /// Обработчик команды перезапуска устройства
        /// </summary>
        private async Task OnRestartDevice()
        {
            // Здесь будет реализация перезапуска виртуального устройства
            // На данном этапе просто сообщим пользователю
            MessageBox.Show("Функция перезапуска устройства будет реализована в будущих версиях.",
                          "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды создания нового профиля
        /// </summary>
        private async Task OnNewProfile()
        {
            // Создаем диалог для ввода имени профиля
            // Простая реализация с использованием MessageBox.Show
            string defaultName = "Новый профиль";

            // В реальном приложении здесь будет использоваться кастомный диалог
            string profileName = Microsoft.VisualBasic.Interaction.InputBox(
                "Введите имя нового профиля:", "Создание профиля", defaultName);

            if (!string.IsNullOrWhiteSpace(profileName))
            {
                // Создаем новый профиль
                await _profileService.CreateProfileAsync(profileName, "Пользовательский профиль");

                // Обновляем список доступных профилей
                var profiles = await _profileService.GetProfilesAsync();
                AvailableProfiles.Clear();
                foreach (var profile in profiles)
                {
                    AvailableProfiles.Add(profile);
                }

                // Переключаемся на новый профиль
                ActiveProfile = profileName;
            }
        }

        /// <summary>
        /// Обработчик команды сохранения настроек
        /// </summary>
        private async Task OnSaveSettings()
        {
            await SaveSettingsAsync();
            MessageBox.Show("Настройки успешно сохранены.", "Сохранение настроек",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды запуска/остановки маппинга
        /// </summary>
        private async Task OnToggleMapping()
        {
            if (IsDeviceActive)
            {
                // Остановить маппинг (контроллер остается подключенным)
                await _inputMappingService.SetMappingEnabledAsync(false);
                IsDeviceActive = false;
            }
            else
            {
                // Запустить маппинг (подключит контроллер если нужно)
                await _inputMappingService.SetMappingEnabledAsync(true);
                IsDeviceActive = true;
            }
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

        /// <summary>
        /// Инициализация контроллера при запуске приложения
        /// </summary>
        private async Task InitializeControllerAsync()
        {
            try
            {
                // Подключаем контроллер заранее для быстрого запуска маппинга
                bool connected = await _inputMappingService.ConnectControllerAsync();
                
                if (connected)
                {
                    DeviceType = _currentConfig?.AppSettings?.SelectedControllerType ?? "Xbox 360 Controller";
                    DeviceId = "VID_045E&PID_028E"; // Default Xbox 360 ID
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Controller initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Переключение профиля
        /// </summary>
        private async Task SwitchProfileAsync(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName)) return;

            // Переключаемся на выбранный профиль
            await _profileService.SwitchToProfileAsync(profileName);

            // Обновляем настройки
            _currentConfig = await _configService.LoadConfigAsync();
            UpdatePropertiesFromConfig();
            
            // Уведомляем InputMappingService об изменении конфигурации
            await _inputMappingService.RefreshConfigurationAsync();
        }

    } 
}