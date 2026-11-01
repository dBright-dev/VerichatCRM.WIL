using System.Net;

namespace VerichatCRM.Web.Services;

public class ApiFallbackHandler : DelegatingHandler
{
    private readonly string _primaryBaseUrl;
    private readonly string _backupBaseUrl;

    public ApiFallbackHandler(IConfiguration configuration)
    {
        _primaryBaseUrl = configuration["ApiBaseUrl"]?.TrimEnd('/') ?? "";
        _backupBaseUrl = configuration["BackupApiBaseUrl"]?.TrimEnd('/') ?? "";
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Only apply fallback if the request is going to the primary API and we have a backup configured
        if (request.RequestUri != null &&
            request.RequestUri.ToString().StartsWith(_primaryBaseUrl, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrEmpty(_backupBaseUrl))
        {
            try
            {
                // 1. Try the primary API (MonsterASP)
                var response = await base.SendAsync(request, cancellationToken);

                // If the server returns a transient error (Gateway Timeout, Service Unavailable, Internal Server Error)
                if (IsTransientError(response.StatusCode))
                {
                    return await RetryWithBackup(request, cancellationToken);
                }

                return response;
            }
            catch (HttpRequestException)
            {
                // 2. If the primary API is completely down (DNS failure, connection refused, timeout)
                return await RetryWithBackup(request, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Timeout occurred
                return await RetryWithBackup(request, cancellationToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<HttpResponseMessage> RetryWithBackup(HttpRequestMessage originalRequest, CancellationToken cancellationToken)
    {
        // We must create a new request message because the original one may have already been read
        var backupRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

        // Swap the domain from the primary to the backup
        var originalUri = originalRequest.RequestUri!.ToString();
        var newUri = originalUri.Replace(_primaryBaseUrl, _backupBaseUrl, StringComparison.OrdinalIgnoreCase);
        backupRequest.RequestUri = new Uri(newUri);

        // Copy headers (including the JWT Authorization header)
        foreach (var header in originalRequest.Headers)
        {
            backupRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy the body if present (e.g., for POST/PUT requests)
        if (originalRequest.Content != null)
        {
            var bodyBytes = await originalRequest.Content.ReadAsByteArrayAsync(cancellationToken);
            backupRequest.Content = new ByteArrayContent(bodyBytes);

            foreach (var header in originalRequest.Content.Headers)
            {
                backupRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Send to the backup (Azure)
        return await base.SendAsync(backupRequest, cancellationToken);
    }

    private static bool IsTransientError(HttpStatusCode statusCode)
    {
        return statusCode == HttpStatusCode.InternalServerError || // 500
               statusCode == HttpStatusCode.BadGateway ||          // 502
               statusCode == HttpStatusCode.ServiceUnavailable ||  // 503
               statusCode == HttpStatusCode.GatewayTimeout;        // 504
    }
}