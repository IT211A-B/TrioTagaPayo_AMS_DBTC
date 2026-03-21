using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Frontend.Helpers
{
    // Thrown whenever the backend returns 401 Unauthorized (expired / missing JWT)
    public class SessionExpiredException : Exception
    {
        public SessionExpiredException()
            : base("Your session has expired. Please log in again.") { }
    }

    public class ApiClient
    {
        private readonly HttpClient _client;
        private static readonly JsonSerializerOptions _opts =
            new() { PropertyNameCaseInsensitive = true };

        public ApiClient(IHttpClientFactory factory, string? token)
        {
            _client = factory.CreateClient("BackendApi");
            if (!string.IsNullOrEmpty(token))
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
        }

        // ── shared 401 check ──────────────────────────────────────
        private static void ThrowIfUnauthorized(HttpResponseMessage res)
        {
            if (res.StatusCode == HttpStatusCode.Unauthorized)
                throw new SessionExpiredException();
        }

        public async Task<T?> GetAsync<T>(string url)
        {
            try
            {
                var res = await _client.GetAsync(url);
                ThrowIfUnauthorized(res);
                if (!res.IsSuccessStatusCode) return default;
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (SessionExpiredException) { throw; }
            catch { return default; }
        }

        public async Task<T?> PostAsync<T>(string url, object body)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var res = await _client.PostAsync(url, content);
                ThrowIfUnauthorized(res);
                if (!res.IsSuccessStatusCode) return default;
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (SessionExpiredException) { throw; }
            catch { return default; }
        }

        public async Task<T?> PutAsync<T>(string url, object body)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
                var res = await _client.PutAsync(url, content);
                ThrowIfUnauthorized(res);
                if (!res.IsSuccessStatusCode) return default;
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (SessionExpiredException) { throw; }
            catch { return default; }
        }

        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var res = await _client.DeleteAsync(url);
                ThrowIfUnauthorized(res);
                return res.IsSuccessStatusCode;
            }
            catch (SessionExpiredException) { throw; }
            catch { return false; }
        }

        public async Task<T?> PatchAsync<T>(string url)
        {
            try
            {
                var res = await _client.PatchAsync(url, null);
                ThrowIfUnauthorized(res);
                if (!res.IsSuccessStatusCode) return default;
                var json = await res.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, _opts);
            }
            catch (SessionExpiredException) { throw; }
            catch { return default; }
        }
    }
}
