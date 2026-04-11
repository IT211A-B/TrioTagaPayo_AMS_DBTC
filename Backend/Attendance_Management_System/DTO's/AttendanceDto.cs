using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class CreateAttendanceDto
    {
        [Required(ErrorMessage = "StudentId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a valid ID.")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "CourseId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a valid ID.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Present|Absent|Late)$", ErrorMessage = "Status must be Present, Absent, or Late.")]
        public string Status { get; set; } = "Present";

        [MaxLength(500, ErrorMessage = "Remarks must not exceed 500 characters.")]
        public string Remarks { get; set; } = string.Empty;
    }

    public class UpdateAttendanceDto
    {
        [Required(ErrorMessage = "StudentId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a valid ID.")]
        public int StudentId { get; set; }

        [Required(ErrorMessage = "CourseId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a valid ID.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateOnly Date { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [RegularExpression("^(Present|Absent|Late)$", ErrorMessage = "Status must be Present, Absent, or Late.")]
        public string Status { get; set; } = "Present";

        [MaxLength(500, ErrorMessage = "Remarks must not exceed 500 characters.")]
        public string Remarks { get; set; } = string.Empty;
    }

    public class AttendanceResponseDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNo { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}