using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Core.Entities;

public class Participant
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public ConsentStatus ConsentStatus { get; set; } = ConsentStatus.Pending;
    public DateTime? ConsentTimestamp { get; set; }
    public string? ConsentIp { get; set; }
    // The exact consent text shown to the participant when they agreed
    public string? ConsentText { get; set; }
    // HMAC-signed token embedded in every email for opt-out links
    public required string OptOutToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organizer Organizer { get; set; } = null!;
    public ICollection<CampaignTarget> CampaignTargets { get; set; } = [];
}
