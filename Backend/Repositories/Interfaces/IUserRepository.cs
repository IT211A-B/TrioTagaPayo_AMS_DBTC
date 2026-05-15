using Attendance_Management_System.Models;

namespace Attendance_Management_System.Repositories.Interfaces
{
    /// <summary>
    /// User-specific repository interface — for login and auth queries.
    /// </summary>
    public interface IUserRepository : IGenericRepository<User>
    {
        // ✅ This is enough — FindAsync + AnyAsync already in IGenericRepository
        Task<User?> GetByUsernameAndPasswordAsync(string username, string passwordHash);
    }
}