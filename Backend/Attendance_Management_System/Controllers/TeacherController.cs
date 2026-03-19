using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService) => _teacherService = teacherService;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _teacherService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var teacher = await _teacherService.GetByIdAsync(id);
            return teacher == null ? NotFound() : Ok(teacher);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var created = await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTeacherDto dto)
        {
            var updated = await _teacherService.UpdateAsync(id, dto);
            return updated == null ? NotFound() : Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _teacherService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        // Enable / Disable teacher — for the toggle button in the UI
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _teacherService.ToggleStatusAsync(id);
            return result == null ? NotFound() : Ok(result);
        }
    }
}