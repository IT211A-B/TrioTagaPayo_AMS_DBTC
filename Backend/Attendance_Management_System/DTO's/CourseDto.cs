namespace Attendance_Management_System.DTOs
{
    public class CreateCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; } = 3;
        public string Section { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public int TeacherId { get; set; }
    }

    public class UpdateCourseDto
    {
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; } = 3;
        public string Section { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public int TeacherId { get; set; }
    }

    public class CourseResponseDto
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public int Units { get; set; }
        public string Section { get; set; } = string.Empty;
        public string Schedule { get; set; } = string.Empty;
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public int StudentCount { get; set; } // for "Students" column in Teacher view
        public DateTime CreatedAt { get; set; }
    }
}