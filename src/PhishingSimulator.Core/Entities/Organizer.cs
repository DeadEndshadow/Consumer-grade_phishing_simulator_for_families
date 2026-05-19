namespace PhishingSimulator.Core.Entities;

public class Organizer
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Participant> Participants { get; set; } = [];
    public ICollection<Campaign> Campaigns { get; set; } = [];
}
