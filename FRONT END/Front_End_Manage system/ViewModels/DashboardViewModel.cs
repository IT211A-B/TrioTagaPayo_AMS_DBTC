// ============================================================
// ViewModels/DashboardViewModel.cs
// ============================================================
namespace ASM.ViewModels
{
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
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Course { get; set; } = "";
        public string Section { get; set; } = "";
        public string Time { get; set; } = "";
        public string TimeIn { get; set; } = "";
        public string Status { get; set; } = "";
        public string Date { get; set; } = "";
    }

    public class RecentAttendanceViewModel
    {
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string Section { get; set; } = "";
        public string TimeIn { get; set; } = "";
        public string Status { get; set; } = "";
        public string Date { get; set; } = "";
    }
}