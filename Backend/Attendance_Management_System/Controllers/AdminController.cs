using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ICourseService _courseService;
        private readonly IAttendanceService _attendanceService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;

        public AdminController(
            IStudentService studentService,
            ITeacherService teacherService,
            ICourseService courseService,
            IAttendanceService attendanceService,
            IUserRepository userRepository,
            IPasswordHasher hasher)
        {
            _studentService = studentService;
            _teacherService = teacherService;
            _courseService = courseService;
            _attendanceService = attendanceService;
            _userRepository = userRepository;
            _hasher = hasher;
        }

        /// <summary>
        /// Gets dashboard statistics.
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Dashboard()
        {
            var students = await _studentService.GetAllAsync();
            var teachers = await _teacherService.GetAllAsync();
            var courses = await _courseService.GetAllAsync();
            var attendance = await _attendanceService.GetAllAsync();

            var recent = attendance.OrderByDescending(a => a.CreatedAt).Take(10);
            var totalPresent = attendance.Count(a => a.Status == "Present");

            return Ok(new
            {
                totalStudents = students.Count(),
                totalTeachers = teachers.Count(),
                totalCourses = courses.Count(),
                attendanceRate = attendance.Any() ? (totalPresent * 100 / attendance.Count()) : 0,
                recentAttendance = recent.Select(a => new
                {
                    a.StudentName,
                    a.CourseName,
                    a.Date,
                    a.Status
                })
            });
        }

        /// <summary>
        /// Admin resets a user's password (forgot password scenario).
        /// </summary>
        [HttpPost("reset-user-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetUserPassword([FromBody] AdminResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { success = false, message = "Username and new password are required" });

            var user = await _userRepository.FindAsync(u => u.Username == dto.Username);
            if (user == null)
                return NotFound(new { success = false, message = "User not found" });

            user.PasswordHash = _hasher.Hash(dto.NewPassword);
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return Ok(new { success = true, message = $"Password for {dto.Username} has been reset successfully." });
        }
    }
}