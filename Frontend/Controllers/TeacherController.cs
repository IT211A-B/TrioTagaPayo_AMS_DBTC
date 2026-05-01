// Controllers/TeacherController.cs
// FIX: Was namespace ASM.Controllers — corrected to AMS.Controllers

using Microsoft.AspNetCore.Mvc;

namespace AMS.Controllers
{
    public class TeacherController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}