using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers
{
    public class TeacherDashboardController : Controller
    {
        private readonly IHttpClientFactory _http;
        public TeacherDashboardController(IHttpClientFactory http) => _http = http;

        private ApiClient Api() =>
            new(_http, HttpContext.Session.GetString("JwtToken"));

        private bool IsLoggedIn() =>
            !string.IsNullOrEmpty(HttpContext.Session.GetString("JwtToken"));

        private void SetViewBagUser()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username") ?? "Teacher";
            ViewBag.Role = "Teacher";
        }

        public async Task<IActionResult> Attendance()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();
            var courses = await Api().GetAsync<List<CourseDto>>("api/course") ?? new();
            var students = await Api().GetAsync<List<StudentDto>>("api/student") ?? new();
            ViewBag.Courses = courses;
            ViewBag.Students = students;
            ViewBag.MyCourses = courses.Count;
            ViewBag.TotalStudents = students.Count;
            return View();
        }

        public async Task<IActionResult> Courses()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();
            var courses = await Api().GetAsync<List<CourseDto>>("api/course") ?? new();
            ViewBag.Courses = courses;
            return View();
        }

        public async Task<IActionResult> Students()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();
            var students = await Api().GetAsync<List<StudentDto>>("api/student") ?? new();
            var courses = await Api().GetAsync<List<CourseDto>>("api/course") ?? new();
            ViewBag.Students = students;
            ViewBag.Courses = courses;
            return View();
        }

        public async Task<IActionResult> Records()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Login");
            SetViewBagUser();
            var courses = await Api().GetAsync<List<CourseDto>>("api/course") ?? new();
            ViewBag.Courses = courses;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendance(
            [FromBody] List<AttendanceSaveItem> records)
        {
            if (records == null || records.Count == 0)
                return BadRequest("No records.");

            var dtos = records.Select(r => new
            {
                studentId = r.StudentId,
                courseId = r.CourseId,
                date = r.Date,
                status = r.Status,
                remarks = r.Remarks
            }).ToList();

            var result = await Api().PostAsync<object>("api/attendance/bulk", dtos);
            if (result != null)
                return Ok(new { message = "Attendance saved!" });
            else
                return StatusCode(500, "Failed to save attendance.");
        }

        [HttpGet]
        public async Task<IActionResult> GetRecords(int courseId, string from, string to)
        {
            var records = await Api().GetAsync<List<AttendanceDto>>(
                $"api/attendance/filter?courseId={courseId}&from={from}&to={to}") ?? new();
            return Json(records);
        }
    }
}