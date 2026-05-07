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

            if (!IsTeacher())
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Teacher Dashboard";

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

            // Use TeacherId if available
            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
            {
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            }
            else
            {
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();
            }

            var myStudents = students.Where(s => myCourses.Any(c => c.Section == s.Section)).ToList();
            var myAttendance = attendance.Where(a => myCourses.Any(c => c.CourseName == a.CourseName)).ToList();

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var todayAttendance = myAttendance.Where(a => a.Date == today).ToList();
            int todayRate = todayAttendance.Count == 0 ? 0
                : (int)Math.Round(todayAttendance.Count(a => a.Status == "Present") * 100.0 / todayAttendance.Count);

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
        // MY COURSES (Teacher's assigned courses)
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyCourses()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            ViewData["ActivePage"] = "MyCourses";
            ViewData["PageTitle"] = "My Courses";

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");

            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
            {
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            }
            else
            {
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();
            }

            var courseVMs = myCourses.Select(c => new CourseViewModel
            {
                DbId = c.Id,
                CourseCode = c.CourseCode,
                CourseName = c.CourseName,
                Section = c.Section,
                Schedule = c.Schedule
            }).ToList();

            return View(courseVMs);
        }

        // ─────────────────────────────────────────────────────
        // GENERATE QR CODE FOR COURSE ENROLLMENT - FIXED
        // ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCourseQRCode(int courseId)
        {
            if (!IsLoggedIn() || !IsTeacher()) return Unauthorized();

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var course = courses.FirstOrDefault(c => c.Id == courseId);

            if (course == null)
            {
                return Json(new { success = false, message = "Course not found" });
            }

            // FIX: Send courseId as JSON body (not query string)
            var requestBody = new { courseId = courseId };
            var result = await _api.PostAsync<object>("/api/QR/generate-enrollment", requestBody);

            return result.Success && result.Data != null
                ? Json(new { success = true, qrCode = result.Data, courseName = course.CourseName })
                : Json(new { success = false, message = result.Error ?? "Failed to generate QR code" });
        }

        // ─────────────────────────────────────────────────────
        // MY STUDENTS
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyStudents(string? search)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            ViewData["ActivePage"] = "MyStudents";
            ViewData["PageTitle"] = "My Students";

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");

            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
            {
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            }
            else
            {
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();
            }

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
        // MY ATTENDANCE
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyAttendance(int? courseId, string? from, string? to)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            ViewData["ActivePage"] = "MyAttendance";
            ViewData["PageTitle"] = "Class Attendance";

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");

            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
            {
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            }
            else
            {
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();
            }

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
        // INDEX - Redirect to Dashboard
        // ─────────────────────────────────────────────────────
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            return RedirectToAction("Dashboard");
        }
    }
}