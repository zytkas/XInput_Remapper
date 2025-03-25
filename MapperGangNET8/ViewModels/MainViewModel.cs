using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;
using MapperGang.Models;

namespace MapperGang.ViewModels
{

    public class MainViewModel : ViewModelBase
    {
        #region Приватные поля

        private int _selectedTabIndex;
        private bool _isDeviceActive;
        private string _deviceType;
        private string _deviceId;
        private string _activeProfile;
        private double _mouseSensitivity;
        private double _joystickSensitivity;
        private ViewModelBase _currentViewModel;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Индекс выбранной вкладки
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (SetProperty(ref _selectedTabIndex, value))
                {
                    UpdateCurrentViewModel();
                }
            }
        }

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
            set => SetProperty(ref _activeProfile, value);
        }


        /// <summary>
        /// Чувствительность мыши (0-100%)
        /// </summary>
        public double MouseSensitivity
        {
            get => _mouseSensitivity;
            set => SetProperty(ref _mouseSensitivity, value);
        }

        /// <summary>
        /// Чувствительность джойстика (0-100%)
        /// </summary>
        public double JoystickSensitivity
        {
            get => _joystickSensitivity;
            set => SetProperty(ref _joystickSensitivity, value);
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

        #endregion

        // Зависимости для навигации
        private readonly ControllerViewModel _controllerViewModel;

        /// <summary>
        /// Конструктор MainViewModel
        /// </summary>
        public MainViewModel(ControllerViewModel controllerViewModel)
        {
            _controllerViewModel = controllerViewModel;

            // Инициализация свойств тестовыми данными
            IsDeviceActive = true;
            DeviceType = "Xbox 360 Controller";
            DeviceId = "VID_045E&PID_028E";
            ActiveProfile = "Default";
            SelectedTabIndex = 0;
            MouseSensitivity = 65;
            JoystickSensitivity = 80;
            // Инициализация команд
            RestartDeviceCommand = new RelayCommand(OnRestartDevice);
            NewProfileCommand = new RelayCommand(OnNewProfile);
            SaveSettingsCommand = new RelayCommand(OnSaveSettings);
            NavigateCommand = new RelayCommand(OnNavigate);

            // Установка начального представления
            UpdateCurrentViewModel();
        }

        #region Обработчики команд

        private void OnRestartDevice(object parameter)
        {
            // На этапе 1 ничего не делаем, просто заглушка
        }

        private void OnNewProfile(object parameter)
        {
            // На этапе 1 ничего не делаем, просто заглушка
        }

        private void OnSaveSettings(object parameter)
        {
            // На этапе 1 ничего не делаем, просто заглушка
        }

        private void OnNavigate(object parameter)
        {
            if (parameter is int tabIndex)
            {
                SelectedTabIndex = tabIndex;
            }
        }

        #endregion

        /// <summary>
        /// Обновляет текущую ViewModel в зависимости от выбранной вкладки
        /// </summary>
        private void UpdateCurrentViewModel()
        {
            switch (SelectedTabIndex)
            {
                case 0: // Dashboard
                    CurrentViewModel = this;
                    break;
                case 1: // Controller
                    CurrentViewModel = _controllerViewModel;
                    break;
                // Остальные вкладки будут добавлены позже
                default:
                    CurrentViewModel = this;
                    break;
            }
        }
    }
}