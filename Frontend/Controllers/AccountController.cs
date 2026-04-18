using ASM.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken")))
                return RedirectToAction("Dashboard", "Admin");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Please enter your username and password.";
                return View();
            }

            var result = await _api.LoginAsync(username, password);

            if (result == null || string.IsNullOrEmpty(result.Token))
            {
                TempData["Error"] = "Invalid credentials. Please try again.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);
            HttpContext.Session.SetString("UserRole", result.Role ?? "Admin");
            TempData["Success"] = "Welcome back!";
            return RedirectToAction("Dashboard", "Admin");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}