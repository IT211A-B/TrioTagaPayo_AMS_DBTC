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
                _ctx.HttpContext?.Session.SetString("Username", result.Username);
                _ctx.HttpContext?.Session.SetString("Role", result.Role);

                if (!string.IsNullOrEmpty(result.RefreshToken))
                    _ctx.HttpContext?.Session.SetString("RefreshToken", result.RefreshToken);
            }

            return result;
        }

        // ── LOGOUT ───────────────────────────────────────────
        public async Task LogoutAsync()
        {
            try
            {
                AttachToken();
                await _http.PostAsync("/api/auth/logout",
                    new StringContent("", Encoding.UTF8, "application/json"));
            }
            catch { }
            finally
            {
                _ctx.HttpContext?.Session.Clear();
            }
        }

        // ── REFRESH TOKEN ─────────────────────────────────────
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

        // ── GET USER PROFILE (includes profilePhotoUrl) ────────
        public async Task<UserProfileDto?> GetUserProfileAsync()
        {
            AttachToken();
            var res = await _http.GetAsync("/api/Account/profile");
            if (!res.IsSuccessStatusCode) return null;
            var json = await res.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<UserProfileDto>(json, Opts);
        }

        // ── UPLOAD PROFILE PHOTO (correct endpoint) ────────────
        public async Task<(bool Success, string? PhotoUrl, string Error)> UpdateProfilePhotoAsync(Stream fileStream, string fileName)
        {
            try
            {
                AttachToken();
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.Add("Content-Type", "image/jpeg");
                content.Add(streamContent, "file", fileName);

                var response = await _http.PostAsync("/api/Account/update-profile-photo", content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    return (false, null, json);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("photoUrl", out var urlProp))
                {
                    var photoUrl = urlProp.GetString();
                    return (true, photoUrl, "");
                }
                return (false, null, "No photoUrl in response");
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }
    }

    // ── Login response model ─────────────────────────────────
    public class LoginApiResponse
    {
        public string Token { get; set; } = "";
        public string Username { get; set; } = "";
        public string Role { get; set; } = "";
        public DateTime Expiration { get; set; }
        public string RefreshToken { get; set; } = "";
        public DateTime RefreshTokenExpiry { get; set; }

        public int? StudentId { get; set; }
        public string? StudentNo { get; set; }
        public string? FullName { get; set; }
        public int? TeacherId { get; set; }
        public string? TeacherName { get; set; }
        public string? TeacherNo { get; set; }
    }

    // ── User profile DTO (from GET /api/Account/profile) ─────
    public class UserProfileDto
    {
        public string? ProfilePhotoUrl { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}   