using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace PhishingSimulator.Web.Pages.Track;

public class ClickModel(AppDbContext db) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string token)
    {
        var target = await db.CampaignTargets
            .Include(t => t.Participant)
            .FirstOrDefaultAsync(t => t.TrackingToken == token);

        if (target is null) return NotFound();

        // Only advance forward — never downgrade status
        if (target.Status == TargetStatus.Sent || target.Status == TargetStatus.Opened)
        {
            target.Status    = TargetStatus.Clicked;
            target.ClickedAt = DateTime.UtcNow;
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ipHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(ip)));

        db.Events.Add(new PhishingEvent
        {
            CampaignTargetId = target.Id,
            EventType        = EventType.Clicked,
            OccurredAt       = DateTime.UtcNow,
            UserAgent        = Request.Headers.UserAgent.ToString()[..Math.Min(500, Request.Headers.UserAgent.ToString().Length)],
            IpAddressHash    = ipHash[..Math.Min(64, ipHash.Length)],
        });

        await db.SaveChangesAsync();

        return RedirectToPage("/Reveal/Index", new { token });
    }
}
