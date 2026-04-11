using System;
using System.Collections.Generic;

namespace _3TAGAPAYU.Models
{
    public class AttendanceModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double WorkingHours { get; set; }
    }

    public class AttendanceDashboardModel
    {
        public List<AttendanceModel> AttendanceRecords { get; set; } = new List<AttendanceModel>();
        public int TotalEmployees { get; set; }
        public int PresentToday { get; set; }
        public int AbsentToday { get; set; }
        public int LateToday { get; set; }
        public double AverageWorkingHours { get; set; }
        public DateTime SelectedDate { get; set; }
    }
}