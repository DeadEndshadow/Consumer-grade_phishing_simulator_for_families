using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Claims;

namespace PhishingSimulator.Web.Pages.Campaigns;

[Authorize]
public class IndexModel(AppDbContext db) : PageModel
{
    public List<Campaign> Campaigns { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        Campaigns = await db.Campaigns
            .Where(c => c.OrganizerId == id)
            .Include(c => c.Template)
            .Include(c => c.Targets)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }
}
