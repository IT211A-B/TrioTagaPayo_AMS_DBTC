namespace Attendance_Management_System.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Teacher Teacher { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}