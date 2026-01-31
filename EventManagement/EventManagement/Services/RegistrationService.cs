using EventManagement.Data;
using EventManagement.DTOs;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

public class RegistrationService : IRegistrationService
{
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(ApplicationDbContext db, IEmailService emailService, ILogger<RegistrationService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegistrationResponse?> RegisterAsync(int eventId, RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var evt = await _db.Events
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId, cancellationToken);
        if (evt is null) return null;

        // Business rule: registration for past events is not allowed
        if (evt.Date.Date < DateTime.UtcNow.Date)
            throw new InvalidOperationException("Registration for past events is not allowed.");

        // Business rule: same email cannot register twice for the same event
        if (evt.Registrations.Any(r => r.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("This email is already registered for this event.");

        // Business rule: event capacity cannot be exceeded
        if (evt.Registrations.Count >= evt.Capacity)
            throw new InvalidOperationException("Event capacity has been reached.");

        var registration = new Registration
        {
            EventId = eventId,
            Name = request.Name,
            Email = request.Email.Trim()
        };
        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync(cancellationToken);

        var emailSent = true;
        string? emailError = null;
        try
        {
            await _emailService.SendRegistrationConfirmationAsync(
                registration.Email,
                registration.Name,
                evt.Title,
                evt.Date,
                evt.Location,
                registration.Id,
                cancellationToken);
        }
        catch (Exception ex)
        {
            emailSent = false;
            emailError = $"{ex.GetType().Name}: {ex.Message}";
            _logger.LogWarning(ex, "Confirmation email could not be sent for registration {RegistrationId}. Check Mailtrap ApiToken in appsettings.", registration.Id);
        }

        return ToResponse(registration, emailSent, emailError);
    }

    public async Task<RegistrationResponse?> GetByIdAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Registrations.FindAsync(new object[] { registrationId }, cancellationToken);
        return entity is null ? null : ToResponse(entity, true, null);
    }

    public async Task<IReadOnlyList<RegistrationResponse>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var list = await _db.Registrations
            .Where(r => r.EventId == eventId)
            .OrderBy(r => r.RegisteredAt)
            .ToListAsync(cancellationToken);
        return list.Select(r => ToResponse(r, true, null)).ToList();
    }

    public async Task<bool> CancelAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Registrations.FindAsync(new object[] { registrationId }, cancellationToken);
        if (entity is null) return false;
        _db.Registrations.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static RegistrationResponse ToResponse(Registration r, bool emailSent = true, string? emailError = null) =>
        new(r.Id, r.EventId, r.Name, r.Email, r.RegisteredAt, emailSent, emailError);
}
