// MapperGang/ViewModels/KeyboardViewModel.cs
using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек клавиатуры
    /// </summary>
    public class KeyboardViewModel : ViewModelBase
    {
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
            set => SetProperty(ref _keyboardLayout, value);
        }

        /// <summary>
        /// Включен ли повтор клавиш
        /// </summary>
        public bool KeyRepeatEnabled
        {
            get => _keyRepeatEnabled;
            set => SetProperty(ref _keyRepeatEnabled, value);
        }

        /// <summary>
        /// Скорость повтора клавиш (0-100%)
        /// </summary>
        public double KeyRepeatRate
        {
            get => _keyRepeatRate;
            set => SetProperty(ref _keyRepeatRate, value);
        }

        /// <summary>
        /// Чувствительность аналоговых клавиш (0-100%)
        /// </summary>
        public double AnalogKeySensitivity
        {
            get => _analogKeySensitivity;
            set => SetProperty(ref _analogKeySensitivity, value);
        }

        /// <summary>
        /// Клавиша движения вверх
        /// </summary>
        public string MovementUp
        {
            get => _movementUp;
            set => SetProperty(ref _movementUp, value);
        }

        /// <summary>
        /// Клавиша движения влево
        /// </summary>
        public string MovementLeft
        {
            get => _movementLeft;
            set => SetProperty(ref _movementLeft, value);
        }

        /// <summary>
        /// Клавиша движения вниз
        /// </summary>
        public string MovementDown
        {
            get => _movementDown;
            set => SetProperty(ref _movementDown, value);
        }

        /// <summary>
        /// Клавиша движения вправо
        /// </summary>
        public string MovementRight
        {
            get => _movementRight;
            set => SetProperty(ref _movementRight, value);
        }

        /// <summary>
        /// Стиль движения (8-Way/4-Way)
        /// </summary>
        public string MovementStyle
        {
            get => _movementStyle;
            set => SetProperty(ref _movementStyle, value);
        }

        /// <summary>
        /// Включить аналоговый режим клавиатуры
        /// </summary>
        public bool AnalogKeyboardEnabled
        {
            get => _analogKeyboardEnabled;
            set => SetProperty(ref _analogKeyboardEnabled, value);
        }

        /// <summary>
        /// Коллекция маппингов кнопок клавиатуры
        /// </summary>
        public ObservableCollection<KeyboardButtonMapping> ButtonMappings
        {
            get => _buttonMappings;
            set => SetProperty(ref _buttonMappings, value);
        }
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
        public KeyboardViewModel()
        {
            // Инициализация свойств тестовыми данными
            KeyboardLayout = "QWERTY";
            KeyRepeatEnabled = true;
            KeyRepeatRate = 70;
            AnalogKeySensitivity = 65;
            MovementUp = "W";
            MovementLeft = "A";
            MovementDown = "S";
            MovementRight = "D";
            MovementStyle = "8-Way";
            AnalogKeyboardEnabled = true;

            // Инициализация коллекции маппингов
            ButtonMappings = new ObservableCollection<KeyboardButtonMapping>
            {
                new KeyboardButtonMapping { KeyboardKey = "Space", ControllerButton = "A Button" },
                new KeyboardButtonMapping { KeyboardKey = "Left Ctrl", ControllerButton = "B Button" },
                new KeyboardButtonMapping { KeyboardKey = "Left Shift", ControllerButton = "X Button" },
                new KeyboardButtonMapping { KeyboardKey = "Left Alt", ControllerButton = "Y Button" },
                new KeyboardButtonMapping { KeyboardKey = "Q", ControllerButton = "Left Bumper" },
                new KeyboardButtonMapping { KeyboardKey = "E", ControllerButton = "Right Bumper" },
                new KeyboardButtonMapping { KeyboardKey = "R", ControllerButton = "Left Trigger" },
                new KeyboardButtonMapping { KeyboardKey = "F", ControllerButton = "Right Trigger" }
            };

            // Инициализация команд
            ResetToDefaultsCommand = new RelayCommand(OnResetToDefaults);
            SaveMappingsCommand = new RelayCommand(OnSaveMappings);
            AddMappingCommand = new RelayCommand(OnAddMapping);
            RemoveMappingCommand = new RelayCommand(OnRemoveMapping);
        }

        #region Обработчики команд
        private void OnResetToDefaults(object parameter)
        {
            // Заглушка для сброса на значения по умолчанию
        }

        private void OnSaveMappings(object parameter)
        {
            // Заглушка для сохранения маппингов
        }

        private void OnAddMapping(object parameter)
        {
            // Добавление нового маппинга
            ButtonMappings.Add(new KeyboardButtonMapping { KeyboardKey = "New Key", ControllerButton = "Select Action" });
        }

        private void OnRemoveMapping(object parameter)
        {
            if (parameter is KeyboardButtonMapping mapping)
            {
                ButtonMappings.Remove(mapping);
            }
        }
        #endregion
    }

    /// <summary>
    /// Модель для хранения маппинга клавиш клавиатуры
    /// </summary>
    public class KeyboardButtonMapping : ViewModelBase
    {
        private string _keyboardKey;
        private string _controllerButton;

        /// <summary>
        /// Клавиша клавиатуры
        /// </summary>
        public string KeyboardKey
        {
            get => _keyboardKey;
            set => SetProperty(ref _keyboardKey, value);
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