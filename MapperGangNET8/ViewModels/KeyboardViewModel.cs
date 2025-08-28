using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGangNET8.Infrastructure.Commands;
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigResetService;
using MapperGangNET8.Services.ConfigService;

namespace MapperGangNET8.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек клавиатуры
    /// </summary>
    public class KeyboardViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;
        private ConfigModel _currentConfig;

        #region Приватные поля
        private string _keyboardLayout;
        private bool _keyRepeatEnabled;
        private double _keyRepeatRate;
        private double _analogKeySensitivity;
        private string _movementUp;
        private string _movementLeft;
        private string _movementDown;
        private string _movementRight;
        private string _movementStyle;
        private bool _analogKeyboardEnabled;
        private ObservableCollection<KeyboardButtonMapping> _buttonMappings;
        #endregion

        #region Публичные свойства
        /// <summary>
        /// Выбранная раскладка клавиатуры
        /// </summary>
        public string KeyboardLayout
        {
            get => _keyboardLayout;
            set
            {
                if (SetProperty(ref _keyboardLayout, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.KeyboardLayout = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Включен ли повтор клавиш
        /// </summary>
        public bool KeyRepeatEnabled
        {
            get => _keyRepeatEnabled;
            set
            {
                if (SetProperty(ref _keyRepeatEnabled, value))
                {
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.KeyRepeatEnabled = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Скорость повтора клавиш (0-100%)
        /// </summary>
        public double KeyRepeatRate
        {
            get => _keyRepeatRate;
            set
            {
                if (SetProperty(ref _keyRepeatRate, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.KeyRepeatRate = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Чувствительность аналоговых клавиш (0-100%)
        /// </summary>
        public double AnalogKeySensitivity
        {
            get => _analogKeySensitivity;
            set
            {
                if (SetProperty(ref _analogKeySensitivity, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.AnalogKeySensitivity = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Клавиша движения вверх
        /// </summary>
        public string MovementUp
        {
            get => _movementUp;
            set
            {
                if (SetProperty(ref _movementUp, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.MovementUp = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Клавиша движения влево
        /// </summary>
        public string MovementLeft
        {
            get => _movementLeft;
            set
            {
                if (SetProperty(ref _movementLeft, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.MovementLeft = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Клавиша движения вниз
        /// </summary>
        public string MovementDown
        {
            get => _movementDown;
            set
            {
                if (SetProperty(ref _movementDown, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.MovementDown = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Клавиша движения вправо
        /// </summary>
        public string MovementRight
        {
            get => _movementRight;
            set
            {
                if (SetProperty(ref _movementRight, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.MovementRight = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Стиль движения (8-Way/4-Way)
        /// </summary>
        public string MovementStyle
        {
            get => _movementStyle;
            set
            {
                if (SetProperty(ref _movementStyle, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.MovementStyle = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Включить аналоговый режим клавиатуры
        /// </summary>
        public bool AnalogKeyboardEnabled
        {
            get => _analogKeyboardEnabled;
            set
            {
                if (SetProperty(ref _analogKeyboardEnabled, value))
                {
                    // Обновляем настройки в текущей конфигурации
                    if (_currentConfig != null)
                    {
                        _currentConfig.KeyboardSettings.AnalogKeyboardEnabled = value;
                        _ = SaveSettingsAsync();
                    }
                }
            }
        }


        /// <summary>
        /// Список доступных клавиш клавиатуры
        /// </summary>
        public List<string> AvailableKeyboardKeys => InputKeyMap.GetAvailableKeyboardKeys();

        /// <summary>
        /// Список доступных действий контроллера
        /// </summary>
        public List<string> AvailableControllerActions => InputKeyMap.GetAvailableControllerActions();
        /// <summary>
        /// Коллекция маппингов кнопок клавиатуры
        /// </summary>
        public ObservableCollection<KeyboardButtonMapping> ButtonMappings
        {
            get => _buttonMappings;
            set => SetProperty(ref _buttonMappings, value);
        }
        /// <summary>
        /// Available keyboard layouts
        /// </summary>
        public ObservableCollection<string> AvailableKeyboardLayouts { get; } =
        [
            "QWERTY",
            "AZERTY",
            "DVORAK",
            "COLEMAK"
        ];
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
        /// Команда удаления маппинга
        /// </summary>
        public ICommand RemoveMappingCommand { get; }
        #endregion

        /// <summary>
        /// Конструктор KeyboardViewModel
        /// </summary>
        public KeyboardViewModel(IConfigService configService, IConfigResetService resetService)
        {
            _configService = configService;
            resetService.ConfigurationReset += async (s, e) => await LoadSettingsAsync();
            // Инициализация коллекции маппингов
            ButtonMappings = new ObservableCollection<KeyboardButtonMapping>();

            // Инициализация команд
            ResetToDefaultsCommand = new RelayCommand(async _ => await OnResetToDefaults());
            SaveMappingsCommand = new RelayCommand(async _ => await OnSaveMappings());
            AddMappingCommand = new RelayCommand(_ => OnAddMapping());
            RemoveMappingCommand = new RelayCommand(OnRemoveMapping);

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

            // Отладка загрузки маппингов
            if (_currentConfig?.KeyboardSettings?.ButtonMappings != null)
            {
                string debug = "Загруженные маппинги:\n";
                foreach (var mapping in _currentConfig.KeyboardSettings.ButtonMappings)
                {
                    debug += $"{mapping.KeyboardKey} -> {mapping.ControllerButton}\n";
                }
                System.Windows.MessageBox.Show(debug, "Загруженные маппинги");
            }
            else
            {
                System.Windows.MessageBox.Show("Маппинги не найдены или пусты", "Загруженные маппинги");
            }

            // Обновляем свойства
            UpdatePropertiesFromConfig();
        }

        /// <summary>
        /// Обновление свойств на основе загруженной конфигурации
        /// </summary>
        private void UpdatePropertiesFromConfig()
        {
            if (_currentConfig == null) return;

            var keyboardSettings = _currentConfig.KeyboardSettings;

            // Обновляем свойства
            KeyboardLayout = keyboardSettings.KeyboardLayout;
            KeyRepeatEnabled = keyboardSettings.KeyRepeatEnabled;
            KeyRepeatRate = keyboardSettings.KeyRepeatRate;
            AnalogKeySensitivity = keyboardSettings.AnalogKeySensitivity;
            MovementUp = keyboardSettings.MovementUp;
            MovementLeft = keyboardSettings.MovementLeft;
            MovementDown = keyboardSettings.MovementDown;
            MovementRight = keyboardSettings.MovementRight;
            MovementStyle = keyboardSettings.MovementStyle;
            AnalogKeyboardEnabled = keyboardSettings.AnalogKeyboardEnabled;

            // Обновляем коллекцию маппингов
            ButtonMappings.Clear();
            foreach (var mapping in keyboardSettings.ButtonMappings)
            {
                // Проверяем, что значения существуют в списках доступных значений
                string keyboardKey = mapping.KeyboardKey;
                string controllerButton = mapping.ControllerButton;

                if (!AvailableKeyboardKeys.Contains(keyboardKey))
                    keyboardKey = AvailableKeyboardKeys.FirstOrDefault() ?? "Space";

                if (!AvailableControllerActions.Contains(controllerButton))
                    controllerButton = AvailableControllerActions.FirstOrDefault() ?? "A Button";

                ButtonMappings.Add(new KeyboardButtonMapping
                {
                    KeyboardKey = keyboardKey,
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
            // Сбрасываем настройки на значения по умолчанию
            KeyboardSettingsModel defaultSettings = new KeyboardSettingsModel();

            // Обновляем настройки в текущей конфигурации
            if (_currentConfig != null)
            {
                _currentConfig.KeyboardSettings = defaultSettings;
                await SaveSettingsAsync();
            }

            // Обновляем свойства
            UpdatePropertiesFromConfig();

            System.Windows.MessageBox.Show("Настройки клавиатуры сброшены к значениям по умолчанию.", "Сброс настроек",
                          System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// Обработчик команды сохранения маппингов
        /// </summary>
        private async Task OnSaveMappings()
        {
            // Добавим отладочный вывод
            string debug = "Сохраняемые маппинги:\n";
            foreach (var mapping in ButtonMappings)
            {
                debug += $"{mapping.KeyboardKey} -> {mapping.ControllerButton}\n";
            }
            System.Windows.MessageBox.Show(debug, "Отладка");

            // Обновляем настройки в текущей конфигурации
            if (_currentConfig != null)
            {
                // Преобразуем ObservableCollection в List
                _currentConfig.KeyboardSettings.ButtonMappings = ButtonMappings
                    .Select(m => new KeyboardButtonMappingModel
                    {
                        KeyboardKey = m.KeyboardKey,
                        ControllerButton = m.ControllerButton
                    })
                    .ToList();

                // Сохраняем конфигурацию и проверяем результат
                await SaveSettingsAsync();

                // Загружаем заново конфигурацию для проверки
                var reloadedConfig = await _configService.LoadConfigAsync();
                string reloadedDebug = "Перезагруженные маппинги:\n";
                foreach (var mapping in reloadedConfig.KeyboardSettings.ButtonMappings)
                {
                    reloadedDebug += $"{mapping.KeyboardKey} -> {mapping.ControllerButton}\n";
                }
                System.Windows.MessageBox.Show(reloadedDebug, "Перезагруженные данные");

                System.Windows.MessageBox.Show("Настройки маппинга клавиатуры успешно сохранены.", "Сохранение настроек",
                              System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        /// <summary>
        /// Обработчик команды добавления нового маппинга
        /// </summary>
        private void OnAddMapping()
        {
            // Используем первые элементы из наших списков
            string defaultKey = AvailableKeyboardKeys.FirstOrDefault() ?? "Space";
            string defaultAction = AvailableControllerActions.FirstOrDefault() ?? "A Button";

            // Добавление нового маппинга с конкретными значениями
            ButtonMappings.Add(new KeyboardButtonMapping
            {
                KeyboardKey = defaultKey,
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
            if (parameter is KeyboardButtonMapping mapping)
            {
                ButtonMappings.Remove(mapping);

                // Автоматически сохраняем изменения
                if (_currentConfig != null)
                {
                    // Преобразуем ObservableCollection в List
                    _currentConfig.KeyboardSettings.ButtonMappings = ButtonMappings
                        .Select(m => new KeyboardButtonMappingModel
                        {
                            KeyboardKey = m.KeyboardKey,
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

        /// <summary>
        /// Публичный метод для сохранения настроек маппингов
        /// </summary>
        public async Task SaveMappingsAsync()
        {
            // Обновляем настройки в текущей конфигурации
            if (_currentConfig != null)
            {
                // Преобразуем ObservableCollection в List
                _currentConfig.KeyboardSettings.ButtonMappings = ButtonMappings
                    .Select(m => new KeyboardButtonMappingModel
                    {
                        KeyboardKey = m.KeyboardKey,
                        ControllerButton = m.ControllerButton
                    })
                    .ToList();

                await SaveSettingsAsync();
            }
        }
        /// <summary>
        /// Модель для хранения маппинга клавиш клавиатуры
        /// </summary>
        public class KeyboardButtonMapping : ViewModelBase
        {
            private string _keyboardKey;
            private string _controllerButton;
            private KeyboardViewModel _parentViewModel;

            /// <summary>
            /// Клавиша клавиатуры
            /// </summary>
            public string KeyboardKey
            {
                get => _keyboardKey;
                set
                {
                    if (SetProperty(ref _keyboardKey, value))
                    {
                        // Автоматически сохраняем изменения
                        _ = _parentViewModel?.SaveMappingsAsync();
                    }
                }
            }

            /// <summary>
            /// Кнопка контроллера
            /// </summary>
            public string ControllerButton
            {
                get => _controllerButton;
                set
                {
                    if (SetProperty(ref _controllerButton, value))
                    {
                        // Автоматически сохраняем изменения
                        _ = _parentViewModel?.SaveMappingsAsync();
                    }
                }
            }

            /// <summary>
            /// Инициализирует маппинг с ссылкой на родительскую ViewModel
            /// </summary>
            public void Initialize(KeyboardViewModel parentViewModel)
            {
                _parentViewModel = parentViewModel;
            }
        }
    }
}