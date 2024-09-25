using RailwayWizzard.App.Dto.User;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.UnitOfWork;

namespace RailwayWizzard.App.Services.Users
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private readonly IRailwayWizzardUnitOfWork _uow;
        private readonly ILogger _logger;

        public UserService(IRailwayWizzardUnitOfWork uow, ILogger<UserService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task CreateOrUpdateAsync(CreateUserDto createUserDto)
        {
            var user = new User
            {
                IdTg = createUserDto.IdTg,
                Username = createUserDto.Username
            };

            await _uow.UserRepository.CreateOrUpdateAsync(user);

            _logger.LogInformation($"Success create or update User. IdTg:{user.IdTg} Username:{user.Username}");
        }
    }
}
