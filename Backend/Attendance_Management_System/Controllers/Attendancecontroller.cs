using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        /// <summary>Gets all attendance records.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _attendanceService.GetAllAsync());
        }

        /// <summary>Gets a single attendance record by ID.</summary>
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
        [HttpGet("course/{courseId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByCourse(int courseId)
        {
            return Ok(await _attendanceService.GetByCourseAsync(courseId));
        }

        /// <summary>Gets all attendance records for a specific student.</summary>
        [HttpGet("student/{studentId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByStudent(int studentId)
        {
            return Ok(await _attendanceService.GetByStudentAsync(studentId));
        }

        /// <summary>Filters attendance records by course and date range.</summary>
        [HttpGet("filter")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByFilter(int courseId, DateOnly from, DateOnly to)
        {
            return Ok(await _attendanceService.GetByFilterAsync(courseId, from, to));
        }

        /// <summary>Creates a single attendance record.</summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceDto dto)
        {
            var created = await _attendanceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Bulk creates attendance records.</summary>
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