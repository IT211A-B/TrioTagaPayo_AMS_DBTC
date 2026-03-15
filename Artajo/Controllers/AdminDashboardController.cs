using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class AdminDashboardController : Controller
    {
        public IActionResult Attendance() => View();
        public IActionResult Courses() => View();
        public IActionResult Students() => View();
        public IActionResult Teachers() => View();
    }
}