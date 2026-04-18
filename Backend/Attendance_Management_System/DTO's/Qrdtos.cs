using System.ComponentModel.DataAnnotations;

namespace Attendance_Management_System.DTOs
{
    /// <summary>
    /// Teacher sends this to generate a new QR session.
    /// </summary>
    public class GenerateQRDto
    {
        [Required(ErrorMessage = "CourseId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "CourseId must be a valid ID.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateOnly Date { get; set; }

        /// <summary>
        /// How many minutes the QR code is valid. Default 10, max 60.
        /// </summary>
        [Range(1, 60, ErrorMessage = "ValidForMinutes must be between 1 and 60.")]
        public int ValidForMinutes { get; set; } = 10;
    }

    /// <summary>
    /// Student sends this after scanning the QR code.
    /// </summary>
    public class ScanQRDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "StudentId is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "StudentId must be a valid ID.")]
        public int StudentId { get; set; }
    }

    /// <summary>
    /// Returned to the teacher after generating a QR code.
    /// The QRCodeBase64 is a PNG image encoded as Base64 — 
    /// frontend renders it as: <img src="data:image/png;base64,{QRCodeBase64}" />
    /// </summary>
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

        /// <summary>
        /// Ready-to-display Base64 PNG of the QR code.
        /// </summary>
        public string QRCodeBase64 { get; set; } = string.Empty;

        /// <summary>
        /// How many minutes are left before expiry.
        /// </summary>
        public int MinutesRemaining { get; set; }
    }

    /// <summary>
    /// Returned to the student after a successful scan.
    /// </summary>
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