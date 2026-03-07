using Attendance_Management_System.Models;

namespace Attendance_Management_System.Interfacess
{
    public interface ICourseService
    {
        Task<IEnumerable<Course>> GetAllAsync();
        Task<Course?> GetByIdAsync(int id);
        Task<Course> CreateAsync(Course course);
        Task<Course?> UpdateAsync(int id, Course course);
        Task<bool> DeleteAsync(int id);
    }
}