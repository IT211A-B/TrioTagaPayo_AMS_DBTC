namespace Attendance_Management_System.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; } = 3;
        public string Section { get; set; } = string.Empty; // IT-3A, IT-4A...
        public string Schedule { get; set; } = string.Empty; // Mon/Wed 8:00-10:00 AM
        public int TeacherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Teacher Teacher { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}