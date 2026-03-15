namespace Frontend.Models
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public int Units { get; set; }
        public string Section { get; set; } = "";
        public int AssignedTeacherId { get; set; }
        public string AssignedTeacherName { get; set; } = "";
    }
}