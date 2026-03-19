namespace Attendance_Management_System.DTOs
{
    public class CreateAttendanceDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = string.Empty;
    }

    public class UpdateAttendanceDto
    {
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = string.Empty;
    }

    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNo { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}