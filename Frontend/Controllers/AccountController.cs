using Microsoft.AspNetCore.Mvc;
using AMS.Services;
using AMS.Models;
using System.Text.Json;

namespace AMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        // ── GET /Account/Login (Admin/Teacher) ─────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }

        // ── POST /Account/Login (Admin/Teacher) ────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username/Email and password are required.";
                return View();
            }

            try
            {
                // Check if username contains @ (it's an email)
                string loginUsername = username;
                if (username.Contains("@"))
                {
                    // Try to find teacher by email and get their actual username
                    var teacherList = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");
                    var teacherByEmail = teacherList.FirstOrDefault(t =>
                        t.Email?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                    if (teacherByEmail != null)
                    {
                        loginUsername = teacherByEmail.Username ?? teacherByEmail.Email;
                    }
                    else
                    {
                        // Check if student by email
                        var studentList = await _api.GetAllAsync<StudentApiModel>("/api/Student");
                        var studentByEmail = studentList.FirstOrDefault(s =>
                            s.Email?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                        if (studentByEmail != null)
                        {
                            loginUsername = studentByEmail.StudentNo;
                        }
                    }
                }

                var result = await _api.LoginAsync(loginUsername, password);

                if (result == null)
                {
                    ViewBag.Error = "Invalid username/email or password.";
                    return View();
                }

                string userRole = result.Role ?? "Admin";

                HttpContext.Session.SetString("Role", userRole);
                HttpContext.Session.SetString("Username", result.Username);

                if (userRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    var teacherList = await _api.GetAllAsync<TeacherApiModel>("/api/Teacher");

                    var foundTeacher = teacherList.FirstOrDefault(t =>
                        t.Username?.Equals(loginUsername, StringComparison.OrdinalIgnoreCase) == true ||
                        t.Email?.Equals(username, StringComparison.OrdinalIgnoreCase) == true);

                    if (foundTeacher != null)
                    {
                        HttpContext.Session.SetString("TeacherId", foundTeacher.Id.ToString());
                        HttpContext.Session.SetString("TeacherName", $"{foundTeacher.FirstName} {foundTeacher.LastName}");
                        HttpContext.Session.SetString("TeacherEmail", foundTeacher.Email);
                    }
                }

                // Store student info if role is Student
                if (userRole.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    var studentList = await _api.GetAllAsync<StudentApiModel>("/api/Student");
                    var foundStudent = studentList.FirstOrDefault(s =>
                        s.StudentNo == loginUsername || s.Email == username);
                    if (foundStudent != null)
                    {
                        HttpContext.Session.SetString("StudentId", foundStudent.Id.ToString());
                        HttpContext.Session.SetString("StudentNo", foundStudent.StudentNo);
                        HttpContext.Session.SetString("StudentName", $"{foundStudent.FirstName} {foundStudent.LastName}");
                    }
                }

                // Redirect based on user role
                if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else if (userRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Teacher");
                }
                else if (userRole.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Student");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "Server is waking up from sleep. Please wait 30 seconds and try again.";
                ViewBag.IsWakingUp = true;
                return View();
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to the server. Please check your connection and try again.";
                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "An unexpected error occurred. Please try again.";
                return View();
            }
        }

        // ── GET /Account/StudentLogin (Student only) ───────────
        [HttpGet]
        public IActionResult StudentLogin()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
            {
                var role = HttpContext.Session.GetString("Role");
                if (role == "Student") return RedirectToAction("Dashboard", "Student");
                return RedirectToAction("Dashboard", "Admin");
            }
            return View();
        }

        // ── POST /Account/StudentLogin (Student only) ──────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StudentLogin(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Student ID and password are required.";
                return View();
            }

            try
            {
                var result = await _api.LoginAsync(username, password);

                if (result == null)
                {
                    ViewBag.Error = "Invalid Student ID or password.";
                    return View();
                }

                // Verify this is a student account
                if (!result.Role.Equals("Student", StringComparison.OrdinalIgnoreCase))
                {
                    ViewBag.Error = "This account is not a student account. Please use the teacher/admin login.";
                    return View();
                }

                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("Username", result.Username);

                // Get student details
                var students = await _api.GetAllAsync<StudentApiModel>("/api/Student");
                var student = students.FirstOrDefault(s => s.StudentNo == username || s.Email == username);

                if (student != null)
                {
                    HttpContext.Session.SetString("StudentId", student.Id.ToString());
                    HttpContext.Session.SetString("StudentNo", student.StudentNo);
                    HttpContext.Session.SetString("StudentName", $"{student.FirstName} {student.LastName}");
                }

                return RedirectToAction("Dashboard", "Student");
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "Server is waking up. Please wait and try again.";
                return View();
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to the server. Please check your connection.";
                return View();
            }
            catch (Exception)
            {
                ViewBag.Error = "Login failed. Please try again.";
                return View();
            }
        }

        // ── POST /Account/Logout ──────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _api.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }

        // ── GET /Account/Logout — direct link support ─────────
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _api.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}