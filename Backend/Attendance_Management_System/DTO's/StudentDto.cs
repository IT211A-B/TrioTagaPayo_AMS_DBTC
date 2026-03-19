namespace Attendance_Management_System.DTOs
{
    public class CreateStudentDto
    {
        public string StudentNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
    }

    public class UpdateStudentDto
    {
        public string StudentNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
    }

    public class StudentResponseDto
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string MobileNo { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}