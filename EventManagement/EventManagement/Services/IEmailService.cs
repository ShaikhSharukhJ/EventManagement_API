namespace EventManagement.Services;

public interface IEmailService
{
    Task SendRegistrationConfirmationAsync(
        string toEmail,
        string recipientName,
        string eventName,
        DateTime eventDate,
        string location,
        int registrationId,
        CancellationToken cancellationToken = default);
}
