using MediatR;
using FluentValidation;
using CoreFlow.Application.Behaviors;
using CoreFlow.Application.Commands;
using CoreFlow.Application.Validators;
using CoreFlow.Application.Interfaces;
using CoreFlow.Infrastructure.Services;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddControllers();

var connectionString = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IUserService, EfUserService>();
}
else
{
    builder.Services.AddSingleton<IUserService, InMemoryUserService>();
}

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateUserCommand).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserCommandValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (FluentValidation.ValidationException validationException)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(new
        {
            errors = validationException.Errors.Select(error => new
            {
                field = error.PropertyName,
                message = error.ErrorMessage
            })
        });
    }
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
