namespace Attendance_Management_System.Interfacess
{
    public interface IPasswordHasher
    {
        string Hash(string password);
    }
}