using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Track;

public class OptOutModel(AppDbContext db) : PageModel
{
    public string ParticipantName { get; private set; } = "";
    public bool AlreadyOptedOut { get; private set; }
    public bool Done { get; private set; }

    public async Task<IActionResult> OnGetAsync(string token, bool done = false)
    {
        var participant = await db.Participants
            .FirstOrDefaultAsync(p => p.OptOutToken == token);

        if (participant is null) return NotFound();

        ParticipantName  = participant.Name;
        AlreadyOptedOut  = participant.ConsentStatus == ConsentStatus.OptedOut;
        Done             = done;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string token)
    {
        var participant = await db.Participants
            .FirstOrDefaultAsync(p => p.OptOutToken == token);

        if (participant is null) return NotFound();

        participant.ConsentStatus = ConsentStatus.OptedOut;
        await db.SaveChangesAsync();

        return RedirectToPage(new { token, done = true });
    }
}
