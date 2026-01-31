using EventManagement.Data;
using EventManagement.DTOs;
using EventManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace EventManagement.Services;

public class EventService : IEventService
{
    private readonly ApplicationDbContext _db;

    public EventService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<EventResponse> CreateAsync(CreateEventRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new Event
        {
            Title = request.Title,
            Description = request.Description,
            Date = request.Date,
            Capacity = request.Capacity,
            Location = request.Location
        };
        _db.Events.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return ToResponse(entity);
    }

    public async Task<IReadOnlyList<EventResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var list = await _db.Events.OrderBy(e => e.Date).ToListAsync(cancellationToken);
        return list.Select(ToResponse).ToList();
    }

    public async Task<EventResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Events.FindAsync(new object[] { id }, cancellationToken);
        return entity is null ? null : ToResponse(entity);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Events.Include(e => e.Registrations).FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (entity is null) return false;
        if (entity.Registrations.Count > 0) return false; // Business rule: cannot delete if registrations exist
        _db.Events.Remove(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static EventResponse ToResponse(Event e) =>
        new(e.Id, e.Title, e.Description, e.Date, e.Capacity, e.Location);
}
