// ============================================================
// Models/ApiModels.cs
// FIXES:
//   1. AttendanceApiModel.CreatedAt was present but Id was missing
//      — backend AttendanceResponseDto has Id, needed for Edit/Delete
//   2. Added QRSessionApiModel to match backend QRSessionResponseDto
//   3. ScanResultDto added for when student scans QR
// ============================================================

namespace AMS.Models
{
    // ── Student — matches StudentResponseDto ─────────────────
    public class StudentApiModel
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Section { get; set; } = "";
        public string MobileNo { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ── Teacher — matches TeacherResponseDto ─────────────────
    public class TeacherApiModel
    {
        public int Id { get; set; }
        public string TeacherNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsActive { get; set; } = true;
        public int CourseCount { get; set; }
        public string Username { get; set; } = "";
        public bool HasAccount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ── Course — matches CourseResponseDto ───────────────────
    public class CourseApiModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Units { get; set; }
        public string Section { get; set; } = "";
        public string Schedule { get; set; } = "";
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ── Attendance — matches AttendanceResponseDto ───────────
    public class AttendanceApiModel
    {
        // FIX: Id was missing — needed if you ever want to Edit/Delete a record
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";

        // FIX: Backend returns DateOnly serialized as "yyyy-MM-dd" string
        // Keep as string — works for display; parse only when needed
        public string Date { get; set; } = "";
        public string Status { get; set; } = "";
        public string Remarks { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }

    // ── QR Session — matches QRSessionResponseDto ────────────
    // NEW: Used by the Attendance page QR feature
    public class QRSessionApiModel
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Token { get; set; } = "";
        public string Date { get; set; } = "";       // DateOnly → string
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        // Base64 PNG image — render as: <img src="data:image/png;base64,{QRCodeBase64}" />
        public string QRCodeBase64 { get; set; } = "";
        public int ScanCount { get; set; }
    }

    // ── Scan Result — matches ScanResultDto ──────────────────
    // NEW: What the backend returns when a student scans a QR code
    public class ScanResultApiModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int? AttendanceId { get; set; }
        public string StudentName { get; set; } = "";
        public string Status { get; set; } = "";
    }

    // ── Paginated wrapper — matches PaginationHelper.Paginate ─
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
    // ── User Profile DTO (matches GET /api/Account/profile) ─────
    public class UserProfileDto
    {
        public string? ProfilePhotoUrl { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }

}