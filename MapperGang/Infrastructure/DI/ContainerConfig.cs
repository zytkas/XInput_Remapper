using Microsoft.Extensions.DependencyInjection;
using MapperGang.ViewModels;
using MapperGang.Views;

namespace MapperGang.Infrastructure.DI
{
    public static class ContainerConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<ControllerViewModel>();
            services.AddSingleton<MouseViewModel>();
            services.AddSingleton<KeyboardViewModel>();
            services.AddSingleton<SensitivityViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
            // Здесь в будущем будут зарегистрированы:
            // - Сервис ввода (InputService)
            // - Сервис контроллера (ControllerService)
            // - Сервис настроек (SettingsService)
            // и другие сервисы
        }
    }
}