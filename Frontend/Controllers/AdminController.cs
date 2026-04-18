using ASM.Services;
using ASM.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ASM.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApiService _api;
        private const int PageSize = 10;

        private static readonly string[] AvatarColors =
        {
            "linear-gradient(135deg,#1A56C4,#3B78E7)",
            "linear-gradient(135deg,#7C3AED,#A78BFA)",
            "linear-gradient(135deg,#059669,#34D399)",
            "linear-gradient(135deg,#DC2626,#F87171)",
            "linear-gradient(135deg,#D97706,#FCD34D)",
            "linear-gradient(135deg,#0891B2,#67E8F9)",
            "linear-gradient(135deg,#BE185D,#F9A8D4)",
        };

        public AdminController(ApiService api)
        {
            _api = api;
        }

        // ════════════════════════════════════════════════════
        // DASHBOARD
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Dashboard()
        {
            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Dashboard";

            var students = await _api.GetAsync<List<StudentApiModel>>("/api/Student") ?? new();
            var teachers = await _api.GetAsync<List<TeacherApiModel>>("/api/Teacher") ?? new();
            var courses = await _api.GetAsync<List<CourseApiModel>>("/api/Course") ?? new();
            var attendance = await _api.GetAsync<List<AttendanceApiModel>>("/api/Attendance") ?? new();

            var recent = attendance
                .OrderByDescending(a => a.Date + a.Time)
                .Take(10)
                .Select(a => new AttendanceEntryViewModel
                {
                    StudentName = a.StudentName,
                    StudentId = a.StudentId,
                    Course = a.CourseCode,
                    Time = a.Time,
                    Status = a.Status
                }).ToList();

            int totalRecords = attendance.Count;
            int presentRecords = attendance.Count(a => a.Status == "Present");
            int attRate = totalRecords > 0
                ? (int)Math.Round(presentRecords / (double)totalRecords * 100)
                : 0;

            var model = new DashboardViewModel
            {
                TotalStudents = students.Count,
                TotalTeachers = teachers.Count,
                TotalCourses = courses.Count,
                AttendanceRate = attRate,
                RecentAttendance = recent
            };

            return View(model);
        }

        // ════════════════════════════════════════════════════
        // STUDENTS
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Students(string? search, string? filter, int page = 1)
        {
            ViewData["ActivePage"] = "Students";
            ViewData["PageTitle"] = "Students";

            var all = await _api.GetAsync<List<StudentApiModel>>("/api/Student") ?? new();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s =>
                    $"{s.FirstName} {s.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)
                    || s.StudentId.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
                all = all.Where(s => s.Status == filter).ToList();

            int total = all.Count;
            int totalPages = (int)Math.Ceiling(total / (double)PageSize);

            var paged = all
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(MapStudent)
                .ToList();

            var model = new StudentsPageViewModel
            {
                Students = paged,
                TotalCount = total,
                ActiveCount = all.Count(s => s.Status == "Active"),
                InactiveCount = all.Count(s => s.Status != "Active"),
                Search = search,
                StatusFilter = filter,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = PageSize
            };

            return View(model);
        }

        public async Task<IActionResult> StudentsPartial(string? search, string? filter, int page = 1)
        {
            var all = await _api.GetAsync<List<StudentApiModel>>("/api/Student") ?? new();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(s =>
                    $"{s.FirstName} {s.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)
                    || s.StudentId.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
                all = all.Where(s => s.Status == filter).ToList();

            var paged = all
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(MapStudent)
                .ToList();

            return PartialView("_StudentsRows", paged);
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(
            string firstName, string lastName, string email,
            string studentId, string section, string status)
        {
            var ok = await _api.PostAsync<object>("/api/Student", new
            {
                firstName,
                lastName,
                email,
                studentId,
                section,
                status
            });

            TempData[ok != null ? "Success" : "Error"] = ok != null
                ? $"{firstName} {lastName} added successfully!"
                : "Failed to add student. Please try again.";

            return RedirectToAction("Students");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStudent(
            int id, string firstName, string lastName,
            string email, string section, string status)
        {
            var ok = await _api.PutAsync($"/api/Student/{id}", new
            {
                firstName,
                lastName,
                email,
                section,
                status
            });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Student updated successfully!"
                : "Failed to update student.";

            return RedirectToAction("Students");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var ok = await _api.DeleteAsync($"/api/Student/{id}");

            TempData[ok ? "Success" : "Error"] = ok
                ? "Student deleted successfully."
                : "Failed to delete student.";

            return RedirectToAction("Students");
        }

        // ════════════════════════════════════════════════════
        // TEACHERS
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Teachers(
            string? search, string? dept, string? filter, int page = 1)
        {
            ViewData["ActivePage"] = "Teachers";
            ViewData["PageTitle"] = "Teachers";

            var all = await _api.GetAsync<List<TeacherApiModel>>("/api/Teacher") ?? new();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    $"{t.FirstName} {t.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)
                    || t.TeacherId.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(dept))
                all = all.Where(t => t.Department == dept).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
                all = all.Where(t => t.Status == filter).ToList();

            int total = all.Count;
            int totalPages = (int)Math.Ceiling(total / (double)PageSize);

            var paged = all
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(MapTeacher)
                .ToList();

            var model = new TeachersPageViewModel
            {
                Teachers = paged,
                TotalCount = total,
                ActiveCount = all.Count(t => t.IsActive),
                InactiveCount = all.Count(t => !t.IsActive),
                Search = search,
                DeptFilter = dept,
                StatusFilter = filter,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = PageSize
            };

            return View(model);
        }

        public async Task<IActionResult> TeachersPartial(
            string? search, string? dept, string? filter, int page = 1)
        {
            var all = await _api.GetAsync<List<TeacherApiModel>>("/api/Teacher") ?? new();

            if (!string.IsNullOrWhiteSpace(search))
                all = all.Where(t =>
                    $"{t.FirstName} {t.LastName}".Contains(search, StringComparison.OrdinalIgnoreCase)
                    || t.TeacherId.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(dept))
                all = all.Where(t => t.Department == dept).ToList();

            if (!string.IsNullOrWhiteSpace(filter))
                all = all.Where(t => t.Status == filter).ToList();

            var paged = all
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(MapTeacher)
                .ToList();

            return PartialView("_TeachersRows", paged);
        }

        [HttpPost]
        public async Task<IActionResult> AddTeacher(
            string firstName, string lastName, string email,
            string department, string contactNumber, string status)
        {
            var ok = await _api.PostAsync<object>("/api/Teacher/with-account", new
            {
                firstName,
                lastName,
                email,
                department,
                contactNumber,
                status
            });

            TempData[ok != null ? "Success" : "Error"] = ok != null
                ? $"{firstName} {lastName} added successfully!"
                : "Failed to add teacher.";

            return RedirectToAction("Teachers");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTeacher(
            int id, string firstName, string lastName,
            string email, string department, string contactNumber)
        {
            var ok = await _api.PutAsync($"/api/Teacher/{id}", new
            {
                firstName,
                lastName,
                email,
                department,
                contactNumber
            });

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher updated successfully!"
                : "Failed to update teacher.";

            return RedirectToAction("Teachers");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var ok = await _api.DeleteAsync($"/api/Teacher/{id}");

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher deleted successfully."
                : "Failed to delete teacher.";

            return RedirectToAction("Teachers");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleTeacherStatus(int id)
        {
            var ok = await _api.PatchAsync($"/api/Teacher/{id}/toggle-status");

            TempData[ok ? "Success" : "Error"] = ok
                ? "Teacher status updated."
                : "Failed to update status.";

            return RedirectToAction("Teachers");
        }

        // ════════════════════════════════════════════════════
        // COURSES
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Courses()
        {
            ViewData["ActivePage"] = "Courses";
            ViewData["PageTitle"] = "Courses";

            var courses = await _api.GetAsync<List<CourseApiModel>>("/api/Course") ?? new();
            return View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(
            string courseCode, string courseName, string description, int units)
        {
            var ok = await _api.PostAsync<object>("/api/Course", new
            {
                courseCode,
                courseName,
                description,
                units
            });

            TempData[ok != null ? "Success" : "Error"] = ok != null
                ? $"{courseName} added successfully!"
                : "Failed to add course.";

            return RedirectToAction("Courses");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var ok = await _api.DeleteAsync($"/api/Course/{id}");

            TempData[ok ? "Success" : "Error"] = ok
                ? "Course deleted."
                : "Failed to delete course.";

            return RedirectToAction("Courses");
        }

        // ════════════════════════════════════════════════════
        // ATTENDANCE
        // ════════════════════════════════════════════════════
        public async Task<IActionResult> Attendance(string? courseId)
        {
            ViewData["ActivePage"] = "Attendance";
            ViewData["PageTitle"] = "Attendance";

            var records = string.IsNullOrWhiteSpace(courseId)
                ? await _api.GetAsync<List<AttendanceApiModel>>("/api/Attendance") ?? new()
                : await _api.GetAsync<List<AttendanceApiModel>>($"/api/Attendance/course/{courseId}") ?? new();

            return View(records);
        }

        // ════════════════════════════════════════════════════
        // MAPPERS — ApiModel → ViewModel
        // ════════════════════════════════════════════════════
        private static StudentViewModel MapStudent(StudentApiModel s, int i) => new()
        {
            StudentId = s.StudentId,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            YearLevel = "",           // not in API model — set if available
            Section = s.Section,
            Status = s.Status,
            AttendanceRate = "0%",
            AvatarColor = AvatarColors[i % AvatarColors.Length]
        };

        private static TeacherViewModel MapTeacher(TeacherApiModel t, int i) => new()
        {
            TeacherId = t.TeacherId,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Email = t.Email,
            Department = t.Department,
            ContactNumber = t.ContactNumber,
            Status = t.IsActive ? "Active" : "Inactive",
            AvatarColor = AvatarColors[i % AvatarColors.Length]
        };
    }
}