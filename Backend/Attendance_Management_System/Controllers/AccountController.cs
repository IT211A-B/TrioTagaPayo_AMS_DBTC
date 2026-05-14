using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;
using System.Security.Claims;

namespace Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IWebHostEnvironment _environment;

        public AccountController(IUserRepository userRepository, IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _environment = environment;
        }

        /// <summary>
        /// Update profile photo for the currently logged-in user
        /// </summary>
        [HttpPost("update-profile-photo")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]          // ✅ added
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] // ✅ added
        public async Task<IActionResult> UpdateProfilePhoto()
        {
            try
            {
                // Get current user ID from JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { success = false, message = "User not authenticated" });
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { success = false, message = "User not found" });
                }

                // Check if file exists
                if (Request.Form.Files == null || Request.Form.Files.Count == 0)
                {
                    return BadRequest(new { success = false, message = "No file selected" });
                }

                var file = Request.Form.Files[0];
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { success = false, message = "Invalid file" });
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    return BadRequest(new { success = false, message = "Only image files (jpg, jpeg, png, gif, webp) are allowed" });
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { success = false, message = "File size must be less than 5MB" });
                }

                // Create uploads directory if not exists
                string webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string uploadsFolder = Path.Combine(webRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Delete old profile photo if exists
                if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
                {
                    var oldFilePath = Path.Combine(webRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Save new file
                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Update user with new photo URL
                string photoUrl = $"/uploads/profiles/{uniqueFileName}";
                user.ProfilePhotoUrl = photoUrl;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return Ok(new { success = true, message = "Profile photo updated successfully", photoUrl = photoUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = $"An error occurred: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get current user's profile info
        /// </summary>
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]          // ✅ added
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]// ✅ added
        [ProducesResponseType(StatusCodes.Status404NotFound)]    // ✅ added
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found" });
            }

            return Ok(new
            {
                success = true,
                username = user.Username,
                role = user.Role,
                profilePhotoUrl = user.ProfilePhotoUrl ?? "/images/default-avatar.png",
                createdAt = user.CreatedAt
            });
        }
    }
}