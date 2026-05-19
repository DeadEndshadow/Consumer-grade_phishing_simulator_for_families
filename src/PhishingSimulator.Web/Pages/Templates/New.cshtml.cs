using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Templates;

[Authorize]
public class NewModel(AppDbContext db) : PageModel
{
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string LureType { get; set; } = "";
    [BindProperty] public Difficulty Difficulty { get; set; } = Difficulty.Easy;
    [BindProperty] public string SubjectTemplate { get; set; } = "";
    [BindProperty] public string BodyHtmlTemplate { get; set; } = "";
    [BindProperty] public string WhatToLookFor { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) ||
            string.IsNullOrWhiteSpace(SubjectTemplate) ||
            string.IsNullOrWhiteSpace(BodyHtmlTemplate) ||
            string.IsNullOrWhiteSpace(WhatToLookFor))
        {
            ErrorMessage = "Name, subject, body HTML, and red flags are all required.";
            return Page();
        }

        var template = new Template
        {
            Name = Name.Trim(),
            LureType = LureType.Trim(),
            Difficulty = Difficulty,
            SubjectTemplate = SubjectTemplate.Trim(),
            BodyHtmlTemplate = BodyHtmlTemplate,
            WhatToLookFor = WhatToLookFor.Trim(),
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow,
        };

        db.Templates.Add(template);
        await db.SaveChangesAsync();

        return RedirectToPage("/Templates/Detail", new { id = template.Id });
    }
}
