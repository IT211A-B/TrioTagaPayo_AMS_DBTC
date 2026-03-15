namespace Attendance_Management_System.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}