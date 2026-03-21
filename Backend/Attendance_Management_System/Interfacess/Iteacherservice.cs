using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherResponseDto>> GetAllAsync();
        Task<TeacherResponseDto?> GetByIdAsync(int id);
        Task<TeacherResponseDto> CreateAsync(CreateTeacherDto dto);
        Task<TeacherResponseDto?> CreateWithAccountAsync(CreateTeacherWithAccountDto dto);
        Task<TeacherResponseDto?> UpdateAsync(int id, UpdateTeacherDto dto);
        Task<TeacherResponseDto?> UpdateAccountAsync(int id, UpdateTeacherAccountDto dto);
        Task<bool> DeleteAsync(int id);
        Task<TeacherResponseDto?> ToggleStatusAsync(int id);
    }
}