using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly AppDbContext _context;
        public TeacherService(AppDbContext context) => _context = context;

        public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync() =>
            await _context.Teachers
                .Include(t => t.Courses)
                .Select(t => ToDto(t))
                .ToListAsync();

        public async Task<TeacherResponseDto?> GetByIdAsync(int id)
        {
            var t = await _context.Teachers.Include(t => t.Courses).FirstOrDefaultAsync(t => t.Id == id);
            return t == null ? null : ToDto(t);
        }

        public async Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto)
        {
            var teacher = new Teacher
            {
                TeacherNo = dto.TeacherNo,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();
            return ToDto(teacher);
        }

        public async Task<TeacherResponseDto?> UpdateAsync(int id, UpdateTeacherDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.Courses).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            teacher.TeacherNo = dto.TeacherNo;
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.Email = dto.Email;
            await _context.SaveChangesAsync();
            return ToDto(teacher);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return false;
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TeacherResponseDto?> ToggleStatusAsync(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.Courses).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            teacher.IsActive = !teacher.IsActive; // flip true↔false
            await _context.SaveChangesAsync();
            return ToDto(teacher);
        }

        private static TeacherResponseDto ToDto(Teacher t) => new()
        {
            Id = t.Id,
            TeacherNo = t.TeacherNo,
            FirstName = t.FirstName,
            LastName = t.LastName,
            Email = t.Email,
            IsActive = t.IsActive,
            CourseCount = t.Courses?.Count ?? 0,
            CreatedAt = t.CreatedAt
        };
    }
}