using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.Users
{
    /// <summary>
    /// Репозиторий сущности <see cref="User"/>.
    /// </summary>
    public interface IUserRepository
    {
        public Task CreateOrUpdateAsync(User createUserDto);
    }
}
