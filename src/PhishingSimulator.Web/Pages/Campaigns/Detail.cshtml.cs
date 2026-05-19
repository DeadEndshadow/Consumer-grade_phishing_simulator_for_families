using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using PhishingSimulator.Web.Services;
using System.Security.Claims;

namespace PhishingSimulator.Web.Pages.Campaigns;

[Authorize]
public class DetailModel(AppDbContext db, IEmailSender emailSender, ILogger<DetailModel> logger) : PageModel
{
    public Campaign Campaign { get; private set; } = null!;

    private Guid OrgId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Template)
            .Include(c => c.Targets)
                .ThenInclude(t => t.Participant)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizerId == OrgId);

        if (campaign is null) return NotFound();
        Campaign = campaign;
        return Page();
    }

    public async Task<IActionResult> OnPostSendAsync(Guid id)
    {
        var campaign = await db.Campaigns
            .Include(c => c.Template)
            .Include(c => c.Targets)
                .ThenInclude(t => t.Participant)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizerId == OrgId);

        if (campaign is null || campaign.Status != CampaignStatus.Draft)
            return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var sentCount = 0;

        foreach (var target in campaign.Targets.Where(t => t.Status == TargetStatus.Pending))
        {
            if (target.Participant.ConsentStatus != ConsentStatus.Confirmed) continue;

            var firstName    = target.Participant.Name.Split(' ')[0];
            var trackingLink = $"{baseUrl}/track/click/{target.TrackingToken}";
            var optOutLink   = $"{baseUrl}/track/optout/{target.Participant.OptOutToken}";

            var subject = campaign.Template.SubjectTemplate
                .Replace("{first_name}", firstName);

            var body = campaign.Template.BodyHtmlTemplate
                .Replace("{first_name}", firstName)
                .Replace("{tracking_link}", trackingLink)
                .Replace("{opt_out_link}", optOutLink);

            try
            {
                await emailSender.SendAsync(target.Participant.Email, target.Participant.Name, subject, body);

                target.Status  = TargetStatus.Sent;
                target.SentAt  = DateTime.UtcNow;

                db.Events.Add(new PhishingEvent
                {
                    CampaignTargetId = target.Id,
                    EventType        = EventType.Sent,
                    OccurredAt       = DateTime.UtcNow,
                });

                sentCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send phishing email to {Email}", target.Participant.Email);
            }
        }

        if (sentCount > 0)
            campaign.Status = CampaignStatus.Sent;

        await db.SaveChangesAsync();

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var campaign = await db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizerId == OrgId);

        if (campaign is not null)
        {
            db.Campaigns.Remove(campaign);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Campaigns/Index");
    }
}
