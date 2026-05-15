using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Models;
using AMS.ViewModels;
using QRCoder;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace AMS.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApiService _api;

        public TeacherController(ApiService api)
        {
            _api = api;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));
        private bool IsTeacher()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Teacher" || role == "teacher";
        }
        private IActionResult RequireLogin() => RedirectToAction("Login", "Account");

        private async Task RefreshTeacherInfoIfNeeded()
        {
            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var currentName = HttpContext.Session.GetString("TeacherName");
            if ((string.IsNullOrEmpty(currentName) || currentName.StartsWith("TCH-")) && !string.IsNullOrEmpty(teacherIdStr))
            {
                if (int.TryParse(teacherIdStr, out int tid))
                {
                    var teacher = await _api.GetAsync<TeacherApiModel>($"/api/Teacher/{tid}");
                    if (teacher != null)
                    {
                        var fullName = $"{teacher.FirstName} {teacher.LastName}".Trim();
                        HttpContext.Session.SetString("TeacherName", fullName);
                        HttpContext.Session.SetString("Username", fullName);
                    }
                }
            }
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            await RefreshTeacherInfoIfNeeded();

            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Teacher Dashboard";

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ??
                              HttpContext.Session.GetString("Username") ?? "Teacher";

            int teacherId = 0;
            if (!string.IsNullOrEmpty(teacherIdStr))
                int.TryParse(teacherIdStr, out teacherId);

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

            List<CourseApiModel> myCourses = new List<CourseApiModel>();
            if (teacherId > 0)
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            if (myCourses.Count == 0 && !string.IsNullOrEmpty(teacherName))
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();
            if (myCourses.Count == 0)
                Console.WriteLine($"No courses found for teacher ID {teacherId} or name {teacherName}");

            var myStudents = students.Where(s => myCourses.Any(c => c.Section == s.Section)).ToList();
            var myAttendance = attendance.Where(a => myCourses.Any(c => c.CourseName == a.CourseName)).ToList();

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var todayAttendance = myAttendance.Where(a => a.Date == today).ToList();
            int todayRate = todayAttendance.Count == 0 ? 0
                : (int)Math.Round(todayAttendance.Count(a => a.Status == "Present") * 100.0 / todayAttendance.Count);

            var recentAttendance = myAttendance.OrderByDescending(a => a.CreatedAt).Take(10)
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

        public async Task<IActionResult> MyCourses()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");
            await RefreshTeacherInfoIfNeeded();

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ?? HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            else
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();

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

        // Enrollment QR (kept for compatibility)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCourseQRCode(int courseId)
        {
            if (!IsLoggedIn() || !IsTeacher()) return Unauthorized();

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var course = courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return Json(new { success = false, message = "Course not found" });

            try
            {
                var enrollmentUrl = $"/Student/SelfEnroll?courseId={courseId}";
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(enrollmentUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
                return Json(new { success = true, qrCode = qrCodeBase64, courseName = course.CourseName });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"QR generation failed: {ex.Message}" });
            }
        }

        // New: Attendance QR (direct URL to Student/RecordAttendance)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateAttendanceQR(int courseId, string date)
        {
            if (!IsLoggedIn() || !IsTeacher()) return Unauthorized();

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var course = courses.FirstOrDefault(c => c.Id == courseId);
            if (course == null) return Json(new { success = false, message = "Course not found" });

            // Use provided date or today's date
            if (string.IsNullOrEmpty(date))
                date = DateTime.Today.ToString("yyyy-MM-dd");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var attendanceUrl = $"{baseUrl}/Student/RecordAttendance?courseId={courseId}&date={date}";

            try
            {
                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(attendanceUrl, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);
                var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);
                return Json(new { success = true, qrCode = qrCodeBase64, courseName = course.CourseName, date = date });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"QR generation failed: {ex.Message}" });
            }
        }

        public async Task<IActionResult> MyStudents(string? search)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ?? HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            else
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();

            var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
            var myStudents = students.Where(s => myCourses.Any(c => c.Section == s.Section)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                myStudents = myStudents.Where(s =>
                    s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

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

        public async Task<IActionResult> MyAttendance(int? courseId, string? from, string? to)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsTeacher()) return RedirectToAction("Dashboard", "Admin");

            var teacherIdStr = HttpContext.Session.GetString("TeacherId");
            var teacherName = HttpContext.Session.GetString("TeacherName") ?? HttpContext.Session.GetString("Username") ?? "Teacher";

            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            List<CourseApiModel> myCourses;
            if (!string.IsNullOrEmpty(teacherIdStr) && int.TryParse(teacherIdStr, out int teacherId))
                myCourses = courses.Where(c => c.TeacherId == teacherId).ToList();
            else
                myCourses = courses.Where(c => c.TeacherName == teacherName).ToList();

            List<AttendanceApiModel> attendance;
            if (courseId.HasValue && courseId.Value > 0)
                attendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{courseId}");
            else
            {
                attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
                attendance = attendance.Where(a => myCourses.Any(c => c.CourseName == a.CourseName)).ToList();
            }

            if (DateOnly.TryParse(from, out var fromDate))
                attendance = attendance.Where(a => DateOnly.TryParse(a.Date, out var d) && d >= fromDate).ToList();
            if (DateOnly.TryParse(to, out var toDate))
                attendance = attendance.Where(a => DateOnly.TryParse(a.Date, out var d) && d <= toDate).ToList();

            var courseVMs = myCourses.Select(c => new CourseViewModel { DbId = c.Id, CourseName = c.CourseName, Section = c.Section }).ToList();
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

        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> DebugTeacherData()
        {
            var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
            var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
            var result = new
            {
                Teachers = teachers.Select(t => new { t.Id, t.TeacherNo, t.FirstName, t.LastName, t.Username }),
                Courses = courses.Select(c => new { c.Id, c.CourseCode, c.CourseName, c.TeacherId, c.TeacherName }),
                SessionTeacherId = HttpContext.Session.GetString("TeacherId"),
                SessionTeacherName = HttpContext.Session.GetString("TeacherName")
            };
            return Json(result);
        }
    }
}