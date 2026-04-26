// ============================================================
// Controllers/AccountController.cs
// Login calls POST /api/auth/login
// Body: { "username": "...", "password": "..." }
// Response: { "token", "username", "role", "expiration" }
// ============================================================

using Microsoft.AspNetCore.Mvc;
using AMS.Services;

namespace AMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        // GET /Account/Login
        public IActionResult Login()
        {
            // Already logged in → go to dashboard
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Dashboard", "Admin");

            return View();
        }

        // POST /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter your username and password.";
                return View();
            }

            // Calls POST /api/auth/login with { username, password }
            var result = await _api.LoginAsync(username, password);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                ViewBag.Error = "Invalid username or password. Please try again.";
                return View();
            }

            // Save to session — all future API calls will use this token
            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("Username", result.Username);
            HttpContext.Session.SetString("UserRole", result.Role);

            TempData["Success"] = $"Welcome back, {result.Username}!";
            return RedirectToAction("Dashboard", "Admin");
        }

        // GET /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}