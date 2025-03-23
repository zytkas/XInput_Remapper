using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;

namespace MapperGang.ViewModels
{
    public class SensitivityViewModel : ViewModelBase
    {
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
            set => SetProperty(ref _mouseXAxisSensitivity, value);
        }

        /// <summary>
        /// Чувствительность оси Y мыши (0-100%)
        /// </summary>
        public double MouseYAxisSensitivity
        {
            get => _mouseYAxisSensitivity;
            set => SetProperty(ref _mouseYAxisSensitivity, value);
        }

        /// <summary>
        /// Тип кривой отклика мыши (Linear, S-Curve, Custom)
        /// </summary>
        public string MouseResponseCurveType
        {
            get => _mouseResponseCurveType;
            set => SetProperty(ref _mouseResponseCurveType, value);
        }

        /// <summary>
        /// Включено ли ускорение мыши
        /// </summary>
        public bool MouseAcceleration
        {
            get => _mouseAcceleration;
            set => SetProperty(ref _mouseAcceleration, value);
        }

        /// <summary>
        /// Сглаживание движений мыши (0-100%)
        /// </summary>
        public double MouseSmoothing
        {
            get => _mouseSmoothing;
            set => SetProperty(ref _mouseSmoothing, value);
        }

        /// <summary>
        /// Блокировка движения по одной оси
        /// </summary>
        public bool MouseAxisLock
        {
            get => _mouseAxisLock;
            set => SetProperty(ref _mouseAxisLock, value);
        }

        /// <summary>
        /// Чувствительность джойстика (0-100%)
        /// </summary>
        public double JoystickSensitivity
        {
            get => _joystickSensitivity;
            set => SetProperty(ref _joystickSensitivity, value);
        }

        /// <summary>
        /// Мертвая зона джойстика (0-100%)
        /// </summary>
        public double JoystickDeadzone
        {
            get => _joystickDeadzone;
            set => SetProperty(ref _joystickDeadzone, value);
        }

        /// <summary>
        /// Тип кривой отклика джойстика (Linear, Step, Custom)
        /// </summary>
        public string JoystickResponseCurveType
        {
            get => _joystickResponseCurveType;
            set => SetProperty(ref _joystickResponseCurveType, value);
        }

        /// <summary>
        /// Компенсация мертвой зоны джойстика
        /// </summary>
        public bool JoystickAntiDeadzone
        {
            get => _joystickAntiDeadzone;
            set => SetProperty(ref _joystickAntiDeadzone, value);
        }

        /// <summary>
        /// Поворот ввода джойстика
        /// </summary>
        public bool JoystickRotation
        {
            get => _joystickRotation;
            set => SetProperty(ref _joystickRotation, value);
        }

        /// <summary>
        /// Использование радиальной мертвой зоны
        /// </summary>
        public bool JoystickRadialDeadzone
        {
            get => _joystickRadialDeadzone;
            set => SetProperty(ref _joystickRadialDeadzone, value);
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
        #endregion

        /// <summary>
        /// Конструктор SensitivityViewModel
        /// </summary>
        public SensitivityViewModel()
        {
            MouseXAxisSensitivity = 65;
            MouseYAxisSensitivity = 60;
            MouseResponseCurveType = "Linear";
            MouseAcceleration = false;
            MouseSmoothing = 30;
            MouseAxisLock = false;

            // Инициализация свойств джойстика
            JoystickSensitivity = 80;
            JoystickDeadzone = 10;
            JoystickResponseCurveType = "Linear";
            JoystickAntiDeadzone = false;
            JoystickRotation = false;
            JoystickRadialDeadzone = true;

            // Инициализация команд
            EditMouseCurveCommand = new RelayCommand(OnEditMouseCurve);
            EditJoystickCurveCommand = new RelayCommand(OnEditJoystickCurve);
            SelectMouseCurvePresetCommand = new RelayCommand(OnSelectMouseCurvePreset);
            SelectJoystickCurvePresetCommand = new RelayCommand(OnSelectJoystickCurvePreset);
        }

        #region Обработчики команд
        private void OnEditMouseCurve(object parameter)
        {
            // Заглушка для редактирования кривой отклика мыши
        }

        private void OnEditJoystickCurve(object parameter)
        {
            // Заглушка для редактирования кривой отклика джойстика
        }

        private void OnSelectMouseCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                MouseResponseCurveType = presetType;
                // Здесь можно добавить логику для изменения формы кривой
            }
        }

        private void OnSelectJoystickCurvePreset(object parameter)
        {
            if (parameter is string presetType)
            {
                JoystickResponseCurveType = presetType;
                // Здесь можно добавить логику для изменения формы кривой
            }
        }
        #endregion
    }
}