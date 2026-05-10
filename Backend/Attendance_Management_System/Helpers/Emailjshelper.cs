using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace Attendance_Management_System.Helpers
{
    public class EmailJSOptions
    {
        public string ServiceId { get; set; } = string.Empty;
        public string TemplateId { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
    }

    public class EmailJSHelper
    {
        private readonly HttpClient _httpClient;
        private readonly EmailJSOptions _options;
        private readonly ILogger<EmailJSHelper> _logger;

        private const string EmailJSApiUrl = "https://api.emailjs.com/api/v1.0/email/send";

        public EmailJSHelper(
            HttpClient httpClient,
            IOptions<EmailJSOptions> options,
            ILogger<EmailJSHelper> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends an attendance notification email to the student.
        /// </summary>
        public async Task<bool> SendAttendanceNotificationAsync(
            string studentEmail,
            string studentName,
            string studentNo,
            string courseName,
            string section,
            string status,
            DateOnly date,
            DateTime timeRecorded)
        {
            try
            {
                var templateParams = new Dictionary<string, string>
                {
                    { "to_email",       studentEmail },
                    { "student_name",   studentName },
                    { "student_no",     studentNo },
                    { "course_name",    courseName },
                    { "section",        section },
                    { "status",         status },
                    { "date",           date.ToString("MMMM dd, yyyy") },
                    { "time_recorded",  timeRecorded.ToString("hh:mm tt") },
                    { "title",          $"Attendance Recorded — {status}" }
                };

                var payload = new
                {
                    service_id = _options.ServiceId,
                    template_id = _options.TemplateId,
                    user_id = _options.PublicKey,
                    accessToken = _options.PrivateKey,
                    template_params = templateParams
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(EmailJSApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[EmailJS] Attendance email sent → {Email}", studentEmail);
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[EmailJS] Attendance email failed → {Code} | Body: {Body}", response.StatusCode, errorBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailJS] Exception sending attendance email to {Email}", studentEmail);
                return false;
            }
        }

        /// <summary>
        /// Sends email verification link using a dedicated EmailJS template.
        /// </summary>
        public async Task<bool> SendVerificationEmailAsync(string email, string username, string verificationLink)
        {
            try
            {
                var templateParams = new Dictionary<string, string>
                {
                    { "to_email", email },
                    { "username", username },
                    { "verification_link", verificationLink }
                };

                var payload = new
                {
                    service_id = _options.ServiceId,
                    template_id = _options.TemplateId,   // You can use a separate template for verification
                    user_id = _options.PublicKey,
                    accessToken = _options.PrivateKey,
                    template_params = templateParams
                };

                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(EmailJSApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("[EmailJS] Verification email sent to {Email}", email);
                    return true;
                }

                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[EmailJS] Verification email failed → {Code} | Body: {Body}", response.StatusCode, errorBody);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[EmailJS] Exception sending verification email to {Email}", email);
                return false;
            }
        }
    }
}