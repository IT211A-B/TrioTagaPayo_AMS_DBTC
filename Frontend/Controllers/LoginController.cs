using Frontend.Models;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Hardcoded for now — we'll connect DB later
            if (model.Username == "admin" && model.Password == "admin123" && model.UserType == "Admin")
                return RedirectToAction("Attendance", "AdminDashboard");

            if (model.Username == "teacher" && model.Password == "teacher123" && model.UserType == "Teacher")
                return RedirectToAction("Attendance", "TeacherDashboard");

            ModelState.AddModelError("", "Invalid username or password.");
            return View(model);
        }
    }
}