using System.Threading.Tasks;
using BuildingBlocks.EFCore;

namespace BuildingBlocks.IntegrationTests.Mock;

public class NullDataSeeder : IDataSeeder
{
    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}