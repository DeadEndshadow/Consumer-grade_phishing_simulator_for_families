using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Claims;

namespace PhishingSimulator.Web.Pages.Dashboard;

[Authorize]
public class IndexModel(AppDbContext db) : PageModel
{
    public int ParticipantCount { get; private set; }
    public int CampaignCount { get; private set; }
    public int TotalSent { get; private set; }
    public int TotalClicked { get; private set; }
    public List<Campaign> RecentCampaigns { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        ParticipantCount = await db.Participants.CountAsync(p => p.OrganizerId == id);
        CampaignCount = await db.Campaigns.CountAsync(c => c.OrganizerId == id);

        RecentCampaigns = await db.Campaigns
            .Where(c => c.OrganizerId == id)
            .OrderByDescending(c => c.CreatedAt)
            .Take(5)
            .Include(c => c.Targets)
            .ToListAsync();

        var statuses = await db.CampaignTargets
            .Where(ct => ct.Campaign.OrganizerId == id && ct.Status != TargetStatus.Pending)
            .Select(ct => ct.Status)
            .ToListAsync();

        TotalSent = statuses.Count;
        TotalClicked = statuses.Count(s => s == TargetStatus.Clicked || s == TargetStatus.Submitted);
    }
}
