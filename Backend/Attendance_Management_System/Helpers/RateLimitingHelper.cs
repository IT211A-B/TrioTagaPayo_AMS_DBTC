using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Attendance_Management_System.Helpers
{
    public static class RateLimitingHelper
    {
        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Max 5 login attempts per minute per IP
                options.AddFixedWindowLimiter("login", o =>
                {
                    o.PermitLimit = 5;
                    o.Window = TimeSpan.FromMinutes(1);
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    o.QueueLimit = 0;
                });

                // Max 100 requests per minute — general API
                options.AddFixedWindowLimiter("global", o =>
                {
                    o.PermitLimit = 100;
                    o.Window = TimeSpan.FromMinutes(1);
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    o.QueueLimit = 5;
                });

                // Max 10 QR scans per 30 seconds per IP
                options.AddFixedWindowLimiter("qrscan", o =>
                {
                    o.PermitLimit = 10;
                    o.Window = TimeSpan.FromSeconds(30);
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    o.QueueLimit = 0;
                });

                // Custom JSON rejection — dili HTML ang error
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsync(
                        """{"statusCode":429,"message":"Too many requests. Please slow down."}""",
                        cancellationToken);
                };
            });

            return services;
        }
    }
}