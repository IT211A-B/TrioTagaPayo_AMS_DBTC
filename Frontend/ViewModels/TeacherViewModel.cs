// ============================================================
// ViewModels/TeacherViewModel.cs
// ============================================================
namespace ASM.ViewModels
{
    public class TeacherViewModel
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Department { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string Status { get; set; } = "";
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

    public class TeachersPageViewModel
    {
        public List<TeacherViewModel> Teachers { get; set; } = new();
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public string? Search { get; set; }
        public string? DeptFilter { get; set; }
        public string? StatusFilter { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
    }
}