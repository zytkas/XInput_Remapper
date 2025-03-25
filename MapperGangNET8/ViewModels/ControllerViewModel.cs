
using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек контроллера
    /// </summary>
    public class ControllerViewModel : ViewModelBase
    {
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
            set => SetProperty(ref _selectedControllerType, value);
        }

        /// <summary>
        /// Номер контроллера
        /// </summary>
        public string ControllerNumber
        {
            get => _controllerNumber;
            set => SetProperty(ref _controllerNumber, value);
        }

        /// <summary>
        /// Включена ли вибрация
        /// </summary>
        public bool VibrationEnabled
        {
            get => _vibrationEnabled;
            set => SetProperty(ref _vibrationEnabled, value);
        }

        /// <summary>
        /// Сила вибрации (0-100%)
        /// </summary>
        public double VibrationStrength
        {
            get => _vibrationStrength;
            set => SetProperty(ref _vibrationStrength, value);
        }

        /// <summary>
        /// Чувствительность нажатия кнопок (0-100%)
        /// </summary>
        public double ButtonPressureSensitivity
        {
            get => _buttonPressureSensitivity;
            set => SetProperty(ref _buttonPressureSensitivity, value);
        }

        /// <summary>
        /// Мертвая зона триггера (0-100%)
        /// </summary>
        public double TriggerDeadzone
        {
            get => _triggerDeadzone;
            set => SetProperty(ref _triggerDeadzone, value);
        }

        /// <summary>
        /// Скрыть физические контроллеры
        /// </summary>
        public bool HidePhysicalControllers
        {
            get => _hidePhysicalControllers;
            set => SetProperty(ref _hidePhysicalControllers, value);
        }

        /// <summary>
        /// Эксклюзивный режим управления
        /// </summary>
        public bool ExclusiveMode
        {
            get => _exclusiveMode;
            set => SetProperty(ref _exclusiveMode, value);
        }

        /// <summary>
        /// Проходной режим для физического контроллера
        /// </summary>
        public bool PassThroughMode
        {
            get => _passThroughMode;
            set => SetProperty(ref _passThroughMode, value);
        }

        /// <summary>
        /// Комбинирование входных данных
        /// </summary>
        public bool CombineInputs
        {
            get => _combineInputs;
            set => SetProperty(ref _combineInputs, value);
        }

        /// <summary>
        /// Автоматическое подключение при запуске
        /// </summary>
        public bool AutoConnect
        {
            get => _autoConnect;
            set => SetProperty(ref _autoConnect, value);
        }

        /// <summary>
        /// Режим назначения кнопок (Стандартный/Пользовательский)
        /// </summary>
        public string ButtonAssignmentMode
        {
            get => _buttonAssignmentMode;
            set => SetProperty(ref _buttonAssignmentMode, value);
        }
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

        /// <summary>
        /// Конструктор ControllerViewModel
        /// </summary>
        public ControllerViewModel()
        {
            // Инициализация свойств тестовыми данными
            SelectedControllerType = "Xbox 360 Controller";
            ControllerNumber = "Controller 1";
            VibrationEnabled = true;
            VibrationStrength = 80;
            ButtonPressureSensitivity = 75;
            TriggerDeadzone = 10;
            HidePhysicalControllers = false;
            ExclusiveMode = true;
            PassThroughMode = false;
            CombineInputs = false;
            AutoConnect = true;
            ButtonAssignmentMode = "Standard";

            // Инициализация команд
            RestartDeviceCommand = new RelayCommand(OnRestartDevice);
            SaveSettingsCommand = new RelayCommand(OnSaveSettings);
        }

        #region Обработчики команд
        private void OnRestartDevice(object parameter)
        {
            // Заглушка для перезапуска устройства
        }

        private void OnSaveSettings(object parameter)
        {
            // Заглушка для сохранения настроек
        }
        #endregion
    }
}