using QRCoder;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class QRService : IQRService
    {
        private readonly IQRSessionRepository _qrRepository;
        private readonly IStudentRepository _studentRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly EmailJSHelper _emailJS;
        private readonly ILogger<QRService> _logger;

        public QRService(
            IQRSessionRepository qrRepository,
            IStudentRepository studentRepository,
            IAttendanceRepository attendanceRepository,
            EmailJSHelper emailJS,
            ILogger<QRService> logger)
        {
            _qrRepository = qrRepository;
            _studentRepository = studentRepository;
            _attendanceRepository = attendanceRepository;
            _emailJS = emailJS;
            _logger = logger;
        }

        // ── GENERATE ─────────────────────────────────────────────────────

        public async Task<QRSessionResponseDto> GenerateAsync(GenerateQRDto dto)
        {
            // 1. Create a unique random token
            var token = Guid.NewGuid().ToString("N"); // e.g. "a1b2c3d4e5f6..."

            // 2. Build the QR session record
            var session = new QRSession
            {
                CourseId = dto.CourseId,
                Token = token,
                Date = dto.Date,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(dto.ValidForMinutes),
                IsActive = true
            };

            await _qrRepository.AddAsync(session);
            await _qrRepository.SaveChangesAsync();

            // 3. Reload with Course details
            var saved = await _qrRepository.GetByIdWithDetailsAsync(session.Id);

            // 4. Generate the QR code image (PNG → Base64)
            var qrBase64 = GenerateQRCodeBase64(token);

            return ToDto(saved!, qrBase64);
        }

        // ── SCAN ──────────────────────────────────────────────────────────

        public async Task<ScanResultDto> ScanAsync(ScanQRDto dto)
        {
            // 1. Find the QR session by token
            var session = await _qrRepository.GetByTokenAsync(dto.Token);

            if (session == null)
                return Fail("Invalid QR code. Please ask your teacher for a new one.");

            // 2. Check if the QR is still active
            if (!session.IsActive)
                return Fail("This QR code has been deactivated by your teacher.");

            // 3. Check if the QR has expired
            if (DateTime.UtcNow > session.ExpiresAt)
                return Fail($"QR code expired. It was only valid until {session.ExpiresAt.ToLocalTime():hh:mm tt}.");

            // 4. Get the student
            var student = await _studentRepository.GetByIdAsync(dto.StudentId);
            if (student == null)
                return Fail("Student not found.");

            // 5. Check if student already scanned this session (prevent cheating)
            var alreadyScanned = await _qrRepository.AlreadyScannedAsync(session.Id, dto.StudentId);
            if (alreadyScanned)
                return Fail($"You already scanned attendance for {session.Course.CourseName} today.");

            // 6. Determine status:
            //    Present  = scanned within first 15 minutes of class (adjust as needed)
            //    Late     = scanned after 15 minutes
            //    You can adjust this logic based on your school's rules
            var minutesSinceCreated = (DateTime.UtcNow - session.CreatedAt).TotalMinutes;
            var status = minutesSinceCreated <= 15 ? "Present" : "Late";

            // 7. Save the Attendance record
            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                CourseId = session.CourseId,
                Date = session.Date,
                Status = status,
                Remarks = "Via QR Code",
                CreatedAt = DateTime.UtcNow
            };

            await _attendanceRepository.AddAsync(attendance);

            // 8. Record the scan (to prevent duplicates next time)
            var scan = new QRScan
            {
                QRSessionId = session.Id,
                StudentId = dto.StudentId,
                ScannedAt = DateTime.UtcNow
            };

            await _qrRepository.AddScanAsync(scan);
            await _qrRepository.SaveChangesAsync();

            // 9. Fire email notification (same as manual attendance — fire and forget)
            if (!string.IsNullOrWhiteSpace(student.Email))
            {
                _ = _emailJS.SendAttendanceNotificationAsync(
                    studentEmail: student.Email,
                    studentName: $"{student.FirstName} {student.LastName}",
                    studentNo: student.StudentNo,
                    courseName: session.Course.CourseName,
                    section: student.Section,
                    status: status,
                    date: session.Date,
                    timeRecorded: attendance.CreatedAt
                );
            }

            _logger.LogInformation(
                "[QR Scan] Student {StudentNo} scanned for {Course} → {Status}",
                student.StudentNo, session.Course.CourseName, status);

            return new ScanResultDto
            {
                Success = true,
                Message = $"Attendance recorded! Status: {status}",
                AttendanceId = attendance.Id,
                StudentName = $"{student.FirstName} {student.LastName}",
                CourseName = session.Course.CourseName,
                Status = status,
                Date = session.Date,
                ScannedAt = attendance.CreatedAt
            };
        }

        // ── DEACTIVATE ────────────────────────────────────────────────────

        public async Task<bool> DeactivateAsync(int sessionId)
        {
            var session = await _qrRepository.GetByIdWithDetailsAsync(sessionId);
            if (session == null) return false;

            session.IsActive = false;
            _qrRepository.Update(session);
            await _qrRepository.SaveChangesAsync();
            return true;
        }

        // ── ACTIVE SESSIONS ───────────────────────────────────────────────

        public async Task<IEnumerable<QRSessionResponseDto>> GetActiveSessionsAsync(int courseId)
        {
            var sessions = await _qrRepository.GetActiveByCourseAsync(courseId);
            return sessions.Select(s => ToDto(s, string.Empty)); // no need to regenerate QR image here
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────

        /// <summary>
        /// Uses QRCoder library to generate a PNG QR code image as Base64 string.
        /// Frontend renders it as: <img src="data:image/png;base64,{result}" />
        /// </summary>
        private static string GenerateQRCodeBase64(string token)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(token, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            var qrBytes = qrCode.GetGraphic(10); // 10 = pixel size per module
            return Convert.ToBase64String(qrBytes);
        }

        private static QRSessionResponseDto ToDto(QRSession s, string qrBase64) => new()
        {
            Id = s.Id,
            CourseId = s.CourseId,
            CourseName = s.Course?.CourseName ?? "",
            Token = s.Token,
            Date = s.Date,
            CreatedAt = s.CreatedAt,
            ExpiresAt = s.ExpiresAt,
            IsActive = s.IsActive,
            QRCodeBase64 = qrBase64,
            MinutesRemaining = s.IsActive
                ? Math.Max(0, (int)(s.ExpiresAt - DateTime.UtcNow).TotalMinutes)
                : 0
        };

        private static ScanResultDto Fail(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}