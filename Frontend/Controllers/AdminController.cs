using Microsoft.AspNetCore.Mvc;
using AMS.Models;
using AMS.Services;
using AMS.ViewModels;

namespace AMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _api;

        private static readonly string[] AvatarColors =
        {
            "#6366f1", "#8b5cf6", "#ec4899", "#f59e0b",
            "#10b981", "#3b82f6", "#ef4444", "#14b8a6"
        };

        public AdminController(ApiService api) => _api = api;

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private IActionResult RequireLogin() =>
            RedirectToAction("Login", "Account");

        private IActionResult AjaxOk(string message) =>
            Json(new { success = true, message });

        private IActionResult AjaxFail(string message) =>
            Json(new { success = false, message });

        // ─────────────────────────────────────────────────────
        // DASHBOARD
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Dashboard";

            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

            var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            var recent = attendance
                .Where(a => DateOnly.TryParse(a.Date, out var d) && d >= cutoff)
                .ToList();

            int rate = recent.Count == 0 ? 0
                : (int)Math.Round(recent.Count(a => a.Status == "Present") * 100.0 / recent.Count);

            var recentRows = attendance
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new AttendanceEntryViewModel
                {
                    StudentName = a.StudentName,
                    StudentNo = a.StudentNo,
                    CourseName = a.CourseName,
                    Date = a.Date,
                    Status = a.Status,
                    Remarks = a.Remarks
                }).ToList();

            return View(new DashboardViewModel
            {
                TotalStudents = students.Count,
                TotalTeachers = teachers.Count,
                TotalCourses = courses.Count,
                AttendanceRate = rate,
                RecentAttendance = recentRows
            });
        }

        // ─────────────────────────────────────────────────────
        // STUDENTS
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Students(string? search, string? section)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewData["ActivePage"] = "Students";
            ViewData["PageTitle"] = "Students";

            var students = await BuildStudentVMs(search, section);

            return View(new StudentsPageViewModel
            {
                Students = students,
                TotalCount = students.Count,
                Search = search,
                SectionFilter = section
            });
        }

        [HttpGet]
        public async Task<IActionResult> StudentsPartial(
            string? search, string? section, int page = 1, int pageSize = 20)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var all = await BuildStudentVMs(search, section);
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return PartialView("_StudentTableRows", paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(
            string studentNo, string firstName, string middleName,
            string lastName, string email, string section, string mobileNo)
        {
            if (!IsLoggedIn()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(studentNo) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
                return AjaxFail("Please fill in all required fields.");

            var body = new { studentNo, firstName, middleName, lastName, email, section, mobileNo };
            var (ok, _, err) = await _api.PostAsync<StudentApiModel>("/api/Student", body);
            return ok ? AjaxOk("Student added successfully.")
                      : AjaxFail($"Failed to add student: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(
            int id, string studentNo, string firstName, string middleName,
            string lastName, string email, string section, string mobileNo)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var body = new { studentNo, firstName, middleName, lastName, email, section, mobileNo };
            var (ok, err) = await _api.PutAsync($"/api/Student/{id}", body);
            return ok ? AjaxOk("Student updated successfully.")
                      : AjaxFail($"Failed to update student: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var (ok, err) = await _api.DeleteAsync($"/api/Student/{id}");
            return ok ? AjaxOk("Student deleted successfully.")
                      : AjaxFail($"Failed to delete student: {ParseError(err)}");
        }

        // ─────────────────────────────────────────────────────
        // TEACHERS
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Teachers(string? search, string? status)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewData["ActivePage"] = "Teachers";
            ViewData["PageTitle"] = "Teachers";

            var teachers = await BuildTeacherVMs(search, status);

            return View(new TeachersPageViewModel
            {
                Teachers = teachers,
                TotalCount = teachers.Count,
                ActiveCount = teachers.Count(t => t.IsActive),
                InactiveCount = teachers.Count(t => !t.IsActive),
                Search = search,
                StatusFilter = status
            });
        }

        [HttpGet]
        public async Task<IActionResult> TeachersPartial(
            string? search, string? status, int page = 1, int pageSize = 20)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var all = await BuildTeacherVMs(search, status);
            var paged = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return PartialView("_TeacherTableRows", paged);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(
            string teacherNo, string firstName, string lastName,
            string email, string? username, string? password)
        {
            if (!IsLoggedIn()) return Unauthorized();

            if (string.IsNullOrWhiteSpace(teacherNo) || string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
                return AjaxFail("Please fill in all required fields.");

            bool withAccount = !string.IsNullOrWhiteSpace(username) &&
                               !string.IsNullOrWhiteSpace(password);
            bool ok; string err;

            if (withAccount)
            {
                var body = new { teacherNo, firstName, lastName, email, username, password };
                (ok, _, err) = await _api.PostAsync<TeacherApiModel>("/api/Teacher/with-account", body);
            }
            else
            {
                var body = new { teacherNo, firstName, lastName, email };
                (ok, _, err) = await _api.PostAsync<TeacherApiModel>("/api/Teacher", body);
            }

            return ok ? AjaxOk("Teacher added successfully.")
                      : AjaxFail($"Failed to add teacher: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeacher(
            int id, string teacherNo, string firstName, string lastName, string email)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var body = new { teacherNo, firstName, lastName, email };
            var (ok, err) = await _api.PutAsync($"/api/Teacher/{id}", body);
            return ok ? AjaxOk("Teacher updated successfully.")
                      : AjaxFail($"Failed to update teacher: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var (ok, err) = await _api.DeleteAsync($"/api/Teacher/{id}");
            return ok ? AjaxOk("Teacher deleted successfully.")
                      : AjaxFail($"Failed to delete teacher: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeacherStatus(int id)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var (ok, err) = await _api.PatchAsync($"/api/Teacher/{id}/toggle-status");
            return ok ? AjaxOk("Teacher status updated.")
                      : AjaxFail($"Failed to toggle status: {ParseError(err)}");
        }

        // ─────────────────────────────────────────────────────
        // COURSES
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Courses(string? search)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewData["ActivePage"] = "Courses";
            ViewData["PageTitle"] = "Courses";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

            if (!string.IsNullOrWhiteSpace(search))
                courses = courses.Where(c =>
                    c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.CourseCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Section.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            var courseVMs = courses.Select(c => new CourseViewModel
            {
                DbId = c.Id,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                Units = c.Units,
                Section = c.Section,
                Schedule = c.Schedule,
                TeacherId = c.TeacherId,
                TeacherName = c.TeacherName
            }).ToList();

            var teacherVMs = teachers.Select((t, i) => new TeacherViewModel
            {
                DbId = t.Id,
                TeacherNo = t.TeacherNo,
                FirstName = t.FirstName,
                LastName = t.LastName,
                IsActive = t.IsActive,
                AvatarColor = AvatarColors[i % AvatarColors.Length]
            }).ToList();

            return View(new CoursesPageViewModel
            {
                Courses = courseVMs,
                Teachers = teacherVMs,
                Search = search
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(
            string courseCode, string courseName, int units,
            string section, string schedule, int teacherId)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var body = new { courseCode, courseName, units, section, schedule, teacherId };
            var (ok, _, err) = await _api.PostAsync<CourseApiModel>("/api/Course", body);
            return ok ? AjaxOk("Course added successfully.")
                      : AjaxFail($"Failed to add course: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(
            int id, string courseCode, string courseName, int units,
            string section, string schedule, int teacherId)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var body = new { courseCode, courseName, units, section, schedule, teacherId };
            var (ok, err) = await _api.PutAsync($"/api/Course/{id}", body);
            return ok ? AjaxOk("Course updated successfully.")
                      : AjaxFail($"Failed to update course: {ParseError(err)}");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var (ok, err) = await _api.DeleteAsync($"/api/Course/{id}");
            return ok ? AjaxOk("Course deleted successfully.")
                      : AjaxFail($"Failed to delete course: {ParseError(err)}");
        }

        // ─────────────────────────────────────────────────────
        // ATTENDANCE
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Attendance(int? courseId, string? from, string? to)
        {
            if (!IsLoggedIn()) return RequireLogin();
            ViewData["ActivePage"] = "Attendance";
            ViewData["PageTitle"] = "Attendance";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var attendance = await FetchAttendance(courseId, from, to);

            var courseVMs = courses.Select(c => new CourseViewModel
            {
                DbId = c.Id,
                CourseName = c.CourseName,
                Section = c.Section
            }).ToList();

            return View(new AttendancePageViewModel
            {
                Records = MapAttendanceRows(attendance),
                Courses = courseVMs,
                SelectedCourseId = courseId,
                FromDate = from,
                ToDate = to
            });
        }

        // Partial for AJAX filter
        [HttpGet]
        public async Task<IActionResult> AttendanceFilter(
            int? courseId, string? from, string? to)
        {
            if (!IsLoggedIn()) return Unauthorized();
            var attendance = await FetchAttendance(courseId, from, to);
            return PartialView("_AttendanceTableRows", MapAttendanceRows(attendance));
        }

        // ─────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────
        private async Task<List<StudentViewModel>> BuildStudentVMs(
            string? search, string? section)
        {
            var all = await _api.GetAllAsync<StudentApiModel>("/api/Student");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s =>
                    s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(section))
                all = all.Where(s => s.Section == section).ToList();

            return all.Select((s, i) => new StudentViewModel
            {
                DbId = s.Id,
                StudentNo = s.StudentNo,
                FirstName = s.FirstName,
                MiddleName = s.MiddleName,
                LastName = s.LastName,
                Email = s.Email,
                Section = s.Section,
                MobileNo = s.MobileNo,
                AvatarColor = AvatarColors[i % AvatarColors.Length]
            }).ToList();
        }

        private async Task<List<TeacherViewModel>> BuildTeacherVMs(
            string? search, string? status)
        {
            var all = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    t.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.TeacherNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(status))
            {
                bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
                all = all.Where(t => t.IsActive == isActive).ToList();
            }

            return all.Select((t, i) => new TeacherViewModel
            {
                DbId = t.Id,
                TeacherNo = t.TeacherNo,
                FirstName = t.FirstName,
                LastName = t.LastName,
                Email = t.Email,
                IsActive = t.IsActive,
                CourseCount = t.CourseCount,
                Username = t.Username,
                HasAccount = t.HasAccount,
                AvatarColor = AvatarColors[i % AvatarColors.Length]
            }).ToList();
        }

        private async Task<List<AttendanceApiModel>> FetchAttendance(
            int? courseId, string? from, string? to)
        {
            if (courseId.HasValue &&
                DateOnly.TryParse(from, out var f) &&
                DateOnly.TryParse(to, out var t))
                return await _api.GetAllAsync<AttendanceApiModel>(
                    $"/api/Attendance/filter?courseId={courseId}&from={f:yyyy-MM-dd}&to={t:yyyy-MM-dd}");

            if (courseId.HasValue)
                return await _api.GetAllAsync<AttendanceApiModel>(
                    $"/api/Attendance/course/{courseId}");

            return await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
        }

        private static List<AttendanceEntryViewModel> MapAttendanceRows(
            List<AttendanceApiModel> src) =>
            src.Select(a => new AttendanceEntryViewModel
            {
                StudentName = a.StudentName,
                StudentNo = a.StudentNo,
                CourseName = a.CourseName,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks
            }).ToList();

        private static string ParseError(string rawJson)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? rawJson;
            }
            catch { }
            return rawJson.Length > 200 ? rawJson[..200] : rawJson;
        }
    }
}