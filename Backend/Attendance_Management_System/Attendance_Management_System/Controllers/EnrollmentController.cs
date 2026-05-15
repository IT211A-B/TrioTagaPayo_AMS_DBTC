using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Microsoft.AspNetCore.Authorization;

namespace Attendance_Management_System.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EnrollmentController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get course details by ID (for enrollment page)
        /// </summary>
        [HttpGet("course/{id}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCourseForEnrollment(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound(new { success = false, message = "Course not found" });
            }

            return Ok(new
            {
                id = course.Id,
                courseCode = course.CourseCode,
                courseName = course.CourseName,
                section = course.Section,
                schedule = course.Schedule,
                teacherId = course.TeacherId,
                teacherName = course.Teacher != null ? $"{course.Teacher.FirstName} {course.Teacher.LastName}" : "",
                units = course.Units
            });
        }

        /// <summary>
        /// Student self-enrollment via QR code (no login required)
        /// </summary>
        [HttpPost("self-enroll")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SelfEnroll([FromBody] SelfEnrollRequest request)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.StudentId) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                request.CourseId <= 0)
            {
                return BadRequest(new SelfEnrollResponse
                {
                    Success = false,
                    Message = "Student ID, Full Name, and Course are required"
                });
            }

            // Check if course exists
            var course = await _context.Courses.FindAsync(request.CourseId);
            if (course == null)
            {
                return BadRequest(new SelfEnrollResponse
                {
                    Success = false,
                    Message = "Course not found"
                });
            }

            // Parse full name into First Name and Last Name
            var (firstName, lastName) = ParseFullName(request.FullName);

            // Check if student exists by StudentNo
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == request.StudentId);

            bool isNewStudent = false;

            if (student == null)
            {
                // Create new student
                student = new Student
                {
                    StudentNo = request.StudentId,
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = "",
                    Email = $"{request.StudentId}@student.edu",
                    Section = course.Section,
                    MobileNo = "",
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Students.AddAsync(student);
                await _context.SaveChangesAsync();
                isNewStudent = true;
            }
            else
            {
                // Check if name matches existing student
                bool nameMatches = student.FirstName.Equals(firstName, StringComparison.OrdinalIgnoreCase) &&
                                   student.LastName.Equals(lastName, StringComparison.OrdinalIgnoreCase);

                if (!nameMatches)
                {
                    return BadRequest(new SelfEnrollResponse
                    {
                        Success = false,
                        Message = $"Student ID {request.StudentId} belongs to a different name. Please check your ID.",
                        IsNewStudent = false,
                        StudentNo = student.StudentNo
                    });
                }
            }

            // Check if already enrolled in this course
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == student.Id && e.CourseId == request.CourseId);

            if (existingEnrollment != null)
            {
                return Ok(new SelfEnrollResponse
                {
                    Success = true,
                    Message = $"You are already enrolled in {course.CourseName}",
                    IsNewStudent = isNewStudent,
                    StudentNo = student.StudentNo
                });
            }

            // Create enrollment
            var enrollment = new Enrollment
            {
                StudentId = student.Id,
                CourseId = request.CourseId,
                EnrolledAt = DateTime.UtcNow
            };
            await _context.Enrollments.AddAsync(enrollment);
            await _context.SaveChangesAsync();

            return Ok(new SelfEnrollResponse
            {
                Success = true,
                Message = $"Successfully enrolled in {course.CourseName}",
                IsNewStudent = isNewStudent,
                StudentNo = student.StudentNo
            });
        }

        /// <summary>
        /// Check if student is enrolled in a course
        /// </summary>
        [HttpGet("check-enrollment")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEnrollment(string studentId, int courseId)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.StudentNo == studentId);

            if (student == null)
            {
                return Ok(new { isEnrolled = false });
            }

            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == student.Id && e.CourseId == courseId);

            return Ok(new { isEnrolled });
        }

        private (string FirstName, string LastName) ParseFullName(string fullName)
        {
            var trimmed = fullName.Trim();
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
            {
                return ("", "");
            }

            if (parts.Length == 1)
            {
                return (parts[0], "");
            }

            return (parts[0], string.Join(" ", parts.Skip(1)));
        }
    }

    public class SelfEnrollRequest
    {
        public string StudentId { get; set; } = "";
        public string FullName { get; set; } = "";
        public int CourseId { get; set; }
    }

    public class SelfEnrollResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public bool IsNewStudent { get; set; }
        public string StudentNo { get; set; } = "";
    }
}