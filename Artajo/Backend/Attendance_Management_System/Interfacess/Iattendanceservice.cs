using Attendance_Management_System.Models;

namespace Attendance_Management_System.Interfacess
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<IEnumerable<Attendance>> GetByCourseAsync(int courseId);
        Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId);
        Task<Attendance?> GetByIdAsync(int id);
        Task<Attendance> CreateAsync(Attendance attendance);
        Task<Attendance?> UpdateAsync(int id, Attendance attendance);
        Task<bool> DeleteAsync(int id);
    }
}