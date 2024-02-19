using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;
using RzdHack.Robot.Core;


namespace RailwayWizzard.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : Controller
    {
        private readonly RailwayWizzardAppContext _context;

        public UsersController(RailwayWizzardAppContext context)
        {
            _context=context;
        }

        [HttpGet]
        public async Task<List<User>> Get()
        {
            return await _context.User.ToListAsync();
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(User user)
        {
            if (ModelState.IsValid)
            {
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
                return Ok("Success User Create");
            }

            return BadRequest("Request param is no valid");
        }
    }
}
