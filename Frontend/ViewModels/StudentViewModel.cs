// ============================================================
// ViewModels/StudentViewModel.cs
// ============================================================
namespace ASM.ViewModels
{
    public class StudentViewModel
    {
        public string StudentId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string YearLevel { get; set; } = "";
        public string Section { get; set; } = "";
        public int TotalCourses { get; set; }
        public string Attendance { get; set; } = "";
        public string Status { get; set; } = "";
        public string AttendanceRate { get; set; } = "";
        public string AvatarColor { get; set; } = "";

        public string Initials
        {
            get
            {
                var parts = (FirstName + " " + LastName).Trim().Split(' ');
                return parts.Length >= 2
                    ? $"{parts[0][0]}{parts[^1][0]}".ToUpper()
                    : parts[0][0].ToString().ToUpper();
            }
        }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class StudentsPageViewModel
    {
        public List<StudentViewModel> Students { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public string? Search { get; set; }
        public string? StatusFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}