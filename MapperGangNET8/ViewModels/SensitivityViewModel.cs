// ViewModels/SensitivityViewModel.cs
using MapperGangNET8.Models;
using MapperGangNET8.Services.ConfigResetService;
using MapperGangNET8.Services.ConfigService;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MapperGangNET8.Infrastructure.Commands;
using MapperGangNET8.Views;

namespace MapperGangNET8.ViewModels
{
    public class SensitivityViewModel : ViewModelBase
    {
        private readonly IConfigService _configService;

        private ConfigModel _currentConfig;

        private double _mouseSensitivity;
        private string _selectedCurve;
        private double _curveParameter;

        public double MouseSensitivity
        {
            get => _mouseSensitivity;
            set
            {
                if (SetProperty(ref _mouseSensitivity, value))
                {
                    SaveSettings();
                }
            }
        }

        public string SelectedCurve
        {
            get => _selectedCurve;
            set
            {
                if (SetProperty(ref _selectedCurve, value))
                {
                    SaveSettings();
                }
            }
        }

        public double CurveParameter
        {
            get => _curveParameter;
            set
            {
                if (SetProperty(ref _curveParameter, value))
                {
                    SaveSettings();
                }
            }
        }

        public ObservableCollection<string> AvailableCurves { get; }

        public ICommand ResetCommand { get; }

        public SensitivityViewModel(IConfigService configService, IConfigResetService resetService)
        {
            _configService = configService;

            ResetCommand = new RelayCommand(async _ => await ResetToDefaults());

            _ = LoadSettings();
        }

        private async Task LoadSettings()
        {
            _currentConfig = await _configService.LoadConfigAsync();

            MouseSensitivity = _currentConfig.SensitivitySettings.MouseXAxisSensitivity;
            SelectedCurve = _currentConfig.SensitivitySettings.MouseResponseCurveType;
            CurveParameter = _currentConfig.SensitivitySettings.MouseExponent;
        }

        private async void SaveSettings()
        {
            if (_currentConfig == null) return;

            _currentConfig.SensitivitySettings.MouseXAxisSensitivity = MouseSensitivity;
            _currentConfig.SensitivitySettings.MouseYAxisSensitivity = MouseSensitivity;
            _currentConfig.SensitivitySettings.MouseResponseCurveType = SelectedCurve;
            _currentConfig.SensitivitySettings.MouseExponent = CurveParameter;

            await _configService.SaveConfigAsync(_currentConfig);
        }

        private async Task ResetToDefaults()
        {
            MouseSensitivity = 65;
            SelectedCurve = "Linear";
            CurveParameter = 2.0;
        }

        // Метод для применения кривой к значению
        public double ApplyCurve(double input)
        {
            return 0; //fallback 
        }
    }
}