using System.Net;
using System.Net.Mail;

namespace EventManagement.Services;

public class MailtrapEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public MailtrapEmailService(IConfiguration config)
    {
        _config = config;
        var section = config.GetSection("Mailtrap");
        _senderEmail = section["SenderEmail"] ?? "noreply@eventregistration.local";
        _senderName = section["SenderName"] ?? "Event Registration";
    }

    public async Task SendRegistrationConfirmationAsync(
        string toEmail,
        string recipientName,
        string eventName,
        DateTime eventDate,
        string location,
        int registrationId,
        CancellationToken cancellationToken = default)
    {
        var host = _config["Mailtrap:Host"];
        var portStr = _config["Mailtrap:Port"];
        var username = _config["Mailtrap:Username"];
        var password = _config["Mailtrap:Password"];

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Mailtrap SMTP is not configured. Set Mailtrap:Host, Username, Password in appsettings (e.g. from Sandboxes → My Sandbox → SMTP).");

        var port = int.TryParse(portStr, out var p) ? p : 587;

        using var smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var subject = $"Registration confirmed – {eventName}";
        var body = $"""
            Hello {recipientName},

            Your registration has been confirmed.

            Event: {eventName}
            Date: {eventDate:yyyy-MM-dd HH:mm}
            Location: {location}
            Registration ID: {registrationId}

            Thank you.
            """;

        using var mail = new MailMessage
        {
            From = new MailAddress(_senderEmail, _senderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        mail.To.Add(new MailAddress(toEmail, recipientName));

        await smtpClient.SendMailAsync(mail, cancellationToken);
    }
}
