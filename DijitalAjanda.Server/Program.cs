
using DijitalAjanda.Server.Data;
using DijitalAjanda.Server.Services;
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

// CORS ayarlarını yapılandırma - Preflight istekleri için doğru yapılandırma
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Sadece React frontend'e izin ver
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
