using VerichatCRM.Core.Models.Pipeline;

namespace VerichatCRM.Web.Services.Interfaces
{
    public interface IPipelineApiService
    {
        Task<PipelineBoardViewModel> GetBoardAsync();
        Task<bool> MoveDealAsync(string dealId, string newStage);
    }
}
