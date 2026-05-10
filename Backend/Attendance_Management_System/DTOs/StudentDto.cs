using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class CreateStudentDto
    {
        [Required(ErrorMessage = "Student number is required.")]
        [MaxLength(20, ErrorMessage = "Student number must not exceed 20 characters.")]
        public string StudentNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Middle name must not exceed 50 characters.")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required.")]
        [MaxLength(20, ErrorMessage = "Section must not exceed 20 characters.")]
        public string Section { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(20, ErrorMessage = "Mobile number must not exceed 20 characters.")]
        public string MobileNo { get; set; } = string.Empty;
    }

    public class UpdateStudentDto
    {
        [Required(ErrorMessage = "Student number is required.")]
        [MaxLength(20, ErrorMessage = "Student number must not exceed 20 characters.")]
        public string StudentNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50, ErrorMessage = "First name must not exceed 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(50, ErrorMessage = "Middle name must not exceed 50 characters.")]
        public string MiddleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50, ErrorMessage = "Last name must not exceed 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Section is required.")]
        [MaxLength(20, ErrorMessage = "Section must not exceed 20 characters.")]
        public string Section { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(20, ErrorMessage = "Mobile number must not exceed 20 characters.")]
        public string MobileNo { get; set; } = string.Empty;
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}