using MapperGangNET8.Models;
using System.Threading.Tasks;

namespace MapperGangNET8.Services.ConfigService
{
    /// <summary>
    /// Интерфейс сервиса конфигурации
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// Загрузить конфигурацию
        /// </summary>
        /// <returns>Модель конфигурации</returns>
        Task<ConfigModel> LoadConfigAsync();

        /// <summary>
        /// Сохранить конфигурацию
        /// </summary>
        /// <param name="config">Модель конфигурации</param>
        Task SaveConfigAsync(ConfigModel config);

        /// <summary>
        /// Сбросить конфигурацию к значениям по умолчанию
        /// </summary>
        /// <returns>Модель конфигурации со значениями по умолчанию</returns>
        Task<ConfigModel> ResetConfigAsync();

        /// <summary>
        /// Экспортировать конфигурацию в файл
        /// </summary>
        /// <param name="config">Модель конфигурации</param>
        /// <param name="path">Путь к файлу</param>
        Task ExportConfigAsync(ConfigModel config, string path);

        /// <summary>
        /// Импортировать конфигурацию из файла
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Модель конфигурации</returns>
        Task<ConfigModel> ImportConfigAsync(string path);

        event EventHandler ConfigurationReset;
        void NotifyConfigurationReset();

        /// <summary>
        /// Получить список всех профилей
        /// </summary>
        Task<List<string>> GetProfilesAsync();

        /// <summary>
        /// Получить профиль по имени
        /// </summary>
        Task<ProfileModel> GetProfileAsync(string name);

        /// <summary>
        /// Создать новый профиль
        /// </summary>
        Task<ProfileModel> CreateProfileAsync(string name, string description = "");

        /// <summary>
        /// Обновить профиль
        /// </summary>
        Task UpdateProfileAsync(string name, ProfileModel profile);

        /// <summary>
        /// Удалить профиль
        /// </summary>
        Task DeleteProfileAsync(string name);

        /// <summary>
        /// Переключиться на профиль
        /// </summary>
        Task SwitchToProfileAsync(string name);

        /// <summary>
        /// Получить активный профиль
        /// </summary>
        Task<ProfileModel> GetActiveProfileAsync();

        /// <summary>
        /// Получить имя активного профиля
        /// </summary>
        Task<string> GetActiveProfileNameAsync();
    }
}
