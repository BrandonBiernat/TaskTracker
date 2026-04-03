using api.Controllers.Auth.Models;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Interfaces.ReturnResults;
using Shared.Interfaces.Services;

namespace api.Controllers.Auth
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] Register_RequestModel request) {
            IOperationResult result = await
                authService
                .RegisterAsync(
                    email: request.Email, 
                    password: request.Password,
                    firstName: request.FirstName,
                    lastName: request.LastName);
            return result.HasSuccessStatus 
                ? Created()
                : BadRequest(new { error = result.Message });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] Login_RequestModel request) {
            IOperationResult<Token> result = await
                authService
                .LoginAsync(
                    email: request.Email,
                    password: request.Password);
            return result.HasSuccessStatus
                ? Ok(new { token = result.Payload!.Value })
                : Unauthorized(new { error = result.Message });
        }
    }
}