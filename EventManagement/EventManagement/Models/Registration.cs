using System.ComponentModel.DataAnnotations;

namespace EventManagement.Models;

public class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    [MaxLength(450)]
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    public Event Event { get; set; } = null!;
}
