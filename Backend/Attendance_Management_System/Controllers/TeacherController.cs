using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Manages teacher records and their login accounts — create, read, update, delete, paginate, and toggle status.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService _teacherService;
        public TeacherController(ITeacherService teacherService) => _teacherService = teacherService;

        /// <summary>Gets all teachers with optional pagination.</summary>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Records per page (default: 10, max: 100).</param>
        /// <response code="200">Returns paginated list of teachers.</response>
        /// <response code="401">Unauthorized — JWT token missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var all = (await _teacherService.GetAllAsync()).ToList();
            pageSize = Math.Min(pageSize, 100);
            var totalCount = all.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);
            return Ok(new { data = paged, page, pageSize, totalCount, totalPages });
        }

        /// <summary>Gets a single teacher by ID.</summary>
        /// <param name="id">The teacher ID.</param>
        /// <response code="200">Returns the teacher.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Teacher not found.</response>
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

        /// <summary>Creates a teacher record without a login account.</summary>
        /// <response code="201">Teacher created.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateTeacherDto dto)
        {
            var created = await _teacherService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Creates a teacher record AND a login account simultaneously.
        /// Used by Admin to provision teacher credentials.
        /// </summary>
        /// <param name="dto">Teacher info plus username and password.</param>
        /// <response code="200">Teacher and login account created.</response>
        /// <response code="400">Username already exists or credentials missing.</response>
        /// <response code="401">Unauthorized.</response>
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

        /// <summary>Updates a teacher's basic info.</summary>
        /// <response code="200">Teacher updated.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Teacher not found.</response>
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

        /// <summary>
        /// Updates a teacher's login credentials (username and/or password).
        /// Leave blank to keep current value unchanged.
        /// </summary>
        /// <response code="200">Account updated.</response>
        /// <response code="400">Username taken or teacher has no account.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Teacher not found.</response>
        [HttpPut("{id}/account")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateTeacherAccountDto dto)
        {
            var result = await _teacherService.UpdateAccountAsync(id, dto);
            return result == null
                ? BadRequest(new { message = "Teacher not found or username already taken." })
                : Ok(result);
        }

        /// <summary>
        /// Deletes a teacher AND their login account permanently.
        /// </summary>
        /// <response code="204">Deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Teacher not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _teacherService.DeleteAsync(id);
            return deleted ? NoContent() : NotFound(new { message = $"Teacher with ID {id} not found." });
        }

        /// <summary>
        /// Toggles a teacher's active/disabled status.
        /// A disabled teacher cannot log in.
        /// </summary>
        /// <response code="200">Status toggled.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Teacher not found.</response>
        [HttpPatch("{id}/toggle-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var result = await _teacherService.ToggleStatusAsync(id);
            return result == null ? NotFound(new { message = $"Teacher with ID {id} not found." }) : Ok(result);
        }
    }
}