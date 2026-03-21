using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;
        public CourseService(AppDbContext context) => _context = context;

        public async Task<IEnumerable<CourseResponseDto>> GetAllAsync() =>
            await _context.Courses
                .Include(c => c.Teacher)
                .Select(c => ToDto(c))
                .ToListAsync();

        public async Task<CourseResponseDto?> GetByIdAsync(int id)
        {
            var c = await _context.Courses.Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);
            return c == null ? null : ToDto(c);
        }

        public async Task<CourseResponseDto> CreateAsync(CreateCourseDto dto)
        {
            var course = new Course
            {
                CourseCode = dto.CourseCode,
                CourseName = dto.CourseName,
                Units = dto.Units,
                Section = dto.Section,
                Schedule = dto.Schedule,
                TeacherId = dto.TeacherId
            };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            await _context.Entry(course).Reference(c => c.Teacher).LoadAsync();
            return ToDto(course);
        }

        public async Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseDto dto)
        {
            var course = await _context.Courses.Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return null;

            course.CourseCode = dto.CourseCode;
            course.CourseName = dto.CourseName;
            course.Units = dto.Units;
            course.Section = dto.Section;
            course.Schedule = dto.Schedule;
            course.TeacherId = dto.TeacherId;
            await _context.SaveChangesAsync();
            await _context.Entry(course).Reference(c => c.Teacher).LoadAsync();
            return ToDto(course);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;
            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();
            return true;
        }

        private static CourseResponseDto ToDto(Course c) => new()
        {
            Id = c.Id,
            CourseCode = c.CourseCode,
            CourseName = c.CourseName,
            Units = c.Units,
            Section = c.Section,
            Schedule = c.Schedule,
            TeacherId = c.TeacherId,
            TeacherName = c.Teacher != null
                ? $"{c.Teacher.FirstName} {c.Teacher.LastName}" : "",
            CreatedAt = c.CreatedAt
        };
    }
}