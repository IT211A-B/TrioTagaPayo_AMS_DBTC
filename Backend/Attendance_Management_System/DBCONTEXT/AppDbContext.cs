using Microsoft.EntityFrameworkCore;
using Attendance_Management_System.Models;

namespace Attendance_Management_System.DBCONTEXT
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<User> Users { get; set; }

        // ✅ ADD THESE TWO
        public DbSet<QRSession> QRSessions { get; set; }
        public DbSet<QRScan> QRScans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // existing relationships (dili tanggalon)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attendance>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Attendances)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ ADD THESE — QR relationships
            modelBuilder.Entity<QRSession>()
                .HasOne(q => q.Course)
                .WithMany()
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QRScan>()
                .HasOne(q => q.QRSession)
                .WithMany(s => s.Scans)
                .HasForeignKey(q => q.QRSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QRScan>()
                .HasOne(q => q.Student)
                .WithMany()
                .HasForeignKey(q => q.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Prevent duplicate scans at DB level
            modelBuilder.Entity<QRScan>()
                .HasIndex(q => new { q.QRSessionId, q.StudentId })
                .IsUnique();
        }
    }
}