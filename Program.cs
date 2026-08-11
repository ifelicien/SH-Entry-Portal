using SH_Entry_Portal.Components;
using SH_Entry_Portal.Services;
using SH_Entry_Portal.Data;
using SH_Entry_Portal.Models.Generated;
using Npgsql;
using Npgsql.NameTranslation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<MemberService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthService>();

// Cookie-based auth: verified against Supabase Auth, session ends on browser close or 5 minutes idle
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        // Session cookie (no persistent expiry) so it clears when the browser is fully closed
        options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Limits repeated login attempts to slow down brute-force/credential-stuffing attempts
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    // Explicit rejection handler so the response is always a clean 429, not a fallback from elsewhere in the pipeline
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "text/plain";
        await context.HttpContext.Response.WriteAsync("Too many login attempts. Please wait a minute and try again.", token);
    };
});

// --- Supabase/Postgres setup ---
var connectionString = builder.Configuration.GetConnectionString("SupabaseConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
// Explicit null translator: without it, Npgsql snake_cases enum labels (e.g. "Member" -> "member"), which don't match our Postgres enum values
dataSourceBuilder.MapEnum<MemberRole>("member_role", nameTranslator: new NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<MemberStatus>("member_status", nameTranslator: new NpgsqlNullNameTranslator());
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dataSource, npgsqlOptions =>
{
    npgsqlOptions.MapEnum<MemberRole>("member_role", nameTranslator: new NpgsqlNullNameTranslator());
    npgsqlOptions.MapEnum<MemberStatus>("member_status", nameTranslator: new NpgsqlNullNameTranslator());
}));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.UseRateLimiter();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Auth endpoints ---
// Real HTTP form POST (not a Blazor event) so the browser reliably receives the auth cookie,
// independent of the SignalR circuit's lifecycle.
app.MapPost("/auth/login", async (HttpContext context, AuthService authService, Microsoft.AspNetCore.Antiforgery.IAntiforgery antiforgery) =>
{
    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (Microsoft.AspNetCore.Antiforgery.AntiforgeryValidationException)
    {
        return Results.Redirect("/login?error=1");
    }

    var form = await context.Request.ReadFormAsync();
    var email = form["email"].ToString();
    var password = form["password"].ToString();

    var valid = await authService.VerifyCredentialsAsync(email, password);
    if (!valid)
    {
        return Results.Redirect("/login?error=1");
    }

    var claims = new List<Claim> { new Claim(ClaimTypes.Name, email) };
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
    {
        // Non-persistent: cookie is deleted when the browser is fully closed
        IsPersistent = false
    });

    return Results.Redirect("/manage");
}).RequireRateLimiting("login");

// Deliberately not antiforgery-protected: must support navigator.sendBeacon() calls
// (idle timeout, tab/browser close) which can't attach a token. Logout is low-risk to force via CSRF.
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();
