namespace EventManagement.DTOs;

public record CreateEventRequest(
    string Title,
    string Description,
    DateTime Date,
    int Capacity,
    string Location);

public record EventResponse(
    int Id,
    string Title,
    string Description,
    DateTime Date,
    int Capacity,
    string Location);
