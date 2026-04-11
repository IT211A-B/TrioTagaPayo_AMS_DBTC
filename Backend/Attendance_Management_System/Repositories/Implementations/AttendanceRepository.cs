using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    public class AttendanceRepository : GenericRepository<Attendance>,
        IAttendanceRepository,
        IAttendanceFilterRepository,
        IAttendanceBulkRepository
    {
        public AttendanceRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Attendance>> GetAllWithDetailsAsync() =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .ToListAsync();

        public async Task<Attendance?> GetByIdWithDetailsAsync(int id) =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Attendance>> GetByCourseAsync(int courseId) =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();

        public async Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId) =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();

        public async Task<IEnumerable<Attendance>> GetByFilterAsync(int courseId, DateOnly from, DateOnly to) =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.CourseId == courseId && a.Date >= from && a.Date <= to)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

        public async Task<IEnumerable<Attendance>> GetByIdsWithDetailsAsync(List<int> ids) =>
            await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => ids.Contains(a.Id))
                .ToListAsync();
    }
}