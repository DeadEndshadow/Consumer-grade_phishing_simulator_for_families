using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Templates;

[Authorize]
public class EditModel(AppDbContext db) : PageModel
{
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string LureType { get; set; } = "";
    [BindProperty] public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    [BindProperty] public string SubjectTemplate { get; set; } = "";
    [BindProperty] public string BodyHtmlTemplate { get; set; } = "";
    [BindProperty] public string WhatToLookFor { get; set; } = "";
    public Guid TemplateId { get; private set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var template = await db.Templates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsBuiltIn);

        if (template is null) return NotFound();

        TemplateId = template.Id;
        Name = template.Name;
        LureType = template.LureType;
        Difficulty = template.Difficulty;
        SubjectTemplate = template.SubjectTemplate;
        BodyHtmlTemplate = template.BodyHtmlTemplate;
        WhatToLookFor = template.WhatToLookFor;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        var template = await db.Templates
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsBuiltIn);

        if (template is null) return NotFound();

        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(SubjectTemplate) ||
            string.IsNullOrWhiteSpace(BodyHtmlTemplate) ||
            string.IsNullOrWhiteSpace(WhatToLookFor))
        {
            TemplateId = id;
            ErrorMessage = "Name, subject, body HTML, and red flags are all required.";
            return Page();
        }

        template.Name = Name.Trim();
        template.LureType = LureType.Trim();
        template.Difficulty = Difficulty;
        template.SubjectTemplate = SubjectTemplate.Trim();
        template.BodyHtmlTemplate = BodyHtmlTemplate;
        template.WhatToLookFor = WhatToLookFor.Trim();

        await db.SaveChangesAsync();

        return RedirectToPage("/Templates/Detail", new { id });
    }
}
