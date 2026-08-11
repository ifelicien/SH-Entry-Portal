using SH_Entry_Portal.Components;
using SH_Entry_Portal.Services;
using SH_Entry_Portal.Data;
using SH_Entry_Portal.Models.Generated;
using Npgsql;
using Npgsql.NameTranslation;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
    
builder.Services.AddScoped<MemberService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Real HTTP form POST so the browser reliably receives the auth cookie (bypasses SignalR circuit entirely)
app.MapPost("/auth/login", async (HttpContext context, AuthService authService) =>
{
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
        IsPersistent = true,
        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
    });

    return Results.Redirect("/manage");
});

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/login");
});

app.Run();
