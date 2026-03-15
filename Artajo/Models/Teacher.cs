namespace Frontend.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string TeacherId { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{LastName}, {FirstName}";
        public string Email { get; set; } = "";
        public int CourseCount { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status => IsActive ? "Active" : "Disabled";
    }
}