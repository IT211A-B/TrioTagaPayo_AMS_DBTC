using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class CreateCourseDto
    {
        [Required(ErrorMessage = "Course code is required.")]
        [MaxLength(20, ErrorMessage = "Course code must not exceed 20 characters.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required.")]
        [MaxLength(100, ErrorMessage = "Course name must not exceed 100 characters.")]
        public string CourseName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Units must be between 1 and 10.")]
        public int Units { get; set; } = 3;

        [Required(ErrorMessage = "Section is required.")]
        [MaxLength(20, ErrorMessage = "Section must not exceed 20 characters.")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Schedule is required.")]
        [MaxLength(100, ErrorMessage = "Schedule must not exceed 100 characters.")]
        public string Schedule { get; set; } = string.Empty;

        [Required(ErrorMessage = "TeacherId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a valid ID.")]
        public int TeacherId { get; set; }
    }

    public class UpdateCourseDto
    {
        [Required(ErrorMessage = "Course code is required.")]
        [MaxLength(20, ErrorMessage = "Course code must not exceed 20 characters.")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Course name is required.")]
        [MaxLength(100, ErrorMessage = "Course name must not exceed 100 characters.")]
        public string CourseName { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Units must be between 1 and 10.")]
        public int Units { get; set; } = 3;

        [Required(ErrorMessage = "Section is required.")]
        [MaxLength(20, ErrorMessage = "Section must not exceed 20 characters.")]
        public string Section { get; set; } = string.Empty;

        [Required(ErrorMessage = "Schedule is required.")]
        [MaxLength(100, ErrorMessage = "Schedule must not exceed 100 characters.")]
        public string Schedule { get; set; } = string.Empty;

        [Required(ErrorMessage = "TeacherId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a valid ID.")]
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
        public int TeacherId { get; set; }  // ✅ Exposed for ownership verification
        public string TeacherName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}