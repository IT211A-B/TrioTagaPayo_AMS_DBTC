using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByCourseAsync(int courseId)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.CourseId == courseId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .Where(a => a.StudentId == studentId)
                .ToListAsync();
        }

        public async Task<Attendance?> GetByIdAsync(int id)
        {
            return await _context.Attendances
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Attendance> CreateAsync(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<Attendance?> UpdateAsync(int id, Attendance updated)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return null;

            attendance.Status = updated.Status;
            attendance.Date = updated.Date;
            attendance.StudentId = updated.StudentId;
            attendance.CourseId = updated.CourseId;

            await _context.SaveChangesAsync();
            return attendance;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var attendance = await _context.Attendances.FindAsync(id);
            if (attendance == null) return false;

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}