// ============================================================
// ViewModels/ViewModels.cs
// ============================================================

namespace AMS.ViewModels
{
    // ── Shared ───────────────────────────────────────────────

    public class PaginationViewModel
    {
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string ActionName { get; set; } = "";
        public string ControllerName { get; set; } = "Admin";
        public string? Search { get; set; }
        public string? Filter { get; set; }
    }

    public class EmptyStateViewModel
    {
        public string Icon { get; set; } = "◍";
        public string Title { get; set; } = "Nothing here yet";
        public string Message { get; set; } = "No records found.";
    }

    // ── Dashboard ────────────────────────────────────────────

    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int AttendanceRate { get; set; }
        public List<AttendanceEntryViewModel> RecentAttendance { get; set; } = new();
    }

    public class AttendanceEntryViewModel
    {
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string Date { get; set; } = "";
        public string Status { get; set; } = "";
        public string Remarks { get; set; } = "";
    }

    // ── Students ─────────────────────────────────────────────

    public class StudentViewModel
    {
        public int DbId { get; set; }
        public string StudentNo { get; set; } = "";   // e.g. 2026-0001
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Section { get; set; } = "";
        public string MobileNo { get; set; } = "";
        public string AvatarColor { get; set; } = "";

        // Attendance computed from records (0 until real data available)
        public int AttendanceRate { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
    }

    public class StudentsPageViewModel
    {
        public List<StudentViewModel> Students { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public string? SectionFilter { get; set; }
    }

    // ── Teachers ─────────────────────────────────────────────

    public class TeacherViewModel
    {
        public int DbId { get; set; }
        public string TeacherNo { get; set; } = "";   // e.g. TCH-001
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int CourseCount { get; set; }
        public string Username { get; set; } = "";
        public bool HasAccount { get; set; }
        public string AvatarColor { get; set; } = "";

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper();
        public string Status => IsActive ? "Active" : "Inactive";
    }

    public class TeachersPageViewModel
    {
        public List<TeacherViewModel> Teachers { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
    }

    // ── Courses ──────────────────────────────────────────────

    public class CourseViewModel
    {
        public int DbId { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Units { get; set; }
        public string Section { get; set; } = "";
        public string Schedule { get; set; } = "";
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";
    }

    public class CoursesPageViewModel
    {
        public List<CourseViewModel> Courses { get; set; } = new();
        public List<TeacherViewModel> Teachers { get; set; } = new(); // for dropdown in Add modal
        public string? Search { get; set; }
    }
}