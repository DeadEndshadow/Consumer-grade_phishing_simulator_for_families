using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;

namespace PhishingSimulator.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Organizer> Organizers => Set<Organizer>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignTarget> CampaignTargets => Set<CampaignTarget>();
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<PhishingEvent> Events => Set<PhishingEvent>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Organizer>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.Name).HasMaxLength(200);
        });

        b.Entity<Participant>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email);
            e.Property(x => x.Email).HasMaxLength(320);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.ConsentStatus).HasConversion<string>();
            e.Property(x => x.OptOutToken).HasMaxLength(256);
            e.HasOne(x => x.Organizer)
                .WithMany(x => x.Participants)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<Template>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.LureType).HasMaxLength(50);
            e.Property(x => x.Difficulty).HasConversion<string>();
        });

        b.Entity<Campaign>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(200);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Organizer)
                .WithMany(x => x.Campaigns)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Template)
                .WithMany(x => x.Campaigns)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CampaignTarget>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TrackingToken).IsUnique();
            e.Property(x => x.TrackingToken).HasMaxLength(256);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasOne(x => x.Campaign)
                .WithMany(x => x.Targets)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Participant)
                .WithMany(x => x.CampaignTargets)
                .HasForeignKey(x => x.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<PhishingEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.EventType).HasConversion<string>();
            e.Property(x => x.UserAgent).HasMaxLength(500);
            e.Property(x => x.IpAddressHash).HasMaxLength(64);
            e.HasOne(x => x.CampaignTarget)
                .WithMany(x => x.Events)
                .HasForeignKey(x => x.CampaignTargetId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
