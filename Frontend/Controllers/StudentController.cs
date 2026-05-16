using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Models;
using AMS.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace AMS.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApiService _api;

        public StudentController(ApiService api)
        {
            _api = api;
        }

        private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private bool IsStudent()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Student" || role == "student";
        }

        private IActionResult RequireLogin() => RedirectToAction("StudentLogin", "Account");

        // ─────────────────────────────────────────────────────
        // STUDENT DASHBOARD
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsStudent()) return RedirectToAction("Login", "Account");

            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Student Dashboard";

            var studentIdStr = HttpContext.Session.GetString("StudentId");
            var studentName = HttpContext.Session.GetString("StudentName") ??
                              HttpContext.Session.GetString("Username") ?? "Student";
            var studentNo = HttpContext.Session.GetString("StudentNo") ?? "";

            List<AttendanceApiModel> myAttendance = new List<AttendanceApiModel>();
            List<CourseApiModel> myCourses = new List<CourseApiModel>();

            if (!string.IsNullOrEmpty(studentIdStr) && int.TryParse(studentIdStr, out int studentId))
            {
                myAttendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/student/{studentId}");
                var allCourses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
                var enrolledCourseIds = myAttendance.Select(a => a.CourseId).Distinct().ToList();
                myCourses = allCourses.Where(c => enrolledCourseIds.Contains(c.Id)).ToList();
            }

            var presentCount = myAttendance.Count(a => a.Status == "Present");
            var lateCount = myAttendance.Count(a => a.Status == "Late");
            var absentCount = myAttendance.Count(a => a.Status == "Absent");
            var totalCount = myAttendance.Count;
            var attendanceRate = totalCount == 0 ? 0 : (int)Math.Round(presentCount * 100.0 / totalCount);

            var recentAttendance = myAttendance
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .Select(a => new AttendanceEntryViewModel
                {
                    CourseName = a.CourseName,
                    Date = a.Date,
                    Status = a.Status,
                    Remarks = a.Remarks
                }).ToList();

            var viewModel = new StudentDashboardViewModel
            {
                StudentName = studentName,
                StudentNo = studentNo,
                AttendanceRate = attendanceRate,
                PresentCount = presentCount,
                LateCount = lateCount,
                AbsentCount = absentCount,
                TotalClasses = totalCount,
                RecentAttendance = recentAttendance,
                MyCourses = myCourses.Select(c => new CourseViewModel
                {
                    DbId = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Section = c.Section
                }).ToList()
            };

            return View(viewModel);
        }

        // ─────────────────────────────────────────────────────
        // QR SCANNER - Camera page for scanning attendance QR
        // ─────────────────────────────────────────────────────
        public IActionResult Scanner()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsStudent()) return RedirectToAction("Login", "Account");
            ViewData["ActivePage"] = "Scanner";
            ViewData["PageTitle"] = "Scan QR Code";
            return View();
        }

        // ─────────────────────────────────────────────────────
        // RECORD ATTENDANCE - REQUIRES LOGIN (SECURE)
        // ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> RecordAttendance(int courseId, string date)
        {
            if (!IsLoggedIn())
            {
                TempData["RedirectCourseId"] = courseId;
                TempData["RedirectDate"] = date;
                return RedirectToAction("StudentLogin", "Account");
            }

            if (!IsStudent())
            {
                return RedirectToAction("Login", "Account");
            }

            var studentIdStr = HttpContext.Session.GetString("StudentId");
            var studentName = HttpContext.Session.GetString("StudentName") ??
                              HttpContext.Session.GetString("Username") ?? "Student";

            if (string.IsNullOrEmpty(studentIdStr) || !int.TryParse(studentIdStr, out int studentId))
            {
                TempData["Error"] = "Student not found. Please log in again.";
                return RedirectToAction("StudentLogin", "Account");
            }

            if (!DateOnly.TryParse(date, out _))
            {
                TempData["Error"] = "Invalid date format.";
                return RedirectToAction("Dashboard");
            }

            var attendanceData = new
            {
                studentId = studentId,
                courseId = courseId,
                date = date,
                status = "Present",
                remarks = "Scanned via QR"
            };

            var result = await _api.PostAsync<object>("/api/Attendance", attendanceData);

            if (result.Success)
            {
                TempData["Success"] = $"Attendance recorded for {studentName}!";
                return RedirectToAction("Dashboard");
            }
            else
            {
                TempData["Error"] = result.Error ?? "Failed to record attendance";
                return RedirectToAction("Dashboard");
            }
        }

        // ─────────────────────────────────────────────────────
        // PROCESS SCANNED QR CODE
        // ─────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessScan(string qrData)
        {
            if (!IsLoggedIn()) return Unauthorized();

            try
            {
                if (qrData.Contains("/Student/RecordAttendance"))
                {
                    var uri = new Uri(qrData);
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var courseId = query["courseId"];
                    var date = query["date"];

                    if (!string.IsNullOrEmpty(courseId) && !string.IsNullOrEmpty(date))
                    {
                        var studentIdStr = HttpContext.Session.GetString("StudentId");
                        if (!string.IsNullOrEmpty(studentIdStr) && int.TryParse(studentIdStr, out int loggedInStudentId))
                        {
                            var attendanceData = new
                            {
                                studentId = loggedInStudentId,
                                courseId = int.Parse(courseId),
                                date = date,
                                status = "Present",
                                remarks = "Scanned via QR (Logged In)"
                            };
                            var result = await _api.PostAsync<object>("/api/Attendance", attendanceData);
                            if (result.Success)
                            {
                                return Json(new { success = true, message = "Attendance recorded successfully!" });
                            }
                            return Json(new { success = false, message = result.Error ?? "Failed to record attendance" });
                        }
                        return Json(new { success = false, message = "Student not found. Please log in again." });
                    }
                }

                string sessionId = "";
                if (qrData.Contains("sessionId="))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(qrData, "sessionId=([^&]+)");
                    if (match.Success) sessionId = match.Groups[1].Value;
                }
                else
                {
                    sessionId = qrData;
                }

                var scanResult = await _api.PostAsync<ScanResultApiModel>($"/api/QR/scan?sessionId={sessionId}", null);

                if (scanResult.Success && scanResult.Data != null)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"Attendance recorded! Status: {scanResult.Data.Status}",
                        status = scanResult.Data.Status
                    });
                }
                else
                {
                    return Json(new { success = false, message = scanResult.Error ?? "Failed to record attendance" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ─────────────────────────────────────────────────────
        // ATTENDANCE HISTORY
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> AttendanceHistory(string? courseFilter, string? statusFilter)
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsStudent()) return RedirectToAction("Login", "Account");

            ViewData["ActivePage"] = "History";
            ViewData["PageTitle"] = "My Attendance History";

            var studentIdStr = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdStr) || !int.TryParse(studentIdStr, out int studentId))
                return View(new List<AttendanceEntryViewModel>());

            var attendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/student/{studentId}");
            var allCourses = await _api.GetAllAsync<CourseApiModel>("/api/Course");

            if (!string.IsNullOrWhiteSpace(courseFilter))
                attendance = attendance.Where(a => a.CourseName == courseFilter).ToList();
            if (!string.IsNullOrWhiteSpace(statusFilter))
                attendance = attendance.Where(a => a.Status == statusFilter).ToList();

            var viewModel = attendance
                .OrderByDescending(a => a.Date)
                .Select(a => new AttendanceEntryViewModel
                {
                    CourseName = a.CourseName,
                    Date = a.Date,
                    Status = a.Status,
                    Remarks = a.Remarks
                }).ToList();

            var courseNames = allCourses.Select(c => c.CourseName).Distinct().ToList();
            ViewBag.Courses = courseNames;
            ViewBag.SelectedCourse = courseFilter;
            ViewBag.SelectedStatus = statusFilter;

            return View(viewModel);
        }

        // ─────────────────────────────────────────────────────
        // MY COURSES
        // ─────────────────────────────────────────────────────
        public async Task<IActionResult> MyCourses()
        {
            if (!IsLoggedIn()) return RequireLogin();
            if (!IsStudent()) return RedirectToAction("Login", "Account");

            ViewData["ActivePage"] = "MyCourses";
            ViewData["PageTitle"] = "My Courses";

            var studentIdStr = HttpContext.Session.GetString("StudentId");
            if (string.IsNullOrEmpty(studentIdStr) || !int.TryParse(studentIdStr, out int studentId))
                return View(new List<CourseViewModel>());

            var attendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/student/{studentId}");
            var allCourses = await _api.GetAllAsync<CourseApiModel>("/api/Course");

            var enrolledCourseIds = attendance.Select(a => a.CourseId).Distinct().ToList();
            var myCourses = allCourses
                .Where(c => enrolledCourseIds.Contains(c.Id))
                .Select(c => new CourseViewModel
                {
                    DbId = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    Section = c.Section,
                    Schedule = c.Schedule,
                    TeacherName = c.TeacherName
                }).ToList();

            return View(myCourses);
        }

        // ─────────────────────────────────────────────────────
        // SELF ENROLLMENT PAGE (from enrollment QR)
        // ─────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet]
        public IActionResult SelfEnroll(int courseId)
        {
            ViewBag.CourseId = courseId;
            return View();
        }

        // ─────────────────────────────────────────────────────
        // INDEX - Redirect to Dashboard
        // ─────────────────────────────────────────────────────
        public IActionResult Index()
        {
            if (!IsLoggedIn()) return RedirectToAction("StudentLogin", "Account");
            return RedirectToAction("Dashboard");
        }
    }
}