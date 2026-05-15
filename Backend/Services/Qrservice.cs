using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Helpers;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace Attendance_Management_System.Services
{
    public class QRService : IQRService
    {
        private readonly DBCONTEXT.AppDbContext _context;
        private readonly ICourseService _courseService;
        private readonly EmailJSHelper _emailJS;

        public QRService(
            DBCONTEXT.AppDbContext context,
            ICourseService courseService,
            EmailJSHelper emailJS)
        {
            _context = context;
            _courseService = courseService;
            _emailJS = emailJS;
        }

        public async Task<QRSessionResponseDto> GenerateAsync(GenerateQRDto dto)
        {
            var token = Guid.NewGuid().ToString();
            var createdAt = DateTime.UtcNow;
            var expiresAt = createdAt.AddMinutes(dto.ValidForMinutes);

            var session = new QRSession
            {
                Token = token,
                CourseId = dto.CourseId,
                Date = dto.Date,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            await _context.QRSessions.AddAsync(session);
            await _context.SaveChangesAsync();

            var course = await _courseService.GetByIdAsync(dto.CourseId);
            var qrCodeBase64 = GenerateQRCodeBase64(token);

            return new QRSessionResponseDto
            {
                Id = session.Id,
                CourseId = dto.CourseId,
                CourseName = course?.CourseName ?? "Unknown",
                Token = token,
                Date = dto.Date,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt,
                IsActive = true,
                QRCodeBase64 = qrCodeBase64,
                MinutesRemaining = dto.ValidForMinutes
            };
        }

        public async Task<ScanResultDto> ScanAsync(ScanQRDto dto)
        {
            var session = await _context.QRSessions
                .Include(q => q.Course)
                .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(q => q.Token == dto.Token && q.IsActive);

            if (session == null)
            {
                return new ScanResultDto
                {
                    Success = false,
                    Message = "Invalid or expired QR code."
                };
            }

            if (session.ExpiresAt < DateTime.UtcNow)
            {
                session.IsActive = false;
                await _context.SaveChangesAsync();
                return new ScanResultDto
                {
                    Success = false,
                    Message = "QR code has expired."
                };
            }

            var alreadyScanned = await _context.QRScans
                .AnyAsync(s => s.QRSessionId == session.Id && s.StudentId == dto.StudentId);

            if (alreadyScanned)
            {
                return new ScanResultDto
                {
                    Success = false,
                    Message = "You have already marked attendance for this session."
                };
            }

            var lateThreshold = session.ExpiresAt.AddMinutes(-5);
            var status = DateTime.UtcNow <= lateThreshold ? "Present" : "Late";

            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
            {
                return new ScanResultDto
                {
                    Success = false,
                    Message = "Student not found."
                };
            }

            // Record the scan
            var scan = new QRScan
            {
                QRSessionId = session.Id,
                StudentId = dto.StudentId,
                ScannedAt = DateTime.UtcNow
            };
            await _context.QRScans.AddAsync(scan);
            await _context.SaveChangesAsync();

            // Create attendance record
            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                CourseId = session.CourseId,
                Date = session.Date,
                Status = status,
                Remarks = $"Scanned via QR code - Session {session.Token}",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Attendances.AddAsync(attendance);
            await _context.SaveChangesAsync();

            // Send email notification (non-blocking)
            _ = SendEmailNotificationAsync(student, session.Course, status, session.Date, attendance.CreatedAt);

            return new ScanResultDto
            {
                Success = true,
                Message = $"Attendance marked as {status}",
                AttendanceId = attendance.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                CourseName = session.Course?.CourseName ?? "Unknown",
                Status = status,
                Date = session.Date,
                ScannedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> DeactivateAsync(int sessionId)
        {
            var session = await _context.QRSessions.FindAsync(sessionId);
            if (session == null) return false;

            session.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<QRSessionResponseDto>> GetActiveSessionsAsync(int courseId)
        {
            var query = _context.QRSessions
                .Include(q => q.Course)
                .Where(q => q.IsActive && q.ExpiresAt > DateTime.UtcNow);

            if (courseId > 0)
            {
                query = query.Where(q => q.CourseId == courseId);
            }

            var sessions = await query
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return sessions.Select(s => new QRSessionResponseDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                CourseName = s.Course?.CourseName ?? "Unknown",
                Token = s.Token,
                Date = s.Date,
                CreatedAt = s.CreatedAt,
                ExpiresAt = s.ExpiresAt,
                IsActive = s.IsActive,
                MinutesRemaining = (int)Math.Max(0, (s.ExpiresAt - DateTime.UtcNow).TotalMinutes)
            }).ToList();
        }

        public async Task<QRSessionResponseDto?> GetSessionByIdAsync(int sessionId)
        {
            var session = await _context.QRSessions
                .Include(q => q.Course)
                .FirstOrDefaultAsync(q => q.Id == sessionId);

            if (session == null) return null;

            return new QRSessionResponseDto
            {
                Id = session.Id,
                CourseId = session.CourseId,
                CourseName = session.Course?.CourseName ?? "Unknown",
                Token = session.Token,
                Date = session.Date,
                CreatedAt = session.CreatedAt,
                ExpiresAt = session.ExpiresAt,
                IsActive = session.IsActive,
                MinutesRemaining = (int)Math.Max(0, (session.ExpiresAt - DateTime.UtcNow).TotalMinutes)
            };
        }

        private string GenerateQRCodeBase64(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }

        private async Task SendEmailNotificationAsync(Student student, Course course, string status, DateOnly date, DateTime scannedAt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(student.Email))
                {
                    Console.WriteLine($"[EMAIL] No email for student {student.StudentNo}");
                    return;
                }

                await _emailJS.SendAttendanceNotificationAsync(
                    studentEmail: student.Email,
                    studentName: $"{student.FirstName} {student.LastName}",
                    studentNo: student.StudentNo,
                    courseName: course?.CourseName ?? "Unknown",
                    section: student.Section,
                    status: status,
                    date: date,
                    timeRecorded: scannedAt
                );

                Console.WriteLine($"[EMAIL] Sent to {student.Email} for QR scan - Status: {status}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send to {student.Email}: {ex.Message}");
            }
        }
    }
}