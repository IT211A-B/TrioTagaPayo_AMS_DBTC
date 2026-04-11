using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    /// <summary>
    /// Teacher-specific repository interface.
    /// </summary>
    public interface ITeacherRepository : IGenericRepository<Teacher>
    {
        Task<IEnumerable<Teacher>> GetAllWithCoursesAsync();
        Task<Teacher?> GetByIdWithCoursesAsync(int id);
    }
}