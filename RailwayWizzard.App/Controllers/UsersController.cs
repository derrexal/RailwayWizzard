using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;

//TODO: заменить все IsValid на FromBody
namespace RailwayWizzard.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly RailwayWizzardAppContext _context;
        private readonly ILogger _logger;

        public UsersController(RailwayWizzardAppContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<List<User>> Get() => 
            await _context.User.ToListAsync();

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(User user)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");
            
            var currentUser = await _context.User.FirstOrDefaultAsync(u=>u.IdTg == user.IdTg);
            if (currentUser is null)
                _context.Add(user);
            else
            {
                currentUser.IdTg = user.IdTg;
                currentUser.Username = user.Username;
                _context.Update(currentUser);
            }

            await _context.SaveChangesAsync();
            _logger.LogTrace($"Success create or update User. IdTg:{user.IdTg} Username:{user.Username}");
            return Ok("Success User Create");
        }
    }
}