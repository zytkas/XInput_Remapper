using MapperGang.Models;

namespace MapperGang.Services.ProfileService
{
    /// <summary>
    /// Интерфейс сервиса управления профилями
    /// </summary>
    public interface IProfileService
    {
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