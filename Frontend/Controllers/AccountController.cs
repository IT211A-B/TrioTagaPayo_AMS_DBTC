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
        // If already logged in, skip to dashboard
        [HttpGet]
        public IActionResult Login()
        {
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
                ViewBag.Error = "Username and password are required.";
                return View();
            }

            // ApiService.LoginAsync stores JwtToken, Username, Role, RefreshToken in session
            var result = await _api.LoginAsync(username, password);

            if (result == null)
            {
                ViewBag.Error = "Invalid username or password.";
                return View();
            }

            return RedirectToAction("Dashboard", "Admin");
        }

        // POST /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Clears session + calls backend logout (best-effort)
            await _api.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }

        // GET /Account/Logout  — support direct link in layout sidebar
        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await _api.LogoutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}