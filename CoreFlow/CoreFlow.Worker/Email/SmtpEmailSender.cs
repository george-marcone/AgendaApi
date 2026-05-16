using System.Net;
using System.Net.Mail;
using CoreFlow.Application.Events;
using Microsoft.Extensions.Options;

namespace CoreFlow.Worker.Email;

public class SmtpEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendContactNotificationAsync(
        ContactChangedEvent contactEvent,
        CancellationToken cancellationToken)
    {
        var failures = new List<Exception>();

        try
        {
            await SendEmailAsync(
                contactEvent.Actor.Name,
                contactEvent.Actor.Email,
                BuildActorSubject(contactEvent),
                BuildActorBody(contactEvent),
                contactEvent,
                "actor",
                cancellationToken);
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        try
        {
            await SendEmailAsync(
                contactEvent.Name,
                contactEvent.Email,
                BuildContactSubject(contactEvent),
                BuildContactBody(contactEvent),
                contactEvent,
                "contact",
                cancellationToken);
        }
        catch (Exception exception)
        {
            failures.Add(exception);
        }

        if (failures.Count > 0)
        {
            throw new AggregateException("One or more contact notification e-mails failed.", failures);
        }
    }

    private async Task SendEmailAsync(
        string toName,
        string toEmail,
        string subject,
        string body,
        ContactChangedEvent contactEvent,
        string recipientKind,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            _logger.LogWarning(
                "Skipped {RecipientKind} {EventType} email for contact {ContactId} because recipient email is empty.",
                recipientKind,
                contactEvent.EventType,
                contactEvent.Id);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(toEmail, toName));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(message, cancellationToken);

        _logger.LogInformation(
            "Sent {RecipientKind} {EventType} email notification to {Email} for contact {ContactId}.",
            recipientKind,
            contactEvent.EventType,
            toEmail,
            contactEvent.Id);
    }

    private static string BuildActorSubject(ContactChangedEvent contactEvent)
    {
        return contactEvent.EventType switch
        {
            ContactEventType.Created => "Voce cadastrou um novo contato",
            ContactEventType.Updated => "Voce alterou um contato da agenda",
            ContactEventType.Deleted => "Voce removeu um contato da agenda",
            _ => "Atualizacao da agenda"
        };
    }

    private static string BuildContactSubject(ContactChangedEvent contactEvent)
    {
        return contactEvent.EventType switch
        {
            ContactEventType.Created => "Voce foi adicionado a agenda",
            ContactEventType.Updated => "Seus dados foram alterados na agenda",
            ContactEventType.Deleted => "Voce foi removido da agenda",
            _ => "Atualizacao da agenda"
        };
    }

    private static string BuildActorBody(ContactChangedEvent contactEvent)
    {
        var action = contactEvent.EventType switch
        {
            ContactEventType.Created => "cadastrou um novo contato",
            ContactEventType.Updated => "alterou as informacoes de um contato",
            ContactEventType.Deleted => "removeu um contato da agenda",
            _ => "registrou uma atualizacao na agenda"
        };

        var result = contactEvent.EventType switch
        {
            ContactEventType.Created => "Cadastro realizado com sucesso.",
            ContactEventType.Updated => "Alteracao realizada com sucesso.",
            ContactEventType.Deleted => "Remocao realizada com sucesso.",
            _ => "Operacao realizada com sucesso."
        };

        return $"""
            Ola, {contactEvent.Actor.Name}.

            Voce {action}.
            {result}

            Dados do contato:
            - Nome: {contactEvent.Name}
            - E-mail: {contactEvent.Email}
            - Telefone: {contactEvent.Phone}
            - Data do evento: {contactEvent.OccurredAt:dd/MM/yyyy HH:mm:ss} UTC

            Esta e uma mensagem automatica. Nao responda este e-mail.
            """;
    }

    private static string BuildContactBody(ContactChangedEvent contactEvent)
    {
        var action = contactEvent.EventType switch
        {
            ContactEventType.Created => "adicionou voce a agenda",
            ContactEventType.Updated => "alterou suas informacoes na agenda",
            ContactEventType.Deleted => "removeu voce da agenda",
            _ => "registrou uma atualizacao na sua agenda"
        };

        var actorEmail = string.IsNullOrWhiteSpace(contactEvent.Actor.Email)
            ? "e-mail nao informado"
            : contactEvent.Actor.Email;

        return $"""
            Ola, {contactEvent.Name}.

            {contactEvent.Actor.Name} ({actorEmail}) {action}.

            Dados registrados:
            - Nome: {contactEvent.Name}
            - E-mail: {contactEvent.Email}
            - Telefone: {contactEvent.Phone}
            - Data do evento: {contactEvent.OccurredAt:dd/MM/yyyy HH:mm:ss} UTC

            Esta e uma mensagem automatica. Nao responda este e-mail.
            """;
    }
}
