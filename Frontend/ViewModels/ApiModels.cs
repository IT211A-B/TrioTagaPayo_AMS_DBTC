// ============================================================
// Models/ApiModels.cs
// These match the exact DTOs your classmate's API returns.
// Field names taken directly from his DTO files.
// ============================================================

namespace AMS.Models
{
    // ── Student — matches StudentResponseDto ─────────────────
    public class StudentApiModel
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";   // "2026-0001" — NOT StudentId
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
        public string TeacherNo { get; set; } = "";  // "TCH-001"
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
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = "";
        public string StudentNo { get; set; } = "";
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string Date { get; set; } = "";  // DateOnly serialized as string
        public string Status { get; set; } = "";  // Present / Absent / Late
        public string Remarks { get; set; } = "";
        public DateTime CreatedAt { get; set; }
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
}