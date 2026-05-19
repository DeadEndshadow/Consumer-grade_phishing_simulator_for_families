using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;

namespace PhishingSimulator.Web.Pages.Consent;

public class ConfirmModel(AppDbContext db) : PageModel
{
    private const string ConsentText =
        "I agree to receive simulated phishing emails from PhishSim as part of a " +
        "family cybersecurity awareness exercise. I understand these are training " +
        "simulations and not real threats. I can opt out at any time.";

    public string ParticipantName { get; private set; } = "";
    public string OrganizerName { get; private set; } = "";
    public bool AlreadyConfirmed { get; private set; }
    public bool AlreadyOptedOut { get; private set; }
    public bool Confirmed { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid token, bool confirmed = false)
    {
        var participant = await db.Participants
            .Include(p => p.Organizer)
            .FirstOrDefaultAsync(p => p.Id == token);

        if (participant is null) return NotFound();

        ParticipantName  = participant.Name;
        OrganizerName    = participant.Organizer.Name;
        AlreadyConfirmed = participant.ConsentStatus == ConsentStatus.Confirmed;
        AlreadyOptedOut  = participant.ConsentStatus == ConsentStatus.OptedOut;
        Confirmed        = confirmed;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid token)
    {
        var participant = await db.Participants
            .FirstOrDefaultAsync(p => p.Id == token && p.ConsentStatus == ConsentStatus.Pending);

        if (participant is null) return NotFound();

        participant.ConsentStatus    = ConsentStatus.Confirmed;
        participant.ConsentTimestamp = DateTime.UtcNow;
        participant.ConsentIp        = HttpContext.Connection.RemoteIpAddress?.ToString();
        participant.ConsentText      = ConsentText;

        await db.SaveChangesAsync();

        return RedirectToPage(new { token, confirmed = true });
    }
}
