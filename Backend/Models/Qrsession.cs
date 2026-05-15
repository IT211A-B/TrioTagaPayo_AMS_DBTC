// QRSession.cs
namespace Attendance_Management_System.Models
{
    public class QRSession
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; } = true;
        public Course Course { get; set; } = null!;
        public ICollection<QRScan> Scans { get; set; } = new List<QRScan>();
    }

    public class QRScan
    {
        public int Id { get; set; }
        public int QRSessionId { get; set; }
        public int StudentId { get; set; }
        public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
        public QRSession QRSession { get; set; } = null!;
        public Student Student { get; set; } = null!;
        public Attendance? Attendance { get; set; }
    }
}