// ============================================================
// Controllers/AdminController.cs
// FIXED: API returns { data, page, totalCount } — not a plain List<T>
// ADDED: AJAX endpoints that return JSON instead of redirecting
// ============================================================

using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Models;
using AMS.ViewModels;

namespace AMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _api;
        private const int PageSize = 10;

        private static readonly string[] Colors =
        {
            "linear-gradient(135deg,#1A56C4,#3B78E7)",
            "linear-gradient(135deg,#7C3AED,#A78BFA)",
            "linear-gradient(135deg,#059669,#34D399)",
            "linear-gradient(135deg,#DC2626,#F87171)",
            "linear-gradient(135deg,#D97706,#FCD34D)",
            "linear-gradient(135deg,#0891B2,#67E8F9)",
            "linear-gradient(135deg,#BE185D,#F9A8D4)",
        };

        public AdminController(ApiService api) => _api = api;

        // ── AUTH GUARD helper ─────────────────────────────────
        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private IActionResult RedirectToLogin() =>
            RedirectToAction("Login", "Account");

        // ════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn()) return RedirectToLogin();
            SetPage("Dashboard");

            // FIX: API returns paged wrapper — fetch page=1 pageSize=1000 to get all
            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var attendance = await _api.GetAsync<List<AttendanceApiModel>>("/api/Attendance") ?? new();

            int total = attendance.Count;
            int present = attendance.Count(a => a.Status == "Present");
            int rate = total > 0 ? (int)Math.Round(present / (double)total * 100) : 0;

            var model = new DashboardViewModel
            {
                TotalStudents = students.Count,
                TotalTeachers = teachers.Count,
                TotalCourses = courses.Count,
                AttendanceRate = rate,
                RecentAttendance = attendance
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
                    }).ToList()
            };

            return View(model);
        }

        // ════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Students(string? search, string? section, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToLogin();
            SetPage("Students");

            var all = await _api.GetAllAsync<StudentApiModel>("/api/Student");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s =>
                    $"{s.FirstName} {s.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(section))
                all = all.Where(s => s.Section.Contains(section, StringComparison.OrdinalIgnoreCase)).ToList();

            int totalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
            var paged = all.Skip((page - 1) * PageSize).Take(PageSize).Select(MapStudent).ToList();

            return View(new StudentsPageViewModel
            {
                Students = paged,
                TotalCount = all.Count,
                CurrentPage = page,
                TotalPages = totalPages,
                Search = search,
                SectionFilter = section,
            });
        }

        // Partial for infinite scroll
        public async Task<IActionResult> StudentsPartial(string? search, string? section, int page = 1)
        {
            var all = await _api.GetAllAsync<StudentApiModel>("/api/Student");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s =>
                    $"{s.FirstName} {s.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(section))
                all = all.Where(s => s.Section.Contains(section, StringComparison.OrdinalIgnoreCase)).ToList();

            var paged = all.Skip((page - 1) * PageSize).Take(PageSize).Select(MapStudent).ToList();
            return PartialView("_StudentTableRows", paged);
        }

        // ── AJAX: Add Student — returns JSON ─────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(
            string studentNo, string firstName, string middleName,
            string lastName, string email, string section, string mobileNo)
        {
            var (ok, _, err) = await _api.PostAsync<object>("/api/Student", new
            {
                studentNo,
                firstName,
                middleName,
                lastName,
                email,
                section,
                mobileNo
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = $"{firstName} {lastName} added successfully!" })
                    : Json(new { success = false, message = $"Failed to add student. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? $"{firstName} {lastName} added successfully!"
                : $"Failed to add student. {ParseError(err)}";
            return RedirectToAction("Students");
        }

        // ── AJAX: Update Student — returns JSON ───────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStudent(
            int id, string studentNo, string firstName, string middleName,
            string lastName, string email, string section, string mobileNo)
        {
            var (ok, err) = await _api.PutAsync($"/api/Student/{id}", new
            {
                studentNo,
                firstName,
                middleName,
                lastName,
                email,
                section,
                mobileNo
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Student updated successfully!" })
                    : Json(new { success = false, message = $"Failed to update student. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Student updated successfully!"
                : $"Failed to update student. {ParseError(err)}";
            return RedirectToAction("Students");
        }

        // ── AJAX: Delete Student — returns JSON ───────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var (ok, err) = await _api.DeleteAsync($"/api/Student/{id}");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Student deleted successfully." })
                    : Json(new { success = false, message = $"Failed to delete student. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Student deleted successfully."
                : $"Failed to delete student. {ParseError(err)}";
            return RedirectToAction("Students");
        }

        // ════════════════════════════════════════════════════
        // TEACHERS
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Teachers(string? search, string? status, int page = 1)
        {
            if (!IsLoggedIn()) return RedirectToLogin();
            SetPage("Teachers");

            var all = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    $"{t.FirstName} {t.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.TeacherNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (status == "Active") all = all.Where(t => t.IsActive).ToList();
            if (status == "Inactive") all = all.Where(t => !t.IsActive).ToList();

            int totalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
            var paged = all.Skip((page - 1) * PageSize).Take(PageSize).Select(MapTeacher).ToList();

            return View(new TeachersPageViewModel
            {
                Teachers = paged,
                TotalCount = all.Count,
                ActiveCount = all.Count(t => t.IsActive),
                InactiveCount = all.Count(t => !t.IsActive),
                CurrentPage = page,
                TotalPages = totalPages,
                Search = search,
                StatusFilter = status,
            });
        }

        // Partial for infinite scroll
        public async Task<IActionResult> TeachersPartial(string? search, string? status, int page = 1)
        {
            var all = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    $"{t.FirstName} {t.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    t.TeacherNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (status == "Active") all = all.Where(t => t.IsActive).ToList();
            if (status == "Inactive") all = all.Where(t => !t.IsActive).ToList();

            var paged = all.Skip((page - 1) * PageSize).Take(PageSize).Select(MapTeacher).ToList();
            return PartialView("_TeacherTableRows", paged);
        }

        // ── AJAX: Add Teacher ─────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(
            string teacherNo, string firstName, string lastName,
            string email, string username, string password)
        {
            var (ok, _, err) = await _api.PostAsync<object>("/api/Teacher/with-account", new
            {
                teacherNo,
                firstName,
                lastName,
                email,
                username,
                password
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = $"{firstName} {lastName} added successfully!" })
                    : Json(new { success = false, message = $"Failed to add teacher. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? $"{firstName} {lastName} added successfully!"
                : $"Failed to add teacher. {ParseError(err)}";
            return RedirectToAction("Teachers");
        }

        // ── AJAX: Update Teacher ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateTeacher(
            int id, string teacherNo, string firstName, string lastName, string email)
        {
            var (ok, err) = await _api.PutAsync($"/api/Teacher/{id}", new
            {
                teacherNo,
                firstName,
                lastName,
                email
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Teacher updated successfully!" })
                    : Json(new { success = false, message = $"Failed to update teacher. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher updated successfully!"
                : $"Failed to update teacher. {ParseError(err)}";
            return RedirectToAction("Teachers");
        }

        // ── AJAX: Delete Teacher ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var (ok, err) = await _api.DeleteAsync($"/api/Teacher/{id}");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Teacher deleted successfully." })
                    : Json(new { success = false, message = $"Failed to delete teacher. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher deleted successfully."
                : $"Failed to delete teacher. {ParseError(err)}";
            return RedirectToAction("Teachers");
        }

        // ── AJAX: Toggle Teacher Status ───────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleTeacherStatus(int id)
        {
            var (ok, err) = await _api.PatchAsync($"/api/Teacher/{id}/toggle-status");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Teacher status updated." })
                    : Json(new { success = false, message = $"Failed to update status. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher status updated."
                : $"Failed to update status. {ParseError(err)}";
            return RedirectToAction("Teachers");
        }

        // ════════════════════════════════════════════════════
        // COURSES
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Courses(string? search)
        {
            if (!IsLoggedIn()) return RedirectToLogin();
            SetPage("Courses");

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

            if (!string.IsNullOrWhiteSpace(search))
                courses = courses.Where(c =>
                    c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.CourseCode.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return View(new CoursesPageViewModel
            {
                Courses = courses.Select(MapCourse).ToList(),
                Teachers = teachers.Select(MapTeacher).ToList(),
                Search = search,
            });
        }

        // ── AJAX: Add Course ──────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(
            string courseCode, string courseName, int units,
            string section, string schedule, int teacherId)
        {
            var (ok, _, err) = await _api.PostAsync<object>("/api/Course", new
            {
                courseCode,
                courseName,
                units,
                section,
                schedule,
                teacherId
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = $"{courseName} added successfully!" })
                    : Json(new { success = false, message = $"Failed to add course. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? $"{courseName} added successfully!"
                : $"Failed to add course. {ParseError(err)}";
            return RedirectToAction("Courses");
        }

        // ── AJAX: Update Course ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCourse(
            int id, string courseCode, string courseName, int units,
            string section, string schedule, int teacherId)
        {
            var (ok, err) = await _api.PutAsync($"/api/Course/{id}", new
            {
                courseCode,
                courseName,
                units,
                section,
                schedule,
                teacherId
            });

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Course updated successfully!" })
                    : Json(new { success = false, message = $"Failed to update course. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Course updated successfully!"
                : $"Failed to update course. {ParseError(err)}";
            return RedirectToAction("Courses");
        }

        // ── AJAX: Delete Course ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var (ok, err) = await _api.DeleteAsync($"/api/Course/{id}");

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return ok
                    ? Json(new { success = true, message = "Course deleted." })
                    : Json(new { success = false, message = $"Failed to delete course. {ParseError(err)}" });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Course deleted."
                : $"Failed to delete course. {ParseError(err)}";
            return RedirectToAction("Courses");
        }

        // ════════════════════════════════════════════════════
        // ATTENDANCE
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Attendance(int? courseId, int? studentId)
        {
            if (!IsLoggedIn()) return RedirectToLogin();
            SetPage("Attendance");

            List<AttendanceApiModel> records;

            if (courseId.HasValue)
                records = await _api.GetAsync<List<AttendanceApiModel>>($"/api/Attendance/course/{courseId}") ?? new();
            else if (studentId.HasValue)
                records = await _api.GetAsync<List<AttendanceApiModel>>($"/api/Attendance/student/{studentId}") ?? new();
            else
                records = await _api.GetAsync<List<AttendanceApiModel>>("/api/Attendance") ?? new();

            var entries = records.Select(a => new AttendanceEntryViewModel
            {
                StudentName = a.StudentName,
                StudentNo = a.StudentNo,
                CourseName = a.CourseName,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks
            }).ToList();

            return View(entries);
        }

        // ════════════════════════════════════════════════════
        // MAPPERS
        // ════════════════════════════════════════════════════
        private static StudentViewModel MapStudent(StudentApiModel s, int i) => new()
        {
            DbId = s.Id,
            StudentNo = s.StudentNo,
            FirstName = s.FirstName,
            MiddleName = s.MiddleName,
            LastName = s.LastName,
            Email = s.Email,
            Section = s.Section,
            MobileNo = s.MobileNo,
            AvatarColor = Colors[i % Colors.Length],
        };

        private static TeacherViewModel MapTeacher(TeacherApiModel t, int i) => new()
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
            AvatarColor = Colors[i % Colors.Length],
        };

        private static CourseViewModel MapCourse(CourseApiModel c) => new()
        {
            DbId = c.Id,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            Units = c.Units,
            Section = c.Section,
            Schedule = c.Schedule,
            TeacherId = c.TeacherId,
            TeacherName = c.TeacherName,
        };

        private void SetPage(string page)
        {
            ViewData["ActivePage"] = page;
            ViewData["PageTitle"] = page;
        }

        private static string ParseError(string raw)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(raw);
                if (doc.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? "";
            }
            catch { }
            return "";
        }
    }
}
