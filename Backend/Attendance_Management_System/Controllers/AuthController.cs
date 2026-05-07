using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using BCrypt.Net;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IPasswordHasher _hasher;

        public AuthController(
            IAuthService authService,
            IUserRepository userRepository,
            IStudentRepository studentRepository,
            IPasswordHasher hasher)
        {
            _authService = authService;
            _userRepository = userRepository;
            _studentRepository = studentRepository;
            _hasher = hasher;
        }

        [AllowAnonymous]
        [HttpGet("test-bcrypt")]
        public IActionResult TestBcrypt()
        {
            try
            {
                var testPassword = "admin123";
                var testHash = BCrypt.Net.BCrypt.HashPassword(testPassword);
                var isValid = BCrypt.Net.BCrypt.Verify(testPassword, testHash);

                return Ok(new { success = true, isValid, message = isValid ? "BCrypt is working!" : "BCrypt failed!" });
            }
            catch (Exception ex)
            {
                return Ok(new { success = false, error = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and password are required." });

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Invalid username or password." });

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

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest? request)
        {
            var token = Request.Cookies["refreshToken"] ?? request?.RefreshToken;

            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Refresh token is required." });

            var result = await _authService.RefreshAsync(token);

            if (result == null)
                return Unauthorized(new { message = "Refresh token is invalid or has expired." });

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

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("accessToken");
            Response.Cookies.Delete("refreshToken");
            return Ok(new { message = "Logged out successfully." });
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetMe()
        {
            var userId = User.GetUserId();
            var username = User.GetUsername();
            var role = User.GetRole();

            return Ok(new { userId, username, role });
        }

        /// <summary>
        /// ✅ STUDENT REGISTRATION ENDPOINT
        /// POST /api/Auth/register
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Password) ||
                string.IsNullOrWhiteSpace(dto.FullName))
            {
                return BadRequest(new { success = false, message = "Username, password, and full name are required" });
            }

            // Check if user already exists
            var existingUser = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (existingUser != null)
            {
                return BadRequest(new { success = false, message = "Username already exists" });
            }

            // Parse full name
            var (firstName, lastName) = ParseFullName(dto.FullName);

            // Generate student number (STU001, STU002, etc.)
            var studentNo = await GenerateStudentNumber();

            // Create User account
            var user = new User
            {
                Username = dto.Username,
                PasswordHash = _hasher.Hash(dto.Password),
                Role = "Student",
                CreatedAt = DateTime.UtcNow,
                RefreshToken = null,
                RefreshTokenExpiry = null
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            // Create Student record
            var student = new Student
            {
                StudentNo = studentNo,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = "",
                Email = $"{studentNo}@student.edu",
                Section = "",
                MobileNo = "",
                CreatedAt = DateTime.UtcNow
            };
            await _studentRepository.AddAsync(student);
            await _studentRepository.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Registration successful",
                studentId = student.Id,
                studentNo = student.StudentNo,
                username = user.Username
            });
        }

        private async Task<string> GenerateStudentNumber()
        {
            var students = await _studentRepository.GetAllAsync();
            var count = students.Count() + 1;
            return $"STU{count:D3}";
        }

        private (string FirstName, string LastName) ParseFullName(string fullName)
        {
            var trimmed = fullName.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return ("", "");

            if (parts.Length == 1)
                return (parts[0], "");

            return (parts[0], string.Join(" ", parts.Skip(1)));
        }
    }
}