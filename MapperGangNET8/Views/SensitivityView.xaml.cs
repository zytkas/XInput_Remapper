using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MapperGangNET8.Services.SensitivityService;
using MapperGangNET8.ViewModels;

namespace MapperGangNET8.Views
{

    public partial class SensitivityView : UserControl
    {
        private readonly SolidColorBrush _gridBrush = new SolidColorBrush(Color.FromRgb(50, 50, 50));
        private readonly SolidColorBrush _curveBrush = new SolidColorBrush(Color.FromRgb(0, 120, 215));
        private SensitivityViewModel _viewModel;

        public SensitivityView()
        {
            InitializeComponent();

            Loaded += SensitivityView_Loaded;
        }

        private void SensitivityView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel = DataContext as SensitivityViewModel;

            if (_viewModel != null)
            {

            }
        }

    }
}