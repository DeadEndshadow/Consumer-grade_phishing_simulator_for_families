using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Infrastructure.Data;
using System.Security.Claims;

namespace PhishingSimulator.Web.Pages.Account;

public class LoginModel(AppDbContext db) : PageModel
{
    [BindProperty] public string Email { get; set; } = "";
    [BindProperty] public string Password { get; set; } = "";
    [BindProperty] public bool RememberMe { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) => ReturnUrl = returnUrl;

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        var organizer = await db.Organizers
            .FirstOrDefaultAsync(o => o.Email == Email.Trim().ToLowerInvariant());

        if (organizer is null || !PasswordHelper.Verify(Password, organizer.PasswordHash))
        {
            ErrorMessage = "Invalid email or password.";
            ReturnUrl = returnUrl;
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, organizer.Id.ToString()),
            new(ClaimTypes.Name, organizer.Name),
            new(ClaimTypes.Email, organizer.Email),
        };

        var props = new AuthenticationProperties
        {
            IsPersistent = RememberMe,
            ExpiresUtc = RememberMe
                ? DateTimeOffset.UtcNow.AddDays(30)
                : DateTimeOffset.UtcNow.AddHours(8),
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            props);

        return LocalRedirect(returnUrl ?? "/dashboard");
    }
}
