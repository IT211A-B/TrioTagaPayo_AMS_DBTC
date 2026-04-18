// ============================================================
// ApiService.cs
// Put this in: Services/ApiService.cs
// This is the single class that talks to your classmate's API.
// ============================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ASM.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        // Base URL of your classmate's API
        private const string BaseUrl = "http://localhost:5167"; // change to whatever port it's actually on
        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
            _http.BaseAddress = new Uri(BaseUrl);
        }

        // ── Attach JWT token from session ───────────────────
        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        // ── Generic GET ─────────────────────────────────────
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachToken();
            var res = await _http.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode) return default;

            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts());
        }

        // ── Generic POST ────────────────────────────────────
        public async Task<T?> PostAsync<T>(string endpoint, object body)
        {
            AttachToken();
            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.PostAsync(endpoint, content);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode) return default;
            return JsonSerializer.Deserialize<T>(json, JsonOpts());
        }

        // ── Generic PUT ─────────────────────────────────────
        public async Task<bool> PutAsync(string endpoint, object body)
        {
            AttachToken();
            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var res = await _http.PutAsync(endpoint, content);
            return res.IsSuccessStatusCode;
        }

        // ── Generic DELETE ──────────────────────────────────
        public async Task<bool> DeleteAsync(string endpoint)
        {
            AttachToken();
            var res = await _http.DeleteAsync(endpoint);
            return res.IsSuccessStatusCode;
        }

        // ── Generic PATCH ───────────────────────────────────
        public async Task<bool> PatchAsync(string endpoint, object? body = null)
        {
            AttachToken();
            var content = body != null
                ? new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
                : new StringContent("", Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var res = await _http.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        // ── Login — no token needed ─────────────────────────
        public async Task<LoginResult?> LoginAsync(string username, string password)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { username, password }),
                Encoding.UTF8, "application/json");

            var res = await _http.PostAsync("/api/auth/login", content);
            var json = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<LoginResult>(json, JsonOpts());
        }

        // ── JSON options — camelCase ────────────────────────
        private static JsonSerializerOptions JsonOpts() => new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    // ── Auth response shape ─────────────────────────────────
    public class LoginResult
    {
        public string Token { get; set; } = "";
        public string Role { get; set; } = "";
        public string Message { get; set; } = "";
    }
}