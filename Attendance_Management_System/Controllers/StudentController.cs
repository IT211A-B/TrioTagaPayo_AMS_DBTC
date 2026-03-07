using Microsoft.AspNetCore.Mvc;

namespace Attendance_Management_System.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
