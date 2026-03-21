namespace Attendance_Management_System.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "Admin"; // Admin, Teacher
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}