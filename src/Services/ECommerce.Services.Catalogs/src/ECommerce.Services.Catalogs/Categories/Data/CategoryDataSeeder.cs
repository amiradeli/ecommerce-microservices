using BuildingBlocks.IdsGenerator;
using ECommerce.Services.Catalogs.Shared.Core.Contracts;

namespace ECommerce.Services.Catalogs.Categories.Data;

public class CategoryDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _dbContext;

    public CategoryDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Categories.AnyAsync())
            return;

        await _dbContext.Categories.AddRangeAsync(new List<Category>
        {
            Category.Create(SnowFlakIdGenerator.NewId(), "Electronics", "0001", "All electronic goods"),
            Category.Create(SnowFlakIdGenerator.NewId(), "Clothing", "0002", "All clothing goods"),
            Category.Create(SnowFlakIdGenerator.NewId(), "Books", "0003", "All books"),
        });
        await _dbContext.SaveChangesAsync();
    }
}