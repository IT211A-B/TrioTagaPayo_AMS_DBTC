using Attendance_Management_System.Models;

namespace Attendance_Management_System.Interfacess
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllAsync();
        Task<Student?> GetByIdAsync(int id);
        Task<Student> CreateAsync(Student student);
        Task<Student?> UpdateAsync(int id, Student student);
        Task<bool> DeleteAsync(int id);
    }
}