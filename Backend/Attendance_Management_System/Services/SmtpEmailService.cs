using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace Attendance_Management_System.Services
{
    public class SmtpEmailService
    {
        private readonly IConfiguration _config;

        public SmtpEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpServer = _config["EmailSettings:SmtpServer"];
                var smtpPortStr = _config["EmailSettings:SmtpPort"];
                var senderEmail = _config["EmailSettings:SenderEmail"];
                var senderPassword = _config["EmailSettings:SenderPassword"];

                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPortStr) ||
                    string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    Console.WriteLine("[SMTP] Missing configuration");
                    return false;
                }

                if (!int.TryParse(smtpPortStr, out int smtpPort))
                {
                    Console.WriteLine("[SMTP] Invalid port number");
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("AMS", senderEmail));
                message.To.Add(new MailboxAddress("", toEmail));
                message.Subject = subject;
                message.Body = new TextPart("html") { Text = body };

                using var client = new SmtpClient();
                await client.ConnectAsync(smtpServer, smtpPort, false); // false = no SSL (STARTTLS is used on port 587)
                await client.AuthenticateAsync(senderEmail, senderPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                Console.WriteLine($"[SMTP] Email sent to {toEmail}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMTP] Error: {ex.Message}");
                return false;
            }
        }
    }
}