using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using MapperGangNET8.Infrastructure.DI;
using MapperGangNET8.ViewModels;
using MapperGangNET8.Services.AutoSaveService;
using MapperGangNET8.Views;

namespace MapperGangNET8
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private AutoSaveService _autoSaveService;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();

            _autoSaveService = _serviceProvider.GetRequiredService<AutoSaveService>();  
        }

        private void ConfigureServices(ServiceCollection services)
        {
            ContainerConfig.Configure(services);

            services.AddSingleton<MainWindow>();
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _autoSaveService?.Dispose();
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }
}