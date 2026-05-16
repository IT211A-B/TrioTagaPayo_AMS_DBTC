using Microsoft.AspNetCore.Mvc;
using AMS.Models;
using AMS.Services;
using AMS.ViewModels;
using System.Text.Json;
using QRCoder;

namespace AMS.Controllers;

public class AdminController : Controller
{
    private readonly ApiService _api;
    private static readonly string[] AvatarColors = ["#6366f1", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#3b82f6", "#ef4444", "#14b8a6"];

    public AdminController(ApiService api)
    {
        _api = api;
    }

    // =============================================
    // HELPER METHODS
    // =============================================

    private bool IsLoggedIn()
    {
        return !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "Admin" || role == "admin";
    }

    private IActionResult? RequireAdmin()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsAdmin()) return RedirectToAction("Dashboard", "Teacher");
        return null;
    }

    private JsonResult AjaxOk(string message)
    {
        return Json(new { success = true, message });
    }

    private JsonResult AjaxFail(string message)
    {
        return Json(new { success = false, message });
    }

    private async Task<string> GenerateStudentNumber()
    {
        var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
        int highestNumber = 0;

        foreach (var student in students)
        {
            var studentNo = student.StudentNo;
            if (!string.IsNullOrEmpty(studentNo) && studentNo.StartsWith("STU"))
            {
                var numberPart = studentNo.Substring(3);
                if (int.TryParse(numberPart, out int num) && num > highestNumber)
                {
                    highestNumber = num;
                }
            }
        }

        return $"STU{(highestNumber + 1):D3}";
    }

    private async Task<string> GenerateTeacherNumber()
    {
        var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
        var lastTeacher = teachers.OrderByDescending(t => t.Id).FirstOrDefault();

        int nextNumber = 1;
        if (lastTeacher != null && !string.IsNullOrEmpty(lastTeacher.TeacherNo))
        {
            var parts = lastTeacher.TeacherNo.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        return $"TCH-{nextNumber:D4}";
    }

    private static string ParseError(string rawJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawJson);
            if (doc.RootElement.TryGetProperty("message", out var msg))
            {
                return msg.GetString() ?? rawJson;
            }
            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? rawJson;
            }
            if (doc.RootElement.TryGetProperty("errors", out var errors))
            {
                return "Validation error occurred.";
            }
        }
        catch { }

        return rawJson.Length > 200 ? rawJson[..200] : rawJson;
    }

    private async Task<List<StudentViewModel>> BuildStudentVMs(string? search, string? section)
    {
        var all = await _api.GetAllAsync<StudentApiModel>("/api/Student");

        if (!string.IsNullOrWhiteSpace(search))
        {
            all = all.Where(s =>
                s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            all = all.Where(s => s.Section == section).ToList();
        }

        var result = new List<StudentViewModel>();
        for (int i = 0; i < all.Count; i++)
        {
            var s = all[i];
            result.Add(new StudentViewModel
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
            });
        }
        return result;
    }

    private async Task<List<TeacherViewModel>> BuildTeacherVMs(string? search, string? status)
    {
        var all = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

        if (!string.IsNullOrWhiteSpace(search))
        {
            all = all.Where(t =>
                t.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.TeacherNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Email.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            all = all.Where(t => t.IsActive == isActive).ToList();
        }

        var result = new List<TeacherViewModel>();
        for (int i = 0; i < all.Count; i++)
        {
            var t = all[i];
            result.Add(new TeacherViewModel
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
            });
        }
        return result;
    }

    private async Task<List<AttendanceApiModel>> FetchAttendance(int? courseId, string? from, string? to, string? status)
    {
        List<AttendanceApiModel> result;

        if (courseId.HasValue && courseId.Value > 0)
        {
            if (DateOnly.TryParse(from, out var f) && DateOnly.TryParse(to, out var t))
            {
                result = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/filter?courseId={courseId}&from={f:yyyy-MM-dd}&to={t:yyyy-MM-dd}");
            }
            else
            {
                result = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{courseId}");
            }
        }
        else if (DateOnly.TryParse(from, out var fromDate) && DateOnly.TryParse(to, out var toDate))
        {
            var all = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
            result = all.Where(a => DateOnly.TryParse(a.Date, out var d) && d >= fromDate && d <= toDate).ToList();
        }
        else
        {
            result = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            result = result.Where(a => a.Status == status).ToList();
        }

        return result;
    }

    private static List<AttendanceEntryViewModel> MapAttendanceRows(List<AttendanceApiModel> src)
    {
        var result = new List<AttendanceEntryViewModel>();
        foreach (var a in src)
        {
            result.Add(new AttendanceEntryViewModel
            {
                AttendanceId = a.Id,
                StudentId = a.StudentId,
                StudentName = a.StudentName,
                StudentNo = a.StudentNo,
                CourseName = a.CourseName,
                Date = a.Date,
                Status = a.Status,
                Remarks = a.Remarks
            });
        }
        return result;
    }

    // =============================================
    // DASHBOARD
    // =============================================

    public async Task<IActionResult> Dashboard()
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Dashboard";
        ViewData["PageTitle"] = "Dashboard";

        var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
        var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var attendance = await _api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
        var recent = attendance.Where(a => DateOnly.TryParse(a.Date, out var d) && d >= cutoff).ToList();
        int rate = recent.Count == 0 ? 0 : (int)Math.Round(recent.Count(a => a.Status == "Present") * 100.0 / recent.Count);

        var recentRows = attendance.OrderByDescending(a => a.CreatedAt).Take(10).Select(a => new AttendanceEntryViewModel
        {
            StudentName = a.StudentName,
            StudentNo = a.StudentNo,
            CourseName = a.CourseName,
            Date = a.Date,
            Status = a.Status,
            Remarks = a.Remarks
        }).ToList();

        var weekDays = new List<string>();
        var weeklyPresent = new List<int>();
        var weeklyAbsent = new List<int>();
        var weeklyLate = new List<int>();

        DateTime currentDate = DateTime.Today;
        while (currentDate.DayOfWeek != DayOfWeek.Monday)
        {
            currentDate = currentDate.AddDays(-1);
        }

        for (int i = 0; i < 5; i++)
        {
            var date = currentDate.AddDays(i);
            var dateStr = date.ToString("yyyy-MM-dd");
            var dayName = date.ToString("ddd");
            var dayAttendance = attendance.Where(a => a.Date == dateStr).ToList();

            weekDays.Add(dayName);
            weeklyPresent.Add(dayAttendance.Count(a => a.Status == "Present"));
            weeklyAbsent.Add(dayAttendance.Count(a => a.Status == "Absent"));
            weeklyLate.Add(dayAttendance.Count(a => a.Status == "Late"));
        }

        var viewModel = new DashboardViewModel
        {
            TotalStudents = students.Count,
            TotalTeachers = teachers.Count,
            TotalCourses = courses.Count,
            AttendanceRate = rate,
            RecentAttendance = recentRows,
            WeeklyPresent = weeklyPresent,
            WeeklyAbsent = weeklyAbsent,
            WeeklyLate = weeklyLate,
            WeekDays = weekDays
        };

        return View(viewModel);
    }

    // =============================================
    // QR CODE GENERATION
    // =============================================

    [HttpGet]
    public async Task<IActionResult> GenerateCourseQRCode(int courseId)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == courseId);

        if (course == null)
        {
            return Json(new { success = false, message = "Course not found" });
        }

        try
        {
            var enrollmentUrl = $"/Student/SelfEnroll?courseId={courseId}";

            using (var qrGenerator = new QRCodeGenerator())
            using (var qrCodeData = qrGenerator.CreateQrCode(enrollmentUrl, QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            {
                var qrCodeBytes = qrCode.GetGraphic(20);
                var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

                return Json(new { success = true, qrCode = qrCodeBase64, courseName = course.CourseName });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"QR generation failed: {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateAttendanceQR(int courseId, string date)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return Json(new { success = false, message = "Course not found" });

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

    // =============================================
    // MODALS
    // =============================================

    [HttpGet]
    public IActionResult AddStudentModal()
    {
        return PartialView("Partials/_AddStudentModal");
    }

    [HttpGet]
    public IActionResult AddTeacherModal()
    {
        return PartialView("Partials/_AddTeacherModal");
    }

    [HttpGet]
    public IActionResult AddCourseModal()
    {
        return PartialView("Partials/_AddCourseModal");
    }

    [HttpGet]
    public IActionResult GetCurrentUserInfo()
    {
        return Json(new
        {
            isLoggedIn = IsLoggedIn(),
            role = HttpContext.Session.GetString("Role"),
            username = HttpContext.Session.GetString("Username"),
            hasJwt = !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"))
        });
    }

    // =============================================
    // STUDENTS
    // =============================================

    public async Task<IActionResult> Students(string? search, string? section)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Students";
        ViewData["PageTitle"] = "Students";

        var students = await BuildStudentVMs(search, section);
        var allStudents = await _api.GetAllAsync<StudentApiModel>("/api/Student");
        var sections = allStudents.Select(s => s.Section).Distinct().OrderBy(s => s).ToList();

        ViewBag.Sections = sections;
        ViewBag.SectionFilter = section;

        return View(new StudentsPageViewModel
        {
            Students = students,
            TotalCount = students.Count,
            Search = search,
            SectionFilter = section
        });
    }

    // =============================================
    // ADD STUDENT (UPDATED WITH PASSWORD)
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(string firstName, string middleName, string lastName, string email, string section, string mobileNo, string password)
    {
        if (!IsLoggedIn()) return Unauthorized();

        // Validate required fields
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(section))
        {
            return AjaxFail("Please fill in all required fields.");
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(password))
        {
            return AjaxFail("Please enter a password for the student.");
        }

        if (password.Length < 6)
        {
            return AjaxFail("Password must be at least 6 characters.");
        }

        try
        {
            // Step 1: Generate Student Number
            var studentNo = await GenerateStudentNumber();

            // Step 2: Create Student record via API
            var newStudent = new
            {
                studentNo = studentNo,
                firstName = firstName,
                middleName = middleName ?? "",
                lastName = lastName,
                email = email,
                section = section,
                mobileNo = mobileNo ?? ""
            };

            var createStudent = await _api.PostAsync<StudentApiModel>("/api/Student", newStudent);

            if (!createStudent.Success)
            {
                return AjaxFail($"Failed to create student: {createStudent.Error}");
            }

            // Step 3: Create User account for the student
            var registerData = new
            {
                username = studentNo,
                password = password,
                fullName = $"{firstName} {lastName}",
                email = email,
                role = "Student"
            };

            var registerUser = await _api.PostAsync<object>("/api/Auth/register", registerData);

            if (!registerUser.Success)
            {
                // Rollback - delete the student if user creation fails
                if (createStudent.Data != null)
                {
                    await _api.DeleteAsync($"/api/Student/{createStudent.Data.Id}");
                }
                return AjaxFail($"Failed to create user account: {registerUser.Error}");
            }

            return AjaxOk($"Student created successfully! Student ID: {studentNo}, Password: [set by admin]");
        }
        catch (Exception ex)
        {
            return AjaxFail($"Error: {ex.Message}");
        }
    }

    // =============================================
    // UPDATE STUDENT
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudent(int id, string firstName, string middleName, string lastName, string email, string section, string mobileNo)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(section))
        {
            return AjaxFail("Please fill in all required fields.");
        }

        var body = new
        {
            firstName = firstName,
            middleName = middleName ?? "",
            lastName = lastName,
            email = email,
            section = section,
            mobileNo = mobileNo ?? ""
        };

        var result = await _api.PutAsync($"/api/Student/{id}", body);

        if (result.Success)
        {
            return AjaxOk("Student updated successfully.");
        }
        return AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // =============================================
    // DELETE STUDENT
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();

        // First, get the student to know their StudentNo
        var student = await _api.GetAsync<StudentApiModel>($"/api/Student/{id}");

        // Delete the student record
        var result = await _api.DeleteAsync($"/api/Student/{id}");

        if (result.Success)
        {
            // Also try to delete the user account if it exists
            if (student != null && !string.IsNullOrEmpty(student.StudentNo))
            {
                await _api.DeleteAsync($"/api/User/{student.StudentNo}");
            }
            return AjaxOk("Student deleted successfully.");
        }

        return AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // =============================================
    // TEACHERS
    // =============================================

    public async Task<IActionResult> Teachers(string? search, string? status)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

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

    // =============================================
    // ADD TEACHER
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTeacher(string firstName, string lastName, string email)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            return AjaxFail("Please fill in all required fields.");
        }

        try
        {
            var teacherNo = await GenerateTeacherNumber();
            var username = $"{firstName.ToLower()}.{lastName.ToLower()}";
            var defaultPassword = $"teacher123";

            // Create User account first
            var registerData = new
            {
                username = username,
                password = defaultPassword,
                fullName = $"{firstName} {lastName}",
                email = email,
                role = "Teacher"
            };

            var registerUser = await _api.PostAsync<object>("/api/Auth/register", registerData);

            if (!registerUser.Success)
            {
                return AjaxFail($"Failed to create teacher account: {registerUser.Error}");
            }

            // Then create Teacher record
            var newTeacher = new
            {
                teacherNo = teacherNo,
                firstName = firstName,
                lastName = lastName,
                email = email,
                isActive = true,
                username = username,
                hasAccount = true
            };

            var createTeacher = await _api.PostAsync<TeacherApiModel>("/api/Teacher", newTeacher);

            if (!createTeacher.Success)
            {
                // Rollback - delete user account if teacher creation fails
                await _api.DeleteAsync($"/api/User/{username}");
                return AjaxFail($"Failed to create teacher record: {createTeacher.Error}");
            }

            return AjaxOk($"Teacher created successfully! Username: {username}, Password: {defaultPassword}");
        }
        catch (Exception ex)
        {
            return AjaxFail($"Error: {ex.Message}");
        }
    }

    // =============================================
    // UPDATE TEACHER
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTeacher(int id, string firstName, string lastName, string email)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
        {
            return AjaxFail("Please fill in all required fields.");
        }

        var body = new
        {
            firstName = firstName,
            lastName = lastName,
            email = email
        };

        var result = await _api.PutAsync($"/api/Teacher/{id}", body);
        return result.Success ? AjaxOk("Teacher updated successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // =============================================
    // DELETE TEACHER
    // =============================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var teacher = await _api.GetAsync<TeacherApiModel>($"/api/Teacher/{id}");
        var result = await _api.DeleteAsync($"/api/Teacher/{id}");

        if (result.Success)
        {
            if (teacher != null && !string.IsNullOrEmpty(teacher.Username))
            {
                await _api.DeleteAsync($"/api/User/{teacher.Username}");
            }
            return AjaxOk("Teacher deleted successfully.");
        }

        return AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // =============================================
    // COURSES
    // =============================================

    public async Task<IActionResult> Courses(string? search)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Courses";

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

        if (!string.IsNullOrWhiteSpace(search))
        {
            courses = courses.Where(c =>
                c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.CourseCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Section.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

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

        var teacherVMs = teachers.Where(t => t.IsActive).Select((t, i) => new TeacherViewModel
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
    public async Task<IActionResult> AddCourse(string courseCode, string courseName, int units, string section, string schedule, int teacherId)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var body = new { courseCode, courseName, units, section, schedule, teacherId };
        var result = await _api.PostAsync<CourseApiModel>("/api/Course", body);
        return result.Success ? AjaxOk("Course added successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCourse(int id, string courseCode, string courseName, int units, string section, string schedule, int teacherId)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var body = new { courseCode, courseName, units, section, schedule, teacherId };
        var result = await _api.PutAsync($"/api/Course/{id}", body);
        return result.Success ? AjaxOk("Course updated successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var result = await _api.DeleteAsync($"/api/Course/{id}");
        return result.Success ? AjaxOk("Course deleted successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    public async Task<IActionResult> CourseManage(int id)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Course Management";

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
        {
            TempData["Error"] = "Course not found";
            return RedirectToAction("Courses", "Admin");
        }

        var teachers = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
        var activeTeachers = teachers.Where(t => t.IsActive).ToList();

        var uniqueTeachers = activeTeachers
            .GroupBy(t => t.TeacherNo)
            .Select(g => g.First())
            .ToList();

        var courseVM = new CourseViewModel
        {
            DbId = course.Id,
            CourseCode = course.CourseCode,
            CourseName = course.CourseName,
            Units = course.Units,
            Section = course.Section,
            Schedule = course.Schedule,
            TeacherId = course.TeacherId,
            TeacherName = course.TeacherName
        };

        var teacherOptions = uniqueTeachers.Select(t => new
        {
            Id = t.Id,
            Name = $"{t.FirstName} {t.LastName}",
            TeacherNo = t.TeacherNo
        }).ToList();

        ViewBag.Teachers = teacherOptions;

        return View(courseVM);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CourseManage(int id, string courseCode, string courseName, int units, string section, string schedule, int teacherId)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(courseName) ||
            string.IsNullOrWhiteSpace(section) || string.IsNullOrWhiteSpace(schedule) || teacherId <= 0)
        {
            TempData["Error"] = "Please fill in all required fields.";
            return RedirectToAction("CourseManage", new { id });
        }

        var body = new { courseCode, courseName, units, section, schedule, teacherId };
        var result = await _api.PutAsync($"/api/Course/{id}", body);

        if (result.Success)
        {
            TempData["Success"] = "Course updated successfully!";
            return RedirectToAction("Courses", "Admin");
        }

        TempData["Error"] = $"Failed to update course: {ParseError(result.Error)}";
        return RedirectToAction("CourseManage", new { id });
    }

    // =============================================
    // COURSE DETAILS
    // =============================================

    public async Task<IActionResult> CourseDetails(int id, string? date)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Course Details";

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
        {
            TempData["Error"] = "Course not found";
            return RedirectToAction("Courses", "Admin");
        }

        var allStudents = await _api.GetAllAsync<StudentApiModel>("/api/Student");
        var studentsInCourse = allStudents.Where(s => s.Section == course.Section).ToList();
        var attendance = await _api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{id}");

        var studentVMs = new List<StudentCourseViewModel>();
        foreach (var student in studentsInCourse)
        {
            var studentAttendance = attendance.Where(a => a.StudentId == student.Id).ToList();
            var attendanceRate = studentAttendance.Count == 0 ? 0 :
                (int)Math.Round(studentAttendance.Count(a => a.Status == "Present") * 100.0 / studentAttendance.Count);

            studentVMs.Add(new StudentCourseViewModel
            {
                StudentId = student.Id,
                StudentNo = student.StudentNo,
                StudentName = $"{student.FirstName} {student.LastName}",
                Email = student.Email,
                AttendanceRate = attendanceRate,
                PresentCount = studentAttendance.Count(a => a.Status == "Present"),
                AbsentCount = studentAttendance.Count(a => a.Status == "Absent"),
                LateCount = studentAttendance.Count(a => a.Status == "Late"),
                IsEnrolled = studentAttendance.Any()
            });
        }

        var selectedDate = date ?? DateTime.Today.ToString("yyyy-MM-dd");
        var todayAttendance = attendance.Where(a => a.Date == selectedDate).ToList();

        var todayAttendanceVMs = todayAttendance.Select(a => new AttendanceEntryViewModel
        {
            AttendanceId = a.Id,
            StudentId = a.StudentId,
            StudentName = a.StudentName,
            StudentNo = a.StudentNo,
            Status = a.Status,
            Remarks = a.Remarks,
            Date = a.Date
        }).ToList();

        ViewBag.SelectedDate = selectedDate;

        var viewModel = new CourseDetailsViewModel
        {
            Course = new CourseViewModel
            {
                DbId = course.Id,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                Units = course.Units,
                Section = course.Section,
                Schedule = course.Schedule,
                TeacherId = course.TeacherId,
                TeacherName = course.TeacherName
            },
            Students = studentVMs,
            TodayAttendance = todayAttendanceVMs,
            TotalStudents = studentsInCourse.Count,
            EnrolledStudents = studentVMs.Count(s => s.IsEnrolled),
            TotalAttendanceRecords = attendance.Count
        };

        return View(viewModel);
    }

    // =============================================
    // ATTENDANCE
    // =============================================

    public async Task<IActionResult> Attendance(int? courseId, string? from, string? to)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Attendance";
        ViewData["PageTitle"] = "Attendance";

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var attendance = await FetchAttendance(courseId, from, to, null);

        var courseVMs = courses.Select(c => new CourseViewModel
        {
            DbId = c.Id,
            CourseName = c.CourseName,
            Section = c.Section,
            CourseCode = c.CourseCode
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

    [HttpGet]
    public async Task<IActionResult> AttendanceFilter(int? courseId, string? from, string? to, string? status)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var attendance = await FetchAttendance(courseId, from, to, status);
        return PartialView("Partials/_AttendanceTableRows", MapAttendanceRows(attendance));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAttendance(int attendanceId, string status, string remarks)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var body = new { status, remarks };
        var result = await _api.PutAsync($"/api/Attendance/{attendanceId}", body);

        return result.Success
            ? Json(new { success = true, message = "Attendance updated successfully." })
            : Json(new { success = false, message = $"Failed: {ParseError(result.Error)}" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllPresent(int courseId, string date)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var courses = await _api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return Json(new { success = false, message = "Course not found" });

        var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
        var courseStudents = students.Where(s => s.Section == course.Section).ToList();

        var attendances = courseStudents.Select(s => new
        {
            studentId = s.Id,
            status = "Present",
            remarks = "Marked all present by admin"
        }).ToList();

        var body = new { courseId, date, attendances };
        var result = await _api.PostAsync<object>("/api/Attendance/bulk", body);

        return result.Success
            ? Json(new { success = true, message = $"Marked {courseStudents.Count} students as present." })
            : Json(new { success = false, message = $"Failed: {ParseError(result.Error)}" });
    }
}
