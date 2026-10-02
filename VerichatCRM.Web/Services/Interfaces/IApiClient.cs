namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IApiClient
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest body);
        Task<TResponse?> PatchAsync<TRequest, TResponse>(string endpoint, TRequest body);
        Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest body);
        void SetToken(string token);
    }

}
