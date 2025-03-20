using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace MapperGang.Views
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Установка темной темы приложения через WPF-UI
            // ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        }
    }
}