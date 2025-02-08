using EmployeeManagementSystem.Application.Contracts;
using EmployeeManagementSystem.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace EmployeeManagementSystem.Presentation.Controllers
{
    [Route("api/auth")]
    [ApiController]
    [AllowAnonymous]
    public class AuthenticationController(IUserAccountRepository accountInterface) : ControllerBase
    {

        //Register endpoint
        [HttpPost("register")]
        public async Task<IActionResult> CreateAsync(Register user)
        {
            if (user == null)
            {
                return BadRequest("User Infor not provided!");
            }
            var result = await accountInterface.CreateAsync(user);
            return Ok(result);
        }

        //Login endpoint
        [HttpPost("login")]
        public async Task<IActionResult> SignInAsync(Login user)
        {
            if (user is null)
            {
                return BadRequest("User Infor not provided!");
            }
            var result = await accountInterface.SignInAsync(user);
            return Ok(result);
        }

        //RefreshToken Endpoint
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshToken token)
        {
            if (token == null) return BadRequest("Valid token not provided!");
            var result = await accountInterface.RefreshTokenAsync(token);
            return Ok(result);
        }
    }
}
