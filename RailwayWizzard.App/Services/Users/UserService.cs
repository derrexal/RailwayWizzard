using RailwayWizzard.App.Dto.User;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Repositories.Users;

namespace RailwayWizzard.App.Services.Users
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger _logger;

        public UserService(
            IUserRepository userRepository,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
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

            await _userRepository.CreateOrUpdateAsync(user);

            _logger.LogInformation($"Success create or update User. IdTg:{user.IdTg} Username:{user.Username}");
        }
    }
}
