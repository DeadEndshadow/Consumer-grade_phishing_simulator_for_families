using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Templates;

[Authorize]
public class DetailModel(AppDbContext db) : PageModel
{
    public Template Template { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var template = await db.Templates.FirstOrDefaultAsync(t => t.Id == id);
        if (template is null) return NotFound();
        Template = template;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        var template = await db.Templates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsBuiltIn);

        if (template is not null)
        {
            db.Templates.Remove(template);
            await db.SaveChangesAsync();
        }

        return RedirectToPage("/Templates/Index");
    }
}
