using System.Threading.Tasks;

namespace BuildingBlocks.Security.ApiKey
{
    public interface IGetApiKeyQuery
    {
        Task<ApiKey> ExecuteAsync(string providedApiKey);
    }
}
