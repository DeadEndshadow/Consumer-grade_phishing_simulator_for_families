using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PhishingSimulator.Web.Pages.Campaigns;

[Authorize]
public class NewModel(AppDbContext db) : PageModel
{
    [BindProperty] public string CampaignName { get; set; } = "";
    [BindProperty] public Guid TemplateId { get; set; }
    [BindProperty] public List<Guid> SelectedParticipants { get; set; } = [];
    public string? ErrorMessage { get; set; }
    public List<Template> Templates { get; private set; } = [];
    public List<Participant> Participants { get; private set; } = [];

    private Guid OrgId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync() => await LoadSelectsAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(CampaignName))
        {
            ErrorMessage = "Campaign name is required.";
            await LoadSelectsAsync();
            return Page();
        }

        if (TemplateId == Guid.Empty)
        {
            ErrorMessage = "Please select a template.";
            await LoadSelectsAsync();
            return Page();
        }

        if (!SelectedParticipants.Any())
        {
            ErrorMessage = "Select at least one family member.";
            await LoadSelectsAsync();
            return Page();
        }

        var orgId = OrgId;

        var participants = await db.Participants
            .Where(p => SelectedParticipants.Contains(p.Id) && p.OrganizerId == orgId)
            .ToListAsync();

        var campaign = new Campaign
        {
            OrganizerId = orgId,
            TemplateId = TemplateId,
            Name = CampaignName.Trim(),
            Status = CampaignStatus.Draft,
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var p in participants)
        {
            campaign.Targets.Add(new CampaignTarget
            {
                ParticipantId = p.Id,
                TrackingToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLower(),
                Status = TargetStatus.Pending,
            });
        }

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync();

        return RedirectToPage("/Campaigns/Detail", new { id = campaign.Id });
    }

    private async Task LoadSelectsAsync()
    {
        var orgId = OrgId;
        Templates = await db.Templates
            .OrderBy(t => t.Difficulty)
            .ThenBy(t => t.Name)
            .ToListAsync();
        Participants = await db.Participants
            .Where(p => p.OrganizerId == orgId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
