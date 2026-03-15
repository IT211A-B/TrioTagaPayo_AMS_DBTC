namespace Frontend.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string MiddleName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string FullName => $"{LastName}, {FirstName} {MiddleName}".Trim();
        public string Section { get; set; } = "";
        public string Gmail { get; set; } = "";
        public string Mobile { get; set; } = "";
    }
}