using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface IQRService
    {
        /// <summary>
        /// Teacher calls this to generate a new QR session.
        /// Returns a DTO with the Base64 QR image ready to display.
        /// </summary>
        Task<QRSessionResponseDto> GenerateAsync(GenerateQRDto dto);

        /// <summary>
        /// Student calls this after scanning the QR code.
        /// Validates token, checks expiry, checks duplicate scan,
        /// saves attendance, fires email notification.
        /// </summary>
        Task<ScanResultDto> ScanAsync(ScanQRDto dto);

        /// <summary>
        /// Teacher manually deactivates a QR session early.
        /// </summary>
        Task<bool> DeactivateAsync(int sessionId);

        /// <summary>
        /// Get all currently active QR sessions for a course.
        /// </summary>
        Task<IEnumerable<QRSessionResponseDto>> GetActiveSessionsAsync(int courseId);
    }
}