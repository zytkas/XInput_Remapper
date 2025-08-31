using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using MapperGangNET8.Models;

namespace MapperGangNET8.Services.ConfigService
{
    public class FileConfigService : IConfigService
    {
        private readonly string _configFolder;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        public event EventHandler ConfigurationReset;

        public FileConfigService()
        {
            _configFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "MapperGang");

            _configFilePath = Path.Combine(_configFolder, "config.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
            if (!Directory.Exists(_configFolder))
            {
                Directory.CreateDirectory(_configFolder);
            }
        }

        public async Task<ConfigModel> LoadConfigAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    var defaultConfig = CreateDefaultConfig();
                    // Save the default config so it persists
                    await SaveConfigAsync(defaultConfig);
                    return defaultConfig;
                }
                string json = await File.ReadAllTextAsync(_configFilePath);

                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                return config ?? new ConfigModel();
            }
            catch (Exception ex)
            {

                MessageBox.Show($"Ошибка при загрузке конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return new ConfigModel();
            }
        }

        public async Task SaveConfigAsync(ConfigModel config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(_configFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<ConfigModel> ResetConfigAsync()
        {
            ConfigModel defaultConfig = new ConfigModel();

            try
            {
                await SaveConfigAsync(defaultConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return defaultConfig;
        }

        public async Task ExportConfigAsync(ConfigModel config, string path)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, _jsonOptions);

                await File.WriteAllTextAsync(path, json);

                MessageBox.Show("Конфигурация успешно экспортирована", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<ConfigModel> ImportConfigAsync(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    MessageBox.Show("Файл конфигурации не найден", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return await LoadConfigAsync();
                }

                string json = await File.ReadAllTextAsync(path);

                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                if (config == null)
                {
                    MessageBox.Show("Некорректный формат файла конфигурации", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return await LoadConfigAsync();
                }

                await SaveConfigAsync(config);

                MessageBox.Show("Конфигурация успешно импортирована", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return await LoadConfigAsync();
            }
        }

        /// <summary>
        /// Create default configuration with basic bindings
        /// </summary>
        private ConfigModel CreateDefaultConfig()
        {
            var config = new ConfigModel();

            // Add default keyboard bindings (basic FPS-like setup)
            config.KeyboardSettings.ButtonMappings.AddRange(new[]
            {
                new KeyboardButtonMappingModel { KeyboardKey = "Space", ControllerButton = "A Button" },
                new KeyboardButtonMappingModel { KeyboardKey = "Left Ctrl", ControllerButton = "B Button" },
                new KeyboardButtonMappingModel { KeyboardKey = "Left Shift", ControllerButton = "X Button" },
                new KeyboardButtonMappingModel { KeyboardKey = "E", ControllerButton = "Y Button" },
                new KeyboardButtonMappingModel { KeyboardKey = "Q", ControllerButton = "Left Bumper" },
                new KeyboardButtonMappingModel { KeyboardKey = "Tab", ControllerButton = "Right Bumper" },
                new KeyboardButtonMappingModel { KeyboardKey = "R", ControllerButton = "Left Stick Press" },
                new KeyboardButtonMappingModel { KeyboardKey = "C", ControllerButton = "Right Stick Press" },
                new KeyboardButtonMappingModel { KeyboardKey = "Esc", ControllerButton = "Back Button" },
                new KeyboardButtonMappingModel { KeyboardKey = "Enter", ControllerButton = "Start Button" }
            });

            // Add default mouse bindings
            config.MouseSettings.ButtonMappings.AddRange(new[]
            {
                new MouseButtonMappingModel { MouseButton = "Left Button", ControllerButton = "Right Trigger" },
                new MouseButtonMappingModel { MouseButton = "Right Button", ControllerButton = "Left Trigger" },
                new MouseButtonMappingModel { MouseButton = "Middle Button", ControllerButton = "Guide Button" },
                new MouseButtonMappingModel { MouseButton = "Side Button 1", ControllerButton = "D-Pad Up" },
                new MouseButtonMappingModel { MouseButton = "Side Button 2", ControllerButton = "D-Pad Down" }
            });

            return config;
        }

        public void NotifyConfigurationReset()
        {
            ConfigurationReset?.Invoke(this, EventArgs.Empty);
        }
    }
}