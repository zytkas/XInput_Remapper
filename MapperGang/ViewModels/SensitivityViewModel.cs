// MapperGang/ViewModels/SensitivityViewModel.cs
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для вкладки настроек чувствительности
    /// </summary>
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
            set
            {
                if (SetProperty(ref _mouseXAxisSensitivity, value))
                {
                    // Дополнительные вычисления при необходимости
                    OnPropertyChanged(nameof(MouseSensitivityOverall));
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
        /// Включено ли сглаживание мыши
        /// </summary>
        public bool MouseSmoothingEnabled
        {
            get => _mouseSmoothing > 0;
            set
            {
                MouseSmoothing = value ? 30 : 0; // Используем 30% по умолчанию при включении
            }
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
            set
            {
                if (SetProperty(ref _joystickResponseCurveType, value))
                {
                    // Обновляем внешний вид всех кнопок кривых
                    OnPropertyChanged(nameof(JoystickLinearCurveAppearance));
                    OnPropertyChanged(nameof(JoystickStepCurveAppearance));
                    OnPropertyChanged(nameof(JoystickCustomCurveAppearance));
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
            // Инициализация свойств мыши
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