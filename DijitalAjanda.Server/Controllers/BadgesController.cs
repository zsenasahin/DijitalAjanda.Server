using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BadgesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Tüm rozet tanımlarını getir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Badge>>> GetBadges()
        {
            return await _context.Badges.ToListAsync();
        }

        // Kullanıcının rozetlerini getir
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserBadge>>> GetUserBadges(int userId)
        {
            return await _context.UserBadges
                .Include(ub => ub.Badge)
                .Where(ub => ub.UserId == userId)
                .OrderByDescending(ub => ub.EarnedAt)
                .ToListAsync();
        }

        // Rozet kontrolü ve kazanım
        [HttpPost("check/{userId}")]
        public async Task<ActionResult> CheckAndAwardBadges(int userId)
        {
            var earnedBadges = new List<UserBadge>();
            var existingBadgeIds = await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.BadgeId)
                .ToListAsync();

            // Alışkanlık streak kontrolü
            var habits = await _context.Habits
                .Include(h => h.Completions)
                .Where(h => h.UserId == userId)
                .ToListAsync();

            foreach (var habit in habits)
            {
                var streak = CalculateStreak(habit);
                
                // 7 gün streak rozeti
                var fireStreakBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "fire_streak");
                if (fireStreakBadge != null && streak >= 7 && !existingBadgeIds.Contains(fireStreakBadge.Id))
                {
                    earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = fireStreakBadge.Id });
                    existingBadgeIds.Add(fireStreakBadge.Id);
                }

                // 30 gün streak rozeti
                var lightningStreakBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "lightning_streak");
                if (lightningStreakBadge != null && streak >= 30 && !existingBadgeIds.Contains(lightningStreakBadge.Id))
                {
                    earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = lightningStreakBadge.Id });
                    existingBadgeIds.Add(lightningStreakBadge.Id);
                }

                // 100 gün streak rozeti
                var diamondStreakBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "diamond_streak");
                if (diamondStreakBadge != null && streak >= 100 && !existingBadgeIds.Contains(diamondStreakBadge.Id))
                {
                    earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = diamondStreakBadge.Id });
                    existingBadgeIds.Add(diamondStreakBadge.Id);
                }
            }

            // İlk alışkanlık rozeti
            if (habits.Count > 0)
            {
                var starterBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "starter");
                if (starterBadge != null && !existingBadgeIds.Contains(starterBadge.Id))
                {
                    earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = starterBadge.Id });
                    existingBadgeIds.Add(starterBadge.Id);
                }
            }

            // Kitap rozet kontrolleri
            var completedBooks = await _context.Books
                .Where(b => b.UserId == userId && b.Status == "Completed")
                .CountAsync();

            var readerBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "reader");
            if (readerBadge != null && completedBooks >= 10 && !existingBadgeIds.Contains(readerBadge.Id))
            {
                earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = readerBadge.Id });
                existingBadgeIds.Add(readerBadge.Id);
            }

            var librarianBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "librarian");
            if (librarianBadge != null && completedBooks >= 50 && !existingBadgeIds.Contains(librarianBadge.Id))
            {
                earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = librarianBadge.Id });
                existingBadgeIds.Add(librarianBadge.Id);
            }

            // Günlük rozet kontrolü
            var journalCount = await _context.JournalEntries
                .Where(j => j.UserId == userId)
                .CountAsync();

            var writerBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "writer");
            if (writerBadge != null && journalCount >= 30 && !existingBadgeIds.Contains(writerBadge.Id))
            {
                earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = writerBadge.Id });
                existingBadgeIds.Add(writerBadge.Id);
            }

            // Hedef rozet kontrolü
            var completedGoals = await _context.Goals
                .Where(g => g.UserId == userId && g.Status == "Completed")
                .CountAsync();

            var goalHunterBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "goal_hunter");
            if (goalHunterBadge != null && completedGoals >= 5 && !existingBadgeIds.Contains(goalHunterBadge.Id))
            {
                earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = goalHunterBadge.Id });
                existingBadgeIds.Add(goalHunterBadge.Id);
            }

            var championBadge = await _context.Badges.FirstOrDefaultAsync(b => b.Name == "champion");
            if (championBadge != null && completedGoals >= 10 && !existingBadgeIds.Contains(championBadge.Id))
            {
                earnedBadges.Add(new UserBadge { UserId = userId, BadgeId = championBadge.Id });
                existingBadgeIds.Add(championBadge.Id);
            }

            if (earnedBadges.Count > 0)
            {
                _context.UserBadges.AddRange(earnedBadges);
                await _context.SaveChangesAsync();
            }

            return Ok(new { newBadges = earnedBadges.Count, badges = earnedBadges.Select(b => b.BadgeId) });
        }

        private int CalculateStreak(Habit habit)
        {
            if (habit.Completions == null || habit.Completions.Count == 0) return 0;

            var sortedCompletions = habit.Completions
                .OrderByDescending(c => c.CompletedAt)
                .ToList();

            int streak = 0;
            var currentDate = DateTime.UtcNow.Date;

            for (int i = 0; i < 365; i++)
            {
                var hasCompletion = sortedCompletions.Any(c => c.CompletedAt.Date == currentDate);
                if (hasCompletion)
                {
                    streak++;
                }
                else
                {
                    break;
                }
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }

        // Seed rozetleri (ilk kurulumda çağrılacak)
        [HttpPost("seed")]
        public async Task<ActionResult> SeedBadges()
        {
            if (await _context.Badges.AnyAsync())
            {
                return Ok("Rozetler zaten mevcut");
            }

            var badges = new List<Badge>
            {
                new Badge { Name = "starter", Description = "İlk alışkanlığını oluşturdun!", Icon = "🌱", Category = "Habits", RequiredCount = 1, Color = "#10b981" },
                new Badge { Name = "fire_streak", Description = "7 gün üst üste alışkanlık tamamladın!", Icon = "🔥", Category = "Habits", RequiredCount = 7, Color = "#f59e0b" },
                new Badge { Name = "lightning_streak", Description = "30 gün üst üste alışkanlık tamamladın!", Icon = "⚡", Category = "Habits", RequiredCount = 30, Color = "#3b82f6" },
                new Badge { Name = "diamond_streak", Description = "100 gün üst üste alışkanlık tamamladın!", Icon = "💎", Category = "Habits", RequiredCount = 100, Color = "#8b5cf6" },
                new Badge { Name = "reader", Description = "10 kitap okudun!", Icon = "📚", Category = "Books", RequiredCount = 10, Color = "#06b6d4" },
                new Badge { Name = "librarian", Description = "50 kitap okudun!", Icon = "📖", Category = "Books", RequiredCount = 50, Color = "#0891b2" },
                new Badge { Name = "writer", Description = "30 günlük yazdın!", Icon = "✍️", Category = "Journal", RequiredCount = 30, Color = "#ec4899" },
                new Badge { Name = "hydration", Description = "7 gün su içme alışkanlığını sürdürdün!", Icon = "💧", Category = "Health", RequiredCount = 7, Color = "#0ea5e9" },
                new Badge { Name = "goal_hunter", Description = "5 hedef tamamladın!", Icon = "🎯", Category = "Goals", RequiredCount = 5, Color = "#f43f5e" },
                new Badge { Name = "champion", Description = "10 hedef tamamladın!", Icon = "🏆", Category = "Goals", RequiredCount = 10, Color = "#eab308" }
            };

            _context.Badges.AddRange(badges);
            await _context.SaveChangesAsync();

            return Ok("Rozetler oluşturuldu");
        }
    }
}
