using Attendance_Management_System.DTOs;

namespace Attendance_Management_System.Interfacess
{
    public interface IAttendanceService
    {
        Task<IEnumerable<AttendanceResponseDto>> GetAllAsync();
        Task<IEnumerable<AttendanceResponseDto>> GetByCourseAsync(int courseId);
        Task<IEnumerable<AttendanceResponseDto>> GetByStudentAsync(int studentId);
        Task<IEnumerable<AttendanceResponseDto>> GetByFilterAsync(int courseId, DateOnly from, DateOnly to); // new
        Task<AttendanceResponseDto?> GetByIdAsync(int id);
        Task<AttendanceResponseDto> CreateAsync(CreateAttendanceDto dto);
        Task<IEnumerable<AttendanceResponseDto>> BulkCreateAsync(List<CreateAttendanceDto> dtos); // new
        Task<AttendanceResponseDto?> UpdateAsync(int id, UpdateAttendanceDto dto);
        Task<bool> DeleteAsync(int id);
    }
}