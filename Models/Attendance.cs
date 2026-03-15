namespace Frontend.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public string StudentNo { get; set; } = "";
        public string StudentName { get; set; } = "";
        public string CourseCode { get; set; } = "";
        public string Section { get; set; } = "";
        public DateTime Date { get; set; }
        public string Status { get; set; } = "Present"; // Present, Absent, Late
        public string Remarks { get; set; } = "";
    }
}