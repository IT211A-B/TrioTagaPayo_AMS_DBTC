using Microsoft.AspNetCore.Mvc;
using AMS.Services;
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

        // ── GET /Account/Login ────────────────────────────────
        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }

        // ── POST /Account/Login ───────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            try
            {
                var result = await _api.LoginAsync(username, password);

                if (result == null)
                {
                    ViewBag.Error = "Invalid username or password.";
                    return View();
                }

                // Get role from the API response
                string userRole = result.Role ?? "Admin";

                // Ensure role is stored in session
                HttpContext.Session.SetString("Role", userRole);
                HttpContext.Session.SetString("Username", result.Username);

                // Store teacher info if role is Teacher
                if (userRole.Equals("Teacher", StringComparison.OrdinalIgnoreCase))
                {
                    HttpContext.Session.SetString("TeacherName", result.Username);
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
            catch (Exception ex)
            {
                ViewBag.Error = "An unexpected error occurred. Please try again.";
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