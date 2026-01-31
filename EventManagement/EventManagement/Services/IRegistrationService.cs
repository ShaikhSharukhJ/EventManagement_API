using EventManagement.DTOs;

namespace EventManagement.Services;

public interface IRegistrationService
{
    Task<RegistrationResponse?> RegisterAsync(int eventId, RegisterRequest request, CancellationToken cancellationToken = default);
    Task<RegistrationResponse?> GetByIdAsync(int registrationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RegistrationResponse>> GetByEventIdAsync(int eventId, CancellationToken cancellationToken = default);
    Task<bool> CancelAsync(int registrationId, CancellationToken cancellationToken = default);
}
