using System.Runtime.CompilerServices;
using System.Windows;
using MapperGangNET8.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MapperGangNET8.Views
{
    public partial class MainWindow : FluentWindow
    {
        private readonly MainViewModel _mainViewModel;
        private readonly MouseViewModel _mouseViewModel;
        private readonly KeyboardViewModel _keyboardViewModel;
        private readonly SettingsViewModel _settingsViewModel;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(MainViewModel mainViewModel, MouseViewModel mouseViewModel,
            KeyboardViewModel keyboardViewModel, SettingsViewModel settingsViewModel,
            IServiceProvider serviceProvider)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _mouseViewModel = mouseViewModel;
            _keyboardViewModel = keyboardViewModel;
            _settingsViewModel = settingsViewModel;
            DataContext = _mainViewModel;
            _serviceProvider = serviceProvider;
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
            MouseTab.Appearance = ControlAppearance.Secondary;
            KeyboardTab.Appearance = ControlAppearance.Secondary;
            SettingsTab.Appearance = ControlAppearance.Secondary;

            switch (selectedIndex)
            {
                case 0: DashboardTab.Appearance = ControlAppearance.Primary; break;
                case 1: MouseTab.Appearance = ControlAppearance.Primary; break;
                case 2: KeyboardTab.Appearance = ControlAppearance.Primary; break;
                case 3: SettingsTab.Appearance = ControlAppearance.Primary; break;
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

                case 1: // Mouse
                    ContentContainer.Content = new MouseView { DataContext = _mouseViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                case 2: // Keyboard
                    ContentContainer.Content = new KeyboardView { DataContext = _keyboardViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                case 3: // Settings
                    ContentContainer.Content = new SettingsView { DataContext = _settingsViewModel };
                    ContentContainer.Visibility = Visibility.Visible;
                    break;
                default:
                    DashboardPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void DebugInput_Click(object sender, RoutedEventArgs e)
        {
            var debugWindow = _serviceProvider.GetRequiredService<InputDebugWindow>();
            debugWindow.Show();
        }

        private void DebugController_Click(object sender, RoutedEventArgs e)
        {
            var debugWindow = _serviceProvider.GetRequiredService<ControllerDebugWindow>();
            debugWindow.Show();
        }
    }
}