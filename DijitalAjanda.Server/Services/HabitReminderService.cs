using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace DijitalAjanda.Server.Services
{
    public class HabitReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HabitReminderService> _logger;

        public HabitReminderService(IServiceProvider serviceProvider, ILogger<HabitReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔔 Habit Reminder Service başlatıldı");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckAndSendReminders();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Hatırlatıcı kontrolünde hata oluştu");
                }

                // Her 1 dakikada bir kontrol et
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task CheckAndSendReminders()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            var now = DateTime.Now;
            var currentTime = now.ToString("HH:mm");
            
            // Şu anki dakikaya denk gelen hatırlatma saatleri olan alışkanlıkları bul
            var habitsWithReminders = await context.Habits
                .Include(h => h.User)
                .Include(h => h.Completions)
                .Where(h => h.IsActive && 
                            h.ReminderTime != null && 
                            h.ReminderTime == currentTime)
                .ToListAsync();

            foreach (var habit in habitsWithReminders)
            {
                // Bugün tamamlanmış mı kontrol et
                var today = DateTime.UtcNow.Date;
                var alreadyCompletedToday = habit.Completions?.Any(c => 
                    c.CompletedAt.Date == today) ?? false;

                // Tamamlanmamışsa hatırlatma gönder
                if (!alreadyCompletedToday)
                {
                    _logger.LogInformation($"📧 Hatırlatma gönderiliyor: {habit.Title} -> User {habit.UserId}");
                    await emailService.SendHabitReminderAsync(habit.UserId, habit.Title, habit.ReminderTime);
                }
            }
        }
    }
}
