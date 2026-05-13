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

                // ✅ Get teacher details if role matches
                int? teacherId = null;
                string? fullName = null;

                if (user.Role == "Teacher")
                {
                    try
                    {
                        var teacher = await _teacherRepository.FindAsync(t =>
                            t.TeacherNo == user.Username || t.Email == user.Username);
                        if (teacher != null)
                        {
                            teacherId = teacher.Id;
                            fullName = $"{teacher.FirstName} {teacher.LastName}";

                            // Store TeacherId in User record if not already set
                            if (user.TeacherId == null)
                            {
                                user.TeacherId = teacher.Id;
                                _userRepository.Update(user);
                                await _userRepository.SaveChangesAsync();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "[LOGIN] Error retrieving teacher details for {Username}", user.Username);
                    }
                }

                // Generate tokens
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
                    RefreshTokenExpiry = refreshExpiry,
                    TeacherId = teacherId,
                    FullName = fullName
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
                if (user == null)
                {
                    _logger.LogWarning("[REFRESH] No user found with this refresh token");
                    return null;
                }

                if (user.RefreshTokenExpiry == null)
                {
                    _logger.LogWarning("[REFRESH] Refresh token expiry is null for user: {Username}", user.Username);
                    return null;
                }

                if (user.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    _logger.LogWarning("[REFRESH] Refresh token expired for user: {Username}", user.Username);
                    return null;
                }

                // Get teacher info again (for the new token – optional, but consistent)
                int? teacherId = null;
                string? fullName = null;
                if (user.Role == "Teacher" && user.TeacherId.HasValue)
                {
                    var teacher = await _teacherRepository.GetByIdAsync(user.TeacherId.Value);
                    if (teacher != null)
                    {
                        teacherId = teacher.Id;
                        fullName = $"{teacher.FirstName} {teacher.LastName}";
                    }
                }

                var newAccessToken = _tokenGenerator.Generate(user);
                var newExpiration = DateTime.UtcNow.AddHours(8);
                var newRefreshToken = RefreshTokenHelper.GenerateRefreshToken();
                var newRefreshExpiry = DateTime.UtcNow.AddDays(7);

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = newRefreshExpiry;
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation("[REFRESH] SUCCESS for user: {Username}", user.Username);

                return new LoginResponse
                {
                    Token = newAccessToken,
                    Username = user.Username,
                    Role = user.Role,
                    Expiration = newExpiration,
                    RefreshToken = newRefreshToken,
                    RefreshTokenExpiry = newRefreshExpiry,
                    TeacherId = teacherId,
                    FullName = fullName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[REFRESH] CRITICAL ERROR");
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
                    IsEmailVerified = true
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