using CoreFlow.Infrastructure.Messaging;
using CoreFlow.Worker.Email;
using CoreFlow.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.AddSingleton<SmtpEmailSender>();
builder.Services.AddHostedService<ContactEmailNotificationWorker>();

var host = builder.Build();
host.Run();
