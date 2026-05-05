using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using System.Security.Claims;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Handles QR code generation (teacher) and scanning (student).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class QRController : ControllerBase
    {
        private readonly IQRService _qrService;
        private readonly ICourseService _courseService;

        public QRController(IQRService qrService, ICourseService courseService)
        {
            _qrService = qrService;
            _courseService = courseService;
        }

        /// <summary>
        /// Teacher generates a QR code for a class session.
        /// Returns a Base64 PNG image ready to display on screen/projector.
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Generate([FromBody] GenerateQRDto dto)
        {
            // Verify teacher owns the course
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            if (role == "Teacher")
            {
                var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim))
                    return Unauthorized(new { message = "Teacher ID not found in token." });

                var teacherId = int.Parse(teacherIdClaim);
                var course = await _courseService.GetByIdAsync(dto.CourseId);

                if (course == null)
                    return NotFound(new { message = "Course not found." });

                if (course.TeacherId != teacherId)
                    return StatusCode(403, new { message = "You don't own this course." });
            }

            var result = await _qrService.GenerateAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Student scans the QR code and submits their StudentId.
        /// Automatically marks them Present or Late based on scan time.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("scan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Scan([FromBody] ScanQRDto dto)
        {
            var result = await _qrService.ScanAsync(dto);

            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(result);
        }

        /// <summary>
        /// Teacher manually deactivates a QR session before it expires.
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpPatch("{sessionId}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Deactivate(int sessionId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Teacher")
            {
                var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim))
                    return Unauthorized();

                var teacherId = int.Parse(teacherIdClaim);

                var session = await _qrService.GetSessionByIdAsync(sessionId);
                if (session == null)
                    return NotFound(new { message = $"QR session {sessionId} not found." });

                var course = await _courseService.GetByIdAsync(session.CourseId);
                if (course == null || course.TeacherId != teacherId)
                    return StatusCode(403, new { message = "You don't own this course." });
            }

            var result = await _qrService.DeactivateAsync(sessionId);
            if (!result)
                return NotFound(new { message = $"QR session {sessionId} not found." });

            return Ok(new { message = "QR session deactivated successfully." });
        }

        /// <summary>
        /// Get all currently active QR sessions for a course.
        /// </summary>
        [Authorize(Roles = "Admin,Teacher")]
        [HttpGet("active/{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetActiveSessions(int courseId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Teacher")
            {
                var teacherIdClaim = User.FindFirst("TeacherId")?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim))
                    return Unauthorized();

                var teacherId = int.Parse(teacherIdClaim);
                var course = await _courseService.GetByIdAsync(courseId);

                if (course == null)
                    return NotFound(new { message = "Course not found." });

                if (course.TeacherId != teacherId)
                    return StatusCode(403, new { message = "You don't own this course." });
            }

            var sessions = await _qrService.GetActiveSessionsAsync(courseId);
            return Ok(sessions);
        }
    }
}