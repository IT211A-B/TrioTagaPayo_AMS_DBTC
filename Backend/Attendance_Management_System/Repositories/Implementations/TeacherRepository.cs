using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    /// <summary>
    /// Teacher repository — handles all DB queries for the Teacher entity.
    /// Inherits generic CRUD from GenericRepository.
    /// </summary>
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(AppDbContext context) : base(context) { }

        /// <summary>Gets all teachers with their assigned Courses included.</summary>
        public async Task<IEnumerable<Teacher>> GetAllWithCoursesAsync() =>
            await _context.Teachers
                .Include(t => t.Courses)
                .ToListAsync();

        /// <summary>Gets a single teacher with their assigned Courses included.</summary>
        public async Task<Teacher?> GetByIdWithCoursesAsync(int id) =>
            await _context.Teachers
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);
    }
}