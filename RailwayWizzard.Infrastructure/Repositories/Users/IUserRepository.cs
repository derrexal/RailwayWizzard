using RailwayWizzard.Core.User;

namespace RailwayWizzard.Infrastructure.Repositories.Users
{
    /// <summary>
    /// Репозиторий сущности <see cref="User"/>.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Получить модель пользователя по идентификатору.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Модель пользователя.</returns>
        public Task<User> GetUserByIdAsync(int userId);
        
        /// <summary>
        /// Получить модель пользователя по идентификатору телеграм.
        /// </summary>
        /// <param name="telegramUserId">Идентификатор телеграм пользователя.</param>
        /// <returns>Модель пользователя.</returns>
        public Task<User> GetUserByTelegramIdAsync(long telegramUserId);
        
        /// <summary>
        /// Создать пользователя или обновляет информацию о нем.
        /// </summary>
        /// <param name="createUserDto">Модель пользователя.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task<User> CreateOrUpdateAsync(User createUserDto);
        
        /// <summary>
        /// Выставляет флаг HasBlockedBot пользователю.
        /// </summary>
        /// <param name="userId">Идентификатор пользователя.</param>
        /// <returns>Задача <see cref="Task"/>.</returns>
        public Task SetHasBlockedBotAsync(int userId);
    }
}
