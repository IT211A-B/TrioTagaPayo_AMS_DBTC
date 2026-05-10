using Attendance_Management_System.Helpers;
using Attendance_Management_System.Interfacess;
using Attendance_Management_System.Models;
using Attendance_Management_System.Repositories.Interfaces;
using BCrypt.Net;

namespace Attendance_Management_System.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthService> _logger;
        private readonly IStudentRepository _studentRepository;
        private readonly ITeacherRepository _teacherRepository;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher hasher,
            IJwtTokenGenerator tokenGenerator,
            ILogger<AuthService> logger,
            IStudentRepository studentRepository,
            ITeacherRepository teacherRepository)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
            _studentRepository = studentRepository;
            _teacherRepository = teacherRepository;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                _logger.LogInformation("[LOGIN] Attempt for user: {Username}", request.Username);

                var user = await _userRepository.FindAsync(u => u.Username == request.Username);
                if (user == null)
                {
                    _logger.LogWarning("[LOGIN] User not found: {Username}", request.Username);
                    return null;
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    _logger.LogError("[LOGIN] Password hash missing for: {Username}", request.Username);
                    return null;
                }

                bool passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
                if (!passwordValid)
                {
                    _logger.LogWarning("[LOGIN] Invalid password for: {Username}", request.Username);
                    return null;
                }

                // ✅ Block login if email not verified for Student or Teacher
                if (!user.IsEmailVerified && (user.Role == "Student" || user.Role == "Teacher"))
                {
                    _logger.LogWarning("[LOGIN] Email not verified for user: {Username}", request.Username);
                    return null; // Frontend should show "Please verify your email" based on this
                }

                // Store TeacherId if user is Teacher
                if (user.Role == "Teacher")
                {
                    try
                    {
                        var teacher = await _teacherRepository.FindAsync(t =>
                            t.TeacherNo == user.Username ||
                            t.Email == user.Username ||
                            (t.FirstName + "." + t.LastName).ToLower() == user.Username.ToLower());

                        if (teacher != null && user.TeacherId == null)
                        {
                            user.TeacherId = teacher.Id;
                            _userRepository.Update(user);
                            await _userRepository.SaveChangesAsync();
                            _logger.LogInformation("[LOGIN] TeacherId {TeacherId} stored for user {Username}", teacher.Id, user.Username);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[LOGIN] Error storing TeacherId for {Username}", user.Username);
                    }
                }

                var token = _tokenGenerator.Generate(user);
                var expiration = DateTime.UtcNow.AddHours(8);
                var refreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var refreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = refreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[LOGIN] SUCCESS for user: {Username}", request.Username);

                return new LoginResponse
                {
                    Token = token,
                    Username = user.Username,
                    Role = user.Role,
                    Expiration = expiration,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = refreshExpiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[LOGIN] CRITICAL ERROR for user: {Username}", request.Username);
                throw;
            }
        }

        public async Task<LoginResponse?> RefreshAsync(string refreshToken)
        {
            try
            {
                _logger.LogInformation("[REFRESH] Attempt with token");

                var user = await _userRepository.FindAsync(u => u.RefreshToken == refreshToken);
                if (user == null || user.RefreshTokenExpiry == null || user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("[REFRESH] Invalid or expired refresh token");
                    return null;
                }

                var newAccessToken = _tokenGenerator.Generate(user);
                var newExpiration = DateTime.UtcNow.AddHours(8);
                var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var newRefreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = newRefreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                return new LoginResponse
                {
                    Token = newAccessToken,
                    Username = user.Username,
                    Role = user.Role,
                    Expiration = newExpiration,
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiry = newRefreshExpiry
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REFRESH] Error");
                throw;
            }
        }

        public async Task SeedAdminAsync()
        {
            try
            {
                _logger.LogInformation("[SEED] Checking if admin exists...");

                var adminExists = await _userRepository.AnyAsync(u => u.Role == "Admin");
                if (adminExists)
                {
                    _logger.LogInformation("[SEED] Admin already exists, skipping seed");
                    return;
                }

                _logger.LogInformation("[SEED] Creating admin user...");

                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = _hasher.Hash("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow,
                    IsEmailVerified = true, // Admin auto-verified
                    EmailVerificationToken = null,
                    EmailVerificationTokenExpiry = null
                };

                await _userRepository.AddAsync(admin);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[SEED] Admin created successfully");
                _logger.LogInformation("   Username: admin");
                _logger.LogInformation("   Password: admin123");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[SEED] ERROR creating admin");
                throw;
            }
        }
    }
}