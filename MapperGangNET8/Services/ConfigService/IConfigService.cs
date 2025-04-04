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
    }
}