using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    public interface IAttendanceRepository : IGenericRepository<Attendance>
    {
        Task<IEnumerable<Attendance>> GetAllWithDetailsAsync();
        Task<Attendance?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<Attendance>> GetByCourseAsync(int courseId);
        Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId);
    }
}