using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    public class GenerateQRDto
    {
        [Required(ErrorMessage = "CourseId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a valid ID.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateOnly Date { get; set; }

        [Range(1, 60, ErrorMessage = "ValidForMinutes must be between 1 and 60.")]
        public int ValidForMinutes { get; set; } = 10;
    }

    public class ScanQRDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "StudentId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a valid ID.")]
        public int StudentId { get; set; }
    }

    public class QRSessionResponseDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public string QRCodeBase64 { get; set; } = string.Empty;
        public int MinutesRemaining { get; set; }
    }

    public class ScanResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? AttendanceId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public DateTime ScannedAt { get; set; }
    }
}