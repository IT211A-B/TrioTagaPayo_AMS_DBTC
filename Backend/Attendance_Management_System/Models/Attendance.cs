namespace Attendance_Management_System.Models
{
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public DateOnly Date { get; set; }
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int? QRScanId { get; set; }

        public Student Student { get; set; } = null!;
        public Course Course { get; set; } = null!;
        public QRScan? QRScan { get; set; }
    }
}