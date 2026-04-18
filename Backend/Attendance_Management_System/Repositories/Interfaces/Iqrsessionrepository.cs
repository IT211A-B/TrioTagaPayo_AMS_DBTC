using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    public interface IQRSessionRepository
    {
        /// <summary>Get a session by its token (what the student sends after scanning).</summary>
        Task<QRSession?> GetByTokenAsync(string token);

        /// <summary>Get a session with Course info included.</summary>
        Task<QRSession?> GetByIdWithDetailsAsync(int id);

        /// <summary>Get all active sessions for a course (teacher dashboard).</summary>
        Task<IEnumerable<QRSession>> GetActiveByCourseAsync(int courseId);

        /// <summary>Check if a student already scanned this session.</summary>
        Task<bool> AlreadyScannedAsync(int qrSessionId, int studentId);

        /// <summary>Record that a student scanned the QR.</summary>
        Task AddScanAsync(QRScan scan);

        Task AddAsync(QRSession session);
        Task SaveChangesAsync();
        void Update(QRSession session);
    }
}