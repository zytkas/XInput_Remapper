using Microsoft.Extensions.DependencyInjection;
using MapperGangNET8.ViewModels;
using MapperGangNET8.Views;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ProfileService;
using MapperGangNET8.Services.AutoSaveService;
using MapperGangNET8.Services.ConfigResetService;
using MapperGangNET8.Services.InputService;
using MapperGangNET8.Services.ControllerService;

namespace MapperGangNET8.Infrastructure.DI
{
    public static class ContainerConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.AddSingleton<IConfigService, FileConfigService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IConfigResetService, ConfigResetService>();
            services.AddSingleton<IInputService, Soju06InputService>();
            services.AddSingleton<IControllerService, ViGemControllerService>();
            services.AddTransient<InputDebugWindow>();
            services.AddTransient<ControllerDebugWindow>();
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