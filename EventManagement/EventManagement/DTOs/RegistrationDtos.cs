namespace EventManagement.DTOs;

public record RegisterRequest(
    string Name,
    string Email);

public record RegistrationResponse(
    int Id,
    int EventId,
    string Name,
    string Email,
    DateTime RegisteredAt,
    bool EmailSent = true,
    string? EmailError = null);
