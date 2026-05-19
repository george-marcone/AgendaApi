using CoreFlow.Infrastructure.Messaging;
using CoreFlow.Worker.Email;
using CoreFlow.Worker.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<MessagingOptions>(
    builder.Configuration.GetSection(MessagingOptions.SectionName));
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<AzureServiceBusOptions>(
    builder.Configuration.GetSection(AzureServiceBusOptions.SectionName));
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection(EmailOptions.SectionName));

builder.Services.AddSingleton<SmtpEmailSender>();

var messagingOptions = builder.Configuration.GetSection(MessagingOptions.SectionName).Get<MessagingOptions>()
    ?? new MessagingOptions();

if (MessagingProviders.Is(messagingOptions.Provider, MessagingProviders.AzureServiceBus))
{
    builder.Services.AddHostedService<AzureServiceBusContactEmailNotificationWorker>();
}
else if (MessagingProviders.Is(messagingOptions.Provider, MessagingProviders.RabbitMq))
{
    builder.Services.AddHostedService<RabbitMqContactEmailNotificationWorker>();
}

var host = builder.Build();
host.Run();
