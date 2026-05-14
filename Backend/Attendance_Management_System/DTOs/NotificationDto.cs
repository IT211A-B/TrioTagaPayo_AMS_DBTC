namespace Attendance_Management_System.DTOs
{
    /// <summary>
    /// Payload nga gi-send sa frontend via SignalR
    /// every time mag-save og attendance.
    /// </summary>
    public class AttendanceNotificationDto
    {
        public int AttendanceId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNo { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;  // Present, Absent, Late
        public DateOnly Date { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Source { get; set; } = string.Empty;  // "manual" or "qr_scan"
    }
}