using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Templates;

[Authorize]
public class IndexModel(AppDbContext db) : PageModel
{
    public List<Template> Templates { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Templates = await db.Templates
            .OrderBy(t => t.Difficulty)
            .ThenBy(t => t.Name)
            .ToListAsync();
    }
}
