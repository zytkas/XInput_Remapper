using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;
using MapperGangNET8.Models;
using System.Collections.Generic;
using System.Linq;

namespace MapperGangNET8.Services.ConfigService
{
    public class FileConfigService : IConfigService
    {
        private readonly string _configFolder;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private ConfigModel _cachedConfig;

        public event EventHandler ConfigurationReset;
        public event EventHandler<string> ProfileChanged;

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

        #region Основные методы конфигурации

        public async Task<ConfigModel> LoadConfigAsync()
        {
            try
            {
                if (!File.Exists(_configFilePath))
                {
                    var defaultConfig = CreateDefaultConfig();
                    await SaveConfigAsync(defaultConfig);
                    _cachedConfig = defaultConfig;
                    return defaultConfig;
                }

                string json = await File.ReadAllTextAsync(_configFilePath);
                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                // Проверяем наличие активного профиля
                if (config != null && !config.Profiles.ContainsKey(config.ActiveProfile))
                {
                    config = await EnsureDefaultProfileExists(config);
                }

                _cachedConfig = config;
                return config ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                var defaultConfig = CreateDefaultConfig();
                _cachedConfig = defaultConfig;
                return defaultConfig;
            }
        }

        public async Task SaveConfigAsync(ConfigModel config)
        {
            try
            {
                string json = JsonSerializer.Serialize(config, _jsonOptions);
                await File.WriteAllTextAsync(_configFilePath, json);
                _cachedConfig = config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<ConfigModel> ResetConfigAsync()
        {
            ConfigModel defaultConfig = CreateDefaultConfig();

            try
            {
                await SaveConfigAsync(defaultConfig);
                NotifyConfigurationReset();
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
                string json = await File.ReadAllTextAsync(path);
                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                if (config != null)
                {
                    await SaveConfigAsync(config);
                    MessageBox.Show("Конфигурация успешно импортирована", "Успех",
                                   MessageBoxButton.OK, MessageBoxImage.Information);
                }

                return config ?? CreateDefaultConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте конфигурации: {ex.Message}", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Error);
                return await LoadConfigAsync();
            }
        }

        #endregion

        #region Методы работы с профилями

        public async Task<List<string>> GetProfilesAsync()
        {
            await EnsureConfigLoaded();
            return _cachedConfig.Profiles.Keys.ToList();
        }

        public async Task<ProfileModel> GetProfileAsync(string name)
        {
            await EnsureConfigLoaded();
            return _cachedConfig.Profiles.TryGetValue(name, out ProfileModel profile) ? profile : null;
        }

        public async Task<ProfileModel> CreateProfileAsync(string name, string description = "")
        {
            await EnsureConfigLoaded();

            if (_cachedConfig.Profiles.ContainsKey(name))
            {
                return _cachedConfig.Profiles[name];
            }

            ProfileModel newProfile = new ProfileModel
            {
                Name = name,
                Description = description,
                // Копируем текущие настройки из активного профиля
                MouseSettings = CloneMouseSettings(_cachedConfig.MouseSettings),
                KeyboardSettings = CloneKeyboardSettings(_cachedConfig.KeyboardSettings),
                SensitivitySettings = CloneSensitivitySettings(_cachedConfig.SensitivitySettings)
            };

            _cachedConfig.Profiles[name] = newProfile;
            await SaveConfigAsync(_cachedConfig);

            return newProfile;
        }

        public async Task UpdateProfileAsync(string name, ProfileModel profile)
        {
            await EnsureConfigLoaded();

            if (!_cachedConfig.Profiles.ContainsKey(name))
            {
                return;
            }

            _cachedConfig.Profiles[name] = profile;

            // Если обновляем активный профиль, обновляем и основные настройки
            if (_cachedConfig.ActiveProfile == name)
            {
                _cachedConfig.MouseSettings = CloneMouseSettings(profile.MouseSettings);
                _cachedConfig.KeyboardSettings = CloneKeyboardSettings(profile.KeyboardSettings);
                _cachedConfig.SensitivitySettings = CloneSensitivitySettings(profile.SensitivitySettings);
            }

            await SaveConfigAsync(_cachedConfig);
        }

        public async Task DeleteProfileAsync(string name)
        {
            await EnsureConfigLoaded();

            if (!_cachedConfig.Profiles.ContainsKey(name))
                return;

            if (_cachedConfig.ActiveProfile == name)
            {
                MessageBox.Show("Невозможно удалить активный профиль", "Ошибка",
                               MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _cachedConfig.Profiles.Remove(name);
            await SaveConfigAsync(_cachedConfig);
        }
        public async Task SwitchToProfileAsync(string name)
        {
            await EnsureConfigLoaded();

            if (!_cachedConfig.Profiles.ContainsKey(name))
                return;

            var profile = _cachedConfig.Profiles[name];

            _cachedConfig.MouseSettings = CloneMouseSettings(profile.MouseSettings);
            _cachedConfig.KeyboardSettings = CloneKeyboardSettings(profile.KeyboardSettings);
            _cachedConfig.SensitivitySettings = CloneSensitivitySettings(profile.SensitivitySettings);
            _cachedConfig.ActiveProfile = name;

            await SaveConfigAsync(_cachedConfig);
            ProfileChanged?.Invoke(this, name);
        }

        public async Task<ProfileModel> GetActiveProfileAsync()
        {
            await EnsureConfigLoaded();
            return _cachedConfig.Profiles.TryGetValue(_cachedConfig.ActiveProfile, out var profile)
                ? profile
                : _cachedConfig.Profiles["Default"];
        }

        public async Task<string> GetActiveProfileNameAsync()
        {
            await EnsureConfigLoaded();
            return _cachedConfig.ActiveProfile;
        }

        #endregion

        #region Вспомогательные методы

        private async Task EnsureConfigLoaded()
        {
            if (_cachedConfig == null)
            {
                _cachedConfig = await LoadConfigAsync();
            }
        }

        private async Task<ConfigModel> EnsureDefaultProfileExists(ConfigModel config)
        {
            ProfileModel defaultProfile = new ProfileModel
            {
                Name = "Default",
                Description = "Профиль по умолчанию",
                MouseSettings = config.MouseSettings,
                KeyboardSettings = config.KeyboardSettings,
                SensitivitySettings = config.SensitivitySettings
            };

            config.Profiles["Default"] = defaultProfile;
            config.ActiveProfile = "Default";

            await SaveConfigAsync(config);
            return config;
        }

        private ConfigModel CreateDefaultConfig()
        {
            var config = new ConfigModel();

            // Создаем профиль по умолчанию
            ProfileModel defaultProfile = new ProfileModel
            {
                Name = "Default",
                Description = "Профиль по умолчанию",
                MouseSettings = config.MouseSettings,
                KeyboardSettings = config.KeyboardSettings,
                SensitivitySettings = config.SensitivitySettings
            };

            config.Profiles["Default"] = defaultProfile;
            config.ActiveProfile = "Default";

            return config;
        }

        // Методы клонирования настроек
        private MouseSettingsModel CloneMouseSettings(MouseSettingsModel original)
        {
            var json = JsonSerializer.Serialize(original, _jsonOptions);
            return JsonSerializer.Deserialize<MouseSettingsModel>(json, _jsonOptions);
        }

        private KeyboardSettingsModel CloneKeyboardSettings(KeyboardSettingsModel original)
        {
            var json = JsonSerializer.Serialize(original, _jsonOptions);
            return JsonSerializer.Deserialize<KeyboardSettingsModel>(json, _jsonOptions);
        }

        private SensitivitySettingsModel CloneSensitivitySettings(SensitivitySettingsModel original)
        {
            var json = JsonSerializer.Serialize(original, _jsonOptions);
            return JsonSerializer.Deserialize<SensitivitySettingsModel>(json, _jsonOptions);
        }

        public void NotifyConfigurationReset()
        {
            ConfigurationReset?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}