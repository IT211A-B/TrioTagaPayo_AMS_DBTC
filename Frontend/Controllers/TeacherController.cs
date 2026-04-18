using Microsoft.AspNetCore.Mvc;

namespace ASM.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}