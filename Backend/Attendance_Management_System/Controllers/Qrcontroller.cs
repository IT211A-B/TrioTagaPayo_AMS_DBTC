using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

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

        public QRController(IQRService qrService) => _qrService = qrService;

        /// <summary>
        /// Teacher generates a QR code for a class session.
        /// Returns a Base64 PNG image ready to display on screen/projector.
        /// </summary>
        /// <response code="201">QR session created. Contains QRCodeBase64 image.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("generate")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Generate([FromBody] GenerateQRDto dto)
        {
            var result = await _qrService.GenerateAsync(dto);
            return CreatedAtAction(nameof(GetActiveSessions),
                new { courseId = result.CourseId }, result);
        }

        /// <summary>
        /// Student scans the QR code and submits their StudentId.
        /// Automatically marks them Present or Late based on scan time.
        /// </summary>
        /// <response code="200">Scan successful. Attendance saved.</response>
        /// <response code="400">QR expired, already scanned, or invalid token.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("scan")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
        /// <response code="204">Session deactivated.</response>
        /// <response code="404">Session not found.</response>
        [HttpPatch("{sessionId}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(int sessionId)
        {
            var result = await _qrService.DeactivateAsync(sessionId);
            return result
                ? NoContent()
                : NotFound(new { message = $"QR session {sessionId} not found." });
        }

        /// <summary>
        /// Get all currently active QR sessions for a course (teacher dashboard).
        /// </summary>
        /// <response code="200">List of active sessions.</response>
        [HttpGet("active/{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveSessions(int courseId)
        {
            var sessions = await _qrService.GetActiveSessionsAsync(courseId);
            return Ok(sessions);
        }
    }
}