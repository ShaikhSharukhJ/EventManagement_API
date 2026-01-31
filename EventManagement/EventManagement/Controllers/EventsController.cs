using EventManagement.DTOs;
using EventManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService)
    {
        _eventService = eventService;
    }

    /// <summary>Create a new event.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EventResponse>> Create([FromBody] CreateEventRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Capacity <= 0)
            return BadRequest("Title is required and Capacity must be greater than 0.");
        var created = await _eventService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Get all events.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EventResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var list = await _eventService.GetAllAsync(cancellationToken);
        return Ok(list);
    }

    /// <summary>Get event by ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(EventResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EventResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var evt = await _eventService.GetByIdAsync(id, cancellationToken);
        if (evt is null) return NotFound();
        return Ok(evt);
    }

    /// <summary>Delete an event. Fails if the event has any registrations.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _eventService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            var evt = await _eventService.GetByIdAsync(id, cancellationToken);
            if (evt is null) return NotFound();
            return BadRequest("Event cannot be deleted because it has existing registrations.");
        }
        return NoContent();
    }
}
