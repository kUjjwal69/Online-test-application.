using Microsoft.AspNetCore.Mvc;
using TestManagementApplication.Common;
using TestManagementApplication.Models.DTOs.Auth;
using TestManagementApplication.Services.Interfaces;

namespace TestManagementApplication.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Register a new candidate account</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthResponse>.Ok(result, "Registration successful."));
        }

        /// <summary>Login and receive a JWT token</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful."));
        }

        /// <summary>Refresh an expired access token using a valid refresh token</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed successfully."));
        }

        /// <summary>Revoke the refresh token (logout)</summary>
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequest request)
        {
            await _authService.RevokeTokenAsync(request.AccessToken);
            return Ok(ApiResponse<string>.Ok("Refresh token revoked successfully.", "Logout successful."));
        }
    }

    public class RevokeTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
    }
}
