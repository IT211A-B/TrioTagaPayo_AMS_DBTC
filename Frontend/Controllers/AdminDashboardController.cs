using Frontend.Helpers;
using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<AdminDashboardController> _logger;

        public AdminDashboardController(IHttpClientFactory http,
            ILogger<AdminDashboardController> logger)
        {
            _http = http;
            _logger = logger;
        }

        private ApiClient Api() =>
            new(_http, HttpContext.Session.GetString("JwtToken"));

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private void SetViewBagUser()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Administrator";
            ViewBag.Role = "Admin";
        }

        // ── PAGES ──────────────────────────────────────────────

        public async Task<IActionResult> Attendance()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();

            var courseResult = await Api().GetAsync<PagedResult<CourseDto>>(
                "api/course?pageSize=100") ?? new();
            var studentResult = await Api().GetAsync<PagedResult<StudentDto>>(
                "api/student?pageSize=100") ?? new();
            var teacherResult = await Api().GetAsync<PagedResult<TeacherDto>>(
                "api/teacher?pageSize=100") ?? new();

            ViewBag.Courses = courseResult.Data;
            ViewBag.Students = studentResult.Data;
            ViewBag.TotalStudents = studentResult.TotalCount;
            ViewBag.ActiveCourses = courseResult.TotalCount;
            ViewBag.TotalTeachers = teacherResult.TotalCount;
            return View();
        }

        public async Task<IActionResult> Courses()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();

            var courseResult = await Api().GetAsync<PagedResult<CourseDto>>(
                "api/course?pageSize=100") ?? new();
            var teacherResult = await Api().GetAsync<PagedResult<TeacherDto>>(
                "api/teacher?pageSize=100") ?? new();

            ViewBag.Courses = courseResult.Data;
            ViewBag.Teachers = teacherResult.Data;
            return View();
        }

        public async Task<IActionResult> Students()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();

            var studentResult = await Api().GetAsync<PagedResult<StudentDto>>(
                "api/student?pageSize=100") ?? new();

            ViewBag.Students = studentResult.Data;
            return View();
        }

        public async Task<IActionResult> Teachers()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();

            var teacherResult = await Api().GetAsync<PagedResult<TeacherDto>>(
                "api/teacher?pageSize=100") ?? new();

            ViewBag.Teachers = teacherResult.Data;
            return View();
        }

        // ── STUDENT ACTIONS ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(string studentNo, string firstName,
            string middleName, string lastName, string email, string section, string mobileNo)
        {
            var result = await Api().PostAsync<StudentDto>("api/student", new
            { studentNo, firstName, middleName, lastName, email, section, mobileNo });

            TempData[result != null ? "Success" : "Error"] = result != null
                ? $"Student {lastName}, {firstName} added!"
                : "Failed to add student.";
            return RedirectToAction("Students");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, string studentNo,
            string firstName, string middleName, string lastName,
            string email, string section, string mobileNo)
        {
            await Api().PutAsync<StudentDto>($"api/student/{id}", new
            { studentNo, firstName, middleName, lastName, email, section, mobileNo });

            TempData["Success"] = "Student updated!";
            return RedirectToAction("Students");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            await Api().DeleteAsync($"api/student/{id}");
            TempData["Success"] = "Student deleted.";
            return RedirectToAction("Students");
        }

        // ── TEACHER ACTIONS ────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(string teacherNo, string firstName,
            string lastName, string email, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Username and password are required.";
                return RedirectToAction("Teachers");
            }

            var result = await Api().PostAsync<TeacherDto>("api/teacher/with-account", new
            { teacherNo, firstName, lastName, email, username, password });

            TempData[result != null ? "Success" : "Error"] = result != null
                ? $"Teacher {lastName}, {firstName} added with login account!"
                : "Failed to add teacher. Username may already exist.";
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, string teacherNo,
            string firstName, string lastName, string email)
        {
            await Api().PutAsync<TeacherDto>($"api/teacher/{id}", new
            { teacherNo, firstName, lastName, email });

            TempData["Success"] = "Teacher info updated!";
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeacherAccount(int id,
            string? newUsername, string? newPassword)
        {
            if (string.IsNullOrWhiteSpace(newUsername) && string.IsNullOrWhiteSpace(newPassword))
            {
                TempData["Error"] = "Provide at least a new username or new password.";
                return RedirectToAction("Teachers");
            }

            var result = await Api().PutAsync<TeacherDto>($"api/teacher/{id}/account", new
            { newUsername, newPassword });

            TempData[result != null ? "Success" : "Error"] = result != null
                ? "Teacher account updated!"
                : "Failed to update account. Username may already exist.";
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeacher(int id)
        {
            await Api().PatchAsync<TeacherDto>($"api/teacher/{id}/toggle-status");
            TempData["Success"] = "Teacher status updated.";
            return RedirectToAction("Teachers");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            await Api().DeleteAsync($"api/teacher/{id}");
            TempData["Success"] = "Teacher and their account deleted.";
            return RedirectToAction("Teachers");
        }

        // ── COURSE ACTIONS ─────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(string courseCode, string courseName,
            int units, string section, string schedule, int teacherId)
        {
            var result = await Api().PostAsync<CourseDto>("api/course", new
            { courseCode, courseName, units, section, schedule, teacherId });

            TempData[result != null ? "Success" : "Error"] = result != null
                ? "Course added!" : "Failed to add course.";
            return RedirectToAction("Courses");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, string courseCode,
            string courseName, int units, string section, string schedule, int teacherId)
        {
            await Api().PutAsync<CourseDto>($"api/course/{id}", new
            { courseCode, courseName, units, section, schedule, teacherId });

            TempData["Success"] = "Course updated!";
            return RedirectToAction("Courses");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            await Api().DeleteAsync($"api/course/{id}");
            TempData["Success"] = "Course deleted.";
            return RedirectToAction("Courses");
        }

        // ── ATTENDANCE (AJAX) ──────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> SaveAttendance(
            [FromBody] List<AttendanceSaveItem> records)
        {
            if (records == null || records.Count == 0)
                return BadRequest("No records.");

            var dtos = records.Select(r => new
            {
                studentId = r.StudentId,
                courseId = r.CourseId,
                date = r.Date,
                status = r.Status,
                remarks = r.Remarks
            }).ToList();

            var result = await Api().PostAsync<object>("api/attendance/bulk", dtos);
            return result != null
                ? Ok(new { message = "Attendance saved!" })
                : StatusCode(500, "Failed to save.");
        }
    }

    // ── DTOs ───────────────────────────────────────────────────

    public class AttendanceSaveItem
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public string Date { get; set; } = "";
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = "";
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Section { get; set; } = "";
        public string MobileNo { get; set; } = "";
    }

    public class TeacherDto
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
        public string Username { get; set; } = "";
        public bool HasAccount { get; set; }
    }

    public class CourseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Units { get; set; }
        public string Section { get; set; } = "";
        public string Schedule { get; set; } = "";
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";
        public int StudentCount { get; set; }
    }

    public class AttendanceDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Status { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}