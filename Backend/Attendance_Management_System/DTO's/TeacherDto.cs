using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class CreateTeacherDto
    {
        [Required(ErrorMessage = "Teacher number is required.")]
        [MaxLength(20, ErrorMessage = "Teacher number must not exceed 20 characters.")]
        public string TeacherNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;
    }

    public class CreateTeacherWithAccountDto
    {
        [Required(ErrorMessage = "Teacher number is required.")]
        [MaxLength(20, ErrorMessage = "Teacher number must not exceed 20 characters.")]
        public string TeacherNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateTeacherDto
    {
        [Required(ErrorMessage = "Teacher number is required.")]
        [MaxLength(20, ErrorMessage = "Teacher number must not exceed 20 characters.")]
        public string TeacherNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateTeacherAccountDto
    {
        [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters.")]
        public string? NewUsername { get; set; }

        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
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