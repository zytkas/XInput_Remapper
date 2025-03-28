using System.Windows.Input;
using System.Threading.Tasks;
using MapperGang.Infrastructure.Commands;
using MapperGang.Services.ConfigService;
using MapperGang.Models;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек чувствительности
    /// </summary>
    public class SensitivityViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private ConfigModel _currentConfig;

        #region Приватные поля
        private double _mouseXAxisSensitivity;
        private double _mouseYAxisSensitivity;
        private string _mouseResponseCurveType;
        private bool _mouseAcceleration;
        private double _mouseSmoothing;
        private bool _mouseAxisLock;

        private double _joystickSensitivity;
        private double _joystickDeadzone;
        private string _joystickResponseCurveType;
        private bool _joystickAntiDeadzone;
        private bool _joystickRotation;
        private bool _joystickRadialDeadzone;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Чувствительность оси X мыши (0-100%)
        /// </summary>
        public double MouseXAxisSensitivity
        {
            get => _mouseXAxisSensitivity;
            set
            {
                if (SetProperty(ref _mouseXAxisSensitivity, value))
                {
                    // Дополнительные вычисления при необходимости
                    OnPropertyChanged(nameof(MouseSensitivityOverall));

                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseXAxisSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность оси Y мыши (0-100%)
        /// </summary>
        public double MouseYAxisSensitivity
        {
            get => _mouseYAxisSensitivity;
            set
            {
                if (SetProperty(ref _mouseYAxisSensitivity, value))
                {
                    // Дополнительные вычисления при необходимости
                    OnPropertyChanged(nameof(MouseSensitivityOverall));

                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseYAxisSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Общая чувствительность мыши (среднее между X и Y)
        /// </summary>
        public double MouseSensitivityOverall => (_mouseXAxisSensitivity + _mouseYAxisSensitivity) / 2;

        /// <summary>
        /// Тип кривой отклика мыши (Linear, S-Curve, Custom)
        /// </summary>
        public string MouseResponseCurveType
        {
            get => _mouseResponseCurveType;
            set
            {
                if (SetProperty(ref _mouseResponseCurveType, value))
                {
                    // Обновляем внешний вид всех кнопок кривых
                    OnPropertyChanged(nameof(MouseLinearCurveAppearance));
                    OnPropertyChanged(nameof(MouseSCurveAppearance));
                    OnPropertyChanged(nameof(MouseCustomCurveAppearance));

                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseResponseCurveType = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Appearance для кнопки линейной кривой мыши
        /// </summary>
        public ControlAppearance MouseLinearCurveAppearance =>
            _mouseResponseCurveType == "Linear" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Appearance для кнопки S-кривой мыши
        /// </summary>
        public ControlAppearance MouseSCurveAppearance =>
            _mouseResponseCurveType == "S-Curve" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Appearance для кнопки пользовательской кривой мыши
        /// </summary>
        public ControlAppearance MouseCustomCurveAppearance =>
            _mouseResponseCurveType == "Custom" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Включено ли ускорение мыши
        /// </summary>
        public bool MouseAcceleration
        {
            get => _mouseAcceleration;
            set
            {
                if (SetProperty(ref _mouseAcceleration, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseAcceleration = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Сглаживание движений мыши (0-100%)
        /// </summary>
        public double MouseSmoothing
        {
            get => _mouseSmoothing;
            set
            {
                if (SetProperty(ref _mouseSmoothing, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseSmoothing = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Включено ли сглаживание мыши
        /// </summary>
        public bool MouseSmoothingEnabled
        {
            get => _mouseSmoothing > 0;
            set
            {
                MouseSmoothing = value ? 30 : 0; // Используем 30% по умолчанию при включении

                // Здесь не нужно вызывать SaveSettingsAsync(),
                // так как оно будет вызвано из сеттера MouseSmoothing
            }
        }

        /// <summary>
        /// Блокировка движения по одной оси
        /// </summary>
        public bool MouseAxisLock
        {
            get => _mouseAxisLock;
            set
            {
                if (SetProperty(ref _mouseAxisLock, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.MouseAxisLock = value;
                        _ = SaveSettingsAsync();
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
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Мертвая зона джойстика (0-100%)
        /// </summary>
        public double JoystickDeadzone
        {
            get => _joystickDeadzone;
            set
            {
                if (SetProperty(ref _joystickDeadzone, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Тип кривой отклика джойстика (Linear, Step, Custom)
        /// </summary>
        public string JoystickResponseCurveType
        {
            get => _joystickResponseCurveType;
            set
            {
                if (SetProperty(ref _joystickResponseCurveType, value))
                {
                    // Обновляем внешний вид всех кнопок кривых
                    OnPropertyChanged(nameof(JoystickLinearCurveAppearance));
                    OnPropertyChanged(nameof(JoystickStepCurveAppearance));
                    OnPropertyChanged(nameof(JoystickCustomCurveAppearance));

                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickResponseCurveType = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Appearance для кнопки линейной кривой джойстика
        /// </summary>
        public ControlAppearance JoystickLinearCurveAppearance =>
            _joystickResponseCurveType == "Linear" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Appearance для кнопки ступенчатой кривой джойстика
        /// </summary>
        public ControlAppearance JoystickStepCurveAppearance =>
            _joystickResponseCurveType == "Step" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Appearance для кнопки пользовательской кривой джойстика
        /// </summary>
        public ControlAppearance JoystickCustomCurveAppearance =>
            _joystickResponseCurveType == "Custom" ? ControlAppearance.Primary : ControlAppearance.Secondary;

        /// <summary>
        /// Компенсация мертвой зоны джойстика
        /// </summary>
        public bool JoystickAntiDeadzone
        {
            get => _joystickAntiDeadzone;
            set
            {
                if (SetProperty(ref _joystickAntiDeadzone, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickAntiDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Поворот ввода джойстика
        /// </summary>
        public bool JoystickRotation
        {
            get => _joystickRotation;
            set
            {
                if (SetProperty(ref _joystickRotation, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickRotation = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Использование радиальной мертвой зоны
        /// </summary>
        public bool JoystickRadialDeadzone
        {
            get => _joystickRadialDeadzone;
            set
            {
                if (SetProperty(ref _joystickRadialDeadzone, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.SensitivitySettings.JoystickRadialDeadzone = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }
        #endregion

        #region Команды
        /// <summary>
        /// Команда редактирования кривой отклика мыши
        /// </summary>
        public ICommand EditMouseCurveCommand { get; }

        /// <summary>
        /// Команда редактирования кривой отклика джойстика
        /// </summary>
        public ICommand EditJoystickCurveCommand { get; }

        /// <summary>
        /// Команда выбора пресета кривой отклика мыши
        /// </summary>
        public ICommand SelectMouseCurvePresetCommand { get; }

        /// <summary>
        /// Команда выбора пресета кривой отклика джойстика
        /// </summary>
        public ICommand SelectJoystickCurvePresetCommand { get; }

        /// <summary>
        /// Команда сброса настроек на значения по умолчанию
        /// </summary>
        public ICommand ResetToDefaultsCommand { get; }

        /// <summary>
        /// Команда сохранения настроек
        /// </summary>
        public ICommand SaveMappingsCommand { get; }
        #endregion

        /// <summary>
        /// Конструктор SensitivityViewModel
        /// </summary>
        public SensitivityViewModel(IConfigService configService)
        {
            _configService = configService;

            // Инициализация команд
            EditMouseCurveCommand = new RelayCommand(async _ => await OnEditMouseCurve());
            EditJoystickCurveCommand = new RelayCommand(async _ => await OnEditJoystickCurve());
            SelectMouseCurvePresetCommand = new RelayCommand(OnSelectMouseCurvePreset);
            SelectJoystickCurvePresetCommand = new RelayCommand(OnSelectJoystickCurvePreset);

            // Добавляем новые команды
            ResetToDefaultsCommand = new RelayCommand(async _ => await OnResetToDefaults());
            SaveMappingsCommand = new RelayCommand(async _ => await OnSaveSettings());

            // Загрузка настроек
            _ = LoadSettingsAsync();
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private async Task LoadSettingsAsync()
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
            if (_currentConfig == null) return;

            var sensitivitySettings = _currentConfig.SensitivitySettings;

            // Обновляем свойства мыши
            MouseXAxisSensitivity = sensitivitySettings.MouseXAxisSensitivity;
            MouseYAxisSensitivity = sensitivitySettings.MouseYAxisSensitivity;
            MouseResponseCurveType = sensitivitySettings.MouseResponseCurveType;
            MouseAcceleration = sensitivitySettings.MouseAcceleration;
            MouseSmoothing = sensitivitySettings.MouseSmoothing;
            MouseAxisLock = sensitivitySettings.MouseAxisLock;

            // Обновляем свойства джойстика
            JoystickSensitivity = sensitivitySettings.JoystickSensitivity;
            JoystickDeadzone = sensitivitySettings.JoystickDeadzone;
            JoystickResponseCurveType = sensitivitySettings.JoystickResponseCurveType;
            JoystickAntiDeadzone = sensitivitySettings.JoystickAntiDeadzone;
            JoystickRotation = sensitivitySettings.JoystickRotation;
            JoystickRadialDeadzone = sensitivitySettings.JoystickRadialDeadzone;
        }

        #region Обработчики команд
        /// <summary>
        /// Обработчик команды редактирования кривой отклика мыши
        /// </summary>
        private async Task OnEditMouseCurve()
        {
            // В будущем здесь будет реализован редактор кривой отклика мыши
            System.Windows.MessageBox.Show("Редактор кривой отклика мыши будет реализован в будущих версиях.",
                                        "Информация",
                                        System.Windows.MessageBoxButton.OK,
                                        System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды редактирования кривой отклика джойстика
        /// </summary>
        private async Task OnEditJoystickCurve()
        {
            // В будущем здесь будет реализован редактор кривой отклика джойстика
            System.Windows.MessageBox.Show("Редактор кривой отклика джойстика будет реализован в будущих версиях.",
                                        "Информация",
                                        System.Windows.MessageBoxButton.OK,
                                        System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды выбора пресета кривой отклика мыши
        /// </summary>
        private void OnSelectMouseCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                MouseResponseCurveType = presetType;
                // Здесь можно добавить логику для изменения формы кривой
            }
        }

        /// <summary>
        /// Обработчик команды выбора пресета кривой отклика джойстика
        /// </summary>
        private void OnSelectJoystickCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                JoystickResponseCurveType = presetType;
                // Здесь можно добавить логику для изменения формы кривой
            }
        }

        /// <summary>
        /// Обработчик команды сброса настроек
        /// </summary>
        private async Task OnResetToDefaults()
        {
            // Запрашиваем подтверждение
            System.Windows.MessageBoxResult result = System.Windows.MessageBox.Show(
                "Сбросить настройки чувствительности к значениям по умолчанию?",
                "Сброс настроек",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // Создаем новые настройки чувствительности с значениями по умолчанию
                SensitivitySettingsModel defaultSettings = new SensitivitySettingsModel();

                // Обновляем настройки в текущей конфигурации
                if (_currentConfig != null)
                {
                    _currentConfig.SensitivitySettings = defaultSettings;
                    await SaveSettingsAsync();
                }

                // Обновляем свойства из новых настроек
                UpdatePropertiesFromConfig();

                System.Windows.MessageBox.Show("Настройки чувствительности сброшены к значениям по умолчанию.",
                                            "Сброс настроек",
                                            System.Windows.MessageBoxButton.OK,
                                            System.Windows.MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработчик команды сохранения настроек
        /// </summary>
        private async Task OnSaveSettings()
        {
            if (_currentConfig != null)
            {
                await SaveSettingsAsync();

                System.Windows.MessageBox.Show("Настройки чувствительности успешно сохранены.",
                                            "Сохранение настроек",
                                            System.Windows.MessageBoxButton.OK,
                                            System.Windows.MessageBoxImage.Information);
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
    }
}