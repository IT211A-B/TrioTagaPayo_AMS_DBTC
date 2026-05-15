using System;
using System.Collections.Generic;

namespace Attendance_Management_System.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; } = 3;
        public string Section { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Teacher Teacher { get; set; } = null!;
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    }
}