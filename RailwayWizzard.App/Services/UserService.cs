using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Dto;
using RailwayWizzard.App.Services.Shared;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore;

namespace RailwayWizzard.App.Services
{
    /// <inheritdoc/>
    public class UserService : IUserService
    {
        private readonly RailwayWizzardAppContext _context;
        private readonly ILogger _logger;

        public UserService(RailwayWizzardAppContext context, ILogger<UserService> logger)
        {
            _context = context;
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

            var currentUser = await _context.User.FirstOrDefaultAsync(u => u.IdTg == user.IdTg);
            if (currentUser is null)
                _context.Add(user);
            else
            {
                currentUser.IdTg = user.IdTg;
                currentUser.Username = user.Username;
                _context.Update(currentUser);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Success create or update User. IdTg:{user.IdTg} Username:{user.Username}");
        }
    }
}
