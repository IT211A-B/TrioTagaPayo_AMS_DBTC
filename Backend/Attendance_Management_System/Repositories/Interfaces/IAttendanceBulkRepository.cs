using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    public interface IAttendanceBulkRepository
    {
        Task<IEnumerable<Attendance>> GetByIdsWithDetailsAsync(List<int> ids);
    }
}