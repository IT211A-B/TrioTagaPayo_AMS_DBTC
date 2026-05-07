using Microsoft.AspNetCore.Mvc;
using AMS.Models;
using AMS.Services;
using AMS.ViewModels;
using System.Text.Json;
using AMS.ViewModels;

namespace AMS.Controllers;

public class AdminController(ApiService api) : Controller
{
    private static readonly string[] AvatarColors = ["#6366f1", "#8b5cf6", "#ec4899", "#f59e0b", "#10b981", "#3b82f6", "#ef4444", "#14b8a6"];

    private bool IsLoggedIn() => !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "Admin" || role == "admin";
    }

    private RedirectToActionResult RequireLogin() => RedirectToAction("Login", "Account");

    private RedirectToActionResult? RequireAdmin()
    {
        if (!IsLoggedIn()) return RedirectToAction("Login", "Account");
        if (!IsAdmin()) return RedirectToAction("Dashboard", "Teacher");
        return null;
    }

    private JsonResult AjaxOk(string message) => Json(new { success = true, message });
    private JsonResult AjaxFail(string message) => Json(new { success = false, message });

    // ─────────────────────────────────────────────────────
    // AUTO-GENERATE STUDENT NUMBER
    // ─────────────────────────────────────────────────────
    private async Task<string> GenerateStudentNumber()
    {
        var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
        var lastStudent = students.OrderByDescending(s => s.Id).FirstOrDefault();

        int nextNumber = 1;
        if (lastStudent != null && !string.IsNullOrEmpty(lastStudent.StudentNo))
        {
            var parts = lastStudent.StudentNo.Split('-');
            if (parts.Length == 2 && int.TryParse(parts[1], out int lastNum))
            {
                nextNumber = lastNum + 1;
            }
        }

        return $"{DateTime.Now.Year}-{nextNumber:D5}";
    }

    // ─────────────────────────────────────────────────────
    // AUTO-GENERATE TEACHER NUMBER
    // ─────────────────────────────────────────────────────
    private async Task<string> GenerateTeacherNumber()
    {
        var teachers = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");
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

    // 1. DASHBOARD
    public async Task<IActionResult> Dashboard()
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Dashboard";
        ViewData["PageTitle"] = "Dashboard";

        var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
        var teachers = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");
        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var attendance = await api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

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

        return View(new DashboardViewModel
        {
            TotalStudents = students.Count,
            TotalTeachers = teachers.Count,
            TotalCourses = courses.Count,
            AttendanceRate = rate,
            RecentAttendance = recentRows
        });
    }

    // 2. STUDENTS
    public async Task<IActionResult> Students(string? search, string? section)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Students";
        ViewData["PageTitle"] = "Students";

        var students = await BuildStudentVMs(search, section);
        var allStudents = await api.GetAllAsync<StudentApiModel>("/api/Student");
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

    // TEACHERS PARTIAL
    [HttpGet]
    public async Task<IActionResult> TeachersPartial(string? search, string? status)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var teachers = await BuildTeacherVMs(search, status);
        return PartialView("_TeacherTableRows", teachers);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(string firstName, string middleName, string lastName, string email, string section, string mobileNo)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            return AjaxFail("Please fill in all required fields.");

        var studentNo = await GenerateStudentNumber();
        var body = new { studentNo, firstName, middleName, lastName, email, section, mobileNo };
        var result = await api.PostAsync<StudentApiModel>("/api/Student", body);
        return result.Success ? AjaxOk($"Student added successfully. Student No: {studentNo}")
                              : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStudent(int id, string studentNo, string firstName, string middleName, string lastName, string email, string section, string mobileNo)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var body = new { studentNo, firstName, middleName, lastName, email, section, mobileNo };
        var result = await api.PutAsync($"/api/Student/{id}", body);
        return result.Success ? AjaxOk("Student updated successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteStudent(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var result = await api.DeleteAsync($"/api/Student/{id}");
        return result.Success ? AjaxOk("Student deleted successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // 3. TEACHERS
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTeacher(string firstName, string lastName, string email)
    {
        if (!IsLoggedIn()) return Unauthorized();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrWhiteSpace(email))
            return AjaxFail("Please fill in all required fields.");

        var teacherNo = await GenerateTeacherNumber();
        var username = $"{firstName.ToLower()}.{lastName.ToLower()}";
        var password = "teacher123";
        var body = new { teacherNo, firstName, lastName, email, username, password };
        var result = await api.PostAsync<TeacherApiModel>("/api/Teacher/with-account", body);

        return result.Success ? AjaxOk($"Teacher added successfully. Username: {username}, Password: {password}")
                              : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTeacher(int id, string teacherNo, string firstName, string lastName, string email)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var body = new { teacherNo, firstName, lastName, email };
        var result = await api.PutAsync($"/api/Teacher/{id}", body);
        return result.Success ? AjaxOk("Teacher updated successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTeacher(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var result = await api.DeleteAsync($"/api/Teacher/{id}");
        return result.Success ? AjaxOk("Teacher deleted successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTeacherStatus(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var result = await api.PatchAsync($"/api/Teacher/{id}/toggle-status");
        return result.Success ? AjaxOk("Teacher status updated.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // 4. COURSES
    public async Task<IActionResult> Courses(string? search)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Courses";

        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var teachers = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");

        if (!string.IsNullOrWhiteSpace(search))
        {
            courses = [.. courses.Where(c =>
                c.CourseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.CourseCode.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                c.Section.Contains(search, StringComparison.OrdinalIgnoreCase))];
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
        var result = await api.PostAsync<CourseApiModel>("/api/Course", body);
        return result.Success ? AjaxOk("Course added successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCourse(int id, string courseCode, string courseName, int units, string section, string schedule, int teacherId)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var body = new { courseCode, courseName, units, section, schedule, teacherId };
        var result = await api.PutAsync($"/api/Course/{id}", body);
        return result.Success ? AjaxOk("Course updated successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        if (!IsLoggedIn()) return Unauthorized();
        var result = await api.DeleteAsync($"/api/Course/{id}");
        return result.Success ? AjaxOk("Course deleted successfully.") : AjaxFail($"Failed: {ParseError(result.Error)}");
    }

    // 5. COURSE MANAGEMENT
    public async Task<IActionResult> CourseManage(int id)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Course Management";

        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
        {
            TempData["Error"] = "Course not found";
            return RedirectToAction("Courses", "Admin");
        }

        var teachers = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");
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
        var result = await api.PutAsync($"/api/Course/{id}", body);

        if (result.Success)
        {
            TempData["Success"] = "Course updated successfully!";
            return RedirectToAction("Courses", "Admin");
        }

        TempData["Error"] = $"Failed to update course: {ParseError(result.Error)}";
        return RedirectToAction("CourseManage", new { id });
    }

    // 6. ATTENDANCE
    public async Task<IActionResult> Attendance(int? courseId, string? from, string? to)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Attendance";
        ViewData["PageTitle"] = "Attendance";

        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
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
        return PartialView("_AttendanceTableRows", MapAttendanceRows(attendance));
    }

    // 7. ENROLLMENT
    public async Task<IActionResult> Enrollment(string? search, string? course, string? status)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Enrollment";
        ViewData["PageTitle"] = "Enrollment";

        var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var attendance = await api.GetAllAsync<AttendanceApiModel>("/api/Attendance");

        var enrollments = new List<EnrollmentViewModel>();

        foreach (var student in students)
        {
            var studentCourses = courses.Where(c => c.Section == student.Section).ToList();

            foreach (var courseItem in studentCourses)
            {
                var studentAttendance = attendance.Where(a => a.StudentId == student.Id && a.CourseId == courseItem.Id).ToList();
                var attendanceRate = studentAttendance.Count == 0 ? 0 :
                    (int)Math.Round(studentAttendance.Count(a => a.Status == "Present") * 100.0 / studentAttendance.Count);

                var enrollmentDate = studentAttendance.Any()
                    ? studentAttendance.Min(a => a.Date)
                    : DateTime.Today.ToString("yyyy-MM-dd");

                enrollments.Add(new EnrollmentViewModel
                {
                    StudentId = student.Id,
                    StudentName = $"{student.FirstName} {student.LastName}",
                    StudentNo = student.StudentNo,
                    Email = student.Email,
                    CourseId = courseItem.Id,
                    CourseName = courseItem.CourseName,
                    Section = courseItem.Section,
                    AttendanceRate = attendanceRate,
                    Status = attendanceRate >= 75 ? "Enrolled" : attendanceRate >= 50 ? "At Risk" : "Probation",
                    EnrollmentDate = enrollmentDate
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            enrollments = enrollments.Where(e =>
                e.StudentName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(course))
        {
            enrollments = enrollments.Where(e => e.CourseName == course).ToList();
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            enrollments = enrollments.Where(e => e.Status == status).ToList();
        }

        var courseList = courses.Select(c => c.CourseName).Distinct().ToList();

        var allStudents = students.Select(s => new { s.Id, Name = $"{s.FirstName} {s.LastName} (ID: {s.StudentNo})", s.Section }).ToList();
        var allCourses = courses.Select(c => new { c.Id, Name = $"{c.CourseName} - {c.Section}", c.Section }).ToList();

        ViewBag.Courses = courseList;
        ViewBag.AllStudents = allStudents;
        ViewBag.AllCourses = allCourses;

        return View(new EnrollmentPageViewModel
        {
            Enrollments = enrollments,
            TotalCount = enrollments.Count,
            Search = search,
            CourseFilter = course,
            StatusFilter = status
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEnrollment(int studentId, int courseId)
    {
        if (!IsLoggedIn()) return Unauthorized();

        try
        {
            if (studentId <= 0 || courseId <= 0)
            {
                return Json(new { success = false, message = "Invalid student or course ID." });
            }

            var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
            var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");

            var student = students.FirstOrDefault(s => s.Id == studentId);
            var course = courses.FirstOrDefault(c => c.Id == courseId);

            if (student == null)
            {
                return Json(new { success = false, message = "Student not found." });
            }

            if (course == null)
            {
                return Json(new { success = false, message = "Course not found." });
            }

            if (!string.Equals(student.Section, course.Section, StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = $"Student section '{student.Section}' does not match course section '{course.Section}'." });
            }

            var existingAttendance = await api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/student/{studentId}");
            var alreadyEnrolled = existingAttendance.Any(a => a.CourseId == courseId);

            if (alreadyEnrolled)
            {
                return Json(new { success = false, message = "Student is already enrolled in this course." });
            }

            var today = DateTime.Today.ToString("yyyy-MM-dd");
            var body = new
            {
                studentId = studentId,
                courseId = courseId,
                date = today,
                status = "Present",
                remarks = "Enrolled via admin"
            };

            var result = await api.PostAsync<object>("/api/Attendance", body);

            if (result.Success)
            {
                return Json(new { success = true, message = $"Student '{student.FirstName} {student.LastName}' successfully enrolled in '{course.CourseName}'." });
            }
            else
            {
                return Json(new { success = false, message = $"Failed to enroll student: {ParseError(result.Error)}" });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEnrollment(int studentId, int courseId)
    {
        if (!IsLoggedIn()) return Unauthorized();

        try
        {
            if (studentId <= 0 || courseId <= 0)
            {
                return Json(new { success = false, message = "Invalid student or course ID." });
            }

            var allAttendance = await api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
            var recordsToDelete = allAttendance.Where(a => a.StudentId == studentId && a.CourseId == courseId).ToList();

            if (!recordsToDelete.Any())
            {
                return Json(new { success = false, message = "No enrollment records found for this student in this course." });
            }

            var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
            var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");

            var student = students.FirstOrDefault(s => s.Id == studentId);
            var course = courses.FirstOrDefault(c => c.Id == courseId);

            string studentName = student != null ? $"{student.FirstName} {student.LastName}" : "Student";
            string courseName = course != null ? course.CourseName : "Course";

            bool allDeleted = true;
            int deletedCount = 0;
            int failedCount = 0;

            foreach (var record in recordsToDelete)
            {
                var result = await api.DeleteAsync($"/api/Attendance/{record.Id}");
                if (result.Success)
                {
                    deletedCount++;
                }
                else
                {
                    allDeleted = false;
                    failedCount++;
                }
            }

            if (allDeleted)
            {
                return Json(new { success = true, message = $"Successfully unenrolled '{studentName}' from '{courseName}'. ({deletedCount} attendance records removed)" });
            }
            else
            {
                return Json(new { success = false, message = $"Partially completed: {deletedCount} records deleted, {failedCount} failed to delete." });
            }
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
        }
    }

    // =============================================
    // NEW: COURSE DETAILS ACTIONS
    // =============================================

    // 10. COURSE DETAILS - View students and attendance for a specific course
    public async Task<IActionResult> CourseDetails(int id, string? date)
    {
        var adminCheck = RequireAdmin();
        if (adminCheck != null) return adminCheck;

        ViewData["ActivePage"] = "Courses";
        ViewData["PageTitle"] = "Course Details";

        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == id);

        if (course == null)
        {
            TempData["Error"] = "Course not found";
            return RedirectToAction("Courses", "Admin");
        }

        var allStudents = await api.GetAllAsync<StudentApiModel>("/api/Student");
        var studentsInCourse = allStudents.Where(s => s.Section == course.Section).ToList();
        var attendance = await api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{id}");

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

        var attendanceByDate = attendance
            .GroupBy(a => a.Date)
            .Select(g => new AttendanceDateSummary
            {
                Date = g.Key,
                PresentCount = g.Count(a => a.Status == "Present"),
                AbsentCount = g.Count(a => a.Status == "Absent"),
                LateCount = g.Count(a => a.Status == "Late"),
                TotalCount = g.Count()
            })
            .OrderBy(g => g.Date)
            .ToList();

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

        var teachers = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");
        var teacherOptions = teachers.Where(t => t.IsActive).Select(t => new
        {
            Id = t.Id,
            Name = $"{t.FirstName} {t.LastName}"
        }).ToList();

        ViewBag.Teachers = teacherOptions;
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
            AttendanceByDate = attendanceByDate,
            TodayAttendance = todayAttendanceVMs,
            TotalStudents = studentsInCourse.Count,
            EnrolledStudents = studentVMs.Count(s => s.IsEnrolled),
            TotalAttendanceRecords = attendance.Count
        };

        return View(viewModel);
    }

    // 11. UPDATE SINGLE ATTENDANCE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAttendance(int attendanceId, string status, string remarks)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var body = new { status, remarks };
        var result = await api.PutAsync($"/api/Attendance/{attendanceId}", body);

        return result.Success
            ? Json(new { success = true, message = "Attendance updated successfully." })
            : Json(new { success = false, message = $"Failed: {ParseError(result.Error)}" });
    }

    // 12. MARK ALL PRESENT FOR COURSE
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllPresent(int courseId, string date)
    {
        if (!IsLoggedIn()) return Unauthorized();

        var courses = await api.GetAllAsync<CourseApiModel>("/api/Course");
        var course = courses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return Json(new { success = false, message = "Course not found" });

        var students = await api.GetAllAsync<StudentApiModel>("/api/Student");
        var courseStudents = students.Where(s => s.Section == course.Section).ToList();

        var attendances = courseStudents.Select(s => new
        {
            studentId = s.Id,
            status = "Present",
            remarks = "Marked all present by admin"
        }).ToList();

        var body = new { courseId, date, attendances };
        var result = await api.PostAsync<object>("/api/Attendance/bulk", body);

        return result.Success
            ? Json(new { success = true, message = $"Marked {courseStudents.Count} students as present." })
            : Json(new { success = false, message = $"Failed: {ParseError(result.Error)}" });
    }

    // 8. DEBUG
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

    // 9. PRIVATE HELPERS
    private async Task<List<StudentViewModel>> BuildStudentVMs(string? search, string? section)
    {
        var all = await api.GetAllAsync<StudentApiModel>("/api/Student");

        if (!string.IsNullOrWhiteSpace(search))
        {
            all = [.. all.Where(s =>
                s.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.StudentNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                s.Email.Contains(search, StringComparison.OrdinalIgnoreCase))];
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            all = [.. all.Where(s => s.Section == section)];
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
        var all = await api.GetAllAsync<TeacherApiModel>("/api/Teacher");

        if (!string.IsNullOrWhiteSpace(search))
        {
            all = [.. all.Where(t =>
                t.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.LastName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.TeacherNo.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                t.Email.Contains(search, StringComparison.OrdinalIgnoreCase))];
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            bool isActive = status.Equals("Active", StringComparison.OrdinalIgnoreCase);
            all = [.. all.Where(t => t.IsActive == isActive)];
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
                result = await api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/filter?courseId={courseId}&from={f:yyyy-MM-dd}&to={t:yyyy-MM-dd}");
            }
            else
            {
                result = await api.GetAllAsync<AttendanceApiModel>($"/api/Attendance/course/{courseId}");
            }
        }
        else if (DateOnly.TryParse(from, out var fromDate) && DateOnly.TryParse(to, out var toDate))
        {
            var all = await api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
            result = [.. all.Where(a => DateOnly.TryParse(a.Date, out var d) && d >= fromDate && d <= toDate)];
        }
        else
        {
            result = await api.GetAllAsync<AttendanceApiModel>("/api/Attendance");
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
        catch
        {
            // Ignore
        }
        return rawJson.Length > 200 ? rawJson[..200] : rawJson;
    }
}