using RailwayWizzard.App.Dto.User;

namespace RailwayWizzard.App.Services.Users
{
    public interface IUserService
    {
        public Task CreateOrUpdateAsync(CreateUserDto user);
    }
}
