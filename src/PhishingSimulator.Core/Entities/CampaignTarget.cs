using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Core.Entities;

public class CampaignTarget
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public Guid ParticipantId { get; set; }
    // Opaque per-email token used for click/open/submit tracking and reveal page routing
    public required string TrackingToken { get; set; }
    public TargetStatus Status { get; set; } = TargetStatus.Pending;
    public DateTime? SentAt { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ReportedAt { get; set; }

    public Campaign Campaign { get; set; } = null!;
    public Participant Participant { get; set; } = null!;
    public ICollection<PhishingEvent> Events { get; set; } = [];
}
