using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Configuration;

namespace Attendance_Management_System.Services
{
    public class SendGridEmailService
    {
        private readonly IConfiguration _config;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            try
            {
                var apiKey = _config["SendGrid:ApiKey"];
                if (string.IsNullOrEmpty(apiKey))
                {
                    Console.WriteLine("[SendGrid] API key missing");
                    return false;
                }

                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("maanojamesneil123@gmail.com", "AMS System");
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    Console.WriteLine($"[SendGrid] Email sent to {toEmail}");
                    return true;
                }

                var body = await response.Body.ReadAsStringAsync();
                Console.WriteLine($"[SendGrid] Failed: {response.StatusCode} - {body}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SendGrid] Exception: {ex.Message}");
                return false;
            }
        }
    }
}