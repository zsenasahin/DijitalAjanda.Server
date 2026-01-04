
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Services;
using DijitalAjanda.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60); // 1 dakika command timeout
    });
});
// Add custom services
builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<SentimentAnalysisService>(); // Sentiment Analysis Service
builder.Services.AddHttpClient();
builder.Services.AddHttpClient<IGeminiService, GeminiService>(); // Gemini için tipli HttpClient

// CORS ayarlarını yapılandırma - Preflight istekleri için doğru yapılandırma
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.SetIsOriginAllowed(origin => true) // Tüm originlere (ipv4, ipv6, localhost, vs) izin ver
              .AllowAnyHeader()  // Tüm header'lara izin ver
              .AllowAnyMethod()  // Tüm HTTP metodlarına izin ver (OPTIONS dahil)
              .AllowCredentials(); // Credentials'a izin ver
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

var app = builder.Build();

// Otomatik rozet seed - uygulama başladığında rozetleri oluştur
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // Rozetler yoksa oluştur
        if (!context.Badges.Any())
        {
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
            context.Badges.AddRange(badges);
            context.SaveChanges();
            Console.WriteLine("✅ Rozetler otomatik oluşturuldu!");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Rozet seed hatası: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS'u en başta kullan - Preflight istekleri (OPTIONS) için kritik
app.UseCors("AllowAllOrigins");

// HTTPS redirection'ı kaldır - CORS sorununa neden oluyor
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
