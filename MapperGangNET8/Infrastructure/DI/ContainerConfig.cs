using Microsoft.Extensions.DependencyInjection;
using MapperGang.ViewModels;
using MapperGang.Views;
using MapperGang.Services.ConfigService;
using MapperGang.Services.ProfileService;
using MapperGang.Services.AutoSaveService;
using MapperGang.Services.ConfigResetService;

namespace MapperGang.Infrastructure.DI
{
    public static class ContainerConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IConfigService, FileConfigService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IConfigResetService, ConfigResetService>();
            services.AddSingleton<AutoSaveService>();

            //VM
            services.AddSingleton<ControllerViewModel>();
            services.AddSingleton<MouseViewModel>();
            services.AddSingleton<KeyboardViewModel>();
            services.AddSingleton<SensitivityViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}