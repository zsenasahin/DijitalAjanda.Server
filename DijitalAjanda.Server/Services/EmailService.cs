using System.Net;
using System.Net.Mail;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Server.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendHabitReminderAsync(int userId, string habitTitle, string reminderTime);
        Task SendGoalReminderAsync(int userId, string goalTitle, DateTime deadline);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ApplicationDbContext context, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPassword = _configuration["Email:SmtpPassword"];
                var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;

                if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
                {
                    _logger.LogWarning("E-posta ayarları yapılandırılmamış. E-posta gönderilemedi.");
                    return;
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = true
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, "Dijital Ajanda"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"E-posta gönderildi: {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"E-posta gönderilirken hata: {ex.Message}");
            }
        }

        public async Task SendHabitReminderAsync(int userId, string habitTitle, string reminderTime)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var subject = $"🔔 Alışkanlık Hatırlatıcısı: {habitTitle}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                    <div style='max-width: 500px; margin: 0 auto; background: white; border-radius: 16px; padding: 30px; box-shadow: 0 4px 15px rgba(0,0,0,0.1);'>
                        <h2 style='color: #10b981; margin-bottom: 20px;'>🔄 Alışkanlık Hatırlatması</h2>
                        <p style='font-size: 16px; color: #333;'>Merhaba!</p>
                        <p style='font-size: 16px; color: #333;'>
                            <strong style='color: #667eea;'>{habitTitle}</strong> alışkanlığını tamamlamayı unutma!
                        </p>
                        <p style='font-size: 14px; color: #666; margin-top: 20px;'>
                            Planlanan saat: <strong>{reminderTime}</strong>
                        </p>
                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='font-size: 12px; color: #999;'>Bu e-posta Dijital Ajanda tarafından gönderildi.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }

        public async Task SendGoalReminderAsync(int userId, string goalTitle, DateTime deadline)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.Email)) return;

            var daysLeft = (deadline - DateTime.UtcNow).Days;
            var urgencyText = daysLeft <= 1 ? "⚠️ BUGÜN!" : daysLeft <= 3 ? "⏰ Yaklaşıyor!" : "";

            var subject = $"🎯 Hedef Hatırlatıcısı: {goalTitle} {urgencyText}";
            var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif; background-color: #f5f5f5; padding: 20px;'>
                    <div style='max-width: 500px; margin: 0 auto; background: white; border-radius: 16px; padding: 30px; box-shadow: 0 4px 15px rgba(0,0,0,0.1);'>
                        <h2 style='color: #667eea; margin-bottom: 20px;'>🎯 Hedef Hatırlatması</h2>
                        <p style='font-size: 16px; color: #333;'>Merhaba!</p>
                        <p style='font-size: 16px; color: #333;'>
                            <strong style='color: #667eea;'>{goalTitle}</strong> hedefinin son tarihi yaklaşıyor!
                        </p>
                        <div style='background: #fef3c7; border-radius: 8px; padding: 15px; margin: 20px 0;'>
                            <p style='font-size: 14px; color: #92400e; margin: 0;'>
                                📅 Son tarih: <strong>{deadline:dd MMMM yyyy}</strong><br/>
                                ⏳ Kalan: <strong>{daysLeft} gün</strong>
                            </p>
                        </div>
                        <div style='margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee;'>
                            <p style='font-size: 12px; color: #999;'>Bu e-posta Dijital Ajanda tarafından gönderildi.</p>
                        </div>
                    </div>
                </body>
                </html>";

            await SendEmailAsync(user.Email, subject, body);
        }
    }
}
