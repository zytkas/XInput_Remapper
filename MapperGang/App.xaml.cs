using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using MapperGang.Infrastructure.DI;

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
            // Регистрация сервисов будет добавлена позже через ContainerConfig
            ContainerConfig.Configure(services);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Можем перенести инициализацию окна сюда, если хотим 
            // использовать DI для создания главного окна
        }
    }
}