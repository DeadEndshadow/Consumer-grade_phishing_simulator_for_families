using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Reveal;

public class IndexModel(AppDbContext db) : PageModel
{
    public CampaignTarget Target { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string token)
    {
        var target = await db.CampaignTargets
            .Include(t => t.Campaign)
                .ThenInclude(c => c.Template)
            .Include(t => t.Participant)
            .FirstOrDefaultAsync(t => t.TrackingToken == token);

        if (target is null) return NotFound();
        Target = target;
        return Page();
    }
}
