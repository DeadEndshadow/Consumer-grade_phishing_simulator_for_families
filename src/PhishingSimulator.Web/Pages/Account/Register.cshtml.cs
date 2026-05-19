using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Claims;

namespace PhishingSimulator.Web.Pages.Account;

public class RegisterModel(AppDbContext db) : PageModel
{
    [BindProperty] public string Name { get; set; } = "";
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public string ConfirmPassword { get; set; } = "";
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "All fields are required.";
            return Page();
        }

        if (Password.Length < 8)
        {
            ErrorMessage = "Password must be at least 8 characters.";
            return Page();
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Passwords do not match.";
            return Page();
        }

        var normalizedEmail = Email.Trim().ToLowerInvariant();

        if (await db.Organizers.AnyAsync(o => o.Email == normalizedEmail))
        {
            ErrorMessage = "An account with that email already exists.";
            return Page();
        }

        var organizer = new Organizer
        {
            Name = Name.Trim(),
            Email = normalizedEmail,
            PasswordHash = PasswordHelper.Hash(Password),
            CreatedAt = DateTime.UtcNow,
        };

        db.Organizers.Add(organizer);
        await db.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, organizer.Id.ToString()),
            new(ClaimTypes.Name, organizer.Name),
            new(ClaimTypes.Email, organizer.Email),
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30) });

        return RedirectToPage("/Dashboard/Index");
    }
}
