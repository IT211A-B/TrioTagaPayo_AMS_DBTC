using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    /// <summary>
    /// Student-specific repository interface.
    /// </summary>
    public interface IStudentRepository : IGenericRepository<Student>
    {
        // Extensible — add student-specific queries here if needed in the future.
        // e.g. Task<Student?> GetByStudentNoAsync(string studentNo);
    }
}