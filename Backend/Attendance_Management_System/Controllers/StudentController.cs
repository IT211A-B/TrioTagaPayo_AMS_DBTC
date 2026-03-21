using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Manages student records — create, read, update, delete, and paginate.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;
        public StudentController(IStudentService studentService) => _studentService = studentService;

        /// <summary>
        /// Gets all students with optional pagination.
        /// </summary>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Number of records per page (default: 10, max: 100).</param>
        /// <response code="200">Returns paginated list of students.</response>
        /// <response code="401">Unauthorized — JWT token missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var all = (await _studentService.GetAllAsync()).ToList();

            pageSize = Math.Min(pageSize, 100);
            var totalCount = all.Count;
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize);

            return Ok(new
            {
                data = paged,
                page,
                pageSize,
                totalCount,
                totalPages
            });
        }

        /// <summary>Gets a single student by ID.</summary>
        /// <param name="id">The student ID.</param>
        /// <response code="200">Returns the student.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Student not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var student = await _studentService.GetByIdAsync(id);
            return student == null
                ? NotFound(new { message = $"Student with ID {id} not found." })
                : Ok(student);
        }

        /// <summary>Creates a new student record.</summary>
        /// <param name="dto">Student data to create.</param>
        /// <response code="201">Student created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            var created = await _studentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing student record.</summary>
        /// <param name="id">The student ID to update.</param>
        /// <param name="dto">Updated student data.</param>
        /// <response code="200">Student updated successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Student not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateStudentDto dto)
        {
            var updated = await _studentService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = $"Student with ID {id} not found." })
                : Ok(updated);
        }

        /// <summary>Deletes a student record by ID.</summary>
        /// <param name="id">The student ID to delete.</param>
        /// <response code="204">Student deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Student not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _studentService.DeleteAsync(id);
            return deleted
                ? NoContent()
                : NotFound(new { message = $"Student with ID {id} not found." });
        }
    }
}