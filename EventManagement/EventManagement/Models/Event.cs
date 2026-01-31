namespace EventManagement.Models;

public class Event
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Capacity { get; set; }
    public string Location { get; set; } = string.Empty;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
