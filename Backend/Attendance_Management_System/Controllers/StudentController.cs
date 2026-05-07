using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentController(IStudentService studentService)
        {
            _studentService = studentService;
        }

        /// <summary>Gets all students with optional pagination.</summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var all = await _studentService.GetAllAsync();
            return Ok(PaginationHelper.Paginate(all, page, pageSize));
        }

        /// <summary>Gets a single student by ID.</summary>
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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateStudentDto dto)
        {
            // Auto-generate student number if not provided
            if (string.IsNullOrWhiteSpace(dto.StudentNo))
            {
                dto.StudentNo = await GenerateStudentNumber();
            }

            var created = await _studentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>Updates an existing student record.</summary>
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

        /// <summary>
        /// Generates a student number in STU001 format
        /// </summary>
        private async Task<string> GenerateStudentNumber()
        {
            var students = await _studentService.GetAllAsync();
            var count = students.Count() + 1;
            return $"STU{count:D3}";  // STU001, STU002, STU003...
        }
    }
}