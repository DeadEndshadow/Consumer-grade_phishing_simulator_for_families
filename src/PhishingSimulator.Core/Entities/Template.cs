using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Core.Entities;

public class Template
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string LureType { get; set; }
    public Difficulty Difficulty { get; set; }
    public required string SubjectTemplate { get; set; }
    // Merge fields: {first_name}, {tracking_link}, {opt_out_link}
    public required string BodyHtmlTemplate { get; set; }
    // Shown on the reveal page — what red flags to look for
    public required string WhatToLookFor { get; set; }
    public bool IsBuiltIn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Campaign> Campaigns { get; set; } = [];
}
