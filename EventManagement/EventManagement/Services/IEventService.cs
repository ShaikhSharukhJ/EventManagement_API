using EventManagement.DTOs;

namespace EventManagement.Services;

public interface IEventService
{
    Task<EventResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EventResponse>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<EventResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
