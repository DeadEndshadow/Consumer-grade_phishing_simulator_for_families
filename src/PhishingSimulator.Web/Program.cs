using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PhishingSimulator.Core.Entities;
using PhishingSimulator.Core.Enums;
using PhishingSimulator.Infrastructure.Data;
using PhishingSimulator.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await SeedTemplatesAsync(db);
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages().WithStaticAssets();

app.Run();

static async Task SeedTemplatesAsync(AppDbContext db)
{
    if (await db.Templates.AnyAsync()) return;

    db.Templates.AddRange(
        new Template
        {
            Name = "Parcel Delivery Failed",
            LureType = "Delivery notification",
            Difficulty = Difficulty.Easy,
            SubjectTemplate = "Your parcel could not be delivered — action required",
            BodyHtmlTemplate = """
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px;color:#1a1a1a;">
                  <div style="background:#fff3f3;border-left:4px solid #e53e3e;padding:14px 16px;margin-bottom:20px;border-radius:0 4px 4px 0;">
                    <strong>&#128230; Delivery attempt failed</strong>
                  </div>
                  <p>Dear {first_name},</p>
                  <p>We attempted to deliver your parcel today but were unable to complete the delivery as nobody was available to receive it.</p>
                  <table style="width:100%;border-collapse:collapse;margin:16px 0;font-size:14px;">
                    <tr style="background:#f7fafc;"><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Parcel ID</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">PK-8472610-GB</td></tr>
                    <tr><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Carrier</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">Express Parcel Services</td></tr>
                    <tr style="background:#f7fafc;"><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Deadline</td><td style="padding:8px 12px;border:1px solid #e2e8f0;color:#e53e3e;font-weight:600;">24 hours</td></tr>
                  </table>
                  <p>To reschedule your delivery, please confirm your address within <strong>24 hours</strong> or the parcel will be returned to the sender.</p>
                  <p style="text-align:center;margin:28px 0;">
                    <a href="{tracking_link}" style="background:#e53e3e;color:#fff;padding:13px 28px;text-decoration:none;border-radius:5px;display:inline-block;font-weight:600;">Reschedule Delivery &rarr;</a>
                  </p>
                  <p style="color:#718096;font-size:12px;border-top:1px solid #e2e8f0;padding-top:12px;margin-top:24px;">
                    You received this because a parcel is registered to your address.
                    <a href="{opt_out_link}" style="color:#718096;">Unsubscribe</a>
                  </p>
                </div>
                """,
            WhatToLookFor = "1. The sender domain doesn't match any real courier company. Real couriers (DHL, Royal Mail, UPS) use their own verified domain. 2. Real couriers leave a physical card when they miss you — they never ask you to click a link to reschedule. 3. The 'Parcel ID' is generic and not searchable on any real carrier website. 4. The 24-hour deadline is a pressure tactic designed to stop you from thinking clearly.",
            IsBuiltIn = true,
        },
        new Template
        {
            Name = "Subscription Payment Failed",
            LureType = "Payment / billing alert",
            Difficulty = Difficulty.Medium,
            SubjectTemplate = "[Action required] Your subscription payment could not be processed",
            BodyHtmlTemplate = """
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px;color:#1a1a1a;">
                  <div style="text-align:center;padding:20px 0 16px;">
                    <span style="font-size:28px;font-weight:900;letter-spacing:-1px;color:#e53e3e;">&#9654; StreamPlus</span>
                  </div>
                  <h2 style="color:#e53e3e;font-size:18px;margin:0 0 16px;">Payment Failed — Update Required</h2>
                  <p>Hi {first_name},</p>
                  <p>We were unable to charge your payment method for your monthly subscription. Your account will be <strong>suspended in 48 hours</strong> unless you update your billing details.</p>
                  <div style="background:#fff5f5;border:1px solid #fed7d7;border-radius:6px;padding:14px 16px;margin:16px 0;font-size:14px;">
                    <strong>&#9888; Last charge attempt:</strong> Today &nbsp;|&nbsp; <strong>Amount:</strong> £9.99 &nbsp;|&nbsp; <strong>Status:</strong> <span style="color:#e53e3e;">Declined</span>
                  </div>
                  <p>To keep your subscription active, please verify your payment method:</p>
                  <p style="text-align:center;margin:28px 0;">
                    <a href="{tracking_link}" style="background:#e53e3e;color:#fff;padding:13px 28px;text-decoration:none;border-radius:5px;display:inline-block;font-weight:600;">Update Payment Method</a>
                  </p>
                  <p style="font-size:13px;color:#718096;">If you believe this is an error, you can ignore this email and your service will continue normally.</p>
                  <p style="color:#718096;font-size:12px;border-top:1px solid #e2e8f0;padding-top:12px;margin-top:24px;">
                    StreamPlus Ltd &middot; <a href="{opt_out_link}" style="color:#718096;">Unsubscribe</a>
                  </p>
                </div>
                """,
            WhatToLookFor = "1. The service name 'StreamPlus' doesn't match any subscription you recognise — a real service email always names itself correctly (Netflix, Spotify, Amazon Prime). 2. Hover over the button: the link URL won't match the service's real domain. 3. Real subscription services show the last 4 digits of your card and never ask you to re-enter it via a link — they send you to your account settings. 4. The 48-hour suspension threat is a scare tactic. 5. The logos and branding are slightly off — too generic.",
            IsBuiltIn = true,
        },
        new Template
        {
            Name = "Unusual Sign-In Detected",
            LureType = "Security / account alert",
            Difficulty = Difficulty.Hard,
            SubjectTemplate = "[Security Alert] Unusual sign-in activity detected on your account",
            BodyHtmlTemplate = """
                <div style="font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:24px;color:#1a1a1a;">
                  <div style="background:#fffbeb;border:1px solid #fcd34d;border-radius:6px;padding:14px 16px;margin-bottom:20px;">
                    &#9888; <strong>Security Alert:</strong> We detected a sign-in attempt from an unrecognised location.
                  </div>
                  <p>Dear {first_name},</p>
                  <p>Our automated security systems flagged a login attempt on your account from an unfamiliar device and location. For your protection, we have temporarily limited access.</p>
                  <table style="width:100%;border-collapse:collapse;margin:16px 0;font-size:14px;">
                    <tr style="background:#f7fafc;"><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Location</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">Kyiv, Ukraine</td></tr>
                    <tr><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Time</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">Today, 02:14 AM</td></tr>
                    <tr style="background:#f7fafc;"><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">Device</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">Windows 11 / Chrome 124</td></tr>
                    <tr><td style="padding:8px 12px;border:1px solid #e2e8f0;font-weight:600;">IP Address</td><td style="padding:8px 12px;border:1px solid #e2e8f0;">91.214.47.x (masked)</td></tr>
                  </table>
                  <p><strong>If this was you</strong>, no action is needed.</p>
                  <p><strong>If this was not you</strong>, your account may be at risk. Please verify your identity immediately to secure your account and review recent activity:</p>
                  <p style="text-align:center;margin:28px 0;">
                    <a href="{tracking_link}" style="background:#2563eb;color:#fff;padding:13px 28px;text-decoration:none;border-radius:5px;display:inline-block;font-weight:600;">Verify My Identity</a>
                  </p>
                  <p style="color:#e53e3e;font-size:13px;font-weight:600;">&#9201; This verification link expires in 60 minutes.</p>
                  <p style="color:#718096;font-size:12px;border-top:1px solid #e2e8f0;padding-top:12px;margin-top:24px;">
                    Security Team &middot; <a href="{opt_out_link}" style="color:#718096;">Unsubscribe</a>
                  </p>
                </div>
                """,
            WhatToLookFor = "1. The foreign location (Kyiv) is chosen to maximise panic — panic is the attacker's best tool. Take a breath before clicking anything. 2. The 60-minute deadline creates artificial urgency. Real security teams give you days to respond. 3. Real companies (Google, Microsoft, Apple) send you to their own known domain to verify — not an unrecognised link. You can always go directly to the company website instead of clicking the email link. 4. The email doesn't address you by your full account username or email address — it only uses your first name. 5. Notice the sender address: it won't match the company it claims to be from.",
            IsBuiltIn = true,
        }
    );

    await db.SaveChangesAsync();
}
