using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;

namespace RailwayWizzard.EntityFrameworkCore.Repositories.Users
{
    public class UserRepository : IUserRepository
    {
        private readonly RailwayWizzardAppContext _context;

        public UserRepository(RailwayWizzardAppContext context)
        {
            _context = context;
        }

        public async Task CreateOrUpdateAsync(User user)
        {
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
        }
    }
}
