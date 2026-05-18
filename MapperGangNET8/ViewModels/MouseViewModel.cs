using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using MapperGangNET8.Infrastructure.Commands;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.MappingService;

namespace MapperGangNET8.ViewModels
{
    public class MouseViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private readonly InputPipeline _inputPipeline;
        private ConfigModel _currentConfig;

        #region Приватные поля
        private string _mouseJoystickMode;
        private double _mouseSensitivity;
        private double _mouseSensitivityX;
        private double _mouseSensitivityY;
        private bool _invertXAxis;
        private bool _invertYAxis;
        private double _scaleFactorX;
        private double _scaleFactorY;
        private double _mouseSmoothing;
        private double _noiseFilter;
        private int _returnTime;
        private string _responseCurveType;
        private ObservableCollection<MouseButtonMapping> _buttonMappings;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Режим преобразования движения мыши в джойстик
        /// Spring Mode = авто-возврат стика в центр
        /// Absolute Position = накопление позиции без возврата
        /// </summary>
        public string MouseJoystickMode
        {
            get => _mouseJoystickMode;
            set
            {
                if (SetProperty(ref _mouseJoystickMode, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.MouseJoystickMode = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }
        public ObservableCollection<string> AvailableJoystickModes { get; } =
        [
        "Spring Mode",
        "Absolute Position"
        ];

        public ObservableCollection<string> AvailableResponseCurves { get; } =
        [
          "Linear",
          "Precision",
          "Aggressive"
        ];

        /// <summary>
        /// Чувствительность мыши (0-200%)
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
                        _currentConfig.MouseSettings.MouseSensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность мыши по оси X (0-200%)
        /// </summary>
        public double MouseSensitivityX
        {
            get => _mouseSensitivityX;
            set
            {
                if (SetProperty(ref _mouseSensitivityX, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.MouseSensitivityX = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность мыши по оси Y (0-200%)
        /// </summary>
        public double MouseSensitivityY
        {
            get => _mouseSensitivityY;
            set
            {
                if (SetProperty(ref _mouseSensitivityY, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.MouseSensitivityY = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Инвертировать ось X
        /// </summary>
        public bool InvertXAxis
        {
            get => _invertXAxis;
            set
            {
                if (SetProperty(ref _invertXAxis, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.InvertXAxis = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Инвертировать ось Y
        /// </summary>
        public bool InvertYAxis
        {
            get => _invertYAxis;
            set
            {
                if (SetProperty(ref _invertYAxis, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.InvertYAxis = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Масштабный коэффициент по оси X (0-200%, 100% = 10000)
        /// </summary>
        public double ScaleFactorX
        {
            get => _scaleFactorX;
            set
            {
                if (SetProperty(ref _scaleFactorX, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.ScaleFactorX = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Масштабный коэффициент по оси Y (0-200%, 100% = 10000)
        /// </summary>
        public double ScaleFactorY
        {
            get => _scaleFactorY;
            set
            {
                if (SetProperty(ref _scaleFactorY, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.ScaleFactorY = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Сглаживание движений мыши (0-10)
        /// </summary>
        public double MouseSmoothing
        {
            get => _mouseSmoothing;
            set
            {
                if (SetProperty(ref _mouseSmoothing, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.MouseSmoothing = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Фильтр шума (0-10, по умолчанию 0)
        /// </summary>
        public double NoiseFilter
        {
            get => _noiseFilter;
            set
            {
                if (SetProperty(ref _noiseFilter, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.NoiseFilter = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Время авто-возврата в миллисекундах (5-255ms, по умолчанию 30ms)
        /// </summary>
        public int ReturnTime
        {
            get => _returnTime;
            set
            {
                if (SetProperty(ref _returnTime, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.ReturnTime = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Тип кривой отклика (Linear, Precision, Aggressive)
        /// </summary>
        public string ResponseCurveType
        {
            get => _responseCurveType;
            set
            {
                if (SetProperty(ref _responseCurveType, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.ResponseCurveType = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Коллекция маппингов кнопок мыши
        /// </summary>
        public ObservableCollection<MouseButtonMapping> ButtonMappings
        {
            get => _buttonMappings;
            set => SetProperty(ref _buttonMappings, value);
        }

        /// <summary>
        /// Список доступных кнопок мыши
        /// </summary>
        public List<string> AvailableMouseButtons => InputKeyMap.GetAvailableMouseButtons();

        /// <summary>
        /// Список доступных действий контроллера
        /// </summary>
        public List<string> AvailableControllerActions => InputKeyMap.GetAvailableControllerActions();

        /// <summary>
        /// Команда для удаления маппинга
        /// </summary>
        public ICommand RemoveMappingCommand { get; }
        #endregion

        #region Команды
        /// <summary>
        /// Команда сброса настроек на значения по умолчанию
        /// </summary>
        public ICommand ResetToDefaultsCommand { get; }

        /// <summary>
        /// Команда сохранения маппингов
        /// </summary>
        public ICommand SaveMappingsCommand { get; }

        /// <summary>
        /// Команда добавления нового маппинга
        /// </summary>
        public ICommand AddMappingCommand { get; }

        /// <summary>
        /// Команда установки кривой отклика
        /// </summary>
        public ICommand SetResponseCurveCommand { get; }
        #endregion

        /// <summary>
        /// Конструктор MouseViewModel
        /// </summary>
        public MouseViewModel(IConfigService configService, InputPipeline inputPipeline)
        {
            _configService = configService;
            _inputPipeline = inputPipeline;

            ButtonMappings = new ObservableCollection<MouseButtonMapping>();

            ResetToDefaultsCommand = new RelayCommand(async _ => await OnResetToDefaults());
            SaveMappingsCommand = new RelayCommand(async _ => await OnSaveMappings());
            AddMappingCommand = new RelayCommand(_ => OnAddMapping());
            RemoveMappingCommand = new RelayCommand(OnRemoveMapping);
            SetResponseCurveCommand = new RelayCommand(OnSetResponseCurve);

            _ = LoadSettingsAsync();
        }

        /// <summary>
        /// Загрузка настроек
        /// </summary>
        private async Task LoadSettingsAsync()
        {
            _currentConfig = await _configService.LoadConfigAsync();

            if (_currentConfig?.MouseSettings?.ButtonMappings != null)
            {
                string debug = "Загруженные маппинги мыши:\n";
                foreach (var mapping in _currentConfig.MouseSettings.ButtonMappings)
                {
                    debug += $"{mapping.MouseButton} -> {mapping.ControllerButton}\n";
                }
                System.Windows.MessageBox.Show(debug, "Загруженные маппинги мыши");
            }
            else
            {
                System.Windows.MessageBox.Show("Маппинги мыши не найдены или пусты", "Загруженные маппинги");
            }

            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Обновление свойств на основе загруженной конфигурации
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            if (_currentConfig == null) return;

            var mouseSettings = _currentConfig.MouseSettings;

            MouseJoystickMode = mouseSettings.MouseJoystickMode;
            MouseSensitivity = mouseSettings.MouseSensitivity;
            MouseSensitivityX = mouseSettings.MouseSensitivityX;
            MouseSensitivityY = mouseSettings.MouseSensitivityY;
            InvertXAxis = mouseSettings.InvertXAxis;
            InvertYAxis = mouseSettings.InvertYAxis;
            ScaleFactorX = mouseSettings.ScaleFactorX;
            ScaleFactorY = mouseSettings.ScaleFactorY;
            MouseSmoothing = mouseSettings.MouseSmoothing;
            NoiseFilter = mouseSettings.NoiseFilter;
            ReturnTime = mouseSettings.ReturnTime;
            ResponseCurveType = mouseSettings.ResponseCurveType;

            ButtonMappings.Clear();
            foreach (var mapping in mouseSettings.ButtonMappings)
            {
                string mouseButton = mapping.MouseButton;
                string controllerButton = mapping.ControllerButton;

                if (!AvailableMouseButtons.Contains(mouseButton))
                    mouseButton = AvailableMouseButtons.FirstOrDefault() ?? "Left Button";

                if (!AvailableControllerActions.Contains(controllerButton))
                    controllerButton = AvailableControllerActions.FirstOrDefault() ?? "A Button";

                ButtonMappings.Add(new MouseButtonMapping
                {
                    MouseButton = mouseButton,
                    ControllerButton = controllerButton
                });
            }
        }

        #region Обработчики команд
        /// <summary>
        /// Обработчик команды сброса настроек
        /// </summary>
        private async Task OnResetToDefaults()
        {
            MouseSettingsModel defaultSettings = new MouseSettingsModel();

            if (_currentConfig != null)
            {
                _currentConfig.MouseSettings = defaultSettings;
                await SaveSettingsAsync();
            }

            UpdatePropertiesFromConfig();

            System.Windows.MessageBox.Show("Настройки мыши сброшены к значениям по умолчанию.", "Сброс настроек",
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Установить кривую отклика
        /// </summary>
        private void OnSetResponseCurve(object parameter)
        {
            if (parameter is string curveType)
            {
                ResponseCurveType = curveType;
            }
        }

        /// <summary>
        /// Обработчик команды сохранения маппингов
        /// </summary>
        private async Task OnSaveMappings()
        {
            string debug = "Сохраняемые маппинги мыши:\n";
            foreach (var mapping in ButtonMappings)
            {
                debug += $"{mapping.MouseButton} -> {mapping.ControllerButton}\n";
            }
            System.Windows.MessageBox.Show(debug, "Отладка");


            if (_currentConfig != null)
{
                _currentConfig.MouseSettings.ButtonMappings = ButtonMappings
                    .Select(m => new MouseButtonMappingModel
                    {
                        MouseButton = m.MouseButton,
                        ControllerButton = m.ControllerButton
                    })
                    .ToList();
                await SaveSettingsAsync();

                System.Windows.MessageBox.Show("Настройки маппинга мыши успешно сохранены.", "Сохранение настроек",
                              System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// Обработчик команды добавления нового маппинга
        /// </summary>
        private void OnAddMapping()
        {

            string defaultButton = AvailableMouseButtons.FirstOrDefault() ?? "Left Button";
            string defaultAction = AvailableControllerActions.FirstOrDefault() ?? "A Button";

            ButtonMappings.Add(new MouseButtonMapping
            {
                MouseButton = defaultButton,
                ControllerButton = defaultAction
            });

            // Сохраняем изменения
            _ = OnSaveMappings();
        }

        /// <summary>
        /// Обработчик команды удаления маппинга
        /// </summary>
        private void OnRemoveMapping(object parameter)
        {
            if (parameter is MouseButtonMapping mapping)
            {
                ButtonMappings.Remove(mapping);

                // Автоматически сохраняем изменения
                if (_currentConfig != null)
                {
                    // Преобразуем ObservableCollection в List
                    _currentConfig.MouseSettings.ButtonMappings = ButtonMappings
                        .Select(m => new MouseButtonMappingModel
                        {
                            MouseButton = m.MouseButton,
                            ControllerButton = m.ControllerButton
                        })
                        .ToList();

                    _ = SaveSettingsAsync();
                }
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

            // Уведомляем InputPipeline об изменении конфигурации
            await _inputPipeline.RefreshConfigurationAsync();
        }
    }

    /// <summary>
    /// Модель для хранения маппинга кнопок мыши
    /// </summary>
    public class MouseButtonMapping : ViewModelBase
    {
        private string _mouseButton;
        private string _controllerButton;

        /// <summary>
        /// Кнопка мыши
        /// </summary>
        public string MouseButton
        {
            get => _mouseButton;
            set => SetProperty(ref _mouseButton, value);
        }

        /// <summary>
        /// Кнопка контроллера
        /// </summary>
        public string ControllerButton
        {
            get => _controllerButton;
            set => SetProperty(ref _controllerButton, value);
        }
    }
}