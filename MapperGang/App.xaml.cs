using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using MapperGang.Infrastructure.DI;
using MapperGang.Views;
using MapperGang.ViewModels;

namespace MapperGang
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            // Настраиваем DI-контейнер
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            // Регистрация сервисов через ContainerConfig
            ContainerConfig.Configure(services);

            // Регистрируем главное окно
            services.AddSingleton<MainWindow>();
        }


        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаем главное окно с использованием DI
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();

            // Устанавливаем главное окно как MainWindow приложения
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
    }
}