using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core.User;
using RailwayWizzard.Infrastructure.Exceptions;

namespace RailwayWizzard.Infrastructure.Repositories.Users
{
    /// <inheritdoc/>
    public class UserRepository : IUserRepository
    {
        private readonly RailwayWizzardAppContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository" /> class.
        /// </summary>
        /// <param name="context">Контекст БД.</param>
        public UserRepository (RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            return user ??
                throw new EntityNotFoundException($"{typeof(User)} with Id: {userId} not found");
        }

        public async Task<User> GetUserByTelegramIdAsync(long telegramUserId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == telegramUserId);

            return user ??
                throw new EntityNotFoundException($"{typeof(User)} with TelegramUserId: {telegramUserId} not found");
        }

        /// <inheritdoc/>
        public async Task<User> CreateOrUpdateAsync(User user)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.TelegramUserId == user.TelegramUserId);
            
            if (existingUser is null)
            {
                await _context.Users.AddAsync(user);
            }
            else
            {
                existingUser.TelegramUserId = user.TelegramUserId;
                existingUser.Username = user.Username;
                existingUser.HasBlockedBot = false;
                _context.Users.Update(existingUser);
            }

            await _context.SaveChangesAsync();
            
            return user;
        }

        public async Task SetHasBlockedBotAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.HasBlockedBot = true;
            
            _context.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
