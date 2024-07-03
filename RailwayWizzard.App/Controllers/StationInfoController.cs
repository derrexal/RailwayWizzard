using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayWizzard.Core;
using RailwayWizzard.EntityFrameworkCore.Data;


namespace RailwayWizzard.App.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class StationInfoController : Controller
    {

        private readonly RailwayWizzardAppContext _context;
        private readonly ILogger _logger;

        public StationInfoController(RailwayWizzardAppContext context, ILogger<StationInfoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetByName")]
        public async Task<StationInfo?> GetByName(StationInfo stationInfo) => 
            await _context.StationInfo.FirstOrDefaultAsync(s=>s.StationName==stationInfo.StationName);
        

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(StationInfo stationInfo)
        {
            if (ModelState.IsValid)
                return BadRequest("Request param is no valid");
            var currentStationInfo = await _context.StationInfo.FirstOrDefaultAsync(u => u.ExpressCode == stationInfo.ExpressCode);
            if (currentStationInfo is null)
                _context.Add(stationInfo);
            else
            {
                currentStationInfo.StationName= stationInfo.StationName;
                currentStationInfo.ExpressCode = stationInfo.ExpressCode;
                _context.Update(currentStationInfo);
            }
            await _context.SaveChangesAsync();
            _logger.LogTrace($"Success create or update StationInfo. StationName:{stationInfo.StationName} ExpressCode:{stationInfo.ExpressCode}");
            return Ok("Success StationInfo CreateOrUpdate");
        }
    }
}
