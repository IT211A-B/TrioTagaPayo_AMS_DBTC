namespace Attendance_Management_System.DTOs
{
    public class CreateTeacherDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class CreateTeacherWithAccountDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateTeacherDto
    {
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateTeacherAccountDto
    {
        public string? NewUsername { get; set; }
        public string? NewPassword { get; set; }
    }

    public class TeacherResponseDto
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int CourseCount { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool HasAccount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}