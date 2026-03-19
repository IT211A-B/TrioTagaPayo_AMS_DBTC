using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _attendanceService;
        public AttendanceController(IAttendanceService attendanceService) => _attendanceService = attendanceService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _attendanceService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var record = await _attendanceService.GetByIdAsync(id);
            return record == null ? NotFound() : Ok(record);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetByCourse(int courseId) =>
            Ok(await _attendanceService.GetByCourseAsync(courseId));

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetByStudent(int studentId) =>
            Ok(await _attendanceService.GetByStudentAsync(studentId));

        // For Att. Records page — filter by course + date range
        [HttpGet("filter")]
        public async Task<IActionResult> GetByFilter(
            [FromQuery] int courseId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to)
        {
            var records = await _attendanceService.GetByFilterAsync(courseId, from, to);
            return Ok(records);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceDto dto)
        {
            var created = await _attendanceService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // For "Save Attendance" button — saves all students in one course at once
        [HttpPost("bulk")]
        public async Task<IActionResult> BulkCreate([FromBody] List<CreateAttendanceDto> dtos)
        {
            if (dtos == null || dtos.Count == 0)
                return BadRequest(new { message = "No attendance records provided." });

            var result = await _attendanceService.BulkCreateAsync(dtos);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAttendanceDto dto)
        {
            var updated = await _attendanceService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _attendanceService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}