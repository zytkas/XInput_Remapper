using Microsoft.Extensions.DependencyInjection;
using MapperGang.ViewModels;

namespace MapperGang.Infrastructure.DI
{
    /// <summary>
    /// Конфигурация DI-контейнера приложения
    /// </summary>
    public static class ContainerConfig
    {
        /// <summary>
        /// Регистрирует сервисы в контейнере
        /// </summary>
        public static void Configure(IServiceCollection services)
        {
            // Регистрация ViewModels
            services.AddSingleton<MainViewModel>();

            // Здесь в будущем будут зарегистрированы:
            // - Сервис ввода (InputService)
            // - Сервис контроллера (ControllerService)
            // - Сервис настроек (SettingsService)
            // и другие сервисы
        }
    }
}