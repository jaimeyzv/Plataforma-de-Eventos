namespace NotificationService.Infrastructure.Notifications;

public sealed class EmailSettings
{
    public const string SectionName = "Smtp";

    /// <summary>When false (default) the notification is simulated/logged instead of sent.</summary>
    public bool Enabled { get; set; } = false;

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public string From { get; set; } = "no-reply@eventplatform.local";
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseSsl { get; set; } = false;
}
