// Services/AttendanceService.cs — FINAL with EmailJS integrated
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAttendanceFilterRepository _filterRepository;
        private readonly IAttendanceBulkRepository _bulkRepository;
        private readonly EmailJSHelper _emailJS;

        public AttendanceService(
            IAttendanceRepository attendanceRepository,
            IAttendanceFilterRepository filterRepository,
            IAttendanceBulkRepository bulkRepository,
            EmailJSHelper emailJS)
        {
            _attendanceRepository = attendanceRepository;
            _filterRepository = filterRepository;
            _bulkRepository = bulkRepository;
            _emailJS = emailJS;
        }

        // ── READ ──────────────────────────────────────────────────────────

        public async Task<IEnumerable<AttendanceResponseDto>> GetAllAsync()
        {
            var records = await _attendanceRepository.GetAllWithDetailsAsync();
            return records.Select(ToDto);
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetByCourseAsync(int courseId)
        {
            var records = await _attendanceRepository.GetByCourseAsync(courseId);
            return records.Select(ToDto);
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetByStudentAsync(int studentId)
        {
            var records = await _attendanceRepository.GetByStudentAsync(studentId);
            return records.Select(ToDto);
        }

        public async Task<IEnumerable<AttendanceResponseDto>> GetByFilterAsync(
            int courseId, DateOnly from, DateOnly to)
        {
            var records = await _filterRepository.GetByFilterAsync(courseId, from, to);
            return records.Select(ToDto);
        }

        public async Task<AttendanceResponseDto?> GetByIdAsync(int id)
        {
            var a = await _attendanceRepository.GetByIdWithDetailsAsync(id);
            return a == null ? null : ToDto(a);
        }

        // ── CREATE (single) ───────────────────────────────────────────────

        public async Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto)
        {
            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                Date = dto.Date,
                Status = dto.Status,
                Remarks = dto.Remarks
            };

            await _attendanceRepository.AddAsync(attendance);
            await _attendanceRepository.SaveChangesAsync();

            // Reload with Student + Course navigation properties included
            var saved = await _attendanceRepository.GetByIdWithDetailsAsync(attendance.Id);

            // ✅ Fire email — does NOT block the API response
            _ = NotifyStudentAsync(saved!);

            return ToDto(saved!);
        }

        // ── BULK CREATE ───────────────────────────────────────────────────

        public async Task<IEnumerable<AttendanceResponseDto>> BulkCreateAsync(
            List<CreateAttendanceDto> dtos)
        {
            var attendances = dtos.Select(dto => new Attendance
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                Date = dto.Date,
                Status = dto.Status,
                Remarks = dto.Remarks
            }).ToList();

            await _attendanceRepository.AddRangeAsync(attendances);
            await _attendanceRepository.SaveChangesAsync();

            var ids = attendances.Select(a => a.Id).ToList();
            var saved = (await _bulkRepository.GetByIdsWithDetailsAsync(ids)).ToList();

            // ✅ Send email to EVERY student in the bulk save
            foreach (var record in saved)
                _ = NotifyStudentAsync(record);

            return saved.Select(ToDto);
        }

        // ── UPDATE / DELETE ───────────────────────────────────────────────

        public async Task<AttendanceResponseDto?> UpdateAsync(int id, UpdateAttendanceDto dto)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null) return null;

            attendance.StudentId = dto.StudentId;
            attendance.CourseId = dto.CourseId;
            attendance.Date = dto.Date;
            attendance.Status = dto.Status;
            attendance.Remarks = dto.Remarks;

            _attendanceRepository.Update(attendance);
            await _attendanceRepository.SaveChangesAsync();

            var updated = await _attendanceRepository.GetByIdWithDetailsAsync(id);
            return ToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _attendanceRepository.GetByIdAsync(id);
            if (attendance == null) return false;

            _attendanceRepository.Remove(attendance);
            await _attendanceRepository.SaveChangesAsync();
            return true;
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────

        /// <summary>
        /// Reads the Student email from the already-loaded navigation property
        /// and calls EmailJSHelper. Never throws — email failure is non-fatal.
        /// </summary>
        private async Task NotifyStudentAsync(Attendance attendance)
        {
            var student = attendance.Student;
            var course = attendance.Course;

            if (student == null || string.IsNullOrWhiteSpace(student.Email))
                return;

            await _emailJS.SendAttendanceNotificationAsync(
                studentEmail: student.Email,
                studentName: $"{student.FirstName} {student.LastName}",
                studentNo: student.StudentNo,
                courseName: course?.CourseName ?? "N/A",
                section: student.Section,
                status: attendance.Status,
                date: attendance.Date,
                timeRecorded: attendance.CreatedAt
            );
        }

        private static AttendanceResponseDto ToDto(Attendance a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student != null
                ? $"{a.Student.LastName}, {a.Student.FirstName}"
                : "",
            StudentNo = a.Student?.StudentNo ?? "",
            CourseId = a.CourseId,
            CourseName = a.Course?.CourseName ?? "",
            Date = a.Date,
            Status = a.Status,
            Remarks = a.Remarks,
            CreatedAt = a.CreatedAt
        };
    }
}