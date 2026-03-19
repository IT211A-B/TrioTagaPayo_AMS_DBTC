using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        public StudentService(AppDbContext context) => _context = context;

        public async Task<IEnumerable<StudentResponseDto>> GetAllAsync() =>
            await _context.Students.Select(s => ToDto(s)).ToListAsync();

        public async Task<StudentResponseDto?> GetByIdAsync(int id)
        {
            var s = await _context.Students.FindAsync(id);
            return s == null ? null : ToDto(s);
        }

        public async Task<StudentResponseDto> CreateAsync(CreateStudentDto dto)
        {
            var student = new Student
            {
                StudentNo = dto.StudentNo,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
            return ToDto(student);
        }

        public async Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentDto dto)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return null;

            student.StudentNo = dto.StudentNo;
            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Email = dto.Email;
            await _context.SaveChangesAsync();
            return ToDto(student);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return false;
            _context.Students.Remove(student);
            await _context.SaveChangesAsync();
            return true;
        }

        private static StudentResponseDto ToDto(Student s) => new()
        {
            Id = s.Id,
            StudentNo = s.StudentNo,
            FirstName = s.FirstName,
            LastName = s.LastName,
            Email = s.Email,
            CreatedAt = s.CreatedAt
        };
    }
}