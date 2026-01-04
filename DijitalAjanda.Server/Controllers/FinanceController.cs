using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Models;

namespace DijitalAjanda.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FinanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanıcının tüm işlemlerini getir
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetUserTransactions(int userId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        // Belirli tarih aralığındaki işlemleri getir
        [HttpGet("user/{userId}/range")]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactionsByRange(
            int userId, 
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            return await _context.Transactions
                .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        // Günlük özet
        [HttpGet("user/{userId}/daily/{date}")]
        public async Task<ActionResult<object>> GetDailySummary(int userId, DateTime date)
        {
            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date == date.Date)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);

            return Ok(new
            {
                date = date.Date,
                income,
                expense,
                balance = income - expense,
                transactions
            });
        }

        // Haftalık özet
        [HttpGet("user/{userId}/weekly")]
        public async Task<ActionResult<object>> GetWeeklySummary(int userId)
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek + 1); // Pazartesi
            var weekEnd = weekStart.AddDays(6);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date >= weekStart && t.Date.Date <= weekEnd)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);

            // Kategori bazlı harcama
            var categoryBreakdown = transactions
                .Where(t => t.Type == "expense")
                .GroupBy(t => t.Category)
                .Select(g => new { category = g.Key, amount = g.Sum(t => t.Amount) })
                .ToList();

            return Ok(new
            {
                weekStart,
                weekEnd,
                income,
                expense,
                balance = income - expense,
                categoryBreakdown,
                transactionCount = transactions.Count
            });
        }

        // Aylık özet
        [HttpGet("user/{userId}/monthly/{year}/{month}")]
        public async Task<ActionResult<object>> GetMonthlySummary(int userId, int year, int month)
        {
            var monthStart = new DateTime(year, month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date >= monthStart && t.Date.Date <= monthEnd)
                .ToListAsync();

            var income = transactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var expense = transactions.Where(t => t.Type == "expense").Sum(t => t.Amount);

            // Geçen ay ile karşılaştırma
            var lastMonthStart = monthStart.AddMonths(-1);
            var lastMonthEnd = monthStart.AddDays(-1);

            var lastMonthTransactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.Date.Date >= lastMonthStart && t.Date.Date <= lastMonthEnd)
                .ToListAsync();

            var lastMonthIncome = lastMonthTransactions.Where(t => t.Type == "income").Sum(t => t.Amount);
            var lastMonthExpense = lastMonthTransactions.Where(t => t.Type == "expense").Sum(t => t.Amount);

            // Kategori bazlı harcama
            var categoryBreakdown = transactions
                .Where(t => t.Type == "expense")
                .GroupBy(t => t.Category)
                .Select(g => new { category = g.Key, amount = g.Sum(t => t.Amount) })
                .OrderByDescending(c => c.amount)
                .ToList();

            return Ok(new
            {
                year,
                month,
                income,
                expense,
                balance = income - expense,
                comparison = new
                {
                    incomeChange = lastMonthIncome > 0 ? ((income - lastMonthIncome) / lastMonthIncome) * 100 : 0,
                    expenseChange = lastMonthExpense > 0 ? ((expense - lastMonthExpense) / lastMonthExpense) * 100 : 0,
                    lastMonthIncome,
                    lastMonthExpense
                },
                categoryBreakdown,
                transactionCount = transactions.Count
            });
        }

        // Yeni işlem ekle
        [HttpPost]
        public async Task<ActionResult<Transaction>> CreateTransaction(Transaction transaction)
        {
            transaction.CreatedAt = DateTime.UtcNow;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUserTransactions), new { userId = transaction.UserId }, transaction);
        }

        // İşlem güncelle
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // İşlem sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }
    }
}
