using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    /// <summary>
    /// Manages course records — create, read, update, delete, and paginate.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly ICourseService _courseService;
        public CourseController(ICourseService courseService) => _courseService = courseService;

        /// <summary>
        /// Gets all courses with optional pagination.
        /// </summary>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Number of records per page (default: 10, max: 100).</param>
        /// <response code="200">Returns paginated list of courses with teacher info.</response>
        /// <response code="401">Unauthorized — JWT token missing or invalid.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var all = (await _courseService.GetAllAsync()).ToList();

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

        /// <summary>Gets a single course by ID.</summary>
        /// <param name="id">The course ID.</param>
        /// <response code="200">Returns the course with teacher info.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Course not found.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetByIdAsync(id);
            return course == null
                ? NotFound(new { message = $"Course with ID {id} not found." })
                : Ok(course);
        }

        /// <summary>Creates a new course and assigns it to a teacher.</summary>
        /// <param name="dto">Course data including teacher assignment.</param>
        /// <response code="201">Course created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Unauthorized.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var created = await _courseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing course.</summary>
        /// <param name="id">The course ID to update.</param>
        /// <param name="dto">Updated course data.</param>
        /// <response code="200">Course updated successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Course not found.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCourseDto dto)
        {
            var updated = await _courseService.UpdateAsync(id, dto);
            return updated == null
                ? NotFound(new { message = $"Course with ID {id} not found." })
                : Ok(updated);
        }

        /// <summary>Deletes a course by ID.</summary>
        /// <param name="id">The course ID to delete.</param>
        /// <response code="204">Course deleted successfully.</response>
        /// <response code="401">Unauthorized.</response>
        /// <response code="404">Course not found.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _courseService.DeleteAsync(id);
            return deleted
                ? NoContent()
                : NotFound(new { message = $"Course with ID {id} not found." });
        }
    }
}