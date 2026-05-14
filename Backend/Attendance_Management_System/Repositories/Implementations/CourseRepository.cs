using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    /// <summary>
    /// Course repository — handles all DB queries for the Course entity.
    /// Inherits generic CRUD from GenericRepository.
    /// </summary>
    public class CourseRepository : GenericRepository<Course>, ICourseRepository
    {
        public CourseRepository(AppDbContext context) : base(context) { }

        /// <summary>Gets all courses with their assigned Teacher included.</summary>
        public async Task<IEnumerable<Course>> GetAllWithTeacherAsync() =>
            await _context.Courses
                .Include(c => c.Teacher)
                .ToListAsync();

        /// <summary>Gets a single course with its assigned Teacher included.</summary>
        public async Task<Course?> GetByIdWithTeacherAsync(int id) =>
            await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.Id == id);
    }
}