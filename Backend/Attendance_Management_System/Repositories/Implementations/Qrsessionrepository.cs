using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    public class QRSessionRepository : IQRSessionRepository
    {
        private readonly AppDbContext _context;

        public QRSessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<QRSession?> GetByTokenAsync(string token) =>
            await _context.QRSessions
                .Include(q => q.Course)
                .Include(q => q.Scans)
                .FirstOrDefaultAsync(q => q.Token == token);

        public async Task<QRSession?> GetByIdWithDetailsAsync(int id) =>
            await _context.QRSessions
                .Include(q => q.Course)
                .Include(q => q.Scans)
                .FirstOrDefaultAsync(q => q.Id == id);

        public async Task<IEnumerable<QRSession>> GetActiveByCourseAsync(int courseId) =>
            await _context.QRSessions
                .Include(q => q.Course)
                .Where(q => q.CourseId == courseId && q.IsActive && q.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

        /// <summary>
        /// Returns true if this student already scanned this session.
        /// This is how we prevent double attendance.
        /// </summary>
        public async Task<bool> AlreadyScannedAsync(int qrSessionId, int studentId) =>
            await _context.QRScans
                .AnyAsync(s => s.QRSessionId == qrSessionId && s.StudentId == studentId);

        public async Task AddScanAsync(QRScan scan) =>
            await _context.QRScans.AddAsync(scan);

        public async Task AddAsync(QRSession session) =>
            await _context.QRSessions.AddAsync(session);

        public void Update(QRSession session) =>
            _context.QRSessions.Update(session);

        public async Task SaveChangesAsync() =>
            await _context.SaveChangesAsync();
    }
}