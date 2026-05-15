using Attendance_Management_System.DTOs;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;
using BCrypt.Net;

namespace Attendance_Management_System.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;

        public TeacherService(
            ITeacherRepository teacherRepository,
            IUserRepository userRepository,
            IPasswordHasher hasher)
        {
            _teacherRepository = teacherRepository;
            _userRepository = userRepository;
            _hasher = hasher;
        }

        public async Task<IEnumerable<TeacherResponseDto>> GetAllAsync()
        {
            var teachers = await _teacherRepository.GetAllWithCoursesAsync();
            var result = new List<TeacherResponseDto>();
            foreach (var t in teachers)
            {
                var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == t.Id);
                var dto = ToDto(t);
                dto.Username = user?.Username ?? "";
                dto.HasAccount = user != null;
                result.Add(dto);
            }
            return result;
        }

        public async Task<TeacherResponseDto?> GetByIdAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdWithCoursesAsync(id);
            if (teacher == null) return null;

            var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == teacher.Id);
            var dto = ToDto(teacher);
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
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _teacherRepository.AddAsync(teacher);
            await _teacherRepository.SaveChangesAsync();
            return ToDto(teacher);
        }

        public async Task<TeacherResponseDto?> CreateWithAccountAsync(CreateTeacherWithAccountDto dto)
        {
            var usernameToUse = dto.Username ?? dto.TeacherNo;
            var exists = await _userRepository.AnyAsync(u => u.Username == usernameToUse);
            if (exists) return null;

            var teacher = new Teacher
            {
                TeacherNo = dto.TeacherNo,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _teacherRepository.AddAsync(teacher);
            await _teacherRepository.SaveChangesAsync();

            var user = new User
            {
                Username = usernameToUse,
                PasswordHash = _hasher.Hash(dto.Password),
                Role = "Teacher",
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = true,   // ✅ Teachers created by admin are auto‑verified
                TeacherId = teacher.Id
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var result = ToDto(teacher);
            result.Username = user.Username;
            result.HasAccount = true;
            return result;
        }

        public async Task<TeacherResponseDto?> UpdateAsync(int id, UpdateTeacherDto dto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return null;

            teacher.TeacherNo = dto.TeacherNo;
            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.Email = dto.Email;

            _teacherRepository.Update(teacher);
            await _teacherRepository.SaveChangesAsync();

            var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == teacher.Id);
            var result = ToDto(teacher);
            result.Username = user?.Username ?? "";
            result.HasAccount = user != null;
            return result;
        }

        public async Task<TeacherResponseDto?> UpdateAccountAsync(int id, UpdateTeacherAccountDto dto)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return null;

            var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == teacher.Id);
            if (user == null) return null;

            if (!string.IsNullOrEmpty(dto.NewUsername))
            {
                var taken = await _userRepository.AnyAsync(u => u.Username == dto.NewUsername && u.Id != user.Id);
                if (taken) return null;
                user.Username = dto.NewUsername;
            }

            if (!string.IsNullOrEmpty(dto.NewPassword))
                user.PasswordHash = _hasher.Hash(dto.NewPassword);

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            var result = ToDto(teacher);
            result.Username = user.Username;
            result.HasAccount = true;
            return result;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return false;

            var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == teacher.Id);
            if (user != null)
            {
                _userRepository.Remove(user);
                await _userRepository.SaveChangesAsync();
            }

            _teacherRepository.Remove(teacher);
            await _teacherRepository.SaveChangesAsync();
            return true;
        }

        public async Task<TeacherResponseDto?> ToggleStatusAsync(int id)
        {
            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null) return null;

            teacher.IsActive = !teacher.IsActive;
            _teacherRepository.Update(teacher);
            await _teacherRepository.SaveChangesAsync();

            var user = await _userRepository.FindAsync(u => u.Role == "Teacher" && u.TeacherId == teacher.Id);
            var result = ToDto(teacher);
            result.Username = user?.Username ?? "";
            result.HasAccount = user != null;
            return result;
        }

        private static TeacherResponseDto ToDto(Teacher t)
        {
            return new TeacherResponseDto
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
}