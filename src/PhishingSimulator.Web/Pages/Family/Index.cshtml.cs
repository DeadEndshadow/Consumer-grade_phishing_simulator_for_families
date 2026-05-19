using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using PhishingSimulator.Web.Services;
using System.Security.Claims;
using System.Security.Cryptography;

namespace PhishingSimulator.Web.Pages.Family;

[Authorize]
public class IndexModel(AppDbContext db, IEmailSender emailSender, ILogger<IndexModel> logger) : PageModel
{
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }
    public List<Participant> Participants { get; private set; } = [];

    private Guid OrgId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task OnGetAsync()
    {
        Participants = await db.Participants
            .Where(p => p.OrganizerId == OrgId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "Name and email are required.";
            await OnGetAsync();
            return Page();
        }

        var normalizedEmail = Email.Trim().ToLowerInvariant();
        var orgId = OrgId;

        if (await db.Participants.AnyAsync(p => p.OrganizerId == orgId && p.Email == normalizedEmail))
        {
            ErrorMessage = $"{normalizedEmail} is already in your family list.";
            await OnGetAsync();
            return Page();
        }

        var organizer = await db.Organizers.FindAsync(orgId);

        var participant = new Participant
        {
            OrganizerId = orgId,
            Name = Name.Trim(),
            Email = normalizedEmail,
            ConsentStatus = ConsentStatus.Pending,
            OptOutToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('='),
            CreatedAt = DateTime.UtcNow,
        };

        db.Participants.Add(participant);
        await db.SaveChangesAsync();

        // Build the consent confirmation URL using the participant's ID as the token
        var confirmUrl = Url.PageLink("/Consent/Confirm", values: new { token = participant.Id });
        var organizerName = organizer?.Name ?? "your family organizer";

        try
        {
            await emailSender.SendAsync(
                participant.Email,
                participant.Name,
                "You've been invited to a phishing awareness exercise",
                BuildConsentEmail(participant.Name, organizerName, confirmUrl!));

            SuccessMessage = $"{participant.Name} added. A consent invite has been sent to {participant.Email}.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send consent email to {Email}", participant.Email);
            SuccessMessage = $"{participant.Name} added, but the consent email could not be sent. " +
                             $"Share this link manually: {confirmUrl}";
        }

        await OnGetAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid id)
    {
        var participant = await db.Participants
            .FirstOrDefaultAsync(p => p.Id == id && p.OrganizerId == OrgId);

        if (participant is not null)
        {
            db.Participants.Remove(participant);
            await db.SaveChangesAsync();
        }

        return RedirectToPage();
    }

    private static string BuildConsentEmail(string name, string organizerName, string confirmUrl) => $"""
        <div style="font-family:Arial,sans-serif;max-width:580px;margin:0 auto;padding:24px;color:#1a1a1a;">
          <div style="text-align:center;margin-bottom:24px;">
            <span style="font-size:1.4rem;font-weight:800;letter-spacing:-0.03em;color:#0f172a;">
              Phish<span style="color:#6366f1;">Sim</span>
            </span>
          </div>

          <h2 style="font-size:18px;font-weight:700;margin:0 0 12px;color:#0f172a;">
            You've been invited to a cybersecurity awareness exercise
          </h2>

          <p>Hi {name},</p>

          <p>
            <strong>{organizerName}</strong> has invited you to participate in a
            phishing awareness training exercise using PhishSim.
          </p>

          <div style="background:#f8fafc;border:1px solid #e2e8f0;border-radius:8px;padding:16px;margin:16px 0;font-size:14px;line-height:1.7;">
            <strong style="display:block;margin-bottom:8px;">How it works:</strong>
            <ol style="margin:0;padding-left:18px;">
              <li>You'll receive one or more emails designed to look like phishing attempts.</li>
              <li>If you click a link, you'll land on a <em>reveal page</em> — completely harmless, no credentials captured.</li>
              <li>The reveal page shows you exactly what red flags to look for next time.</li>
              <li>You can opt out at any time via the link in any simulation email.</li>
            </ol>
          </div>

          <p>To enrol, click the button below. You will <strong>not</strong> be enrolled unless you click it.</p>

          <p style="text-align:center;margin:28px 0;">
            <a href="{confirmUrl}"
               style="background:#6366f1;color:#fff;padding:13px 28px;text-decoration:none;
                      border-radius:6px;display:inline-block;font-weight:600;font-size:15px;">
              I agree — enroll me &rarr;
            </a>
          </p>

          <p style="font-size:13px;color:#64748b;">
            Not interested? Simply ignore this email. You will never receive a simulation without confirming.
          </p>

          <p style="font-size:12px;color:#94a3b8;border-top:1px solid #e2e8f0;padding-top:12px;margin-top:24px;">
            PhishSim &mdash; family cybersecurity awareness training
          </p>
        </div>
        """;
}
