using AMS.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// ── Session ───────────────────────────────────────────────
// Stores JWT + Username + Role + RefreshToken after login.
// IdleTimeout = 8 h matches the backend JWT expiry.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "AMS.Session";
});

builder.Services.AddHttpContextAccessor();

// ── HttpClient → backend API ──────────────────────────────
// Base URL is read from appsettings.json ("ApiBaseUrl").
// Falls back to localhost:5167 if missing.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5167";

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Build ─────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// FIX: UseSession MUST be before UseAuthorization
app.UseSession();
app.UseAuthorization();

// Default route → Login first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();