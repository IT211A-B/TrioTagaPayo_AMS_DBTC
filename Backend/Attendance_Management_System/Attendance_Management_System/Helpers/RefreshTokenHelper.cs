using System.Security.Cryptography;

namespace Attendance_Management_System.Helpers
{
    /// <summary>
    /// Generates a secure random refresh token.
    /// Separate sa JWT — kini opaque random string lang,
    /// gi-store sa DB ug gi-compare kung mag-refresh ang client.
    /// </summary>
    public static class RefreshTokenHelper
    {
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}