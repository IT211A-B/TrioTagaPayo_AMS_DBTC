using Frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace Frontend.Controllers
{
    public class LoginController : Controller
    {
        private readonly IHttpClientFactory _http;
        public LoginController(IHttpClientFactory http) => _http = http;

        [HttpGet]
        public IActionResult Login() => View(new LoginViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var client = _http.CreateClient("BackendApi");
                var payload = JsonSerializer.Serialize(new
                {
                    Username = model.Username,
                    Password = model.Password
                });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                {
                    ModelState.AddModelError("", "Unexpected response from server.");
                    return View(model);
                }

                HttpContext.Session.SetString("JwtToken", result.Token);
                HttpContext.Session.SetString("Role", result.Role);
                HttpContext.Session.SetString("Username", result.Username);

                if (result.Role == "Admin" && model.UserType == "Admin")
                    return RedirectToAction("Attendance", "AdminDashboard");

                if (result.Role == "Teacher" && model.UserType == "Teacher")
                    return RedirectToAction("Attendance", "TeacherDashboard");

                ModelState.AddModelError("", "Role mismatch. Select the correct user type.");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Cannot connect to server: {ex.Message}");
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}