namespace CoreFlow.Worker.Email;

public class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 1025;
    public bool EnableSsl { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = "noreply@coreflow.local";
    public string FromName { get; init; } = "CoreFlow Agenda";
}
