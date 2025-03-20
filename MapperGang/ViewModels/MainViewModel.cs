using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGang.Infrastructure.Commands;
using MapperGang.Models;

namespace MapperGang.ViewModels
{
    /// <summary>
    /// ViewModel для главного окна приложения
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        #region Приватные поля

        private int _selectedTabIndex;
        private bool _isDeviceActive;
        private string _deviceType;
        private string _deviceId;
        private string _activeProfile;

        #endregion

        #region Публичные свойства

        /// <summary>
        /// Индекс выбранной вкладки
        /// </summary>
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
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

        #endregion

        /// <summary>
        /// Конструктор MainViewModel
        /// </summary>
        public MainViewModel()
        {
            // Инициализация свойств тестовыми данными
            IsDeviceActive = true;
            DeviceType = "Xbox 360 Controller";
            DeviceId = "VID_045E&PID_028E";
            ActiveProfile = "Default";
            SelectedTabIndex = 0;

            // Инициализация команд
            RestartDeviceCommand = new RelayCommand(OnRestartDevice);
            NewProfileCommand = new RelayCommand(OnNewProfile);
            SaveSettingsCommand = new RelayCommand(OnSaveSettings);
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

        #endregion
    }
}