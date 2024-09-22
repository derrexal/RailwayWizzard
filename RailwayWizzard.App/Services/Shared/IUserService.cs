using RailwayWizzard.App.Dto;
using RailwayWizzard.Core;

namespace RailwayWizzard.App.Services.Shared
{
    public interface IUserService
    {
        public Task CreateOrUpdateAsync(CreateUserDto user);
    }
}
