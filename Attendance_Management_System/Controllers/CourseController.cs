using Microsoft.AspNetCore.Mvc;

namespace Attendance_Management_System.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
