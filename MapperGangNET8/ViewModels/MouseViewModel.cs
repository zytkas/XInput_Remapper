using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using MapperGang.Infrastructure.Commands;
using MapperGang.Services.ConfigService;
using MapperGang.Models;
using System.Linq;

namespace MapperGang.ViewModels
{
    public class MouseViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private ConfigModel _currentConfig;

        #region Приватные поля
        private string _mouseJoystickMode;
        private double _mouseSensitivity;
        private bool _invertXAxis;
        private bool _invertYAxis;
        private bool _mouseAcceleration;
        private double _mouseSmoothing;
        private string _mouseWheelMapping;
        private ObservableCollection<MouseButtonMapping> _buttonMappings;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Режим преобразования движения мыши в джойстик
        /// </summary>
        public string MouseJoystickMode
        {
            get => _mouseJoystickMode;
            set
            {
                if (SetProperty(ref _mouseJoystickMode, value))
                {
                    // Обновляем настройки в текущей конфигурации
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
        "Absolute Position",
        "Relative Movement",
        "Direct Input"
        ];

        public ObservableCollection<string> AvailableWheelMappings { get; } =
        [
          "Right Stick Y-Axis",
          "Triggers",
          "D-Pad Up/Down",
          "Not Mapped"
        ];

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
                        _currentConfig.MouseSettings.MouseSensitivity = value;
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
        /// Включить ускорение мыши
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
                        _currentConfig.MouseSettings.MouseAcceleration = value;
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
                        _currentConfig.MouseSettings.MouseSmoothing = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Маппинг колеса мыши
        /// </summary>
        public string MouseWheelMapping
        {
            get => _mouseWheelMapping;
            set
            {
                if (SetProperty(ref _mouseWheelMapping, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.MouseSettings.MouseWheelMapping = value;
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
        public List<string> AvailableMouseButtons { get; } = new List<string>
        {
            "Left Button",
            "Right Button",
            "Middle Button",
            "Side Button 1",
            "Side Button 2",
            "Mouse Wheel Up",
            "Mouse Wheel Down",
            "Extra Button 1",
            "Extra Button 2"
        };

        /// <summary>
        /// Список доступных действий контроллера
        /// </summary>
        public List<string> AvailableControllerActions { get; } = new List<string>
        {
            "A Button",
            "B Button",
            "X Button",
            "Y Button",
            "Left Bumper",
            "Right Bumper",
            "Left Trigger",
            "Right Trigger",
            "Left Stick Press",
            "Right Stick Press",
            "D-Pad Up",
            "D-Pad Down",
            "D-Pad Left",
            "D-Pad Right",
            "Start Button",
            "Back Button",
            "Guide Button"
        };

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
        #endregion

        /// <summary>
        /// Конструктор MouseViewModel
        /// </summary>
        public MouseViewModel(IConfigService configService)
        {
            _configService = configService;

            ButtonMappings = new ObservableCollection<MouseButtonMapping>();

            ResetToDefaultsCommand = new RelayCommand(async _ => await OnResetToDefaults());
            SaveMappingsCommand = new RelayCommand(async _ => await OnSaveMappings());
            AddMappingCommand = new RelayCommand(_ => OnAddMapping());
            RemoveMappingCommand = new RelayCommand(OnRemoveMapping);

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
            InvertXAxis = mouseSettings.InvertXAxis;
            InvertYAxis = mouseSettings.InvertYAxis;
            MouseAcceleration = mouseSettings.MouseAcceleration;
            MouseSmoothing = mouseSettings.MouseSmoothing;
            MouseWheelMapping = mouseSettings.MouseWheelMapping;

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