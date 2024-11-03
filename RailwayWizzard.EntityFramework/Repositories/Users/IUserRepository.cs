using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.Users
{
    /// <summary>
    /// Репозиторий сущности <see cref="User"/>.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Создает или обновляет пользователя.
        /// </summary>
        /// <param name="createUserDto"></param>
        /// <returns></returns>
        public Task CreateOrUpdateAsync(User createUserDto);
    }
}
