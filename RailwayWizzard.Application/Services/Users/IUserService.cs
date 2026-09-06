using RailwayWizzard.Application.Dto.User;

namespace RailwayWizzard.Application.Services.Users
{
    /// <summary>
    /// Сервис пользователей.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Создать пользователя или обновляет информацию о нем.
        /// </summary>
        /// <param name="user">Модель пользователя.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task CreateOrUpdateAsync(CreateUserDto user);
    }
}
