using MapperGang.ViewModels;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MapperGang.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : FluentWindow
    {
        private readonly ControllerViewModel _controllerViewModel;
        private readonly MainViewModel _mainViewModel;
        private readonly MouseViewModel _mouseViewModel;
        private readonly KeyboardViewModel _keyboardViewModel;
        private readonly SensitivityViewModel _sensitivityViewModel;

        public MainWindow(MainViewModel mainViewModel, ControllerViewModel controllerViewModel, MouseViewModel mouseViewModel,
            KeyboardViewModel keyboardViewModel, SensitivityViewModel sensitivityViewModel)
        {
            InitializeComponent();

            _mainViewModel = mainViewModel;
            _controllerViewModel = controllerViewModel;
            _mouseViewModel = mouseViewModel;
            _keyboardViewModel = keyboardViewModel;
            _sensitivityViewModel = sensitivityViewModel;
            DataContext = _mainViewModel;
            _sensitivityViewModel = sensitivityViewModel;
        }

        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag && int.TryParse(tag, out int tabIndex))
            {
                // Обновить индекс в MainViewModel
                _mainViewModel.SelectedTabIndex = tabIndex;

                // Обновить внешний вид всех кнопок навигации
                UpdateButtonAppearances(tabIndex);

                // Обновить содержимое в зависимости от выбранной вкладки
                UpdateContent(tabIndex);
            }
        }

        private void UpdateButtonAppearances(int selectedIndex)
        {
            // Сбросить оформление всех кнопок на Secondary
            DashboardTab.Appearance = ControlAppearance.Secondary;
            ControllerTab.Appearance = ControlAppearance.Secondary;
            MouseTab.Appearance = ControlAppearance.Secondary;
            KeyboardTab.Appearance = ControlAppearance.Secondary;
            SensitivityTab.Appearance = ControlAppearance.Secondary;
            SettingsTab.Appearance = ControlAppearance.Secondary;

            // Установить Primary для выбранной вкладки
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
            // Скрыть все панели по умолчанию
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
                default:
                    DashboardPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}