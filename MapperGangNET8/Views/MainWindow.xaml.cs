using MapperGang.ViewModels;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MapperGang.Views
{
    public partial class MainWindow : FluentWindow
    {
        private readonly ControllerViewModel _controllerViewModel;
        private readonly MainViewModel _mainViewModel;
        private readonly MouseViewModel _mouseViewModel;
        private readonly KeyboardViewModel _keyboardViewModel;
        private readonly SensitivityViewModel _sensitivityViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        public MainWindow(MainViewModel mainViewModel, ControllerViewModel controllerViewModel, MouseViewModel mouseViewModel,
            KeyboardViewModel keyboardViewModel, SensitivityViewModel sensitivityViewModel, SettingsViewModel settingsViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _controllerViewModel = controllerViewModel;
            _mouseViewModel = mouseViewModel;
            _keyboardViewModel = keyboardViewModel;
            _sensitivityViewModel = sensitivityViewModel;
            _sensitivityViewModel = sensitivityViewModel;
            _settingsViewModel = settingsViewModel;

            DataContext = _mainViewModel;
            _settingsViewModel = settingsViewModel;
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag && int.TryParse(tag, out int tabIndex))
            {
                UpdateButtonAppearances(tabIndex);
                UpdateContent(tabIndex);
            }
        }

        private void UpdateButtonAppearances(int selectedIndex)
        {
            DashboardTab.Appearance = ControlAppearance.Secondary;
            ControllerTab.Appearance = ControlAppearance.Secondary;
            MouseTab.Appearance = ControlAppearance.Secondary;
            KeyboardTab.Appearance = ControlAppearance.Secondary;
            SensitivityTab.Appearance = ControlAppearance.Secondary;
            SettingsTab.Appearance = ControlAppearance.Secondary;

            switch (selectedIndex)
            {
                case 0: DashboardTab.Appearance = ControlAppearance.Primary; break;
                case 1: ControllerTab.Appearance = ControlAppearance.Primary; break;
                case 2: MouseTab.Appearance = ControlAppearance.Primary; break;
                case 3: KeyboardTab.Appearance = ControlAppearance.Primary; break;
                case 4: SensitivityTab.Appearance = ControlAppearance.Primary; break;
                case 5: SettingsTab.Appearance = ControlAppearance.Primary; break;
            }
        }

        private void UpdateContent(int tabIndex)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            ContentContainer.Visibility = Visibility.Collapsed;
            ContentContainer.Content = null;

            switch (tabIndex)
            {
                case 0: // Dashboard
                    DashboardPanel.Visibility = Visibility.Visible;
                    break;

                case 1: // Controller
                    ContentContainer.Content = new ControllerView { DataContext = _controllerViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;

                case 2: // Mouse
                    ContentContainer.Content = new MouseView { DataContext = _mouseViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                case 3: // Keyboard
                    ContentContainer.Content = new KeyboardView { DataContext = _keyboardViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                case 4: // Sensitivity
                    ContentContainer.Content = new SensitivityView { DataContext = _sensitivityViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                case 5: // Settings
                    ContentContainer.Content = new SettingsView { DataContext = _settingsViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                default:
                    DashboardPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}