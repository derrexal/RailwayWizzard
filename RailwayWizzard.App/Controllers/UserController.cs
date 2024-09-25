using Microsoft.AspNetCore.Mvc;
using RailwayWizzard.App.Dto.User;
using RailwayWizzard.App.Services.Users;

namespace RailwayWizzard.App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("CreateOrUpdate")]
        public async Task<IActionResult> CreateOrUpdate(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest($"Request param is no valid: {ModelState}");

            //TODO: по хорошему должен возвращать юзера. Да и вообще переделать концепцию... Че за колхозный метод.
            await _userService.CreateOrUpdateAsync(createUserDto);

            return Ok("Success User Create");
        }
    }
}