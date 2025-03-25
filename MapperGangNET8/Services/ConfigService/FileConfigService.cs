using MapperGang.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Text.Json;

namespace MapperGang.Services.ConfigService
{
    /// <summary>
    /// Реализация сервиса конфигурации, сохраняющая данные в файлы
    /// </summary>
    public class FileConfigService : IConfigService
    {
        private readonly string _configFolder;
        private readonly string _configFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Конструктор FileConfigService
        /// </summary>
        public FileConfigService()
        {
            // Создаем папку для конфигурации в локальных данных приложения
            _configFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MapperGang");

            // Задаем путь к файлу конфигурации
            _configFilePath = Path.Combine(_configFolder, "config.json");

            // Настраиваем параметры сериализации JSON
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            // Создаем директорию, если она не существует
            if (!Directory.Exists(_configFolder))
            {
                Directory.CreateDirectory(_configFolder);
            }
        }

        /// <summary>
        /// Загрузить конфигурацию из файла
        /// </summary>
        public async Task<ConfigModel> LoadConfigAsync()
        {
            try
            {
                // Проверяем, существует ли файл конфигурации
                if (!File.Exists(_configFilePath))
                {
                    // Если файл не существует, возвращаем стандартную конфигурацию
                    return new ConfigModel();
                }

                // Читаем содержимое файла
                string json = await File.ReadAllTextAsync(_configFilePath);

                // Десериализуем JSON в объект ConfigModel
                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                // Если десериализация не удалась, возвращаем стандартную конфигурацию
                return config ?? new ConfigModel();
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем её и возвращаем стандартную конфигурацию
                MessageBox.Show($"Ошибка при загрузке конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return new ConfigModel();
            }
        }

        /// <summary>
        /// Сохранить конфигурацию в файл
        /// </summary>
        public async Task SaveConfigAsync(ConfigModel config)
        {
            try
            {
                // Сериализуем объект ConfigModel в JSON
                string json = JsonSerializer.Serialize(config, _jsonOptions);

                // Записываем JSON в файл
                await File.WriteAllTextAsync(_configFilePath, json);
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем её
                MessageBox.Show($"Ошибка при сохранении конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Сбросить конфигурацию к значениям по умолчанию
        /// </summary>
        public async Task<ConfigModel> ResetConfigAsync()
        {
            // Создаем новую конфигурацию со значениями по умолчанию
            ConfigModel defaultConfig = new ConfigModel();

            try
            {
                // Сохраняем конфигурацию со значениями по умолчанию
                await SaveConfigAsync(defaultConfig);
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем её
                MessageBox.Show($"Ошибка при сбросе конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Возвращаем конфигурацию со значениями по умолчанию
            return defaultConfig;
        }

        /// <summary>
        /// Экспортировать конфигурацию в файл
        /// </summary>
        public async Task ExportConfigAsync(ConfigModel config, string path)
        {
            try
            {
                // Сериализуем объект ConfigModel в JSON
                string json = JsonSerializer.Serialize(config, _jsonOptions);

                // Записываем JSON в указанный файл
                await File.WriteAllTextAsync(path, json);

                MessageBox.Show("Конфигурация успешно экспортирована", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем её
                MessageBox.Show($"Ошибка при экспорте конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Импортировать конфигурацию из файла
        /// </summary>
        public async Task<ConfigModel> ImportConfigAsync(string path)
        {
            try
            {
                // Проверяем, существует ли указанный файл
                if (!File.Exists(path))
                {
                    MessageBox.Show("Файл конфигурации не найден", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return await LoadConfigAsync();
                }

                // Читаем содержимое файла
                string json = await File.ReadAllTextAsync(path);

                // Десериализуем JSON в объект ConfigModel
                ConfigModel config = JsonSerializer.Deserialize<ConfigModel>(json, _jsonOptions);

                // Если десериализация не удалась, возвращаем текущую конфигурацию
                if (config == null)
                {
                    MessageBox.Show("Некорректный формат файла конфигурации", "Ошибка",
                                   MessageBoxButton.OK, MessageBoxImage.Error);
                    return await LoadConfigAsync();
                }

                // Сохраняем импортированную конфигурацию
                await SaveConfigAsync(config);

                MessageBox.Show("Конфигурация успешно импортирована", "Успех",
                               MessageBoxButton.OK, MessageBoxImage.Information);

                return config;
            }
            catch (Exception ex)
            {
                // В случае ошибки логируем её и возвращаем текущую конфигурацию
                MessageBox.Show($"Ошибка при импорте конфигурации: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                return await LoadConfigAsync();
            }
        }
    }
}