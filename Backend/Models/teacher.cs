using System;
using System.Collections.Generic;

namespace Attendance_Management_System.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}