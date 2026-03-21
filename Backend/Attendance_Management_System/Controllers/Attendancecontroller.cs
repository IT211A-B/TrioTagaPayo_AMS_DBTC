using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Manages attendance records — create, read, update, delete, bulk save, and filter.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        /// <summary>Gets all attendance records.</summary>
        /// <returns>List of all attendance records.</returns>
        /// <response code="200">Returns the list of attendance records.</response>
        /// <response code="401">Unauthorized — JWT token missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll() =>
            Ok(await _attendanceService.GetAllAsync());

        /// <summary>Gets a single attendance record by ID.</summary>
        /// <param name="id">The attendance record ID.</param>
        /// <response code="200">Returns the attendance record.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Attendance record not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _attendanceService.GetByIdAsync(id);
            return record == null ? NotFound(new { message = $"Attendance record {id} not found." }) : Ok(record);
        }

        /// <summary>Gets all attendance records for a specific course.</summary>
        /// <param name="courseId">The course ID.</param>
        /// <response code="200">Returns attendance records for the course.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("course/{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCourse(int courseId) =>
            Ok(await _attendanceService.GetByCourseAsync(courseId));

        /// <summary>Gets all attendance records for a specific student.</summary>
        /// <param name="studentId">The student ID.</param>
        /// <response code="200">Returns attendance records for the student.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByStudent(int studentId) =>
            Ok(await _attendanceService.GetByStudentAsync(studentId));

        /// <summary>
        /// Filters attendance records by course and date range.
        /// Used on the Attendance Records page.
        /// </summary>
        /// <param name="courseId">Course ID to filter by.</param>
        /// <param name="from">Start date (yyyy-MM-dd).</param>
        /// <param name="to">End date (yyyy-MM-dd).</param>
        /// <response code="200">Returns filtered attendance records ordered by date descending.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByFilter(
            [FromQuery] int courseId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to) =>
            Ok(await _attendanceService.GetByFilterAsync(courseId, from, to));

        /// <summary>Creates a single attendance record.</summary>
        /// <param name="dto">Attendance data to create.</param>
        /// <response code="201">Attendance record created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceDto dto)
        {
            var created = await _attendanceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Bulk creates attendance records for all students in a course at once.
        /// Used by the "Save Attendance" button on the dashboard.
        /// </summary>
        /// <param name="dtos">List of attendance records to save.</param>
        /// <response code="200">All attendance records saved successfully.</response>
        /// <response code="400">No records provided or invalid data.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost("bulk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateAttendanceDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { message = "No attendance records provided." });
            var result = await _attendanceService.BulkCreateAsync(dtos);
            return Ok(result);
        }

        /// <summary>Updates an existing attendance record.</summary>
        /// <param name="id">The attendance record ID to update.</param>
        /// <param name="dto">Updated attendance data.</param>
        /// <response code="200">Attendance record updated successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Attendance record not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceDto dto)
        {
            var updated = await _attendanceService.UpdateAsync(id, dto);
            return updated == null ? NotFound(new { message = $"Attendance record {id} not found." }) : Ok(updated);
        }

        /// <summary>Deletes an attendance record by ID.</summary>
        /// <param name="id">The attendance record ID to delete.</param>
        /// <response code="204">Attendance record deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Attendance record not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _attendanceService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = $"Attendance record {id} not found." });
        }
    }
}