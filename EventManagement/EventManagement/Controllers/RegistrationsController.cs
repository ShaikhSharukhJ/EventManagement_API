using EventManagement.DTOs;
using EventManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagement.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/registrations")]
[Produces("application/json")]
public class RegistrationsController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;

    public RegistrationsController(IRegistrationService registrationService, IEventService eventService)
    {
        _registrationService = registrationService;
        _eventService = eventService;
    }

    /// <summary>Register for an event. Sends a confirmation email.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> Register(
        int eventId,
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Name and Email are required.");
        try
        {
            var registration = await _registrationService.RegisterAsync(eventId, request, cancellationToken);
            if (registration is null) return NotFound("Event not found.");
            return CreatedAtAction(nameof(GetByEventId), new { eventId }, registration);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Get a single registration by registration ID.</summary>
    [HttpGet]
    [Route("/api/registrations/{registrationId:int}")]
    [ProducesResponseType(typeof(RegistrationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistrationResponse>> GetById(int registrationId, CancellationToken cancellationToken)
    {
        var registration = await _registrationService.GetByIdAsync(registrationId, cancellationToken);
        if (registration is null) return NotFound("Registration not found.");
        return Ok(registration);
    }

    /// <summary>Get all registrations for an event.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RegistrationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RegistrationResponse>>> GetByEventId(int eventId, CancellationToken cancellationToken)
    {
        var evt = await _eventService.GetByIdAsync(eventId, cancellationToken);
        if (evt is null) return NotFound("Event not found.");
        var list = await _registrationService.GetByEventIdAsync(eventId, cancellationToken);
        return Ok(list);
    }

    /// <summary>Cancel a registration by registration ID.</summary>
    [HttpDelete]
    [Route("/api/registrations/{registrationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Cancel(int registrationId, CancellationToken cancellationToken)
    {
        var deleted = await _registrationService.CancelAsync(registrationId, cancellationToken);
        if (!deleted) return NotFound("Registration not found.");
        return NoContent();
    }
}
