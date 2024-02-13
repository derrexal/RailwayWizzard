using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.App.Data;
using RailwayWizzard.Core;
using RzdHack_Robot.Core;


namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NotificationTaskController : Controller
    {

        private readonly RailwayWizzardAppContext _context;

        public NotificationTaskController(RailwayWizzardAppContext context)
        {
            _context = context;
        }


        [HttpPost("Create")]
        public async Task<IActionResult> Create(NotificationTask stationInfo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(stationInfo);
                await _context.SaveChangesAsync();
                return Ok("Success NotificationTask Create");
            }

            return BadRequest("Request param is no valid");
        }
    }
}
