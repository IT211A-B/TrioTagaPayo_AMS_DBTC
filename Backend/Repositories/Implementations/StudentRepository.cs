using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    /// <summary>
    /// Student repository — handles all DB queries for the Student entity.
    /// Inherits generic CRUD from GenericRepository.
    /// </summary>
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(AppDbContext context) : base(context) { }
    }
}