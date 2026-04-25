using Microsoft.AspNetCore.SignalR;

namespace Attendance_Management_System.Hubs
{
    /// <summary>
    /// SignalR Hub para sa real-time attendance notifications.
    /// 
    /// Groups:
    ///   "course_{courseId}" — teacher joins this para makita
    ///                         real-time kung kinsa nag-scan / gi-mark
    ///   "admin"             — admin joins para sa global feed
    /// </summary>
    public class AttendanceHub : Hub
    {
        /// <summary>
        /// Teacher/Admin calls this after connecting —
        /// para ma-subscribe sila sa specific course updates.
        /// </summary>
        public async Task JoinCourseGroup(int courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"course_{courseId}");
        }

        /// <summary>
        /// Para ma-unsubscribe sa course group.
        /// </summary>
        public async Task LeaveCourseGroup(int courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course_{courseId}");
        }

        /// <summary>
        /// Admin joins global notification group.
        /// </summary>
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admin");
        }
    }
}