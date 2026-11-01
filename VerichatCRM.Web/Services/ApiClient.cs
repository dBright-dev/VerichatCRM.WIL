using System.Net;
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

        /// <inheritdoc />
        /// <remarks>
        /// The typed client is transient, so this describes the failures of the
        /// calls this controller made - it never carries a message from another
        /// request. A call routed through a service uses that service's own
        /// instance, so those failures stay unreported here and the caller keeps
        /// its generic wording rather than showing someone else's error.
        /// </remarks>
        public string? LastError { get; private set; }

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
            if (!response.IsSuccessStatusCode)
            {
                RecordFailure(endpoint, response.StatusCode, await response.Content.ReadAsStringAsync());
                return default;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, _jsonOpts);
        }

        public async Task<ApiFile?> GetFileAsync(string endpoint)
        {
            EnsureToken();
            var response = await _http.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
            {
                RecordFailure(endpoint, response.StatusCode, await response.Content.ReadAsStringAsync());
                return null;
            }

            return new ApiFile(
                await response.Content.ReadAsByteArrayAsync(),
                response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
                response.Content.Headers.ContentDisposition?.FileName?.Trim('"', '\'') ?? "download.pdf");
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

        public async Task<TResponse?> DeleteAsync<TResponse>(string endpoint)
        {
            EnsureToken();
            var response = await _http.DeleteAsync(endpoint);
            return await ReadAsync<TResponse>(endpoint, response);
        }

        public async Task<TResponse?> SendForBodyAsync<TRequest, TResponse>(HttpMethod method, string endpoint, TRequest body)
        {
            EnsureToken();
            var json = JsonSerializer.Serialize(body);
            var request = new HttpRequestMessage(method, endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _http.SendAsync(request);
            var respJson = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                RecordFailure(endpoint, response.StatusCode, respJson);
            }

            if (string.IsNullOrWhiteSpace(respJson)) return default;
            try
            {
                return JsonSerializer.Deserialize<TResponse>(respJson, _jsonOpts);
            }
            catch (JsonException)
            {
                // A proxy that answered with an HTML error page instead of the API's
                // JSON. LastError already carries the status wording for the page.
                return default;
            }
        }

        private async Task<TResponse?> ReadAsync<TResponse>(string endpoint, HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                var respJson = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(respJson)) return default;
                return JsonSerializer.Deserialize<TResponse>(respJson, _jsonOpts);
            }

            RecordFailure(endpoint, response.StatusCode, await response.Content.ReadAsStringAsync());
            return default;
        }

        private void RecordFailure(string endpoint, HttpStatusCode status, string body)
        {
            System.Diagnostics.Debug.WriteLine($"API {(int)status} {endpoint}: {body}");
            LastError = MessageFrom(body) ?? status switch
            {
                HttpStatusCode.BadRequest => "The server rejected the values that were sent.",
                HttpStatusCode.Unauthorized => "The session is no longer valid; sign in again.",
                HttpStatusCode.Forbidden => "You are not allowed to perform this action.",
                HttpStatusCode.NotFound => "The record this refers to no longer exists.",
                HttpStatusCode.Conflict => "Another record already uses one of these values.",
                _ => $"The request failed (HTTP {(int)status})."
            };
        }

        /// <summary>
        /// Pulls the first human-readable message out of an ASP.NET Core error
        /// body. FluentValidation's auto-validation answers with
        /// ValidationProblemDetails, whose field messages live under "errors" as
        /// arrays; a hand-built BadRequestObject(new { message }) puts it under
        /// "message" instead, and ProblemDetails uses "detail".
        /// </summary>
        private static string? MessageFrom(string body)
        {
            if (string.IsNullOrWhiteSpace(body) || body[0] != '{') return null;
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                if (root.TryGetProperty("errors", out var errors)
                    && errors.ValueKind == JsonValueKind.Object)
                {
                    foreach (var field in errors.EnumerateObject())
                    {
                        if (field.Value.ValueKind == JsonValueKind.Array && field.Value.GetArrayLength() > 0)
                        {
                            var message = field.Value[0].GetString();
                            if (!string.IsNullOrWhiteSpace(message)) return message;
                        }
                        else if (field.Value.ValueKind == JsonValueKind.String)
                        {
                            var message = field.Value.GetString();
                            if (!string.IsNullOrWhiteSpace(message)) return message;
                        }
                    }
                }

                foreach (var name in new[] { "message", "detail", "title" })
                {
                    if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        var text = value.GetString();
                        if (!string.IsNullOrWhiteSpace(text)) return text;
                    }
                }
            }
            catch (JsonException)
            {
                // Not JSON after all - the caller falls back to the status code.
            }
            return null;
        }
    }
}
