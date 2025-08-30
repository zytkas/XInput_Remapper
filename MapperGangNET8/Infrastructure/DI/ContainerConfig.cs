using Microsoft.Extensions.DependencyInjection;
using MapperGangNET8.ViewModels;
using MapperGangNET8.Views;
using MapperGangNET8.Services.ConfigService;
using MapperGangNET8.Services.ProfileService;
using MapperGangNET8.Services;
using MapperGangNET8.Services.ConfigResetService;
using MapperGangNET8.Services.InputService;
using MapperGangNET8.Services.ControllerService;
using MapperGangNET8.Services.InputMappingService;
using MapperGangNET8.Services.MappingService;
using MapperGangNET8.Services.InputBlockingService;

namespace MapperGangNET8.Infrastructure.DI
{
    public static class ContainerConfig
    {
        public static void Configure(IServiceCollection services)
        {
            // Register services
            services.AddSingleton<IConfigService, FileConfigService>();
            services.AddSingleton<IProfileService, ProfileService>();
            services.AddSingleton<IConfigResetService, ConfigResetService>();
            services.AddSingleton<IInputService, Soju06InputService>();
            services.AddSingleton<IControllerService, ViGemControllerService>();
            
            // Register Step 7 pipeline components
            services.AddSingleton<InputBlockingManager>();
            services.AddSingleton<KeyToControllerMapper>();
            services.AddSingleton<MouseToStickMapper>();
            services.AddSingleton<InputPipeline>();
            services.AddSingleton<InputMappingService>();
            
            // Keep InputProcessorService for future Steps 5-6
            services.AddSingleton<InputProcessorService>();


            // Register debug windows
            services.AddTransient<InputDebugWindow>();
            services.AddTransient<ControllerDebugWindow>();

            // Register utility services
            services.AddSingleton<AutoSaveService>();

            // Register ViewModels
            services.AddSingleton<MouseViewModel>();
            services.AddSingleton<KeyboardViewModel>();
            services.AddSingleton<SensitivityViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<MainViewModel>();

            // Register main window
            services.AddSingleton<MainWindow>();
        }
    }
}