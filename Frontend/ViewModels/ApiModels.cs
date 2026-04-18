// ============================================================
// Models/ApiModels.cs
// ============================================================
namespace ASM.ViewModels
{
    public class StudentApiModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Section { get; set; } = "";
        public string Status { get; set; } = "Active";
    }

    public class TeacherApiModel
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Department { get; set; } = "";
        public string ContactNumber { get; set; } = "";
        public string Status { get; set; } = "Active";
        public bool IsActive { get; set; } = true;
    }

    public class CourseApiModel
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string Description { get; set; } = "";
        public int Units { get; set; }
        public string Status { get; set; } = "Active";
    }

    public class AttendanceApiModel
    {
        public int Id { get; set; }
        public string StudentId { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public string Section { get; set; } = "";
        public string Date { get; set; } = "";
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class QrSessionApiModel
    {
        public int Id { get; set; }
        public string CourseId { get; set; } = "";
        public string QrCode { get; set; } = "";
        public bool IsActive { get; set; }
        public string ExpiresAt { get; set; } = "";
    }

    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}