using Attendance_Management_System.Models;

namespace Attendance_Management_System.Interfacess
{
    public interface IJwtTokenGenerator
    {
        string Generate(User user);
    }
}