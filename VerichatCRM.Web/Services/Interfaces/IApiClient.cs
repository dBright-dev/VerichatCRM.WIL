namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IApiClient
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<ApiFile?> GetFileAsync(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body);
        Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest body);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body);
        Task<TResponse?> DeleteAsync<TResponse>(string endpoint);
        void SetToken(string token);

        /// <summary>
        /// The message carried by the most recent failed API response, or null
        /// when the failure said nothing. ApiClient is scoped, so this is one
        /// request's answer, not an application-wide log. Without it a rejected
        /// create looked identical to a lost call and the page could only report
        /// "could not be created".
        /// </summary>
        string? LastError { get; }
    }

    /// <summary>
    /// A binary API response handed to the browser for download. Only the web
    /// server holds the JWT, so files such as the quotation PDF are fetched here
    /// rather than from page script.
    /// </summary>
    public sealed record ApiFile(byte[] Content, string ContentType, string FileName);
}
