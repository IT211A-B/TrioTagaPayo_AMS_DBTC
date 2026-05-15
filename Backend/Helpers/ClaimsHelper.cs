using System.Security.Claims;

namespace Attendance_Management_System.Helpers
{
    public static class ClaimsHelper
    {
        /// <summary>Get the logged-in user's ID from JWT claims.</summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(value, out var id) ? id : 0;
        }

        /// <summary>Get the logged-in user's username from JWT claims.</summary>
        public static string GetUsername(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        /// <summary>Get the logged-in user's role from JWT claims.</summary>
        public static string GetRole(this ClaimsPrincipal user)
            => user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}