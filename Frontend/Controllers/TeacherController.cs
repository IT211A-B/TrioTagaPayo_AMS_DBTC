using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Models;
using AMS.ViewModels;

namespace AMS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApiService _api;

        public TeacherController(ApiService api)
        {
            _api = api;
        }

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Teacher" || role == "teacher";
        }

        private IActionResult RequireLogin() =>
            RedirectToAction("Login", "Account");

        // ─────────────────────────────────────────────────────
        // TEACHER DASHBOARD
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn()) return RequireLogin();

            // If admin tries to access teacher dashboard, redirect to admin dashboard
            if (!IsTeacher())
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Teacher Dashboard";

            // Get teacher-specific data from session
            var teacherId = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            // Get all data from API
            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

            // Filter for this teacher
            var myCourses = string.IsNullOrEmpty(teacherId)
                ? courses.Where(c => c.TeacherName == teacherName).ToList()
                : courses.Where(c => c.TeacherId.ToString() == teacherId || c.TeacherName == teacherName).ToList();

            var myStudents = students.Where(s => myCourses.Any(c => c.Section == s.Section)).ToList();

            var myAttendance = attendance.Where(a => myCourses.Any(c => c.CourseName == a.CourseName)).ToList();

            // Calculate today's attendance rate
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var todayAttendance = myAttendance.Where(a => a.Date == today).ToList();
            int todayRate = todayAttendance.Count == 0 ? 0
                : (int)Math.Round(todayAttendance.Count(a => a.Status == "Present") * 100.0 / todayAttendance.Count);

            // Get recent attendance (last 10 records)
            var recentAttendance = myAttendance
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

            var viewModel = new TeacherDashboardViewModel
            {
                TeacherName = teacherName,
                MyCoursesCount = myCourses.Count,
                MyStudentsCount = myStudents.Count,
                TodayAttendanceRate = todayRate,
                RecentAttendance = recentAttendance,
                MyCourses = myCourses.Select(c => new CourseViewModel
                {
                    DbId = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Section = c.Section,
                    Schedule = c.Schedule
                }).ToList()
            };

            return View(viewModel);
        }

        // ─────────────────────────────────────────────────────
        // MY STUDENTS (Teacher's students)
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyStudents(string? search)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            ViewData["ActivePage"] = "MyStudents";
            ViewData["PageTitle"] = "My Students";

            var teacherId = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var myCourses = string.IsNullOrEmpty(teacherId)
                ? courses.Where(c => c.TeacherName == teacherName).ToList()
                : courses.Where(c => c.TeacherId.ToString() == teacherId || c.TeacherName == teacherName).ToList();

            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var myStudents = students.Where(s => myCourses.Any(c => c.Section == s.Section)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                myStudents = myStudents.Where(s =>
                    s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var studentVMs = myStudents.Select(s => new StudentViewModel
            {
                DbId = s.Id,
                StudentNo = s.StudentNo,
                FirstName = s.FirstName,
                MiddleName = s.MiddleName,
                LastName = s.LastName,
                Email = s.Email,
                Section = s.Section,
                MobileNo = s.MobileNo
            }).ToList();

            return View(studentVMs);
        }

        // ─────────────────────────────────────────────────────
        // MY ATTENDANCE (Teacher's class attendance)
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyAttendance(int? courseId, string? from, string? to)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            ViewData["ActivePage"] = "MyAttendance";
            ViewData["PageTitle"] = "Class Attendance";

            var teacherId = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var myCourses = string.IsNullOrEmpty(teacherId)
                ? courses.Where(c => c.TeacherName == teacherName).ToList()
                : courses.Where(c => c.TeacherId.ToString() == teacherId || c.TeacherName == teacherName).ToList();

            List<AttendanceApiModel> attendance;

            if (courseId.HasValue && courseId.Value > 0)
            {
                attendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{courseId}");
            }
            else
            {
                attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
                attendance = attendance.Where(a => myCourses.Any(c => c.CourseName == a.CourseName)).ToList();
            }

            // Apply date filters
            if (DateOnly.TryParse(from, out var fromDate))
                attendance = attendance.Where(a => DateOnly.TryParse(a.Date, out var d) && d >= fromDate).ToList();
            if (DateOnly.TryParse(to, out var toDate))
                attendance = attendance.Where(a => DateOnly.TryParse(a.Date, out var d) && d <= toDate).ToList();

            var courseVMs = myCourses.Select(c => new CourseViewModel
            {
                DbId = c.Id,
                CourseName = c.CourseName,
                Section = c.Section
            }).ToList();

            var attendanceRows = attendance.Select(a => new AttendanceEntryViewModel
            {
                StudentName = a.StudentName,
                StudentNo = a.StudentNo,
                CourseName = a.CourseName,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks
            }).ToList();

            var viewModel = new TeacherAttendanceViewModel
            {
                Records = attendanceRows,
                Courses = courseVMs,
                SelectedCourseId = courseId,
                FromDate = from,
                ToDate = to
            };

            return View(viewModel);
        }

        // ─────────────────────────────────────────────────────
        // Index (Redirect to Dashboard)
        // ─────────────────────────────────────────────────────
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            return RedirectToAction("Dashboard");
        }
    }
}