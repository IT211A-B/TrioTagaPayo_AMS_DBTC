namespace AMS.ViewModels
{
    public class RecordAttendanceViewModel
    {
        public int CourseId { get; set; }
        public string Date { get; set; } = "";
        public bool RequiresLogin { get; set; }
    }

    public class RecordAttendanceRequest
    {
        public int CourseId { get; set; }
        public string Date { get; set; } = "";
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
    }
}