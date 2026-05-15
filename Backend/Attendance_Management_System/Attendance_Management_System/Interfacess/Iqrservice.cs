using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface IQRService
    {
        Task<QRSessionResponseDto> GenerateAsync(GenerateQRDto dto);
        Task<ScanResultDto> ScanAsync(ScanQRDto dto);
        Task<bool> DeactivateAsync(int sessionId);
        Task<List<QRSessionResponseDto>> GetActiveSessionsAsync(int courseId);
        Task<QRSessionResponseDto?> GetSessionByIdAsync(int sessionId);
    }
}