using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Core.Entities;

public class Campaign
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public Guid TemplateId { get; set; }
    public required string Name { get; set; }
    public CampaignStatus Status { get; set; } = CampaignStatus.Draft;
    public DateTime? ScheduledAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Organizer Organizer { get; set; } = null!;
    public Template Template { get; set; } = null!;
    public ICollection<CampaignTarget> Targets { get; set; } = [];
}
