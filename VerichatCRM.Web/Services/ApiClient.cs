using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using VerichatCRM.Web.Services.Interfaces;

namespace VerichatCRM.Web.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContext;
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ApiClient(HttpClient http, IHttpContextAccessor httpContext)
        {
            _http = http;
            _httpContext = httpContext;
        }

        public void SetToken(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private void EnsureToken()
        {
            var token = _httpContext.HttpContext?.Session.GetString("JwtToken");
            if (!string.IsNullOrEmpty(token) && _http.DefaultRequestHeaders.Authorization == null)
            {
                SetToken(token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            EnsureToken();
            var response = await _http.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return default;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            EnsureToken();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(endpoint, content);
            return await ReadAsync<TResponse>(endpoint, response);
        }

        public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            EnsureToken();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PatchAsync(endpoint, content);
            return await ReadAsync<TResponse>(endpoint, response);
        }

        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body)
        {
            EnsureToken();
            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PutAsync(endpoint, content);
            return await ReadAsync<TResponse>(endpoint, response);
        }

        private static async Task<TResponse?> ReadAsync<TResponse>(string endpoint, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var respJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(respJson)) return default;
                return JsonSerializer.Deserialize<TResponse>(respJson, _jsonOpts);
            }

            var body = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.WriteLine($"API {(int)response.StatusCode} {endpoint}: {body}");
            return default;
        }
    }
}
