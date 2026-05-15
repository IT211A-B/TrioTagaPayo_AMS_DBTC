using AMS.Controllers;
using AMS.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// No hardcoded UseUrls – let the environment decide the port.

// ── Authentication ─────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP
    options.Cookie.Name = "AMS.Auth";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// ── Session ───────────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "AMS.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});
builder.Services.AddHttpContextAccessor();

// ── HttpClient → backend API ──────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://triotagapayo-ams-dbtc.onrender.com";
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120);
});

// ── Build ─────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    // app.UseHsts(); // Disable for Render (HTTP only)
}

// app.UseHttpsRedirection(); // Disable – Render uses HTTP

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=account}/{action=login}/{id?}");

// --- Important: Bind to the port provided by Render (0.0.0.0) ---
// The PORT environment variable is set automatically by Render.
// If not set (e.g., local dev), default to 5166.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5166";
app.Run($"http://0.0.0.0:{port}");