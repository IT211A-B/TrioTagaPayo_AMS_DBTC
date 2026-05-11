using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]           // ✅ Only Admin can access any action
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService) => _teacherService = teacherService;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var all = await _teacherService.GetAllAsync();
            return Ok(PaginationHelper.Paginate(all, page, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            return teacher == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var created = await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost("with-account")]
        public async Task<IActionResult> CreateWithAccount([FromBody] CreateTeacherWithAccountDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Username and password are required." });
            var result = await _teacherService.CreateWithAccountAsync(dto);
            return result == null
                ? BadRequest(new { message = "Username already exists." })
                : Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            var updated = await _teacherService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(updated);
        }

        [HttpPut("{id}/account")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateTeacherAccountDto dto)
        {
            var result = await _teacherService.UpdateAccountAsync(id, dto);
            return result == null
                ? BadRequest(new { message = "Teacher not found or username already taken." })
                : Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _teacherService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = $"Teacher with ID {id} not found." });
        }

        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _teacherService.ToggleStatusAsync(id);
            return result == null
                ? NotFound(new { message = $"Teacher with ID {id} not found." })
                : Ok(result);
        }
    }
}