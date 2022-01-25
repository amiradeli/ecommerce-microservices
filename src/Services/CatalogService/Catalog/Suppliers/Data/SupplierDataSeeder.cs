using Bogus;
using BuildingBlocks.IdsGenerator;
using Catalog.Shared.Core.Contracts;

namespace Catalog.Suppliers.Data;

public class SupplierDataSeeder : IDataSeeder
{
    private readonly ICatalogDbContext _dbContext;

    public SupplierDataSeeder(ICatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _dbContext.Suppliers.AnyAsync())
            return;

        var suppliersFaker = new Faker<Supplier>().CustomInstantiator(faker =>
        {
            var supplier = Supplier.Create(SnowFlakIdGenerator.NewId(), faker.Person.FullName);
            return supplier;
        });

        var suppliers = suppliersFaker.Generate(5);
        await _dbContext.Suppliers.AddRangeAsync(suppliers);

        await _dbContext.SaveChangesAsync();
    }
}