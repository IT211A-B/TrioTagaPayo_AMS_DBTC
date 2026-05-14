using System;

namespace Attendance_Management_System.Models
{
    public class Enrollment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
    }
}