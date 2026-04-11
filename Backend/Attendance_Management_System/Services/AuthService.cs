using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;

namespace Attendance_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher hasher,
            IJwtTokenGenerator tokenGenerator)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAndPasswordAsync(
                request.Username, _hasher.Hash(request.Password));

            if (user == null) return null;

            return new LoginResponse
            {
                Token = _tokenGenerator.Generate(user),
                Username = user.Username,
                Role = user.Role,
                Expiration = DateTime.UtcNow.AddHours(8)
            };
        }

        public async Task SeedAdminAsync()
        {
            if (await _userRepository.AnyAsync(_ => true)) return;

            var admin = new User
            {
                Username = "admin",
                PasswordHash = _hasher.Hash("admin123"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };
            await _userRepository.AddAsync(admin);
            await _userRepository.SaveChangesAsync();
        }
    }
}