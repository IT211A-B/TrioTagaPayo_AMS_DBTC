using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponseDto>> GetAllAsync();
        Task<CourseResponseDto?> GetByIdAsync(int id);
        Task<CourseResponseDto> CreateAsync(CreateCourseDto dto);
        Task<CourseResponseDto?> UpdateAsync(int id, UpdateCourseDto dto);
        Task<bool> DeleteAsync(int id);
    }
}