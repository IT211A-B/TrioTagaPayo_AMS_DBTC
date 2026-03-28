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

        public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
        {
            var teachers = await _context.Teachers
                .Include(t => t.Courses)
                .ToListAsync();

            var result = new List<TeacherResponseDto>();
            foreach (var t in teachers)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Role == "Teacher"
                        && u.Username == t.TeacherNo);
                var dto = ToDto(t);
                dto.Username = user?.Username ?? "";
                dto.HasAccount = user != null;
                result.Add(dto);
            }
            return result;
        }

        public async Task<TeacherResponseDto?> GetByIdAsync(int id)
        {
            var t = await _context.Teachers.Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (t == null) return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Teacher"
                    && u.Username == t.TeacherNo);
            var dto = ToDto(t);
            dto.Username = user?.Username ?? "";
            dto.HasAccount = user != null;
            return dto;
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

        public async Task<TeacherResponseDto?> CreateWithAccountAsync(
            CreateTeacherWithAccountDto dto)
        {
            // Use TeacherNo as Username so lookup is always consistent
            var usernameToUse = dto.TeacherNo;

            var exists = await _context.Users
                .AnyAsync(u => u.Username == usernameToUse);
            if (exists) return null;

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

            var user = new User
            {
                Username = usernameToUse,
                PasswordHash = AuthService.HashPassword(dto.Password),
                Role = "Teacher",
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = ToDto(teacher);
            result.Username = user.Username;
            result.HasAccount = true;
            return result;
        }

        public async Task<TeacherResponseDto?> UpdateAsync(int id, UpdateTeacherDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            teacher.TeacherNo = dto.TeacherNo;
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.Email = dto.Email;
            await _context.SaveChangesAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Teacher"
                    && u.Username == teacher.TeacherNo);
            var result = ToDto(teacher);
            result.Username = user?.Username ?? "";
            result.HasAccount = user != null;
            return result;
        }

        public async Task<TeacherResponseDto?> UpdateAccountAsync(
            int id, UpdateTeacherAccountDto dto)
        {
            var teacher = await _context.Teachers.Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Teacher"
                    && u.Username == teacher.TeacherNo);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(dto.NewUsername))
            {
                var taken = await _context.Users
                    .AnyAsync(u => u.Username == dto.NewUsername && u.Id != user.Id);
                if (taken) return null;
                user.Username = dto.NewUsername;
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
                user.PasswordHash = AuthService.HashPassword(dto.NewPassword);

            await _context.SaveChangesAsync();

            var result = ToDto(teacher);
            result.Username = user.Username;
            result.HasAccount = true;
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null) return false;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Teacher"
                    && u.Username == teacher.TeacherNo);
            if (user != null) _context.Users.Remove(user);

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<TeacherResponseDto?> ToggleStatusAsync(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return null;

            teacher.IsActive = !teacher.IsActive;
            await _context.SaveChangesAsync();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Role == "Teacher"
                    && u.Username == teacher.TeacherNo);
            var result = ToDto(teacher);
            result.Username = user?.Username ?? "";
            result.HasAccount = user != null;
            return result;
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
