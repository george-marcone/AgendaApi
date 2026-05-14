using MediatR;
using FluentValidation;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Validators;
using CoreFlow.Application.Interfaces;
using CoreFlow.Infrastructure.Services;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// register DI
var conn = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(conn))
{
    // usa EF Core com a connection string definida (SQL Server)
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));
    builder.Services.AddScoped<IUserService, EfUserService>();
}
else
{
    // fallback in-memory
    builder.Services.AddSingleton<IUserService, InMemoryUserService>();
}
// mediatR + validators + pipeline
builder.Services.AddMediatR(typeof(CreateUserCommand).Assembly);
// register validator explicitly to avoid extension method ambiguity in this small sample
builder.Services.AddTransient<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(CoreFlow.Application.Behaviors.ValidationBehavior<,>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
