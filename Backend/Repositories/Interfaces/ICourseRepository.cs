using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    /// <summary>
    /// Course-specific repository interface.
    /// </summary>
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<IEnumerable<Course>> GetAllWithTeacherAsync();
        Task<Course?> GetByIdWithTeacherAsync(int id);
    }
}