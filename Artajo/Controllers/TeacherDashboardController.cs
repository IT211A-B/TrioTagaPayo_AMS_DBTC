using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class TeacherDashboardController : Controller
    {
        public IActionResult Attendance() => View();
        public IActionResult Courses() => View();
        public IActionResult Students() => View();
        public IActionResult Records() => View();
    }
}