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

        // ... (your existing methods: Login, Refresh, Logout, GetMe, Register)

        /// <summary>
        /// Allows a user to change their password
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
                string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                return BadRequest(new { success = false, message = "Username, current password, and new password are required" });
            }

            // Validate new password length
            if (dto.NewPassword.Length < 6)
            {
                return BadRequest(new { success = false, message = "New password must be at least 6 characters" });
            }

            // Find the user
            var user = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            // Verify current password
            bool currentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!currentPasswordValid)
            {
                return BadRequest(new { success = false, message = "Current password is incorrect" });
            }

            // Check if new password is same as current
            bool sameAsCurrent = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);
            if (sameAsCurrent)
            {
                return BadRequest(new { success = false, message = "New password must be different from current password" });
            }

            // Update password
            user.PasswordHash = _hasher.Hash(dto.NewPassword);
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { success = true, message = "Password changed successfully" });
        }
    }
}