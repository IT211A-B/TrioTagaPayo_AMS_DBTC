using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Handles user authentication — login and JWT token generation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// The token is valid for 8 hours and must be included in the
        /// Authorization header as "Bearer {token}" for all protected endpoints.
        /// </summary>
        /// <param name="request">Username and password credentials.</param>
        /// <response code="200">Login successful. Returns token, username, role, and expiration.</response>
        /// <response code="400">Username or password is missing.</response>
        /// <response code="401">Invalid username or password.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required." });

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });

            return Ok(result);
        }
    }
}