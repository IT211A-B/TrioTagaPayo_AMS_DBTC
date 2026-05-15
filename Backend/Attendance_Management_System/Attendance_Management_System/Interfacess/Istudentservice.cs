using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface IStudentService
    {
        Task<IEnumerable<StudentResponseDto>> GetAllAsync();
        Task<StudentResponseDto?> GetByIdAsync(int id);
        Task<StudentResponseDto> CreateAsync(CreateStudentDto dto);
        Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}