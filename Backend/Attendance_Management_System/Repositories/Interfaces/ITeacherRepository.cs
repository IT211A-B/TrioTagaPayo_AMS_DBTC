using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    public interface ITeacherRepository : IGenericRepository<Teacher>
    {
        Task<IEnumerable<Teacher>> GetAllWithCoursesAsync();
        Task<Teacher?> GetByIdWithCoursesAsync(int id);
        // FindAsync is already inherited from IGenericRepository<Teacher>
    }
}