using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using System.Security.Claims;
using QRCoder;

namespace Attendance_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        private readonly IConfiguration _configuration;
        private readonly ICourseService _courseService;

        public TeacherController(
            ITeacherService teacherService,
            IConfiguration configuration,
            ICourseService courseService)
        {
            _teacherService = teacherService;
            _configuration = configuration;
            _courseService = courseService;
        }

        /// <summary>Gets all teachers with optional pagination.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var all = await _teacherService.GetAllAsync();
            return Ok(PaginationHelper.Paginate(all, page, pageSize));
        }

        /// <summary>Gets a single teacher by ID.</summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            return teacher == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(teacher);
        }

        /// <summary>Creates a new teacher record.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var created = await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Creates a new teacher with a linked user account.</summary>
        [HttpPost("with-account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateWithAccount([FromBody] CreateTeacherWithAccountDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });
            var result = await _teacherService.CreateWithAccountAsync(dto);
            return result == null
                ? BadRequest(new { message = "Username already exists." })
                : Ok(result);
        }

        /// <summary>Updates an existing teacher record.</summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            var updated = await _teacherService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(updated);
        }

        /// <summary>Updates the linked user account of a teacher.</summary>
        [HttpPut("{id}/account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateTeacherAccountDto dto)
        {
            var result = await _teacherService.UpdateAccountAsync(id, dto);
            return result == null
                ? BadRequest(new { message = "Teacher not found or username already taken." })
                : Ok(result);
        }

        /// <summary>Deletes a teacher record by ID.</summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _teacherService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = $"Teacher with ID {id} not found." });
        }

        /// <summary>Toggles the active/inactive status of a teacher.</summary>
        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _teacherService.ToggleStatusAsync(id);
            return result == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(result);
        }

        /// <summary>
        /// Generates an enrollment QR code for a specific course.
        /// This is called by the frontend button in the teacher dashboard.
        /// </summary>
        [HttpPost("GenerateCourseQRCode")]
        [Authorize(Roles = "Admin,Teacher")]
        [ValidateAntiForgeryToken]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GenerateCourseQRCode([FromForm] int courseId)
        {
            // Verify the course exists
            var course = await _courseService.GetByIdAsync(courseId);
            if (course == null)
                return Ok(new { success = false, message = "Course not found" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Teacher")
            {
                var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim))
                    return Ok(new { success = false, message = "Teacher ID not found in token." });

                var teacherId = int.Parse(teacherIdClaim);
                if (course.TeacherId != teacherId)
                    return Ok(new { success = false, message = "You don't own this course." });
            }

            // Generate the QR code
            var frontendUrl = _configuration["FrontendUrl"] ?? "https://your-frontend.onrender.com";
            var enrollmentUrl = $"{frontendUrl}/Student/SelfEnroll?courseId={courseId}";
            var qrCodeBase64 = GenerateQRCodeBase64(enrollmentUrl);

            return Ok(new
            {
                success = true,
                qrCode = qrCodeBase64,
                courseName = course.CourseName
            });
        }

        private string GenerateQRCodeBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }
    }
}