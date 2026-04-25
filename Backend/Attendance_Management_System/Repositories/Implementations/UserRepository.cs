using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.DBCONTEXT;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Repositories.Implementations
{
    /// <summary>
    /// User repository — handles all DB queries for the User entity (login/auth).
    /// Inherits generic CRUD from GenericRepository.
    /// </summary>
    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Finds a user by matching username and hashed password — used for login.
        /// </summary>
        public async Task<User?> GetByUsernameAndPasswordAsync(string username, string passwordHash) =>
            await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == passwordHash);

        // ✅ FindAsync + AnyAsync — inherited from GenericRepository, no need to re-declare
    }
}