using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    public class TeacherRepository : GenericRepository<Teacher>, ITeacherRepository
    {
        public TeacherRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Teacher>> GetAllWithCoursesAsync() =>
            await _context.Teachers
                .Include(t => t.Courses)
                .ToListAsync();

        public async Task<Teacher?> GetByIdWithCoursesAsync(int id) =>
            await _context.Teachers
                .Include(t => t.Courses)
                .FirstOrDefaultAsync(t => t.Id == id);

        // FindAsync, GetAllAsync, GetByIdAsync, etc. are already inherited from GenericRepository<Teacher>
        // No need to redeclare them
    }
}