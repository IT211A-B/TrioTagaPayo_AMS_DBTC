using Attendance_Management_System.Interfacess;

namespace Attendance_Management_System.Services
{
    public class BCryptPasswordHasher : IPasswordHasher
    {
        private readonly int _workFactor;

        public BCryptPasswordHasher(int workFactor = 12)
        {
            _workFactor = workFactor;
        }

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, _workFactor);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}