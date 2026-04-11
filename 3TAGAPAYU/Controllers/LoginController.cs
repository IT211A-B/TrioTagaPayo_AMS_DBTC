using Microsoft.AspNetCore.Mvc;
using _3TAGAPAYU.Models;

namespace _3TAGAPAYU.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Validate credentials
            if (model.Email == "admin@example.com" && model.Password == "password123")
            {
                // TODO: Implement proper authentication here
                // Set authentication cookie/JWT token
                return RedirectToAction("Index", "Attendance");
            }

            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        public IActionResult Logout()
        {
            // TODO: Clear authentication
            return RedirectToAction("Index");
        }
    }
}