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

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
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
        public int? AttendanceId { get; set; }
        public int? StudentId { get; set; }
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
        public string StudentNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Section { get; set; } = "";
        public string MobileNo { get; set; } = "";
        public string AvatarColor { get; set; } = "#6366f1";
        public int AttendanceRate { get; set; }

        public string FullName => $"{FirstName} {(string.IsNullOrWhiteSpace(MiddleName) ? "" : MiddleName + " ")}{LastName}".Trim();
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper().Trim();
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
        public string TeacherNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int CourseCount { get; set; }
        public string Username { get; set; } = "";
        public bool HasAccount { get; set; }
        public string AvatarColor { get; set; } = "#6366f1";

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Initials => $"{FirstName.FirstOrDefault()}{LastName.FirstOrDefault()}".ToUpper().Trim();
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
        public List<TeacherViewModel> Teachers { get; set; } = new();
        public string? Search { get; set; }
    }

    // ── Attendance ───────────────────────────────────────────

    public class AttendancePageViewModel
    {
        public List<AttendanceEntryViewModel> Records { get; set; } = new();
        public List<CourseViewModel> Courses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }

        public int TotalCount => Records.Count;
        public int PresentCount => Records.Count(r => r.Status == "Present");
        public int AbsentCount => Records.Count(r => r.Status == "Absent");
        public int LateCount => Records.Count(r => r.Status == "Late");
    }

    // ── Teacher Dashboard ViewModels ─────────────────────────────
    public class TeacherDashboardViewModel
    {
        public string TeacherName { get; set; } = string.Empty;
        public int MyCoursesCount { get; set; }
        public int MyStudentsCount { get; set; }
        public int TodayAttendanceRate { get; set; }
        public List<AttendanceEntryViewModel> RecentAttendance { get; set; } = new();
        public List<CourseViewModel> MyCourses { get; set; } = new();
    }

    public class TeacherAttendanceViewModel
    {
        public List<AttendanceEntryViewModel> Records { get; set; } = new();
        public List<CourseViewModel> Courses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    // ── Enrollment ──────────────────────────────────────────────
    public class EnrollmentViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public string Email { get; set; } = "";
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Section { get; set; } = "";
        public int AttendanceRate { get; set; }
        public string Status { get; set; } = "";
        public string EnrollmentDate { get; set; } = "";
    }

    public class EnrollmentPageViewModel
    {
        public List<EnrollmentViewModel> Enrollments { get; set; } = new();
        public int TotalCount { get; set; }
        public string? Search { get; set; }
        public string? CourseFilter { get; set; }
        public string? StatusFilter { get; set; }
    }

    // ── Course Details ViewModels ──────────────────────────────
    // THESE ARE THE NEW CLASSES - ADDED ONCE ONLY

    public class StudentCourseViewModel
    {
        public int StudentId { get; set; }
        public string StudentNo { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Email { get; set; } = "";
        public int AttendanceRate { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public bool IsEnrolled { get; set; }
    }

    public class AttendanceDateSummary
    {
        public string Date { get; set; } = "";
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class CourseDetailsViewModel
    {
        public CourseViewModel Course { get; set; } = new();
        public List<StudentCourseViewModel> Students { get; set; } = new();
        public List<AttendanceDateSummary> AttendanceByDate { get; set; } = new();
        public List<AttendanceEntryViewModel> TodayAttendance { get; set; } = new();
        public int TotalStudents { get; set; }
        public int EnrolledStudents { get; set; }
        public int TotalAttendanceRecords { get; set; }
    }
    // ── Student Dashboard ViewModel ──────────────────────────────
    public class StudentDashboardViewModel
    {
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public int AttendanceRate { get; set; }
        public int PresentCount { get; set; }
        public int LateCount { get; set; }
        public int AbsentCount { get; set; }
        public int TotalClasses { get; set; }
        public List<AttendanceEntryViewModel> RecentAttendance { get; set; } = new();
        public List<CourseViewModel> MyCourses { get; set; } = new();
    }
}