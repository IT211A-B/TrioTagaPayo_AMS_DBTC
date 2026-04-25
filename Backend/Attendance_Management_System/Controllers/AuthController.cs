using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Handles user authentication — login, JWT generation, token refresh, and logout.
    /// Supports both Bearer token (mobile) and HttpOnly cookie (browser).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService) => _authService = authService;

        /// <summary>
        /// Authenticates a user and returns a JWT access token + refresh token.
        /// Also stores both tokens in HttpOnly cookies for browser-based clients.
        /// Access token expires in 8 hours. Refresh token expires in 7 days.
        /// </summary>
        /// <param name="request">Username and password credentials.</param>
        /// <response code="200">Login successful. Returns token, username, role, and expiration.</response>
        /// <response code="400">Username or password is missing.</response>
        /// <response code="401">Invalid username or password.</response>
        /// <response code="429">Too many login attempts. Try again after 1 minute.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required." });

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });

            // ✅ COOKIE SESSION — store JWT sa HttpOnly cookie
            // HttpOnly = JS dili maka-access (XSS protection)
            // Secure   = HTTPS only
            // SameSite = CSRF protection
            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Expiration
            });

            // ✅ Refresh token sa separate cookie
            // Path = accessible sa /api/Auth/refresh endpoint lang
            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiry,
                Path = "/api/Auth/refresh"
            });

            return Ok(result);
        }

        /// <summary>
        /// Refreshes an expired JWT access token using a valid refresh token.
        /// Accepts either JSON body OR cookie — auto-detect.
        /// Works for both browser (cookie) and mobile (body) clients.
        /// </summary>
        /// <param name="request">The refresh token (optional if cookie is present).</param>
        /// <response code="200">Returns new access token + new refresh token.</response>
        /// <response code="400">Refresh token is missing.</response>
        /// <response code="401">Refresh token is invalid or expired.</response>
        [AllowAnonymous]
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request)
        {
            // ✅ Auto-detect: cookie first (browser), then JSON body (mobile)
            var token = Request.Cookies["refreshToken"]
                        ?? request?.RefreshToken;

            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Refresh token is required." });

            var result = await _authService.RefreshAsync(token);

            if (result == null)
                return Unauthorized(new { message = "Refresh token is invalid or has expired. Please log in again." });

            // ✅ Update cookies with new tokens (rotation)
            Response.Cookies.Append("accessToken", result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Expiration
            });

            Response.Cookies.Append("refreshToken", result.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.RefreshTokenExpiry,
                Path = "/api/Auth/refresh"
            });

            return Ok(result);
        }

        /// <summary>
        /// Logs out the user — clears both JWT cookies from the browser.
        /// </summary>
        /// <response code="200">Logged out successfully.</response>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Logout()
        {
            // ✅ Delete both cookies on logout
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully." });
        }

        /// <summary>
        /// Returns the currently logged-in user's info from JWT claims.
        /// Useful for frontend to know who is logged in after page refresh.
        /// </summary>
        /// <response code="200">Returns userId, username, and role.</response>
        /// <response code="401">Unauthorized — JWT token missing or invalid.</response>
        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult GetMe()
        {
            // ✅ CLAIMS INFO — read directly from validated JWT
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var role = User.GetRole();

            return Ok(new { userId, username, role });
        }
    }
}