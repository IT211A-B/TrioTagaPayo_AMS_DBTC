// ============================================================
// Services/ApiService.cs
// FIXED: Added GetAllAsync<T> for paginated endpoints
// The backend wraps list responses in { data, page, totalCount }
// GetAllAsync unwraps that automatically.
// ============================================================

using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AMS.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _ctx;

        private static readonly JsonSerializerOptions Opts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiService(HttpClient http, IHttpContextAccessor ctx)
        {
            _http = http;
            _ctx = ctx;
        }

        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent ToJson(object obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        // ── GET (plain response — use for Attendance which returns a List directly) ──
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachToken();
            var res = await _http.GetAsync(endpoint);
            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, Opts);
        }

        // ── GET ALL (paginated — fetches all records in one call with pageSize=1000) ──
        // Handles the { data: [], page, totalCount } wrapper the backend returns
        // for Student, Teacher, and Course endpoints.
        public async Task<List<T>> GetAllAsync<T>(string endpoint)
        {
            AttachToken();
            // Pass a large pageSize so we get everything in one request
            var url = endpoint.Contains('?')
                ? $"{endpoint}&page=1&pageSize=1000"
                : $"{endpoint}?page=1&pageSize=1000";

            var res = await _http.GetAsync(url);
            if (!res.IsSuccessStatusCode) return new List<T>();

            var json = await res.Content.ReadAsStringAsync();

            // Try paginated wrapper first: { data: [...], page, totalCount }
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataProp))
                {
                    var list = JsonSerializer.Deserialize<List<T>>(dataProp.GetRawText(), Opts);
                    return list ?? new List<T>();
                }
            }
            catch { }

            // Fallback: plain array response
            return JsonSerializer.Deserialize<List<T>>(json, Opts) ?? new List<T>();
        }

        // ── POST ─────────────────────────────────────────────
        public async Task<(bool Success, T? Data, string Error)> PostAsync<T>(string endpoint, object body)
        {
            AttachToken();
            var res = await _http.PostAsync(endpoint, ToJson(body));
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return (false, default, json);
            var data = JsonSerializer.Deserialize<T>(json, Opts);
            return (true, data, "");
        }

        // ── PUT ──────────────────────────────────────────────
        public async Task<(bool Success, string Error)> PutAsync(string endpoint, object body)
        {
            AttachToken();
            var res = await _http.PutAsync(endpoint, ToJson(body));
            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        // ── DELETE ───────────────────────────────────────────
        public async Task<(bool Success, string Error)> DeleteAsync(string endpoint)
        {
            AttachToken();
            var res = await _http.DeleteAsync(endpoint);
            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        // ── PATCH ────────────────────────────────────────────
        public async Task<(bool Success, string Error)> PatchAsync(string endpoint, object? body = null)
        {
            AttachToken();
            var content = body != null
                ? ToJson(body)
                : new StringContent("", Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            var res = await _http.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        // ── LOGIN ─────────────────────────────────────────────
        public async Task<LoginApiResponse?> LoginAsync(string username, string password)
        {
            var res = await _http.PostAsync("/api/auth/login", ToJson(new { username, password }));
            var json = await res.Content.ReadAsStringAsync();
            if (!res.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<LoginApiResponse>(json, Opts);
        }
    }

    public class LoginApiResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime Expiration { get; set; }
    }
}
