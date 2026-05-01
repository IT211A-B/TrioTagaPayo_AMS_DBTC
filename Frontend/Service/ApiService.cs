using System.Net;
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

        // ── Token helpers ─────────────────────────────────────
        private void AttachToken()
        {
            var token = _ctx.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token))
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent ToJson(object obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        // ── Auto-refresh on 401 ───────────────────────────────
        // Called by every HTTP method after receiving a 401.
        // Returns true if a new token was obtained and attached.
        private async Task<bool> Handle401Async()
        {
            var refreshed = await TryRefreshTokenAsync();
            if (refreshed) AttachToken();
            return refreshed;
        }

        // ── GET (single item / plain object) ─────────────────
        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AttachToken();
            var res = await _http.GetAsync(endpoint);

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await _http.GetAsync(endpoint);

            if (!res.IsSuccessStatusCode) return default;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, Opts);
        }

        // ── GET ALL (handles paginated wrapper and plain array) ─
        public async Task<List<T>> GetAllAsync<T>(string endpoint)
        {
            AttachToken();
            var url = endpoint.Contains('?')
                ? $"{endpoint}&page=1&pageSize=1000"
                : $"{endpoint}?page=1&pageSize=1000";

            var res = await _http.GetAsync(url);

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await _http.GetAsync(url);

            if (!res.IsSuccessStatusCode) return new List<T>();

            var json = await res.Content.ReadAsStringAsync();

            // Try paginated wrapper first: { data: [], page, totalCount, ... }
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("data", out var dataProp))
                {
                    var list = JsonSerializer.Deserialize<List<T>>(dataProp.GetRawText(), Opts);
                    return list ?? new List<T>();
                }
            }
            catch { /* not paginated — fall through */ }

            return JsonSerializer.Deserialize<List<T>>(json, Opts) ?? new List<T>();
        }

        // ── POST ─────────────────────────────────────────────
        public async Task<(bool Success, T? Data, string Error)> PostAsync<T>(string endpoint, object body)
        {
            AttachToken();
            var res = await _http.PostAsync(endpoint, ToJson(body));

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await _http.PostAsync(endpoint, ToJson(body));

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

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await _http.PutAsync(endpoint, ToJson(body));

            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        // ── DELETE ───────────────────────────────────────────
        public async Task<(bool Success, string Error)> DeleteAsync(string endpoint)
        {
            AttachToken();
            var res = await _http.DeleteAsync(endpoint);

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await _http.DeleteAsync(endpoint);

            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        // ── PATCH ────────────────────────────────────────────
        public async Task<(bool Success, string Error)> PatchAsync(string endpoint, object? body = null)
        {
            AttachToken();
            HttpResponseMessage res = await SendPatch(endpoint, body);

            if (res.StatusCode == HttpStatusCode.Unauthorized && await Handle401Async())
                res = await SendPatch(endpoint, body);

            var json = await res.Content.ReadAsStringAsync();
            return (res.IsSuccessStatusCode, json);
        }

        private async Task<HttpResponseMessage> SendPatch(string endpoint, object? body)
        {
            var content = body != null
                ? ToJson(body)
                : new StringContent("", Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
            return await _http.SendAsync(req);
        }

        // ── LOGIN ─────────────────────────────────────────────
        // FIX: Now stores Username and Role in session.
        // The layout (_AdminLayout) reads Session["Username"] for the topbar.
        public async Task<LoginApiResponse?> LoginAsync(string username, string password)
        {
            var res = await _http.PostAsync("/api/auth/login",
                ToJson(new { username, password }));

            if (!res.IsSuccessStatusCode) return null;

            var json = await res.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginApiResponse>(json, Opts);

            if (result != null)
            {
                _ctx.HttpContext?.Session.SetString("JwtToken", result.Token);
                _ctx.HttpContext?.Session.SetString("Username", result.Username); // FIX: layout needs this
                _ctx.HttpContext?.Session.SetString("Role", result.Role);         // FIX: role-based UI needs this

                if (!string.IsNullOrEmpty(result.RefreshToken))
                    _ctx.HttpContext?.Session.SetString("RefreshToken", result.RefreshToken);
            }

            return result;
        }

        // ── LOGOUT ───────────────────────────────────────────
        // Clears session AND tells backend to clear its cookies.
        public async Task LogoutAsync()
        {
            try
            {
                AttachToken();
                await _http.PostAsync("/api/auth/logout",
                    new StringContent("", Encoding.UTF8, "application/json"));
            }
            catch { /* backend call is best-effort */ }
            finally
            {
                _ctx.HttpContext?.Session.Clear();
            }
        }

        // ── REFRESH TOKEN ─────────────────────────────────────
        // Sends stored refresh token to /api/Auth/refresh.
        // Updates session with new JWT on success.
        public async Task<bool> TryRefreshTokenAsync()
        {
            var refreshToken = _ctx.HttpContext?.Session.GetString("RefreshToken");
            if (string.IsNullOrEmpty(refreshToken)) return false;

            var res = await _http.PostAsync("/api/Auth/refresh",
                ToJson(new { refreshToken }));

            if (!res.IsSuccessStatusCode) return false;

            var json = await res.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<LoginApiResponse>(json, Opts);
            if (result == null || string.IsNullOrEmpty(result.Token)) return false;

            _ctx.HttpContext?.Session.SetString("JwtToken", result.Token);
            _ctx.HttpContext?.Session.SetString("Username", result.Username);
            _ctx.HttpContext?.Session.SetString("Role", result.Role);
            _ctx.HttpContext?.Session.SetString("RefreshToken", result.RefreshToken);
            return true;
        }
    }

    // ── Login response model — matches backend LoginResponse ─
    public class LoginApiResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = "";
        public DateTime RefreshTokenExpiry { get; set; }
    }
}