namespace Attendance_Management_System.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = string.Empty; // TCH-001, TCH-002...
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; // for Enable/Disable button
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}