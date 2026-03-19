using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;
        public AttendanceService(AppDbContext context) => _context = context;

        public async Task<IEnumerable<AttendanceResponseDto>> GetAllAsync() =>
            await _context.Attendances.Include(a => a.Student).Include(a => a.Course)
                .Select(a => ToDto(a)).ToListAsync();

        public async Task<IEnumerable<AttendanceResponseDto>> GetByCourseAsync(int courseId) =>
            await _context.Attendances.Include(a => a.Student).Include(a => a.Course)
                .Where(a => a.CourseId == courseId).Select(a => ToDto(a)).ToListAsync();

        public async Task<IEnumerable<AttendanceResponseDto>> GetByStudentAsync(int studentId) =>
            await _context.Attendances.Include(a => a.Student).Include(a => a.Course)
                .Where(a => a.StudentId == studentId).Select(a => ToDto(a)).ToListAsync();

        // Filter by course + date range — for Att. Records page
        public async Task<IEnumerable<AttendanceResponseDto>> GetByFilterAsync(int courseId, DateOnly from, DateOnly to) =>
            await _context.Attendances.Include(a => a.Student).Include(a => a.Course)
                .Where(a => a.CourseId == courseId && a.Date >= from && a.Date <= to)
                .OrderByDescending(a => a.Date)
                .Select(a => ToDto(a))
                .ToListAsync();

        public async Task<AttendanceResponseDto?> GetByIdAsync(int id)
        {
            var a = await _context.Attendances.Include(a => a.Student).Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
            return a == null ? null : ToDto(a);
        }

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
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            await _context.Entry(attendance).Reference(a => a.Student).LoadAsync();
            await _context.Entry(attendance).Reference(a => a.Course).LoadAsync();
            return ToDto(attendance);
        }

        // Save all students' attendance in one course at once — for "Save Attendance" button
        public async Task<IEnumerable<AttendanceResponseDto>> BulkCreateAsync(List<CreateAttendanceDto> dtos)
        {
            var attendances = dtos.Select(dto => new Attendance
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                Date = dto.Date,
                Status = dto.Status,
                Remarks = dto.Remarks
            }).ToList();

            _context.Attendances.AddRange(attendances);
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            var ids = attendances.Select(a => a.Id).ToList();
            var saved = await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();

            return saved.Select(ToDto);
        }

        public async Task<AttendanceResponseDto?> UpdateAsync(int id, UpdateAttendanceDto dto)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return null;

            attendance.StudentId = dto.StudentId;
            attendance.CourseId = dto.CourseId;
            attendance.Date = dto.Date;
            attendance.Status = dto.Status;
            attendance.Remarks = dto.Remarks;
            await _context.SaveChangesAsync();
            await _context.Entry(attendance).Reference(a => a.Student).LoadAsync();
            await _context.Entry(attendance).Reference(a => a.Course).LoadAsync();
            return ToDto(attendance);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return false;
            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }

        private static AttendanceResponseDto ToDto(Attendance a) => new()
        {
            Id = a.Id,
            StudentId = a.StudentId,
            StudentName = a.Student != null ? $"{a.Student.LastName}, {a.Student.FirstName}" : "",
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