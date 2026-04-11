using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    public interface IAttendanceFilterRepository
    {
        Task<IEnumerable<Attendance>> GetByFilterAsync(int courseId, DateOnly from, DateOnly to);
    }
}