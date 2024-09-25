using RailwayWizzard.App.Dto.User;

namespace RailwayWizzard.App.Services.Users
{
    public interface IUserService : IDisposable
    {
        public Task CreateOrUpdateAsync(CreateUserDto user);
    }
}
