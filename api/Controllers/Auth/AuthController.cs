using System.Security.Claims;
using api.Controllers.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared;
using Shared.Interfaces.ReturnResults;
using Shared.Interfaces.Services;
using Shared.Models;

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
                : BadRequest(new { errors = new[] { result.Message } });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] Login_RequestModel request) {
            IOperationResult<AuthTokens> result = await
                authService
                .LoginAsync(
                    email: request.Email,
                    password: request.Password);
            return result.HasSuccessStatus
                ? Ok(new {
                    access_token = result.Payload!.AccessToken.Value,
                    refresh_token = result.Payload.RefreshToken
                }) : Unauthorized(new { errors = new[] { result.Message } });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] Refresh_RequestModel request) {
            IOperationResult<AuthTokens> result = await
                authService
                .RefreshAsync(request.RefreshToken);
            return result.HasSuccessStatus
                ? Ok(new { 
                    access_token = result.Payload!.AccessToken.Value, 
                    refresh_token = result.Payload.RefreshToken 
                }) : Unauthorized(new { errors = new[] { result.Message } });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            string? userUid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userUid is null)
                return Unauthorized(new { errors = new[] { "Invalid token." } });

            IOperationResult result = await
                authService
                .LogoutAsync(new UserUID(Guid.Parse(userUid)));
            return result.HasSuccessStatus
                ? NoContent()
                : BadRequest(new { errors = new[] { result.Message } });
        }
    }
}