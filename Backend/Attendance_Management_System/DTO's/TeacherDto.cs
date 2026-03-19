namespace Attendance_Management_System.DTOs
{
    public class CreateTeacherDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateTeacherDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class TeacherResponseDto
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CourseCount { get; set; } // for "Courses" column in UI
        public DateTime CreatedAt { get; set; }
    }
}