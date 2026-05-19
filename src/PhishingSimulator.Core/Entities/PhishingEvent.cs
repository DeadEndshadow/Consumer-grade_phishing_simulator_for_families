using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Core.Entities;

public class PhishingEvent
{
    public Guid Id { get; set; }
    public Guid CampaignTargetId { get; set; }
    public EventType EventType { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? UserAgent { get; set; }
    // Stored as a one-way hash — never raw IP
    public string? IpAddressHash { get; set; }

    public CampaignTarget CampaignTarget { get; set; } = null!;
}
