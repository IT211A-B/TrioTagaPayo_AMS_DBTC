using AMS.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

// ── Session ───────────────────────────────────────────────
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
// Render.com free tier cold-starts can take 60–90 seconds.
// Timeout increased to 120s to survive the wake-up delay.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5167";
builder.Services.AddHttpClient<ApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(120); // was 30 — too short for Render cold start
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
app.UseSession();      // MUST be before UseAuthorization
app.UseAuthorization();

// Default route → Login first
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();