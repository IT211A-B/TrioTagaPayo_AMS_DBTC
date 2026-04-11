using Microsoft.AspNetCore.Mvc;
using _3TAGAPAYU.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _3TAGAPAYU.Controllers
{
    public class AttendanceController : Controller
    {
        public IActionResult Index(DateTime? date)
        {
            var selectedDate = date ?? DateTime.Now;
            var attendanceRecords = GetSampleAttendanceData(selectedDate);

            var model = new AttendanceDashboardModel
            {
                AttendanceRecords = attendanceRecords,
                TotalEmployees = attendanceRecords.Count,
                PresentToday = attendanceRecords.Count(x => x.Status == "Present"),
                AbsentToday = attendanceRecords.Count(x => x.Status == "Absent"),
                LateToday = attendanceRecords.Count(x => x.Status == "Late"),
                AverageWorkingHours = attendanceRecords.Count > 0 ? attendanceRecords.Average(x => x.WorkingHours) : 0,
                SelectedDate = selectedDate
            };

            return View(model);
        }

        private List<AttendanceModel> GetSampleAttendanceData(DateTime date)
        {
            return new List<AttendanceModel>
            {
                new AttendanceModel
                {
                    EmployeeId = 1,
                    EmployeeName = "John Smith",
                    Department = "IT",
                    Date = date,
                    CheckInTime = "09:00 AM",
                    CheckOutTime = "05:30 PM",
                    Status = "Present",
                    WorkingHours = 8.5
                },
                new AttendanceModel
                {
                    EmployeeId = 2,
                    EmployeeName = "Sarah Johnson",
                    Department = "HR",
                    Date = date,
                    CheckInTime = "09:15 AM",
                    CheckOutTime = "05:45 PM",
                    Status = "Late",
                    WorkingHours = 8.5
                },
                new AttendanceModel
                {
                    EmployeeId = 3,
                    EmployeeName = "Mike Davis",
                    Department = "Sales",
                    Date = date,
                    CheckInTime = "-",
                    CheckOutTime = "-",
                    Status = "Absent",
                    WorkingHours = 0
                },
                new AttendanceModel
                {
                    EmployeeId = 4,
                    EmployeeName = "Emily Brown",
                    Department = "Finance",
                    Date = date,
                    CheckInTime = "08:50 AM",
                    CheckOutTime = "05:20 PM",
                    Status = "Present",
                    WorkingHours = 8.5
                }
            };
        }
    }
}